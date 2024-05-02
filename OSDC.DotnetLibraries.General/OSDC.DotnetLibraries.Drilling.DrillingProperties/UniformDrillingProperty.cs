using DWIS.Client.ReferenceImplementation;
using OSDC.DotnetLibraries.General.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class UniformDrillingProperty : ContinuousDrillingProperty
    {
        /// <summary>
        /// synonym property to UniformValue
        /// </summary>
        [JsonIgnore]
        public override ContinuousDistribution? Value
        {
            get
            {
                return UniformValue;
            }
            set
            {
                if (value != null && value is UniformDistribution uniformDistribution)
                {
                    UniformValue = uniformDistribution;
                }
            }
        }
        /// <summary>
        /// synonym property to the Value but with the correct type
        /// </summary>
        public UniformDistribution UniformValue { get; set; } = new UniformDistribution();
        /// <summary>
        /// convenience property to access directly the min value of UniformValue
        /// </summary>
        [JsonIgnore]
        public double? Min
        {
            get
            {
                if (UniformValue != null)
                {
                    return UniformValue.Min;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (UniformValue != null)
                {
                    UniformValue.Min = value;
                }
            }
        }
        /// <summary>
        /// Convenience property to directly access the max value of UniformValue
        /// </summary>
        [JsonIgnore]
        public double? Max
        {
            get
            {
                if (UniformValue != null)
                {
                    return UniformValue.Max;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (UniformValue != null)
                {
                    UniformValue.Max = value;
                }
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UniformDrillingProperty() : base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public UniformDrillingProperty(UniformDrillingProperty src) : base(src)
        {
        }

        public override bool FuseData(List<AcquiredSignals>? signals)
        {
            if (signals != null && signals.Count > 0)
            {
                double amin = double.MaxValue;
                double amax = double.MinValue;
                foreach (var signalList in signals)
                {
                    if (signalList != null)
                    {
                        foreach (var signal in signalList)
                        {
                            if (signal.Value != null && signal.Value.Count >= 2)
                            {
                                double min = signal.Value[0].GetValue<double>();
                                if (min < amin)
                                {
                                    amin = min;
                                }
                                double max = signal.Value[1].GetValue<double>();
                                if (max > amax)
                                {
                                    amax = max;
                                }
                            }
                        }
                    }
                }
                if (amin <= amax)
                {
                    Min = amin;
                    Max = amax;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(DrillingProperty? cmp)
        {
            if (cmp is not null and UniformDrillingProperty drillProp)
            {
                if (UniformValue != null && drillProp.UniformValue != null)
                {
                    return UniformValue.Equals(drillProp.UniformValue);
                }
                else
                {
                    return UniformValue == null && drillProp.UniformValue == null;
                }
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (UniformValue != null && dest is not null and UniformDrillingProperty drillProp && drillProp.UniformValue != null)
            {
                UniformValue.CopyTo(drillProp.UniformValue);
            }
        }
    }
}
