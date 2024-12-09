using DWIS.Vocabulary.Schemas;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Struct|AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class SemanticFactAttribute : Attribute, ISemanticFactAttribute
    {
        public Nouns.Enum? Subject { get; protected set; } = null;
        public string? SubjectName { get; protected set; } = null;
        public Verbs.Enum Verb { get; protected set; } = Verbs.Enum.DWISVerb;
        public Nouns.Enum? Object { get; protected set; } = null;
        public string? ObjectName { get; protected set; } = null;
        public BasePhysicalQuantity.QuantityEnum? ObjectPhysicalQuantity { get; protected set; } = null;
        public PhysicalQuantity.QuantityEnum? ObjectDrillingQuantity { get; protected set; } = null;
        public Tuple<string, string>[]? ObjectAttributes { get; protected set; } = null;
        
        protected SemanticFactAttribute() { }

        public SemanticFactAttribute(string subject, Verbs.Enum verb, Nouns.Enum @object, params string[] objectAttributes)
        {
            SubjectName = subject;
            Verb = verb;
            Object = @object;
            ProcessAttributes(objectAttributes);
        }

        public SemanticFactAttribute(string subject, Nouns.Enum @object, params string[] objectAttributes)
        {
            SubjectName = subject;
            Verb = Verbs.Enum.BelongsToClass;
            Object = @object;
            ProcessAttributes(objectAttributes);
        }

        public SemanticFactAttribute(string subject, Verbs.Enum verb, string @object, params string[] objectAttributes)
        {
            SubjectName = subject;
            Verb = verb;
            ObjectName = @object;
            ProcessAttributes(objectAttributes);
        }
        public SemanticFactAttribute(Nouns.Enum subject, Verbs.Enum verb, string @object, params string[] objectAttributes)
        {
            Subject = subject;
            Verb = verb;
            ObjectName = @object;
            ProcessAttributes(objectAttributes);
        }
        public SemanticFactAttribute(string subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes)
        {
            SubjectName = subject;
            Verb = verb;
            ObjectPhysicalQuantity = quantity;
            ProcessAttributes(objectAttributes);
        }
        public SemanticFactAttribute(Nouns.Enum subject, Verbs.Enum verb, BasePhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes)
        {
            Subject = subject;
            Verb = verb;
            ObjectPhysicalQuantity = quantity;
            ProcessAttributes(objectAttributes);
        }
        public SemanticFactAttribute(string subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes)
        {
            SubjectName = subject;
            Verb = verb;
            ObjectDrillingQuantity = quantity;
            ProcessAttributes(objectAttributes);
        }
        public SemanticFactAttribute(Nouns.Enum subject, Verbs.Enum verb, PhysicalQuantity.QuantityEnum quantity, params string[] objectAttributes)
        {
            Subject = subject;
            Verb = verb;
            ObjectDrillingQuantity = quantity;
            ProcessAttributes(objectAttributes);
        }

        public virtual SemanticFact GetSemanticFact()
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

        protected void ProcessAttributes(string[] objectAttributes)
        {
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
