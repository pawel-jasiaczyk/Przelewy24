using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24
{
    public class Parameter<T> : IParameter
    {
        public string Name { get; set; }
        public T Value { get; set; }
        public string StringValue { get { return this.Value.ToString (); } }
        public Type Type { get { return typeof(T); } }

        public Parameter(string name, T value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public interface IParameter
    {
        string Name { get; set; }
        string StringValue { get; }
        Type Type { get; }
    }
}


