using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class MultinomialDrillingProperty : DiscreteDrillingProperty
    {
        /// <summary>
        /// redefined to use the synonym property that is of the correct type
        /// </summary>
        [JsonIgnore]
        public override DiscreteDistribution? Value
        {
            get
            {
                return MultinomialValue;
            }
            set
            {
                if (value != null && value is BernoulliDistribution multinomialValue)
                {
                    MultinomialValue = multinomialValue;
                }
            }
        }
        /// <summary>
        /// synonym property to Value but with the correct type
        /// </summary>
        public MultinomialDistribution? MultinomialValue { get; set; } = new MultinomialDistribution();
        /// <summary>
        /// convenience property to access directly the probability value of the BernoulliValue
        /// </summary>
        [JsonIgnore]
        public double[]? Probabilities
        {
            get
            {
                if (MultinomialValue != null)
                {
                    return MultinomialValue.Probabilities;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (MultinomialValue != null && MultinomialValue.Probabilities != null && value != null && MultinomialValue.Probabilities.Length == value.Length)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        MultinomialValue.Probabilities[i] = value[i];
                    }
                }
            }
        }
        /// <summary>
        /// return the state with the largest probability.
        /// For assignment, it sets the probability to 1 for the passed state and 0 to all other states.
        /// </summary>
        [JsonIgnore]
        public uint? StateValue
        {
            get
            {
                if (MultinomialValue != null && MultinomialValue.Probabilities != null)
                {
                    int state = 0;
                    double maxProb = 0;
                    for (int i = 0; i <  MultinomialValue.Probabilities.Length; i++)
                    {
                        if (MultinomialValue.Probabilities[i] > maxProb)
                        {
                            state = i;
                            maxProb = MultinomialValue.Probabilities[i];
                        }
                    }
                    return (uint)state;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null && MultinomialValue != null && MultinomialValue.Probabilities != null && value < MultinomialValue.Probabilities.Length)
                {
                    for (int i = 0; i < MultinomialValue.Probabilities.Length; i++)
                    {
                        MultinomialValue.Probabilities[i] = 0;
                    }
                    MultinomialValue.Probabilities[value.Value] = 1;
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public MultinomialDrillingProperty() : base() { }

        public MultinomialDrillingProperty(uint? numberOfTrials, uint? numberOfStates) : base()
        {
            MultinomialValue = new MultinomialDistribution(numberOfTrials, numberOfStates, null);
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public MultinomialDrillingProperty(MultinomialDrillingProperty src) : base(src)
        {
            if (src != null && src.MultinomialValue != null)
            {
                MultinomialValue = new MultinomialDistribution(src.MultinomialValue.NumberTrials, src.MultinomialValue.NumberOfStates, src.MultinomialValue.Probabilities);
            }
        }

        public override bool Equals(DrillingProperty? cmp)
        {
            if (cmp is not null and MultinomialDrillingProperty drillProp)
            {
                if (MultinomialValue != null && drillProp.MultinomialValue != null)
                {
                    return MultinomialValue.Equals(drillProp.MultinomialValue);
                }
                else
                {
                    return MultinomialValue == null && drillProp.MultinomialValue == null;
                }
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (MultinomialValue != null && dest is not null and MultinomialDrillingProperty drillProp && drillProp.MultinomialValue != null)
            {
                MultinomialValue.CopyTo(drillProp.MultinomialValue);
            }
        }
    }
}
