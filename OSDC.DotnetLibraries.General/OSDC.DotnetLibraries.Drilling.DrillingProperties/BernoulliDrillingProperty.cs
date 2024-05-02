using DWIS.Client.ReferenceImplementation;
using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;


namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class BernoulliDrillingProperty : MultinomialDrillingProperty
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
        public BernoulliDrillingProperty(BernoulliDrillingProperty src) : base()
        {
            if (src != null && src.BernoulliValue != null)
            {
                BernoulliValue.Probability = src.BernoulliValue.Probability;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signals"></param>
        /// <returns></returns>
        public override bool FuseData(List<AcquiredSignals>? signals)
        {
            bool ok = false;
            if (signals != null && signals.Count > 0)
            {
                double sumProbability = 0;
                double productProbability = 1;
                foreach (var signalList in signals)
                {
                    if (signalList != null)
                    {
                        foreach (var signal in signalList)
                        {
                            if (signal.Value != null && signal.Value.Count >= 1)
                            {
                                double[]? probabilities = signal.Value[0].GetValue<double[]>();
                                if (probabilities != null)
                                {
                                    sumProbability += probabilities[0];
                                    productProbability *= probabilities[0];
                                }
                            }
                        }
                    }
                }
                double fusedProbability = sumProbability - productProbability; 
                Probability = fusedProbability;
                ok = true;
            }
            return ok;
        }
        public override bool Equals(DrillingProperty? cmp)
        {
            if (cmp is not null and BernoulliDrillingProperty drillProp)
            {
                if (BernoulliValue != null && drillProp.BernoulliValue != null)
                {
                    return BernoulliValue.Equals(drillProp.BernoulliValue);
                }
                else
                {
                    return BernoulliValue == null && drillProp.BernoulliValue == null;
                }
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (BernoulliValue != null && dest is not null and BernoulliDrillingProperty drillProp && drillProp.BernoulliValue != null)
            {
                BernoulliValue.CopyTo(drillProp.BernoulliValue);
            }
        }
     
    }
}
