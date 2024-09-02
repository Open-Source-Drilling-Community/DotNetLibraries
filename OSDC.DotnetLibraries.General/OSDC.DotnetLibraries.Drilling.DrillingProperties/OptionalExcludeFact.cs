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
    public class OptionalExcludeFact : OptionalFact
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public OptionalExcludeFact()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public OptionalExcludeFact(OptionalExcludeFact src) : base(src)
        {
        }
        /// <summary>
        /// initialization constructor
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        public OptionalExcludeFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) : base(groupIdx, subject, verb, @object, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte groupIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) : base(groupIdx, subject, verb, @object, objectAttributes)
        {
        }

        public OptionalExcludeFact(byte groupIdx, string subject, Verbs.Enum verb, string @object, Tuple<string, string>[] objectAttributes) : base(groupIdx, subject, verb, @object, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params Tuple<string, string>[] objectAttributes) : base(groupIdx, subject, verb, @object, objectAttributes)
        {
        }

        public OptionalExcludeFact(byte groupIdx, string subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(groupIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(groupIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte groupIdx, string subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(groupIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(groupIdx, subject, verb, quantity, objectAttributes)
        {
        }

        public OptionalExcludeFact(byte parentGroupIdx, byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) : base(parentGroupIdx, groupIdx, subject, verb, @object, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte parentGroupIdx, byte groupIdx, string subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) : base(parentGroupIdx, groupIdx, subject, verb, @object, objectAttributes)
        {
        }

        public OptionalExcludeFact(byte parentGroupIdx, byte groupIdx, string subject, Verbs.Enum verb, string @object, Tuple<string, string>[] objectAttributes) : base(parentGroupIdx, groupIdx, subject, verb, @object, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte parentGroupIdx, byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, string @object, params Tuple<string, string>[] objectAttributes) : base(parentGroupIdx, groupIdx, subject, verb, @object, objectAttributes)
        {
        }

        public OptionalExcludeFact(byte parentGroupIdx, byte groupIdx, string subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(parentGroupIdx, groupIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte parentGroupIdx, byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(parentGroupIdx, groupIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte parentGroupIdx, byte groupIdx, string subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(parentGroupIdx, groupIdx, subject, verb, quantity, objectAttributes)
        {
        }
        public OptionalExcludeFact(byte parentGroupIdx, byte groupIdx, Nouns.Enum subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(parentGroupIdx, groupIdx, subject, verb, quantity, objectAttributes)
        {
        }
    }
}
