using DWIS.Client.ReferenceImplementation;
using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class TernaryDrillingProperty : MultinomialDrillingProperty
    {
        /// <summary>
        /// redefined to use the synonym property that is of the correct type
        /// </summary>
        [JsonIgnore]
        public override DiscreteDistribution? Value
        {
            get
            {
                return TernaryValue;
            }
            set
            {
                if (value != null && value is TernaryDistribution ternaryValue)
                {
                    TernaryValue = ternaryValue;
                }
            }
        }
        /// <summary>
        /// synonym property to Value but with the correct type
        /// </summary>
        public TernaryDistribution TernaryValue { get; set; } = new TernaryDistribution();
        /// <summary>
        /// convenience property to access directly the probability value of the BernoulliValue
        /// </summary>
        [JsonIgnore]
        public double? Probability1
        {
            get
            {
                if (TernaryValue != null)
                {
                    return TernaryValue.Probability1;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (TernaryValue != null)
                {
                    TernaryValue.Probability1 = value;
                }
            }
        }
        /// <summary>
        /// convenience property to access directly the probability value of the BernoulliValue
        /// </summary>
        [JsonIgnore]
        public double? Probability2
        {
            get
            {
                if (TernaryValue != null)
                {
                    return TernaryValue.Probability2;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (TernaryValue != null)
                {
                    TernaryValue.Probability2 = value;
                }
            }
        }
        /// <summary>
        /// convenience property to access directly the probability value of the TernaryValue
        /// </summary>
        [JsonIgnore]
        public double[]? Probability
        {
            get
            {
                if (TernaryValue != null)
                {
                    return TernaryValue.Probabilities;
                }
                else
                {
                    return null;
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
                if (TernaryValue != null && TernaryValue.Probabilities != null && TernaryValue.Probabilities.Length > 0)
                {
                    uint state = 0;
                    double maxProbability = 0;
                    for (int i = 0; i < TernaryValue.Probabilities.Length; i++)
                    {
                        if (TernaryValue.Probabilities[i] > maxProbability)
                        {
                            state = (uint)i;
                            maxProbability = TernaryValue.Probabilities[i];
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
                if (value != null && TernaryValue != null && TernaryValue.Probabilities != null && value >= 0 && value < TernaryValue.Probabilities.Length)
                {
                    for (int i = 0; i < TernaryValue.Probabilities.Length; i++)
                    {
                        TernaryValue.Probabilities[i] = 0;
                    }
                    TernaryValue.Probabilities[(int)value] = 1.0;
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public TernaryDrillingProperty() : base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public TernaryDrillingProperty(TernaryDrillingProperty src) : base()
        {
            if (src != null && src.TernaryValue != null)
            {
                TernaryValue.Probability1 = src.TernaryValue.Probability1;
                TernaryValue.Probability2 = src.TernaryValue.Probability2;
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
            if (signals != null && signals.Count > 0 && NumberOfStates != null)
            {
                double[] sums = new double[NumberOfStates.Value];
                double[] products = new double[NumberOfStates.Value];
                for (int i = 0; i < NumberOfStates.Value; i++)
                {
                    sums[i] = 0;
                    products[i] = 1;
                }
                foreach (var signalList in signals)
                {
                    if (signalList != null)
                    {
                        foreach (var signal in signalList)
                        {
                            if (signal.Value != null && signal.Value.Count >= 1)
                            {
                                double[]? probabilities = signal.Value[0].GetValue<double[]>();
                                if (probabilities != null && probabilities.Length == NumberOfStates.Value)
                                {
                                    for (int i = 0; i < NumberOfStates.Value; i++)
                                    {
                                        sums[i] += probabilities[i];
                                        products[i] *= probabilities[i];
                                    }
                                }
                            }
                        }
                    }
                }
                double[] probs = new double[NumberOfStates.Value];
                double sum = 0;
                for (int i = 0; i < NumberOfStates.Value; i++)
                {
                    probs[i] = sums[i] - products[i];
                    sum += probs[i];
                }
                for (int i = 0; i < NumberOfStates.Value; i++)
                {
                    probs[i] /= sum;
                }
                Probabilities = probs;
                ok = true;
            }
            return ok;
        }
        public override bool Equals(DrillingProperty? cmp)
        {
            if (cmp is not null and TernaryDrillingProperty drillProp)
            {
                if (TernaryValue != null && drillProp.TernaryValue != null)
                {
                    return TernaryValue.Equals(drillProp.TernaryValue);
                }
                else
                {
                    return TernaryValue == null && drillProp.TernaryValue == null;
                }
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (TernaryValue != null && dest is not null and TernaryDrillingProperty drillProp && drillProp.TernaryValue != null)
            {
                TernaryValue.CopyTo(drillProp.TernaryValue);
            }
        }

    }
}
