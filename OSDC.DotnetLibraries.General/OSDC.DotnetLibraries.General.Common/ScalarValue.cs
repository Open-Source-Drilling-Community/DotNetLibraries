
namespace OSDC.DotnetLibraries.General.Common
{
    public class ScalarValue : ISimilar, IDefinable, IValidable
    {
        public double? Value { get; set; }

        public bool Valid { get; set; }
  

        public bool EQ(ISimilar v)
        {
            if (v != null && v is ScalarValue)
            {
                return Value == ((ScalarValue)v).Value;
            }
            else
            {
                return false;
            }
        }

        public bool GE(ISimilar v)
        {
            if (v != null && v is ScalarValue value)
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
            if (v != null && v is ScalarValue value)
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
            if (v != null && v is ScalarValue value)
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
            if (v != null && v is ScalarValue value)
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
            if (v != null && v is ScalarValue value)
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
