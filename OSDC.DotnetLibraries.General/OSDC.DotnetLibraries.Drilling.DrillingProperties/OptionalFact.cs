using DWIS.Vocabulary.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class OptionalFact : SemanticFact
    {
        public byte GroupIndex { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public OptionalFact()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public OptionalFact(OptionalFact src) : base(src)
        {
            if (src != null)
            {
                GroupIndex = src.GroupIndex;
            }
        }
        /// <summary>
        /// initialization constructor
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        public OptionalFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes)
        {
            GroupIndex = groupIdx;
            Subject = subject;
            Verb = verb;
            Object = @object;
            ObjectAttributes = objectAttributes;
        }
        public OptionalFact(byte groupIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes)
        {
            GroupIndex = groupIdx;
            SubjectName = subject;
            Verb = verb;
            Object = @object;
            ObjectAttributes = objectAttributes;
        }

        public OptionalFact(byte groupIdx, string subject, Verbs.Enum verb, string @object, Tuple<string, string>[] objectAttributes)
        {
            GroupIndex = groupIdx;
            SubjectName = subject;
            Verb = verb;
            ObjectName = @object;
            ObjectAttributes = objectAttributes;
        }
        public OptionalFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params Tuple<string, string>[] objectAttributes)
        {
            GroupIndex = groupIdx;
            Subject = subject;
            Verb = verb;
            ObjectName = @object;
            ObjectAttributes = objectAttributes;
        }
    }
}
