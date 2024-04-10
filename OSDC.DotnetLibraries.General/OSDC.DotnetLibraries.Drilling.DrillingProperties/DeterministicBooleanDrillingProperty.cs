using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class DeterministicBooleanDrillingProperty : DiscreteDrillingProperty
    {
        /// <summary>
        /// redefined to use the synonym property that is of the correct type
        /// </summary>
        [JsonIgnore]
        public override DiscreteDistribution? Value
        {
            get
            {
                return DeterministicDiscreteValue;
            }
            set
            {
                if (value != null && value is DeterministicDiscreteDistribution deterministicDiscreteValue)
                {
                    DeterministicDiscreteValue = deterministicDiscreteValue;
                }
            }
        }
        /// <summary>
        /// synonym property to Value but with the correct type
        /// </summary>
        public DeterministicDiscreteDistribution DeterministicDiscreteValue { get; set; } = new DeterministicDiscreteDistribution();
        /// <summary>
        /// convenience property to access directly the probability value of the DeterministicDiscreteValue
        /// </summary>
        [JsonIgnore]
        public double? Probability
        {
            get
            {
                if (DeterministicDiscreteValue != null)
                {
                    return DeterministicDiscreteValue.GetProbability(0);
                }
                else
                {
                    return null;
                }
            }
            set
            {
            }
        }
        [JsonIgnore]
        public bool? BooleanValue
        {
            get
            {
                if (DeterministicDiscreteValue != null)
                {
                    return DeterministicDiscreteValue.Target != 0;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (DeterministicDiscreteValue != null)
                {
                    DeterministicDiscreteValue.Target = (value != null && value.Value) ? (uint)1 : 0;
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public DeterministicBooleanDrillingProperty() : base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public DeterministicBooleanDrillingProperty(DeterministicBooleanDrillingProperty src) : base(src)
        {
        }
    }
}
