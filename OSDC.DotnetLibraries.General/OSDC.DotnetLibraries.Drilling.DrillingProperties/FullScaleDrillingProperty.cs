
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
 
        private void ProcessFullScaleProportionError()
        {
            if (fullScale_ != null && proportionError_ != null && GaussianValue != null)
            {
                GaussianValue.StandardDeviation = proportionError_.Value * fullScale_.Value;
            }
        }
    }
}
