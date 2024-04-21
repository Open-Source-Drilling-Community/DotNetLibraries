
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticUniformVariableAttribute : Attribute
    {
        public string? MinValueVariable { get; } = null;
        public string? MaxValueVariable { get; } = null;

        public SemanticUniformVariableAttribute(string? minValue, string? maxValue)
        {
            MinValueVariable = minValue;
            MaxValueVariable = maxValue;
        }
    }
}
