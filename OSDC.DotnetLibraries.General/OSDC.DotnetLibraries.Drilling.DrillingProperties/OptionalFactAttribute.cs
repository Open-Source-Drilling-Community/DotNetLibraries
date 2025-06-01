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
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class OptionalFactAttribute : SemanticFactAttribute
    {
        public byte GroupIndex { get; }
        public byte ParentGroupIndex { get; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bundleIdx"></param>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        /// <param name="objectAttributes"></param>
        /// <exception cref="ArgumentException"></exception>
        public OptionalFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = bundleIdx;
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
        public OptionalFactAttribute(byte bundleIdx, string subject, Nouns.Enum @object, params string[] objectAttributes) : base(subject, @object, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = bundleIdx;
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
        public OptionalFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = bundleIdx;
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
        public OptionalFactAttribute(byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = bundleIdx;
        }
        public OptionalFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = bundleIdx;
        }
        public OptionalFactAttribute(byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = bundleIdx;
        }
        public OptionalFactAttribute(byte bundleIdx, string subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = bundleIdx;
        }
        public OptionalFactAttribute(byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = bundleIdx;
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
        public OptionalFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = parentBundleIdx;
            GroupIndex = bundleIdx;
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
        public OptionalFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Nouns.Enum @object, params string[] objectAttributes) : base(subject, @object, objectAttributes)
        {
            ParentGroupIndex = parentBundleIdx;
            GroupIndex = bundleIdx;
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
        public OptionalFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = parentBundleIdx;
            GroupIndex = bundleIdx;
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
        public OptionalFactAttribute(byte parentBundleIdx, byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = parentBundleIdx;
            GroupIndex = bundleIdx;
        }
        public OptionalFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = parentBundleIdx;
            GroupIndex = bundleIdx;
        }
        public OptionalFactAttribute(byte parentBundleIdx, byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = parentBundleIdx;
            GroupIndex = bundleIdx;
        }
        public OptionalFactAttribute(byte parentBundleIdx, byte bundleIdx, string subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = parentBundleIdx;
            GroupIndex = bundleIdx;
        }
        public OptionalFactAttribute(byte parentBundleIdx, byte bundleIdx, Nouns.Enum subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = parentBundleIdx;
            GroupIndex = bundleIdx;
        }

        public override SemanticFact GetSemanticFact()
        {
            SemanticFact semanticFact = new SemanticFact();
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
