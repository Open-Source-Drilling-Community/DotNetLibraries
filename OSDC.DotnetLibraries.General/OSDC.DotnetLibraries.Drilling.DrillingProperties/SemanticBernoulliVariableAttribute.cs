
namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SemanticBernoulliVariableAttribute : Attribute
    {
        public string? ProbabilistVariable { get; } = null;

        public string? DeterministVariable { get; } = null;

        public double? DeterministDefaultUncertainty { get; } = null;

        public SemanticBernoulliVariableAttribute(string probabilistVariable)
        {
            ProbabilistVariable = probabilistVariable;
            DeterministVariable = null;
            DeterministDefaultUncertainty = null;
        }

        public SemanticBernoulliVariableAttribute(string probabilistVariable, string deterministVariable)
        {
            ProbabilistVariable = probabilistVariable;
            DeterministVariable = deterministVariable;
            DeterministDefaultUncertainty = null;
        }

        public SemanticBernoulliVariableAttribute(string deterministVariable, double defaultUncertainty)
        {
            ProbabilistVariable = null;
            DeterministVariable = deterministVariable;
            DeterministDefaultUncertainty = defaultUncertainty;
        }
        public SemanticBernoulliVariableAttribute(string probabilistVariable, string deterministVariable, double defaultUncertainty)
        {
            ProbabilistVariable = probabilistVariable;
            DeterministVariable = deterministVariable;
            DeterministDefaultUncertainty = defaultUncertainty;
        }
    }
}
