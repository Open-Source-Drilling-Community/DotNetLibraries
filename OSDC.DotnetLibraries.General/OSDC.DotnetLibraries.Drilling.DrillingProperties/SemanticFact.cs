using DWIS.Vocabulary.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class SemanticFact
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Nouns.Enum? Subject { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SubjectName { get; set; } = null;
        public Verbs.Enum Verb { get; set; } = Verbs.Enum.DWISVerb;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Nouns.Enum? Object { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ObjectName { get; set; } = null;
        public Tuple<string, string>[]? ObjectAttributes { get; set; } = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public SemanticFact()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public SemanticFact(SemanticFact src)
        {
            if (src != null)
            {
                Subject = src.Subject;
                SubjectName = src.SubjectName;
                Verb = src.Verb;
                Object = src.Object;
                ObjectName = src.ObjectName;
                ObjectAttributes = src.ObjectAttributes;
            }
        }
        /// <summary>
        /// initialization constructor
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        public SemanticFact(Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes)
        {
            Subject = subject;
            Verb = verb;
            Object = @object;
            ObjectAttributes = objectAttributes; 
        }
        public SemanticFact(string subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes)
        {
            SubjectName = subject;
            Verb = verb;
            Object = @object;
            ObjectAttributes = objectAttributes;
        }

        public SemanticFact(string subject, Verbs.Enum verb, string @object, Tuple<string, string>[] objectAttributes)
        {
            SubjectName = subject;
            Verb = verb;
            ObjectName = @object;
            ObjectAttributes = objectAttributes;
        }
        public SemanticFact(Nouns.Enum subject, Verbs.Enum verb, string @object, params Tuple<string, string>[] objectAttributes)
        {
            Subject = subject;
            Verb = verb;
            ObjectName = @object;
            ObjectAttributes = objectAttributes;
        }
    }
}
