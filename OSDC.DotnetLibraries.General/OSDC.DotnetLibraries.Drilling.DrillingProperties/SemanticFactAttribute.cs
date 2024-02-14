using DWIS.Vocabulary.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SemanticFactAttribute : Attribute
    {
        public Nouns.Enum? Subject { get; } = null;
        public string? SubjectName { get; } = null;
        public Verbs.Enum Verb { get; } = Verbs.Enum.DWISVerb;
        public Nouns.Enum? Object { get; } = null;
        public string? ObjectName { get; }= null;
        public Tuple<string, string>[]? ObjectAttributes { get; } = null;
        
        public SemanticFactAttribute(Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes) 
        {
            Subject = subject;
            Verb = verb;
            Object = @object;
            if (objectAttributes != null)
            {
                if (objectAttributes.Length % 2 != 0)
                {
                    throw new ArgumentException("Object attributes go in pair. Expecting an even number of strings");
                }
                ObjectAttributes = new Tuple<string, string>[objectAttributes.Length / 2];
                for (int i = 0; i < objectAttributes.Length / 2; i += 1)
                {
                    ObjectAttributes[i] = new Tuple<string, string>(objectAttributes[2 * i + 0], objectAttributes[2 * i + 1]);
                }
            }
        }
        public SemanticFactAttribute(string subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes)
        {
            SubjectName = subject;
            Verb = verb;
            Object = @object;
            if (objectAttributes != null)
            {
                if (objectAttributes.Length % 2 != 0)
                {
                    throw new ArgumentException("Object attributes go in pair. Expecting an even number of strings");
                }
                ObjectAttributes = new Tuple<string, string>[objectAttributes.Length / 2];
                for (int i = 0; i < objectAttributes.Length / 2; i += 1)
                {
                    ObjectAttributes[i] = new Tuple<string, string>(objectAttributes[2 * i + 0], objectAttributes[2 * i + 1]);
                }
            }
        }
        public SemanticFactAttribute(string subject, Verbs.Enum verb, string @object, params string[] objectAttributes)
        {
            SubjectName = subject;
            Verb = verb;
            ObjectName = @object;
            if (objectAttributes != null)
            {
                if (objectAttributes.Length % 2 != 0)
                {
                    throw new ArgumentException("Object attributes go in pair. Expecting an even number of strings");
                }
                ObjectAttributes = new Tuple<string, string>[objectAttributes.Length / 2];
                for (int i = 0; i < objectAttributes.Length / 2; i += 1)
                {
                    ObjectAttributes[i] = new Tuple<string, string>(objectAttributes[2 * i + 0], objectAttributes[2 * i + 1]);
                }
            }
        }
        public SemanticFactAttribute(Nouns.Enum subject, Verbs.Enum verb, string @object, params string[] objectAttributes)
        {
            Subject = subject;
            Verb = verb;
            ObjectName = @object;
            if (objectAttributes != null)
            {
                if (objectAttributes.Length % 2 != 0)
                {
                    throw new ArgumentException("Object attributes go in pair. Expecting an even number of strings");
                }
                ObjectAttributes = new Tuple<string, string>[objectAttributes.Length / 2];
                for (int i = 0; i < objectAttributes.Length / 2; i += 1)
                {
                    ObjectAttributes[i] = new Tuple<string, string>(objectAttributes[2 * i + 0], objectAttributes[2 * i + 1]);
                }
            }
        }
    }
}
