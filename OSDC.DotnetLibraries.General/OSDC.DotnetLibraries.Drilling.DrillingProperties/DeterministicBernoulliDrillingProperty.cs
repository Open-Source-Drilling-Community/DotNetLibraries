using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class DeterministicBernoulliDrillingProperty : DeterministicCategoricalDrillingProperty
    {
        /// <summary>
        /// redefined to use the synonym property that is of the correct type
        /// </summary>
        [JsonIgnore]
        public override DiscreteDistribution? Value
        {
            get
            {
                return DeterministicBernoulliValue;
            }
            set
            {
                if (value != null && value is DeterministicBernoulliDistribution deterministicBernoulliValue)
                {
                    DeterministicBernoulliValue = deterministicBernoulliValue;
                }
            }
        }
        /// <summary>
        /// synonym property to Value but with the correct type
        /// </summary>
        public DeterministicBernoulliDistribution DeterministicBernoulliValue { get; set; } = new DeterministicBernoulliDistribution();
        [JsonIgnore]
        public bool? BooleanValue
        {
            get
            {
                if (DeterministicBernoulliValue != null)
                {
                    return DeterministicBernoulliValue.State == 1;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (DeterministicBernoulliValue != null)
                {
                    DeterministicBernoulliValue.State = (value != null && value.Value) ? (uint)1 : 0;
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public DeterministicBernoulliDrillingProperty() : base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public DeterministicBernoulliDrillingProperty(DeterministicBernoulliDrillingProperty src) : base(src)
        {
        }
        public override bool Equals(DrillingProperty? cmp)
        {
            if (cmp is not null and DeterministicBernoulliDrillingProperty drillProp)
            {
                if (DeterministicBernoulliValue != null && drillProp.DeterministicBernoulliValue != null)
                {
                    return DeterministicBernoulliValue.Equals(drillProp.DeterministicBernoulliValue);
                }
                else
                {
                    return DeterministicBernoulliValue == null && drillProp.DeterministicBernoulliValue == null;
                }
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (DeterministicBernoulliValue != null && dest is not null and DeterministicBernoulliDrillingProperty drillProp && drillProp.DeterministicBernoulliValue != null)
            {
                DeterministicBernoulliValue.CopyTo(drillProp.DeterministicBernoulliValue);
            }
        }
    }
}
