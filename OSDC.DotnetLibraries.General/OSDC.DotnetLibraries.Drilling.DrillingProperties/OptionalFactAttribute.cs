using DWIS.Vocabulary.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class OptionalFactAttribute : SemanticFactAttribute
    {
        public byte GroupIndex { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleIdx"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalFactAttribute(byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes)
        {
            GroupIndex = bundleIdx;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleIdx"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes)
        {
            GroupIndex = bundleIdx;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleIdx"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, string @object, params string[] objectAttributes)
        {
            GroupIndex = bundleIdx;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleIdxNouns"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalFactAttribute(byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params string[] objectAttributes)
        {
            GroupIndex = bundleIdx;
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
