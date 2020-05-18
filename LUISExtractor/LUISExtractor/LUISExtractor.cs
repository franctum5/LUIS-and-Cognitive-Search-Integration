using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Net.Http;
using System.Threading;
using System.Linq;

namespace AzureCognitiveSearch.PowerSkills.Text.LUISExtractor
{
    /// <summary>
    /// Receives a blob of text as an input and invoke LUIS-D prediction endpoint to perform entities extraction and 
    /// document classification (depending on the LUIS model definition)
    /// </summary>
    public static class LUISExtractor
    {
        private static string LUISPredictionendpoint = Environment.GetEnvironmentVariable("LUISPredictionEndpoint");
        private static string LUISCheckOperationStatusEndpoint = Environment.GetEnvironmentVariable("LUISCheckOperationStatusEndpoint");
        private static string LUISGetResultEndpoint = Environment.GetEnvironmentVariable("LUISGetResultEndpoint");
        private static string LUISAppId = Environment.GetEnvironmentVariable("LUISAppId");
        private static string LUISPredictionKey = Environment.GetEnvironmentVariable("LUISPredictionKey");
        private readonly static HttpClient httpClient = new HttpClient();

        [FunctionName("luis-extractor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            Microsoft.Azure.WebJobs.ExecutionContext executionContext)
        {
            log.LogInformation("LUIS-D Extractor and Classifier function");
            
            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }


                       
            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {

                    var text = inRecord.Data["text"] as string;
                    Dictionary<string, string> luisQuery = new Dictionary<string, string>();
                    luisQuery.Add("query", text);
                    string luisInput = JsonConvert.SerializeObject(luisQuery);

                    //call luis
                    log.LogInformation("Sending the document to LUIS-D for classification and extraction");

                    string operationId = null;
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format(LUISPredictionendpoint, LUISAppId)))
                    {
                        requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", LUISPredictionKey);
                        requestMessage.Content = new StringContent(luisInput);
                        requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                        HttpResponseMessage response = httpClient.SendAsync(requestMessage).GetAwaiter().GetResult();

                        operationId = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult())["operationId"];
                    }


                    log.LogInformation("Checking for classification and extraction status...");

                    string status = null;
                    do
                    {
                        Thread.Sleep(1000);
                        using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, string.Format(LUISCheckOperationStatusEndpoint,LUISAppId, operationId)))
                        {
                            requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", LUISPredictionKey);

                            HttpResponseMessage response = httpClient.SendAsync(requestMessage).GetAwaiter().GetResult();
                            status = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult())["status"];
                        }

                        log.LogInformation($"Extraction status: {status}");
                    }
                    while (status != "succeeded" && status != "failed");


                    
                    if(status == "succeeded")
                    {
                        LUISResult luisResult = null;
                        log.LogInformation("Downloading for classification and extraction results...");

                        using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, string.Format(LUISGetResultEndpoint, LUISAppId, operationId)))
                        {
                            requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", LUISPredictionKey);

                            HttpResponseMessage response = httpClient.SendAsync(requestMessage).GetAwaiter().GetResult();
                            luisResult = JsonConvert.DeserializeObject<LUISResult>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                        }

                        Dictionary<string, object> output = new Dictionary<string, object>();

                        if (luisResult.prediction.classifiers != null && luisResult.prediction.classifiers.Count > 0)
                            outRecord.Data.Add("Class", luisResult.prediction.classifiers.First().Value);

                        foreach (var entity in luisResult.prediction.extractors)
                        {
                            if (entity.Key != "$instance")
                                outRecord.Data.Add(entity.Key, entity.Value);
                        }


                        outRecord.Data["result"] = "success";
                    }
                    else
                    {
                        outRecord.Data["result"] = "failed";
                    }


                    return outRecord;
                });

            return new OkObjectResult(response);
        }
    }
}
