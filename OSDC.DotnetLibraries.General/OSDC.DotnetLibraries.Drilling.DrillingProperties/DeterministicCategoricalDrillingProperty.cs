using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class DeterministicCategoricalDrillingProperty : DiscreteDrillingProperty
    {
        /// <summary>
        /// redefined to use the synonym property that is of the correct type
        /// </summary>
        [JsonIgnore]
        public override DiscreteDistribution? Value
        {
            get
            {
                return DeterministicCategoricalValue;
            }
            set
            {
                if (value != null && value is DeterministicCategoricalDistribution deterministicDiscreteValue)
                {
                    DeterministicCategoricalValue = deterministicDiscreteValue;
                }
            }
        }
        /// <summary>
        /// synonym property to Value but with the correct type
        /// </summary>
        public DeterministicCategoricalDistribution DeterministicCategoricalValue { get; set; } = new DeterministicCategoricalDistribution();
        [JsonIgnore]
        public uint? StateValue
        {
            get
            {
                if (DeterministicCategoricalValue != null)
                {
                    return DeterministicCategoricalValue.State;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (DeterministicCategoricalValue != null)
                {
                    DeterministicCategoricalValue.State = value;
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public DeterministicCategoricalDrillingProperty() : base() { }

        public DeterministicCategoricalDrillingProperty(uint numberOfStates) : base()
        {
            DeterministicCategoricalValue = new DeterministicCategoricalDistribution(numberOfStates);
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public DeterministicCategoricalDrillingProperty(DeterministicCategoricalDrillingProperty src) : base(src)
        {
            if (src != null)
            {
                DeterministicCategoricalValue = new DeterministicCategoricalDistribution(src.DeterministicCategoricalValue.NumberOfStates);
            }
        }

        public override bool Equals(DrillingProperty? cmp)
        {
            if (cmp is not null and DeterministicCategoricalDrillingProperty drillProp)
            {
                if (DeterministicCategoricalValue != null && drillProp.DeterministicCategoricalValue != null)
                {
                    return DeterministicCategoricalValue.Equals(drillProp.DeterministicCategoricalValue);
                }
                else
                {
                    return DeterministicCategoricalValue == null && drillProp.DeterministicCategoricalValue == null;
                }
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (DeterministicCategoricalValue != null && dest is not null and DeterministicCategoricalDrillingProperty drillProp && drillProp.DeterministicCategoricalValue != null)
            {
                DeterministicCategoricalValue.CopyTo(drillProp.DeterministicCategoricalValue);
            }
        }
    }
}
