using System;
using System.Collections.Generic;
using System.Text;

namespace AzureCognitiveSearch.PowerSkills.Text.LUISExtractor
{
    public class LUISResult
    {
        public Prediction prediction { get; set; }
    }

    public class Prediction
    {
        public List<string> positiveClassifiers { get; set; }
        public Dictionary<string, object> classifiers { get; set; }
        public Dictionary<string, object> extractors { get; set; }
    }
}
