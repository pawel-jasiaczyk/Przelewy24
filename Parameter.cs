﻿// Author: Paweł Jasiaczyk

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
        public string StringValue
        {
            get
            {
                if (this.Value == null)
                    return "";
                else
                    return this.Value.ToString();
            }
        }
        public Type Type { get { return typeof(T); } }

        public Parameter(string name, T value)
        {
            this.Name = name;
            this.Value = value;
        }

        public override string ToString()
        {
            string result = string.Format("[Name=\"{0}\", Value=\"{1}\"]", this.Name, this.StringValue);
            return result;
        }
    }

    public interface IParameter
    {
        string Name { get; set; }
        string StringValue { get; }
        Type Type { get; }
    }
}


