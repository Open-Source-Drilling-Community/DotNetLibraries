using OSDC.DotnetLibraries.General.Statistics;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public abstract class ContinuousDrillingProperty : DrillingProperty
    {
        /// <summary>
        /// the probability distribution for the property
        /// </summary>
        public virtual ContinuousDistribution? Value { get; set; } = null;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ContinuousDrillingProperty() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public ContinuousDrillingProperty(ContinuousDrillingProperty src)
        {
            if (src != null)
            {
                if (src.Value != null)
                {
                    Value = src.Value.Clone();
                }
            }
        }

        /// <summary>
        /// Draw a value according to the probability distribution defined in Value
        /// </summary>
        /// <returns></returns>
        public virtual double? Realize()
        {
            if (Value == null) return null;
            return Value.Realize();
        }
    }
}
