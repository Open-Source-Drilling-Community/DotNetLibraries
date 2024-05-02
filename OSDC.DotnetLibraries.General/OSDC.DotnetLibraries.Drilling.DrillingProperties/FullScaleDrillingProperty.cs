using DWIS.Client.ReferenceImplementation;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class FullScaleDrillingProperty : GaussianDrillingProperty
    {
        private double? fullScale_ = null;
        private double? proportionError_ = null;

        /// <summary>
        /// the full scale for this measurement.
        /// </summary>
        public double? FullScale
        {
            get
            {
                return fullScale_;
            }
            set
            {
                fullScale_ = value;
                ProcessFullScaleProportionError();
            }
        }
        /// <summary>
        /// the proportion error for this measurement
        /// </summary>
        public double? ProportionError
        {
            get
            {
                return proportionError_;
            }
            set
            {
                proportionError_ = value;
                ProcessFullScaleProportionError();
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
        /// default constructor
        /// </summary>
        public FullScaleDrillingProperty(): base()
        {

        }
        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="src"></param>
        public FullScaleDrillingProperty(FullScaleDrillingProperty src) : base(src)
        {
            if (src != null)
            {
                fullScale_ = src.fullScale_;
                proportionError_ = src.proportionError_;
                if (GaussianValue != null && src.GaussianValue != null)
                {
                    GaussianValue.Mean = src.GaussianValue.Mean;
                }
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
                                    double? fullScale = signal.Value[1].GetValue<double>();
                                    double? proportionError = signal.Value[2].GetValue<double>();
                                    if (fullScale != null && proportionError != null)
                                    {
                                        stdDev = fullScale * proportionError;
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

        private void ProcessFullScaleProportionError()
        {
            if (fullScale_ != null && proportionError_ != null && GaussianValue != null)
            {
                GaussianValue.StandardDeviation = proportionError_.Value * fullScale_.Value;
            }
        }
    }
}
