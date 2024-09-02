using DWIS.Vocabulary.Schemas;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class OptionalFact : SemanticFact
    {
        public byte ParentGroupIndex { get; set; } = 0;
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
                ParentGroupIndex = src.ParentGroupIndex;
                GroupIndex = src.GroupIndex;
            }
        }
        /// <summary>
        /// initialization constructor
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        public OptionalFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte groupIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = groupIdx;
        }

        public OptionalFact(byte groupIdx, string subject, Verbs.Enum verb, string @object, Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = groupIdx;
        }

        public OptionalFact(byte groupIdx, string subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte groupIdx, string subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = 0;
            GroupIndex = groupIdx;
        }

        public OptionalFact(byte parentGroupIdx, byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = parentGroupIdx;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte parentGroupIdx, byte groupIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = parentGroupIdx;
            GroupIndex = groupIdx;
        }

        public OptionalFact(byte parentGroupIdx, byte groupIdx, string subject, Verbs.Enum verb, string @object, Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = parentGroupIdx;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte parentGroupIdx, byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
            ParentGroupIndex = parentGroupIdx;
            GroupIndex = groupIdx;
        }

        public OptionalFact(byte parentGroupIdx, byte groupIdx, string subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = parentGroupIdx;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte parentGroupIdx, byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = parentGroupIdx;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte parentGroupIdx, byte groupIdx, string subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = parentGroupIdx;
            GroupIndex = groupIdx;
        }
        public OptionalFact(byte parentGroupIdx, byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
            ParentGroupIndex = parentGroupIdx;
            GroupIndex = groupIdx;
        }
    }
}
