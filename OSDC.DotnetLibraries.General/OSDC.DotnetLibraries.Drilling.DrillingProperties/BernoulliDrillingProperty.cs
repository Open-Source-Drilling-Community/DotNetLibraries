using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;


namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class BernoulliDrillingProperty : CategoricalDrillingProperty
    {
        /// <summary>
        /// redefined to use the synonym property that is of the correct type
        /// </summary>
        [JsonIgnore]
        public override DiscreteDistribution? Value
        {
            get
            {
                return BernoulliValue;
            }
            set
            {
                if (value != null && value is BernoulliDistribution bernoulliValue)
                {
                    BernoulliValue = bernoulliValue;
                }
            }
        }
        /// <summary>
        /// synonym property to Value but with the correct type
        /// </summary>
        public BernoulliDistribution BernoulliValue { get; set; } = new BernoulliDistribution();
        /// <summary>
        /// convenience property to access directly the probability value of the BernoulliValue
        /// </summary>
        [JsonIgnore]
        public double? Probability
        {
            get
            {
                if (BernoulliValue != null)
                {
                    return BernoulliValue.Probability;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (BernoulliValue != null)
                {
                    BernoulliValue.Probability = value;
                }
            }
        }
        /// <summary>
        /// true if Probability > 0.5, false otherwise
        /// When assigning a value, flip the probability if there is a boolean negation.
        /// </summary>
        [JsonIgnore]
        public bool? BooleanValue
        {
            get
            {
                if (BernoulliValue != null && BernoulliValue.Probability != null)
                {
                    return BernoulliValue.Probability >= 0.5;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null && BernoulliValue != null && BernoulliValue.Probability != null)
                {
                    if (value.Value && BernoulliValue.Probability < 0.5)
                    {
                        BernoulliValue.Probability = 1 - BernoulliValue.Probability;
                    }
                    else if (!value.Value && BernoulliValue.Probability >= 0.5)
                    {
                        BernoulliValue.Probability = 1 - BernoulliValue.Probability;
                    }
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public BernoulliDrillingProperty() : base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public BernoulliDrillingProperty(BernoulliDrillingProperty src) : base(src)
        {
        }
    }
}
