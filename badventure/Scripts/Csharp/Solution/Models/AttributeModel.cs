using Badventure.Scripts.Csharp.Solution.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badventure.Scripts.Csharp.Solution.Models
{
    public class AttributeModel
    {
        public Enums.AttributeType Attribute { get; set; }
        public int Value { get; set; }

        public AttributeModel() { }

        public AttributeModel(Enums.AttributeType attribute, int value)
        {
            Attribute = attribute;
            Value = value;
        }
    }
}
