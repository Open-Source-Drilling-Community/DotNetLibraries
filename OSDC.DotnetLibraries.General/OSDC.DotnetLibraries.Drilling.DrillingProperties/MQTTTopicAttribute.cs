using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MQTTTopicAttribute : Attribute
    {
        public virtual string? Topic { get; set; } = null;

        public MQTTTopicAttribute(string? topic) : base()
        {
            Topic = topic;
        }
    }
}
