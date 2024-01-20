using DWIS.Vocabulary.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SemanticFactAttribute : Attribute
    {
        public Nouns.Enum? Subject { get; } = null;
        public string? SubjectName { get; } = null;
        public Verbs.Enum Verb { get; } = Verbs.Enum.DWISVerb;
        public Nouns.Enum? Object { get; } = null;
        public string? ObjectName { get; }= null;
        
        public SemanticFactAttribute(Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object) 
        {
            Subject = subject;
            Verb = verb;
            Object = @object;
        }
        public SemanticFactAttribute(string subject, Verbs.Enum verb, Nouns.Enum @object)
        {
            SubjectName = subject;
            Verb = verb;
            Object = @object;
        }
        public SemanticFactAttribute(string subject, Verbs.Enum verb, string @object)
        {
            SubjectName = subject;
            Verb = verb;
            ObjectName = @object;
        }
        public SemanticFactAttribute(Nouns.Enum subject, Verbs.Enum verb, string @object)
        {
            Subject = subject;
            Verb = verb;
            ObjectName = @object;
        }
    }
}
