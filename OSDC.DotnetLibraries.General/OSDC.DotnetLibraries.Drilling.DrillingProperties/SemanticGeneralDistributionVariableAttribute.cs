
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticGeneralDistributionVariableAttribute : SemanticOneVariableAttribute
    {
        public string? HistogramVariable
        {
            get
            {
                return ValueVariable;
            }
        }

        public SemanticGeneralDistributionVariableAttribute(string? histogram) : base(histogram)
        {
        }
    }
}
