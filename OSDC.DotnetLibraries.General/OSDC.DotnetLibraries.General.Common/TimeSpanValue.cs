using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Common
{
    public class TimeSpanValue : ITimeSpanValue, ISimilar, IDefinable, IValidable
    {
        public TimeSpan? Value { get; set; }

        public bool Valid { get; set; }

        public bool EQ(ISimilar v)
        {
            if (v != null && v is TimeSpanValue)
            {
                return Value == ((TimeSpanValue)v).Value;
            }
            else
            {
                return false;
            }
        }

        public bool GE(ISimilar v)
        {
            if (v != null && v is TimeSpanValue value)
            {
                return Value >= value.Value;
            }
            else
            {
                return false;
            }
        }

        public bool GT(ISimilar v)
        {
            if (v != null && v is TimeSpanValue value)
            {
                return Value > value.Value;
            }
            else
            {
                return false;
            }
        }

        public bool LE(ISimilar v)
        {
            if (v != null && v is TimeSpanValue value)
            {
                return Value <= value.Value;
            }
            else
            {
                return false;
            }
        }

        public bool LT(ISimilar v)
        {
            if (v != null && v is TimeSpanValue value)
            {
                return Value < value.Value;
            }
            else
            {
                return false;
            }
        }

        public bool NEQ(ISimilar v)
        {
            if (v != null && v is TimeSpanValue value)
            {
                return Value != value.Value;
            }
            else
            {
                return false;
            }
        }

        public bool IsDefined()
        {
            return Value != null;
        }

        public bool IsValid()
        {
            return Valid;
        }

    }
}
