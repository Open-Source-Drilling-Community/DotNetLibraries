
namespace OSDC.DotnetLibraries.General.Common
{
    public class BooleanValue : IBooleanValue, IDefinable, IValidable
    {
        public bool? Value { get; set; }

        public bool Valid { get; set; }


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
