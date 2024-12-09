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
    public class ExcludeFactAttribute : SemanticFactAttribute
    {
        protected ExcludeFactAttribute() { }

        public ExcludeFactAttribute(string subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
        }

        public ExcludeFactAttribute(string subject, Nouns.Enum @object, params string[] objectAttributes) : base(subject,  @object, objectAttributes)
        {
        }

        public ExcludeFactAttribute(string subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
        }
        public ExcludeFactAttribute(Nouns.Enum subject, Verbs.Enum verb, string @object, params string[] objectAttributes) : base(subject, verb, @object, objectAttributes)
        {
        }
        public ExcludeFactAttribute(string subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
        }
        public ExcludeFactAttribute(Nouns.Enum subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
        }
        public ExcludeFactAttribute(string subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
        {
        }
        public ExcludeFactAttribute(Nouns.Enum subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes) : base(subject, verb, quantity, objectAttributes)
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
