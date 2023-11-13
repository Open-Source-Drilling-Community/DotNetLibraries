using OSDC.DotnetLibraries.General.Statistics;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public abstract class DrillingProperty
    {
        /// <summary>
        /// the probability distribution for the property
        /// </summary>
        public virtual ContinuousDistribution? Value { get; set; } = null;
        public virtual List<SemanticFact>? ClassLevelSemantic { get; set; } = null;
        public virtual List<SemanticFact>? InstanceLevelSemantic { get; set; } = null;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DrillingProperty() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public DrillingProperty(DrillingProperty src)
        {
            if (src != null)
            {
                if (src.Value != null)
                {
                    Value = src.Value.Clone();
                }
                if (src.ClassLevelSemantic != null)
                {
                    ClassLevelSemantic = new List<SemanticFact>();
                    foreach (SemanticFact fact in src.ClassLevelSemantic)
                    {
                        ClassLevelSemantic.Add(new SemanticFact(fact));
                    }
                }
                if (src.InstanceLevelSemantic != null)
                {
                    InstanceLevelSemantic = new List<SemanticFact>();
                    foreach (SemanticFact fact in src.InstanceLevelSemantic)
                    {
                        InstanceLevelSemantic.Add(new SemanticFact(fact));
                    }
                }
            }
        }
    }
}