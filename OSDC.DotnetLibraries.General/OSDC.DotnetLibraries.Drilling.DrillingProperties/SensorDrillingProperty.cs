using DWIS.Client.ReferenceImplementation;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class SensorDrillingProperty : GaussianDrillingProperty
    {
        private double? accuracy_ = null;
        private double? precision_ = null;
        /// <summary>
        /// The standard deviation of the systematic bias on the measurement
        /// </summary>
        public double? Accuracy
        {
            get
            {
                return accuracy_;
            }
            set
            {
                accuracy_ = value;
                ProcessAccuracyPrecision();
            }
        }
        /// <summary>
        /// The standard deviation of the repetitive error on the measurement
        /// </summary>
        public double? Precision {
            get
            {
                return precision_;
            }
            set
            {
                precision_ = value;
                ProcessAccuracyPrecision();
            }
        }
        /// <summary>
        /// the current mean value
        /// </summary>
        public new double? Mean
        {
            get
            {
                if (GaussianValue != null)
                {
                    return GaussianValue.Mean;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (GaussianValue != null)
                {
                    GaussianValue.Mean = value;
                }
            }
        }
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SensorDrillingProperty() : base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public SensorDrillingProperty(SensorDrillingProperty src) : base(src)
        {
            if (src != null)
            {
                accuracy_ = src.Accuracy;
                precision_ = src.Precision;
                if (GaussianValue != null && src.GaussianValue != null)
                {
                    GaussianValue.Mean = src.GaussianValue.Mean;
                }
            }
        }

        public override bool FuseData(List<AcquiredSignals>? signals)
        {
            bool ok = false;
            if (signals != null && signals.Count > 0)
            {
                double meanSum = 0;
                double invVarSum = 0;
                foreach (var signalList in signals)
                {
                    if (signalList != null)
                    {
                        foreach (var signal in signalList)
                        {
                            if (signal.Value != null && signal.Value.Count >= 3)
                            {
                                double? mean = signal.Value[0].GetValue<double>();
                                if (mean != null)
                                {
                                    double? stdDev = null;
                                        double? precision = signal.Value[1].GetValue<double>();
                                        double? accuracy = signal.Value[2].GetValue<double>();
                                        if (precision != null && accuracy != null)
                                        {
                                            stdDev = Math.Sqrt(precision.Value * precision.Value + accuracy.Value * accuracy.Value);
                                        }
                                    if (stdDev != null)
                                    {
                                        invVarSum += 1.0 / (stdDev.Value * stdDev.Value);
                                        meanSum += mean.Value / (stdDev.Value * stdDev.Value);
                                    }
                                }
                            }
                        }
                    }
                }
                if (!Numeric.EQ(invVarSum, 0))
                {
                    Mean = meanSum / invVarSum;
                    StandardDeviation = 1.0 / Numeric.SqrtEqual(invVarSum);
                    ok = true;
                }
            }
            return ok;
        }

        private void ProcessAccuracyPrecision()
        {
            if (accuracy_ != null && precision_ != null && GaussianValue != null)
            {
                double acc = accuracy_.Value;
                double prec = precision_.Value;
                GaussianValue.StandardDeviation = System.Math.Sqrt(acc*acc + prec*prec);
            }
        }
    }
}
