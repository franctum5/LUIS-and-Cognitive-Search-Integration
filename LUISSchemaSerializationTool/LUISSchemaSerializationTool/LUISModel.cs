using System;
using System.Collections.Generic;
using System.Text;

namespace TestLUISDeserialization
{

    public class LUISModel
    {
        public Model[] models { get; set; }
    }

    public class Model
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public int typeId { get; set; }
        public string readableType { get; set; }
        public Child[] children { get; set; }
        public Feature[] features { get; set; }
        public DateTime lastTrainDateTime { get; set; }
    }

    public class Child
    {
        public string id { get; set; }
        public string name { get; set; }
        public object lastModifiedDateTime { get; set; }
        public int typeId { get; set; }
        public string readableType { get; set; }
        public Child[] children { get; set; }
        public Feature[] features { get; set; }
    }

    public class Feature
    {
        public string id { get; set; }
        public string featureType { get; set; }
        public bool isActive { get; set; }
        public Featuredetails featureDetails { get; set; }
    }

    public class Featuredetails
    {
        public Featuredetails1 featureDetails { get; set; }
    }

    public class Featuredetails1
    {
        public string featureId { get; set; }
        public string featureName { get; set; }
        public string featureType { get; set; }
        public int featureTypeId { get; set; }
    }


}
