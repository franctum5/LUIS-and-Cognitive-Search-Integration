using System;
using System.Collections.Generic;
using System.Text;

namespace TestLUISDeserialization
{

    public class IndexSchema
    {
        public string name { get; set; }
        public string type { get; set; }
        public List<Field> fields { get; set; }

        public IndexSchema()
        {
            fields = new List<Field>();
        }
    }

    public class Field
    {
        public string name { get; set; }
        public string type { get; set; }
        public bool retrievable { get; set; }
        public bool facetable { get; set; }
        public bool filterable { get; set; }
        public List<Field> fields { get; set; }

        public Field()
        {
            fields = new List<Field>();
        }
    }

}
