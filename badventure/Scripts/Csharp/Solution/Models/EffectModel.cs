using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badventure.Scripts.Csharp.Solution.Models
{
    public class EffectModel
    {
        public int _id;
        public string _name;
        public string _description;
        public int _value;
        public int _duration;
        public static int _idCounter = 0;

        public EffectModel(string name, string description, int value, int duration)
        {
            _idCounter++;
            this._id = _idCounter;
            this._name = name;
            this._description = description;
            this._value = value;
            this._duration = duration;
        }
    }
}
