
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    /// <summary>
    ///  define the names of the variables in a semantic query that provide the probabilities of the different states of the CategoricalDrillingProperty
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SemanticCategoricalVariableAttribute : SemanticOneVariableAttribute
    {
        public uint? NumberOfStates { get; } = null;

        public SemanticCategoricalVariableAttribute(string? variable, uint numberOfStates) :base(variable)
        {
            NumberOfStates = numberOfStates;
        }
    }
}
