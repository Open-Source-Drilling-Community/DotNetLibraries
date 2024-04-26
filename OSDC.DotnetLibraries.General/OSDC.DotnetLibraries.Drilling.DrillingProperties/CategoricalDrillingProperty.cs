using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class CategoricalDrillingProperty : MultinomialDrillingProperty
    {
        [JsonIgnore]
        public uint? NumberOfStates
        {
            get
            {
                if (CategoricalValue != null)
                {
                    return CategoricalValue.NumberOfStates;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (CategoricalValue != null)
                {
                    CategoricalValue.NumberOfStates = value;
                }
            }
        }

        /// <summary>
        /// redefined to use the synonym property that is of the correct type
        /// </summary>
        [JsonIgnore]
        public override DiscreteDistribution? Value
        {
            get
            {
                return CategoricalValue;
            }
            set
            {
                if (value != null && value is CategoricalDistribution categoricalValue)
                {
                    CategoricalValue = categoricalValue;
                }
            }
        }
        /// <summary>
        /// synonym property to Value but with the correct type
        /// </summary>
        public CategoricalDistribution CategoricalValue { get; set; } = new CategoricalDistribution();
        /// <summary>
        /// convenience property to access directly the probability value of the BernoulliValue
        /// </summary>
        [JsonIgnore]
        public new double[]? Probabilities
        {
            get
            {
                if (CategoricalValue != null)
                {
                    return CategoricalValue.Probabilities;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (CategoricalValue != null && CategoricalValue.Probabilities != null && value != null && value.Length >= CategoricalValue.Probabilities.Length)
                {
                    double sum = 0;
                    for (int i = 0; i < CategoricalValue.Probabilities.Length; i++)
                    {
                        CategoricalValue.Probabilities[i] = value[i];
                        sum += value[i];
                    }
                    for (int i = 0; i < CategoricalValue.Probabilities.Length; i++)
                    {
                        CategoricalValue.Probabilities[i] /= sum;
                    }
                }
            }
        }
        /// <summary>
        /// return the state with the largest probability
        /// When assigning, set the probability for that state to 1 and all the others to 0.
        /// </summary>
        [JsonIgnore]
        public new uint? StateValue
        {
            get
            {
                if (CategoricalValue != null && CategoricalValue.Probabilities != null && CategoricalValue.Probabilities.Length > 0)
                {
                    uint state = 0;
                    double maxProbability = 0;
                    for (int i = 0; i < CategoricalValue.Probabilities.Length; i++)
                    {
                        if (CategoricalValue.Probabilities[i] > maxProbability)
                        {
                            state = (uint)i;
                            maxProbability = CategoricalValue.Probabilities[i];
                        }
                    }
                    return state;

                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null && CategoricalValue != null && CategoricalValue.Probabilities != null && value >= 0 && value < CategoricalValue.Probabilities.Length)
                {
                    for (int i = 0; i < CategoricalValue.Probabilities.Length; i++)
                    {
                        CategoricalValue.Probabilities[i] = 0;
                    }
                    CategoricalValue.Probabilities[(int)value] = 1.0;
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CategoricalDrillingProperty() : base() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfStates"></param>
        public CategoricalDrillingProperty(uint numberOfStates) : base()
        {
            NumberOfStates = numberOfStates;
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public CategoricalDrillingProperty(CategoricalDrillingProperty src) : base(src)
        {
   
        }

        public override bool Equals(DrillingProperty? cmp)
        {
            if (cmp is not null and CategoricalDrillingProperty drillProp)
            {
                if (CategoricalValue != null && drillProp.CategoricalValue != null)
                {
                    return CategoricalValue.Equals(drillProp.CategoricalValue);
                }
                else
                {
                    return CategoricalValue == null && drillProp.CategoricalValue == null;
                }
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (CategoricalValue != null && dest is not null and CategoricalDrillingProperty drillProp && drillProp.CategoricalValue != null)
            {
                CategoricalValue.CopyTo(drillProp.CategoricalValue);
            }
        }
    }
}
