
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    /// <summary>
    ///  define the names of the variables in a semantic query that provide the probabilities of the different states of the CategoricalDrillingProperty
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticCategoricalVariableAttribute : Attribute
    {
        public string[]? Variables { get; } = null;

        public SemanticCategoricalVariableAttribute(params string[]? variables) 
        {
            Variables = variables;
        }
    }
}
