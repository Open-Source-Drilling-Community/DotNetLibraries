using DWIS.Vocabulary.Schemas;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true)]
    public class OptionalExcludeFactAttribute: OptionalFactAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleIdx"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalExcludeFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes) : base(bundleIdx, subject, verb, @object, objectAttributes)
        {
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
        public OptionalExcludeFactAttribute(byte bundleIdx, string subject, Nouns.Enum @object, params string[] objectAttributes) : base(bundleIdx, subject, @object, objectAttributes)
        {
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
        public OptionalExcludeFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(bundleIdx, subject, verb, @object, objectAttributes)
        {
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
        public OptionalExcludeFactAttribute(byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(bundleIdx, subject, verb, @object, objectAttributes)
        {
        }
        public OptionalExcludeFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(bundleIdx, subject, verb,quantity, objectAttributes)
        {
        }
        public OptionalExcludeFactAttribute(byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(bundleIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(bundleIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFactAttribute(byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(bundleIdx, subject, verb, quantity, objectAttributes)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentBundleIdx"></param>
        /// <param name="bundleIdx"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalExcludeFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes) : base(bundleIdx, subject, verb, @object, objectAttributes)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentBundleIdx"></param>
        /// <param name="bundleIdx"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalExcludeFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Nouns.Enum @object, params string[] objectAttributes) : base(bundleIdx, subject, @object, objectAttributes)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentBundleIdx"></param>
        /// <param name="bundleIdx"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalExcludeFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(bundleIdx, subject, verb, @object, objectAttributes)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentBundleIdx"></param>
        /// <param name="bundleIdxNouns"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalExcludeFactAttribute(byte parentBundleIdx, byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(bundleIdx, subject, verb, @object, objectAttributes)
        {
        }
        public OptionalExcludeFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(bundleIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFactAttribute(byte parentBundleIdx, byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(bundleIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(bundleIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFactAttribute(byte parentBundleIdx, byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(bundleIdx, subject, verb, quantity, objectAttributes)
        {
        }

        public override ExcludeFact GetSemanticFact()
        {
            ExcludeFact semanticFact = new ExcludeFact();
            semanticFact.Subject = Subject;
            semanticFact.Verb = Verb;
            semanticFact.Object = Object;
            semanticFact.ObjectAttributes = ObjectAttributes;
            semanticFact.SubjectName = SubjectName;
            semanticFact.ObjectName = ObjectName;
            semanticFact.ObjectPhysicalQuantity = ObjectPhysicalQuantity;
            semanticFact.ObjectDrillingQuantity = ObjectDrillingQuantity;
            return semanticFact;
        }
    }
}
