using System;
using System.Collections.Generic;
using System.Text;

namespace TestLUISDeserialization
{

    public class OutputFieldMapping
    {
        public string sourceFieldName { get; set; }
        public string targetFieldName { get; set; }
        public object mappingFunction { get; set; }
    }

}
