using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

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
        public double? Mean
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
