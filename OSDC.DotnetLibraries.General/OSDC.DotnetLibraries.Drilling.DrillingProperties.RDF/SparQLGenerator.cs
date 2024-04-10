using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DWIS.Vocabulary.Schemas;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties.RDF
{
    public class SparQLGenerator
    {
        /// <summary>
        /// This static method returns a list of SparQL queries generated from the drilling property
        /// attributes associated with a DrillingProperty
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static List<string>? GetSparQL(Assembly assembly, string typeName, string propertyName)
        {
            List<string>? sparqls = null;
            if (assembly == null)
            {
                return null;
            }
            Type[] types = assembly.GetTypes();
            if (types != null)
            {
                foreach (Type type in types)
                {
                    if (type.FullName == typeName && type.IsClass)
                    {
                        PropertyInfo[] properties = type.GetProperties();

                        // Print property information
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.PropertyType.IsSubclassOf(typeof(DrillingProperty)) || property.PropertyType.IsAssignableFrom(typeof(DrillingProperty)))
                            {
                                var accessToVariableAttribute = property.GetCustomAttribute<AccessToVariableAttribute>();
                                var mandatoryAttritbute = property.GetCustomAttribute<MandatoryAttribute>();
                                var semanticFactAttributes = property.GetCustomAttributes<SemanticFactAttribute>();
                                var optionalFactAttributes = property.GetCustomAttributes<OptionalFactAttribute>();
                                var semanticDiracVariableAttribute = property.GetCustomAttribute<SemanticDiracVariableAttribute>();
                                var semanticGaussianVariableAttribute = property.GetCustomAttribute<SemanticGaussianVariableAttribute>();
                                var semanticSensorVariableAttribute = property.GetCustomAttribute<SemanticSensorVariableAttribute>();
                                var semanticFullScaleVariableAttribute = property.GetCustomAttribute<SemanticFullScaleVariableAttribute>();
                                var semanticUniformVariableAttribute = property.GetCustomAttribute<SemanticUniformVariableAttribute>();
                                var semanticGeneralDistributionVariableAttribute = property.GetCustomAttribute<SemanticGeneralDistributionVariableAttribute>();
                                var semanticExclusiveOrAttributes = property.GetCustomAttributes<SemanticExclusiveOrAttribute>();
                                if (accessToVariableAttribute != null ||
                                    semanticDiracVariableAttribute != null ||
                                    semanticGaussianVariableAttribute != null ||
                                    semanticSensorVariableAttribute != null ||
                                    semanticFullScaleVariableAttribute != null ||
                                    semanticUniformVariableAttribute != null ||
                                    semanticGeneralDistributionVariableAttribute != null ||
                                    (semanticExclusiveOrAttributes != null  && semanticExclusiveOrAttributes.Any())||
                                    mandatoryAttritbute != null ||
                                    (semanticFactAttributes != null && semanticFactAttributes.Any()) ||
                                    (optionalFactAttributes != null && optionalFactAttributes.Any()))
                                {
                                    if (semanticDiracVariableAttribute != null || 
                                        semanticGaussianVariableAttribute != null ||
                                        semanticSensorVariableAttribute != null ||
                                        semanticFullScaleVariableAttribute != null ||
                                        semanticUniformVariableAttribute != null ||
                                        semanticGeneralDistributionVariableAttribute != null)
                                    {
                                        List<List<SemanticFact>> combinations = GetCombinations(semanticFactAttributes, optionalFactAttributes, semanticExclusiveOrAttributes);
                                        foreach (var combination in combinations)
                                        {
                                            string sparql = string.Empty;
                                            sparql += "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\n";
                                            sparql += "PREFIX ddhub: <http://ddhub.no/>\n";
                                            sparql += "PREFIX quantity: <http://ddhub.no/UnitAndQuantity>\n\n";
                                            if (semanticDiracVariableAttribute != null && !string.IsNullOrEmpty(semanticDiracVariableAttribute.Value))
                                            {
                                                List<string> queryVariables = new List<string>();
                                                queryVariables.Add(semanticDiracVariableAttribute.Value.Trim());
                                                string var1 = ProcessQueryVariable(semanticDiracVariableAttribute.Value, queryVariables);
                                                sparql += "SELECT " + var1 + "\n";
                                                List<string> alreadyTyped = new List<string>();
                                                foreach (var fact in combination)
                                                {
                                                    if (!string.IsNullOrEmpty(fact.SubjectName))
                                                    {
                                                        fact.SubjectName = ProcessQueryVariable(fact.SubjectName, queryVariables);
                                                    }
                                                    if (!string.IsNullOrEmpty(fact.ObjectName))
                                                    {
                                                        fact.ObjectName = ProcessQueryVariable(fact.ObjectName, queryVariables);
                                                    }
                                                    sparql += GenerateWhereStatement(fact, alreadyTyped);
                                                }
                                                sparql += "WHERE {\n";
                                                sparql += "}\n";

                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return sparqls;
        }

        public static List<string>? GetMarkDownSparQLFormatting(List<string>? sparqls)
        {
            List<string>? results = null;
            if (sparqls != null)
            {
                results = new List<string>();
                foreach (var  sparql in sparqls)
                {
                    string result = string.Empty;
                    result += "```sparql\n";
                    result += sparql + "\n";
                    result += "```\n\n";
                    results.Add(result);
                }
            }
            return results;
        }

        private static string ProcessQueryVariable(string variable, List<string> queryVariables)
        {
            variable = variable.Trim();
            if (queryVariables != null && queryVariables.Contains(variable))
            {
                if (!variable.StartsWith('?'))
                {
                    variable = '?' + variable;
                }
            }
            return variable;
        }

        private static string GenerateWhereStatement(SemanticFact fact, List<string> alreadyTyped)
        {
            string result = string.Empty;
            if (fact != null && 
                (fact.Subject != null || !string.IsNullOrEmpty(fact.SubjectName)) &&
                (fact.Object != null || !string.IsNullOrEmpty(fact.ObjectName)))
            {
                string subject = string.Empty;
                if (!string.IsNullOrEmpty(fact.SubjectName))
                {
                    subject = fact.SubjectName;
                } else
                {
                    subject = "ddhub:" + fact.Subject.ToString();
                }
                string obj = string.Empty;
                if (!string.IsNullOrEmpty(fact.ObjectName))
                {
                    obj = fact.ObjectName;
                }
                else
                {
                    obj = "ddhub:" + fact.Object.ToString();
                }
                string verb = string.Empty;
                if (fact.Verb == Verbs.Enum.BelongsToClass)
                {
                    if (alreadyTyped != null && !alreadyTyped.Contains(subject))
                    {
                        verb = "rdf:type";
                        alreadyTyped.Add(subject);
                    }
                    else
                    {
                        verb = "ddhub:" + fact.Verb.ToString();
                    }
                }
                else
                {
                    verb = "ddhub:" + fact.Verb.ToString();
                }

                result += "\t" + subject + " " + verb + " " + obj + " .\n";
            }
            return result;
        }

        private static List<List<SemanticFact>>? GetCombinations(IEnumerable<SemanticFactAttribute>? facts,
                                                                 IEnumerable<OptionalFactAttribute>? optionalFacts,
                                                                 IEnumerable<SemanticExclusiveOrAttribute>? exclusiveOrs)
        {
            List<List<SemanticFact>>? results = null;
            if (facts != null)
            {
                Dictionary<byte, List<OptionalFactAttribute>> bundlesOfOptionalFacts = new Dictionary<byte, List<OptionalFactAttribute>>();
                if (optionalFacts != null)
                {
                    foreach (var fact in optionalFacts)
                    {
                        if (fact != null)
                        {
                            if (!bundlesOfOptionalFacts.ContainsKey(fact.GroupIndex))
                            {
                                bundlesOfOptionalFacts.Add(fact.GroupIndex, new List<OptionalFactAttribute>());
                            }
                            bundlesOfOptionalFacts[fact.GroupIndex].Add(fact);
                        }
                    }
                }
                List<byte> bundles = bundlesOfOptionalFacts.Keys.ToList();
                List<List<byte>>? combinations = GetCombinations(exclusiveOrs, bundles);
            }
            return results;
        }
        private static List<List<byte>>? GetCombinations(IEnumerable<SemanticExclusiveOrAttribute>? exclusiveOrs, List<byte> bundles)
        {
            List<List<byte>>? results = null;
            if (exclusiveOrs != null)
            {

            }
            return results;
        }
    }
}
