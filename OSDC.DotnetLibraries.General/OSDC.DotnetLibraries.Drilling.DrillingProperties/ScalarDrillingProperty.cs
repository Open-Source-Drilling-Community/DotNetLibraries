using OSDC.UnitConversion.Conversion.DrillingEngineering;
using OSDC.UnitConversion.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Statistics;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Reflection;
using DWIS.Client.ReferenceImplementation;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class ScalarDrillingProperty : ContinuousDrillingProperty
    {

        /// <summary>
        /// the value of the property
        /// </summary>
        [JsonIgnore]
        public override ContinuousDistribution? Value
        {
            get
            {
                return DiracDistributionValue;
            }
            set
            {
                if (value != null && value is DiracDistribution distribution)
                {
                    DiracDistributionValue = distribution;
                }
            }
        }
        /// <summary>
        /// synonym of the Value property but with the correct type
        /// </summary>
        public DiracDistribution DiracDistributionValue { get; set; } = new DiracDistribution();
        /// <summary>
        /// convenience property to access the value of the DiracDistributionValue.
        /// </summary>
        [JsonIgnore]
        public double? ScalarValue
        {
            get
            {
                if (DiracDistributionValue != null)
                {
                    return DiracDistributionValue.Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (DiracDistributionValue != null)
                {
                    DiracDistributionValue.Value = value;
                }
            }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ScalarDrillingProperty() :base() { }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="src"></param>
        public ScalarDrillingProperty(ScalarDrillingProperty src) :base(src)
        {

        }

        public override bool FuseData(List<AcquiredSignals>? signals)
        {
            if (signals != null && signals.Count > 0)
            {
                double sum = 0;
                int count = 0;
                foreach (var signalList in signals)
                {
                    if (signalList != null)
                    {
                        foreach (var signal in signalList)
                        {
                            if (signal.Value != null)
                            {
                                foreach (var sig in signal.Value)
                                {
                                    if (sig != null)
                                    {
                                        sum += sig.GetValue<double>();
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                }
                if (count > 0)
                {
                    ScalarValue = sum / count;
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
            if (cmp is not null and ScalarDrillingProperty drillProp)
            {
                if (DiracDistributionValue != null && drillProp.DiracDistributionValue != null)
                {
                    return DiracDistributionValue.Equals(drillProp.DiracDistributionValue);
                }
                else
                {
                    return DiracDistributionValue == null && drillProp.DiracDistributionValue == null;
                }
            }
            return false;
        }

        public override void CopyTo(DrillingProperty? dest)
        {
            if (DiracDistributionValue != null && dest is not null and ScalarDrillingProperty drillProp && drillProp.DiracDistributionValue != null)
            {
                DiracDistributionValue.CopyTo(drillProp.DiracDistributionValue);
            }
        }
    }
}
