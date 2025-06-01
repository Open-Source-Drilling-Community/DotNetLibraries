using DWIS.Vocabulary.Schemas;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class ExcludeFact : SemanticFact
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ExcludeFact()
        {

        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public ExcludeFact(ExcludeFact src) : base(src)
        {
        }
        /// <summary>
        /// initialization constructor
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        public ExcludeFact(Nouns.Enum subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) :base(subject, verb, @object, objectAttributes)
        {
        }
        public ExcludeFact(string subject, Verbs.Enum verb, Nouns.Enum @object, params Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
        }

        public ExcludeFact(string subject, Verbs.Enum verb, string @object, Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
        }
        public ExcludeFact(Nouns.Enum subject, Verbs.Enum verb, string @object, params Tuple<string, string>[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
        }

        public ExcludeFact(string subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
        }
        public ExcludeFact(Nouns.Enum subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
        }
        public ExcludeFact(string subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
        }
        public ExcludeFact(Nouns.Enum subject, Verbs.Enum verb, DrillingPhysicalQuantity.QuantityEnum quantity, params Tuple<string, string>[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
        }

    }
}
