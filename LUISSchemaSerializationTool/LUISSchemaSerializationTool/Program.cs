using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;


namespace TestLUISDeserialization
{
    class Program
    {
        private static string luisModelSchemaEndpoint = "https://westus.api.cognitive.microsoft.com/luis/authoring/v4.0-preview/documents/apps/{0}/models";
        private readonly static HttpClient httpClient = new HttpClient();

        static void Main(string[] args)
        {
            //FileStream fs = File.OpenRead("ModelSchema.json");
            //StreamReader sr = new StreamReader(fs);
            //string json = sr.ReadToEnd();

            Console.WriteLine("Insert your LUIS App Id: ");
            string luisAppId = Console.ReadLine();

            Console.WriteLine("Insert your LUIS App Key: ");
            string luisAppKey = Console.ReadLine();

            string json = null;
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, string.Format(luisModelSchemaEndpoint, luisAppId)))
            {
                requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", luisAppKey);
                HttpResponseMessage response = httpClient.SendAsync(requestMessage).GetAwaiter().GetResult();

                json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            var result = JsonConvert.DeserializeObject<LUISModel>(json);


            List<IndexSchema> indexFields = new List<IndexSchema>();
            List<OutputFieldMapping> outputFieldMappings = new List<OutputFieldMapping>();
            List<Output> outputs = new List<Output>();
            CustomSkillSchema customSkillSchema = new CustomSkillSchema();

            //classifiers
            if (result.models.Any(m => m.typeId == 0))
            {
                //index
                IndexSchema indexField = new IndexSchema()
                {
                    name = "Classes",
                    type = "Collection(Edm.String)"
                };

                indexFields.Add(indexField);

                //indexer
                OutputFieldMapping outputFieldMapping = new OutputFieldMapping()
                {
                    sourceFieldName = "/document/content/Classes",
                    targetFieldName = "Classes"
                };

                outputFieldMappings.Add(outputFieldMapping);

                //skillset
                Output output = new Output()
                {
                    name = "Classes",
                    targetName = "Classes"
                };

                outputs.Add(output);
            }



            foreach (Model model in result.models)
            {
               //extractors
               if (model.typeId == 1)
                {
                    //index
                    IndexSchema indexField = new IndexSchema()
                    {
                        name = model.name,
                        type = model.children == null || model.children.Length == 0 ? "Collection(Edm.String)" : "Collection(Edm.ComplexType)"
                    };

                    foreach (Child child in model.children)
                    {
                        Field field = new Field()
                        {
                            name = child.name,
                            type = "Collection(Edm.String)",
                            facetable = true,
                            filterable = true,
                            retrievable = true
                        };

                        indexField.fields.Add(field);
                    }

                    indexFields.Add(indexField);

                    //indexer
                    OutputFieldMapping outputFieldMapping = new OutputFieldMapping()
                    {
                        sourceFieldName = $"/document/content/{model.name}",
                        targetFieldName = model.name
                    };

                    outputFieldMappings.Add(outputFieldMapping);

                    //skillset
                    Output output = new Output()
                    {
                        name = model.name,
                        targetName = model.name
                    };

                    outputs.Add(output);
                }
            }

            customSkillSchema.outputs = outputs;

            string indexJson = JsonConvert.SerializeObject(indexFields);
            string indexerJson = JsonConvert.SerializeObject(outputFieldMappings);
            string skillsetJson = JsonConvert.SerializeObject(customSkillSchema);


            using (StreamWriter sw = File.CreateText("index.json"))
            {
                sw.Write(indexJson);
            }

            using (StreamWriter sw = File.CreateText("indexer.json"))
            {
                sw.Write(indexerJson);
            }

            using (StreamWriter sw = File.CreateText("skillset.json"))
            {
                sw.Write(skillsetJson);
            }

            //Dictionary<string, object> output = new Dictionary<string, object>();

            //if(result.prediction.classifiers != null && result.prediction.classifiers.Count > 0)
            //    output.Add("Class", result.prediction.classifiers.First().Value);

            //foreach(var entity in result.prediction.extractors)
            //{
            //    if(entity.Key != "$instance")
            //        output.Add(entity.Key, entity.Value);
            //}
        }
    }
}
