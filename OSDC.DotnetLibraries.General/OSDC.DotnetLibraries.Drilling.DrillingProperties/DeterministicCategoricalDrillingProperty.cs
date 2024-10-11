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
        /// 
        /// </summary>
        [JsonIgnore]
        public override uint? NumberOfStates {
            get
            {
                return DeterministicCategoricalValue?.NumberOfStates;
            }
        }
        [JsonIgnore]
        public override double[]? Probabilities
        {
            get
            {
                if (DeterministicCategoricalValue != null)
                {
                    if (DeterministicCategoricalValue.State != null && DeterministicCategoricalValue.NumberOfStates != null && DeterministicCategoricalValue.State < DeterministicCategoricalValue.NumberOfStates)
                    {
                        double[] probabilities = new double[DeterministicCategoricalValue.NumberOfStates.Value];
                        for (int i = 0; i < probabilities.Length; i++)
                        {
                            probabilities[i] = 0;
                        }
                        probabilities[DeterministicCategoricalValue.State.Value] = 1.0;
                        return probabilities;
                    }
                }
                return null;
            }
            set
            {
                if (value != null && DeterministicCategoricalValue != null)
                {
                    double max = 0.0;
                    int imax = -1;
                    for (int i = 0; i <  value.Length; i++)
                    {
                        if (value[i]> max)
                        {
                            max = value[i];
                            imax = i;
                        }
                    }
                    if (imax>= 0)
                    {
                        DeterministicCategoricalValue.State = (uint)imax;
                    }
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
