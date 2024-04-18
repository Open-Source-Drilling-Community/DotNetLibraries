using DWIS.API.DTO;
using DWIS.Vocabulary.Schemas;
using System.Reflection;


namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public abstract class DrillingProperty : IDrillingProperty
    {
        public ManifestFile? GetManifestFile(Assembly? assembly, string? typeName, string? propertyName)
        {
            if (assembly == null || typeName == null || propertyName == null)
            {
                return null;
            }
            Type[] types = assembly.GetTypes();
            if (types != null)
            {
                ManifestFile manifestFile = new ();
                foreach (Type type in types)
                {
                    if (type.FullName == typeName && type.IsClass)
                    {
                        PropertyInfo[] properties = type.GetProperties();
                        // Print property information
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.Name == propertyName &&
                                (property.PropertyType.IsSubclassOf(typeof(DrillingProperty)) || property.PropertyType.IsAssignableFrom(typeof(DrillingProperty))))
                            {
                                var accessToVariableAttribute = property.GetCustomAttribute<AccessToVariableAttribute>();
                                var mandatoryAttribute = property.GetCustomAttribute<MandatoryAttribute>();
                                var workingSemanticFactAttributes = property.GetCustomAttributes<SemanticFactAttribute>();
                                var excludeFactAttributes = property.GetCustomAttributes<ExcludeFactAttribute>();
                                var optionalFactAttributes = property.GetCustomAttributes<OptionalFactAttribute>();
                                var excludeOptionalFactAttributes = property.GetCustomAttributes<OptionalExcludeFactAttribute>();
                                var semanticDiracVariableAttribute = property.GetCustomAttribute<SemanticDiracVariableAttribute>();
                                var semanticGaussianVariableAttribute = property.GetCustomAttribute<SemanticGaussianVariableAttribute>();
                                var semanticSensorVariableAttribute = property.GetCustomAttribute<SemanticSensorVariableAttribute>();
                                var semanticFullScaleVariableAttribute = property.GetCustomAttribute<SemanticFullScaleVariableAttribute>();
                                var semanticUniformVariableAttribute = property.GetCustomAttribute<SemanticUniformVariableAttribute>();
                                var semanticGeneralDistributionVariableAttribute = property.GetCustomAttribute<SemanticGeneralDistributionVariableAttribute>();
                                var semanticDeterministicBooleanVariableAttribute = property.GetCustomAttribute<SemanticDeterministicBooleanVariableAttribute>();
                                var semanticBernoulliVariableAttribute = property.GetCustomAttribute<SemanticBernoulliVariableAttribute>();
                                var semanticExclusiveOrAttributes = property.GetCustomAttributes<SemanticExclusiveOrAttribute>();
                                // remove the optional facts from the list of facts
                                List<SemanticFactAttribute> semanticFactAttributes = new List<SemanticFactAttribute>();
                                foreach (var attr in workingSemanticFactAttributes)
                                {
                                    if (!optionalFactAttributes.Contains(attr))
                                    {
                                        semanticFactAttributes.Add(attr);
                                    }
                                }
                                if (accessToVariableAttribute != null ||
                                    semanticDiracVariableAttribute != null ||
                                    semanticGaussianVariableAttribute != null ||
                                    semanticSensorVariableAttribute != null ||
                                    semanticFullScaleVariableAttribute != null ||
                                    semanticUniformVariableAttribute != null ||
                                    semanticGeneralDistributionVariableAttribute != null ||
                                    semanticBernoulliVariableAttribute != null ||
                                    semanticDeterministicBooleanVariableAttribute != null ||
                                    (semanticExclusiveOrAttributes != null && semanticExclusiveOrAttributes.Any()) ||
                                    mandatoryAttribute != null ||
                                    (semanticFactAttributes != null && semanticFactAttributes.Any()) ||
                                    (excludeFactAttributes != null && excludeFactAttributes.Any()) ||
                                    (optionalFactAttributes != null && optionalFactAttributes.Any()) ||
                                    (excludeOptionalFactAttributes != null && excludeOptionalFactAttributes.Any()))
                                {
                                    if (semanticDiracVariableAttribute != null ||
                                        semanticGaussianVariableAttribute != null ||
                                        semanticSensorVariableAttribute != null ||
                                        semanticFullScaleVariableAttribute != null ||
                                        semanticUniformVariableAttribute != null ||
                                        semanticGeneralDistributionVariableAttribute != null ||
                                        semanticDeterministicBooleanVariableAttribute != null ||
                                        semanticBernoulliVariableAttribute != null)
                                    {
                                        List<OptionalFactAttribute> topLevelOptionalFacts = new List<OptionalFactAttribute>();
                                        List<OptionalFactAttribute> subLevelOptionalFacts = new List<OptionalFactAttribute>();
                                        if (optionalFactAttributes != null)
                                        {
                                            foreach (var optionalFact in optionalFactAttributes)
                                            {
                                                if (optionalFact != null)
                                                {
                                                    if (optionalFact.ParentGroupIndex == 0)
                                                    {
                                                        topLevelOptionalFacts.Add(optionalFact);
                                                    }
                                                    else
                                                    {
                                                        subLevelOptionalFacts.Add(optionalFact);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    return manifestFile;
                }
            }
            return null;
        }
        public Dictionary<string, Tuple<int, string>>? GetSparQLQueries(Assembly? assembly, string? typeName, string? propertyName)
        {
            if (assembly == null || typeName == null || propertyName == null)
            {
                return null;
            }
            Type[] types = assembly.GetTypes();
            if (types != null)
            {
                int count = 0;
                Dictionary<string, Tuple<int, string>> queries = new Dictionary<string, Tuple<int, string>>();
                foreach (Type type in types)
                {
                    if (type.FullName == typeName && type.IsClass)
                    {
                        PropertyInfo[] properties = type.GetProperties();

                        // Print property information
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.Name == propertyName &&
                                (property.PropertyType.IsSubclassOf(typeof(DrillingProperty)) || property.PropertyType.IsAssignableFrom(typeof(DrillingProperty))))
                            {
                                var accessToVariableAttribute = property.GetCustomAttribute<AccessToVariableAttribute>();
                                var mandatoryAttribute = property.GetCustomAttribute<MandatoryAttribute>();
                                var workingSemanticFactAttributes = property.GetCustomAttributes<SemanticFactAttribute>();
                                var excludeFactAttributes = property.GetCustomAttributes<ExcludeFactAttribute>();
                                var optionalFactAttributes = property.GetCustomAttributes<OptionalFactAttribute>();
                                var excludeOptionalFactAttributes = property.GetCustomAttributes<OptionalExcludeFactAttribute>();
                                var semanticDiracVariableAttribute = property.GetCustomAttribute<SemanticDiracVariableAttribute>();
                                var semanticGaussianVariableAttribute = property.GetCustomAttribute<SemanticGaussianVariableAttribute>();
                                var semanticSensorVariableAttribute = property.GetCustomAttribute<SemanticSensorVariableAttribute>();
                                var semanticFullScaleVariableAttribute = property.GetCustomAttribute<SemanticFullScaleVariableAttribute>();
                                var semanticUniformVariableAttribute = property.GetCustomAttribute<SemanticUniformVariableAttribute>();
                                var semanticGeneralDistributionVariableAttribute = property.GetCustomAttribute<SemanticGeneralDistributionVariableAttribute>();
                                var semanticDeterministicBooleanVariableAttribute = property.GetCustomAttribute<SemanticDeterministicBooleanVariableAttribute>();
                                var semanticBernoulliVariableAttribute = property.GetCustomAttribute<SemanticBernoulliVariableAttribute>();
                                var semanticExclusiveOrAttributes = property.GetCustomAttributes<SemanticExclusiveOrAttribute>();
                                // remove the optional facts from the list of facts
                                List<SemanticFactAttribute> semanticFactAttributes = new List<SemanticFactAttribute>();
                                foreach (var attr in workingSemanticFactAttributes)
                                {
                                    if (!optionalFactAttributes.Contains(attr))
                                    {
                                        semanticFactAttributes.Add(attr);
                                    }
                                }
                                if (accessToVariableAttribute != null ||
                                    semanticDiracVariableAttribute != null ||
                                    semanticGaussianVariableAttribute != null ||
                                    semanticSensorVariableAttribute != null ||
                                    semanticFullScaleVariableAttribute != null ||
                                    semanticUniformVariableAttribute != null ||
                                    semanticGeneralDistributionVariableAttribute != null ||
                                    semanticDeterministicBooleanVariableAttribute != null ||
                                    semanticBernoulliVariableAttribute != null ||
                                    (semanticExclusiveOrAttributes != null && semanticExclusiveOrAttributes.Any()) ||
                                    mandatoryAttribute != null ||
                                    (semanticFactAttributes != null && semanticFactAttributes.Any()) ||
                                    (excludeFactAttributes != null && excludeFactAttributes.Any()) ||
                                    (optionalFactAttributes != null && optionalFactAttributes.Any()) ||
                                    (excludeOptionalFactAttributes != null && excludeOptionalFactAttributes.Any()))
                                {
                                    if (semanticDiracVariableAttribute != null ||
                                        semanticGaussianVariableAttribute != null ||
                                        semanticSensorVariableAttribute != null ||
                                        semanticFullScaleVariableAttribute != null ||
                                        semanticUniformVariableAttribute != null ||
                                        semanticGeneralDistributionVariableAttribute != null ||
                                        semanticDeterministicBooleanVariableAttribute != null ||
                                        semanticBernoulliVariableAttribute != null)
                                    {
                                        List<OptionalFactAttribute> topLevelOptionalFacts = new List<OptionalFactAttribute>();
                                        List<OptionalFactAttribute> subLevelOptionalFacts = new List<OptionalFactAttribute>();
                                        if (optionalFactAttributes != null)
                                        {
                                            foreach (var optionalFact in optionalFactAttributes)
                                            {
                                                if (optionalFact != null)
                                                {
                                                    if (optionalFact.ParentGroupIndex == 0)
                                                    {
                                                        topLevelOptionalFacts.Add(optionalFact);
                                                    }
                                                    else
                                                    {
                                                        subLevelOptionalFacts.Add(optionalFact);
                                                    }
                                                }
                                            }
                                        }
                                        List<List<SemanticFact>>? combinations = GetCombinations(semanticFactAttributes, topLevelOptionalFacts, subLevelOptionalFacts, semanticExclusiveOrAttributes);
                                        if (combinations != null)
                                        {
                                            List<ExcludeFact> excludeFacts = new List<ExcludeFact>();
                                            if (excludeFactAttributes != null)
                                            {
                                                foreach (var excludedFactAttribute in excludeFactAttributes)
                                                {
                                                    if (excludedFactAttribute != null)
                                                    {
                                                        ExcludeFact excludeFact = new ExcludeFact();
                                                        excludeFact.Subject = excludedFactAttribute.Subject;
                                                        excludeFact.SubjectName = excludedFactAttribute.SubjectName;
                                                        excludeFact.Verb  = excludedFactAttribute.Verb;
                                                        excludeFact.Object = excludedFactAttribute.Object;
                                                        excludeFact.ObjectName = excludedFactAttribute.ObjectName;
                                                        excludeFact.ObjectPhysicalQuantity = excludedFactAttribute.ObjectPhysicalQuantity;
                                                        excludeFact.ObjectDrillingQuantity = excludedFactAttribute.ObjectDrillingQuantity;
                                                        excludeFacts.Add(excludeFact);
                                                    }
                                                }
                                            }
                                            foreach (var combination in combinations)
                                            {
                                                string sparql = string.Empty;
                                                int argCount = 0;
                                                sparql += "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\n";
                                                sparql += "PREFIX ddhub: <http://ddhub.no/>\n";
                                                sparql += "PREFIX quantity: <http://ddhub.no/UnitAndQuantity>\n\n";
                                                if (semanticDiracVariableAttribute != null && 
                                                    !string.IsNullOrEmpty(semanticDiracVariableAttribute.ValueVariable) &&
                                                    IsUsed(combination, semanticDiracVariableAttribute.ValueVariable))
                                                {
                                                    string var1 = ProcessQueryVariable(semanticDiracVariableAttribute.ValueVariable);
                                                    sparql += "SELECT " + var1 + "\n";
                                                    argCount = 1;
                                                }
                                                else if (semanticUniformVariableAttribute != null && 
                                                         !string.IsNullOrEmpty(semanticUniformVariableAttribute.MinValueVariable) && 
                                                         !string.IsNullOrEmpty(semanticUniformVariableAttribute.MaxValueVariable) &&
                                                         IsUsed(combination, semanticUniformVariableAttribute.MinValueVariable) &&
                                                         IsUsed(combination, semanticUniformVariableAttribute.MaxValueVariable))
                                                {
                                                    string min = ProcessQueryVariable(semanticUniformVariableAttribute.MinValueVariable);
                                                    string max = ProcessQueryVariable(semanticUniformVariableAttribute.MaxValueVariable);
                                                    sparql += "SELECT " + min + ", " + max + "\n";
                                                    argCount = 2;
                                                }
                                                else if (semanticSensorVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticSensorVariableAttribute.MeanVariable) &&
                                                         IsUsed(combination, semanticSensorVariableAttribute.MeanVariable) &&
                                                         (!string.IsNullOrEmpty(semanticSensorVariableAttribute.AccuracyVariable) ||
                                                          semanticSensorVariableAttribute.DefaultAccuracy != null) &&
                                                         (!string.IsNullOrEmpty(semanticSensorVariableAttribute.PrecisionVariable) ||
                                                          semanticSensorVariableAttribute.DefaultPrecision != null) &&
                                                         (string.IsNullOrEmpty(semanticSensorVariableAttribute.AccuracyVariable) || 
                                                          IsUsed(combination, semanticSensorVariableAttribute.AccuracyVariable)) &&
                                                         (semanticSensorVariableAttribute.DefaultAccuracy == null || 
                                                          !IsUsed(combination, semanticSensorVariableAttribute.AccuracyVariable)) &&
                                                         (string.IsNullOrEmpty(semanticSensorVariableAttribute.PrecisionVariable) || 
                                                          IsUsed(combination, semanticSensorVariableAttribute.PrecisionVariable)) &&
                                                         (semanticSensorVariableAttribute.DefaultPrecision == null ||
                                                          !IsUsed(combination, semanticSensorVariableAttribute.PrecisionVariable)))
                                                {
                                                    string mean = ProcessQueryVariable(semanticSensorVariableAttribute.MeanVariable);
                                                    sparql += "SELECT " + mean;
                                                    argCount = 1;
                                                    if (!string.IsNullOrEmpty(semanticSensorVariableAttribute.PrecisionVariable) &&
                                                        IsUsed(combination, semanticSensorVariableAttribute.PrecisionVariable))
                                                    {
                                                        string precision = ProcessQueryVariable(semanticSensorVariableAttribute.PrecisionVariable);
                                                        sparql += ", " + precision;
                                                        argCount++;
                                                    }
                                                    if (!string.IsNullOrEmpty(semanticSensorVariableAttribute.AccuracyVariable) &&
                                                        IsUsed(combination, semanticSensorVariableAttribute.AccuracyVariable))
                                                    {
                                                        string accuracy = ProcessQueryVariable(semanticSensorVariableAttribute.AccuracyVariable);
                                                        sparql += ", " + accuracy;
                                                        argCount++;
                                                    }
                                                    sparql += "\n";
                                                    
                                                }
                                                else if (semanticFullScaleVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticFullScaleVariableAttribute.MeanVariable) &&
                                                         (!string.IsNullOrEmpty(semanticFullScaleVariableAttribute.FullScaleVariable) ||
                                                          semanticFullScaleVariableAttribute.DefaultFullScale != null) &&
                                                         (!string.IsNullOrEmpty(semanticFullScaleVariableAttribute.ProportionErrorVariable) ||
                                                          semanticFullScaleVariableAttribute.DefaultProportionError != null) &&
                                                          IsUsed(combination, semanticFullScaleVariableAttribute.MeanVariable) &&
                                                         (string.IsNullOrEmpty(semanticFullScaleVariableAttribute.FullScaleVariable) ||
                                                          IsUsed(combination, semanticFullScaleVariableAttribute.FullScaleVariable)) &&
                                                         (semanticFullScaleVariableAttribute.DefaultFullScale == null ||
                                                          !IsUsed(combination, semanticFullScaleVariableAttribute.FullScaleVariable)) &&
                                                         (string.IsNullOrEmpty(semanticFullScaleVariableAttribute.ProportionErrorVariable) ||
                                                          IsUsed(combination, semanticFullScaleVariableAttribute.ProportionErrorVariable)) &&
                                                         (semanticFullScaleVariableAttribute.DefaultProportionError == null ||
                                                          !IsUsed(combination, semanticFullScaleVariableAttribute.ProportionErrorVariable)))
                                                {
                                                    string mean = ProcessQueryVariable(semanticFullScaleVariableAttribute.MeanVariable);
                                                    sparql += "SELECT " + mean;
                                                    argCount = 1;
                                                    if (!string.IsNullOrEmpty(semanticFullScaleVariableAttribute.FullScaleVariable) &&
                                                        IsUsed(combination, semanticFullScaleVariableAttribute.FullScaleVariable))
                                                    {
                                                        string fullScale = ProcessQueryVariable(semanticFullScaleVariableAttribute.FullScaleVariable);
                                                        sparql += ", " + fullScale;
                                                        argCount++;
                                                    }
                                                    if (!string.IsNullOrEmpty(semanticFullScaleVariableAttribute.ProportionErrorVariable) &&
                                                        IsUsed(combination, semanticFullScaleVariableAttribute.ProportionErrorVariable))
                                                    {
                                                        string proportionError = ProcessQueryVariable(semanticFullScaleVariableAttribute.ProportionErrorVariable);
                                                        sparql += ", " + proportionError;
                                                        argCount++;
                                                    }
                                                    sparql += "\n";
                                                }
                                                else if (semanticGaussianVariableAttribute != null &&
                                                        !string.IsNullOrEmpty(semanticGaussianVariableAttribute.MeanVariable) &&
                                                        IsUsed(combination, semanticGaussianVariableAttribute.MeanVariable) &&
                                                        (!string.IsNullOrEmpty(semanticGaussianVariableAttribute.StandardDeviationVariable) ||
                                                         semanticGaussianVariableAttribute.DefaultStandardDeviation != null) &&
                                                        (semanticGaussianVariableAttribute.DefaultStandardDeviation == null ||
                                                         !IsUsed(combination, semanticGaussianVariableAttribute.StandardDeviationVariable)))
                                                {
                                                    string mean = ProcessQueryVariable(semanticGaussianVariableAttribute.MeanVariable);
                                                    sparql += "SELECT " + mean;
                                                    argCount = 1;
                                                    if (!string.IsNullOrEmpty(semanticGaussianVariableAttribute.StandardDeviationVariable) &&
                                                        IsUsed(combination, semanticGaussianVariableAttribute.StandardDeviationVariable))
                                                    {
                                                        string stdDev = ProcessQueryVariable(semanticGaussianVariableAttribute.StandardDeviationVariable);
                                                        sparql += ", " + stdDev;
                                                        argCount++;
                                                    }
                                                    sparql += "\n";
                                                }
                                                else if (semanticGeneralDistributionVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticGeneralDistributionVariableAttribute.HistogramVariable) &&
                                                         IsUsed(combination, semanticGeneralDistributionVariableAttribute.HistogramVariable))
                                                {
                                                    string histoVar = ProcessQueryVariable(semanticGeneralDistributionVariableAttribute.HistogramVariable);
                                                    sparql += "SELECT " + histoVar + "\n";
                                                    argCount = 1;
                                                }
                                                else if (semanticDeterministicBooleanVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticDeterministicBooleanVariableAttribute.Variable) &&
                                                         IsUsed(combination, semanticDeterministicBooleanVariableAttribute.Variable))
                                                {
                                                    string variable = ProcessQueryVariable(semanticDeterministicBooleanVariableAttribute.Variable);
                                                    sparql += "SELECT " + variable + "\n";
                                                    argCount = 1;
                                                }
                                                else if (semanticBernoulliVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticBernoulliVariableAttribute.ProbabilistVariable) &&
                                                         IsUsed(combination, semanticBernoulliVariableAttribute.ProbabilistVariable) && 
                                                         (!string.IsNullOrEmpty(semanticBernoulliVariableAttribute.DeterministVariable) ||
                                                          semanticBernoulliVariableAttribute.DeterministDefaultUncertainty != null) &&            
                                                         (string.IsNullOrEmpty(semanticBernoulliVariableAttribute.DeterministVariable) ||
                                                          IsUsed(combination, semanticBernoulliVariableAttribute.DeterministVariable)) &&
                                                         (semanticBernoulliVariableAttribute.DeterministDefaultUncertainty == null ||
                                                          !IsUsed(combination, semanticBernoulliVariableAttribute.DeterministVariable)))
                                                {
                                                    string probabilistic = ProcessQueryVariable(semanticBernoulliVariableAttribute.ProbabilistVariable);
                                                    sparql += "SELECT " + probabilistic;
                                                    argCount = 1;
                                                    if (!string.IsNullOrEmpty(semanticBernoulliVariableAttribute.DeterministVariable) &&
                                                        IsUsed(combination, semanticBernoulliVariableAttribute.DeterministVariable))
                                                    {
                                                        string deterministic = ProcessQueryVariable(semanticBernoulliVariableAttribute.DeterministVariable);
                                                        sparql += ", " + deterministic;
                                                        argCount++;
                                                    }
                                                    sparql += "\n";
                                                }
                                                sparql += "WHERE {\n";
                                                List<string> alreadyTyped = new List<string>();
                                                foreach (var fact in combination)
                                                {
                                                    if (!string.IsNullOrEmpty(fact.SubjectName))
                                                    {
                                                        fact.SubjectName = ProcessQueryVariable(fact.SubjectName);
                                                    }
                                                    if (!string.IsNullOrEmpty(fact.ObjectName))
                                                    {
                                                        fact.ObjectName = ProcessQueryVariable(fact.ObjectName);
                                                    }
                                                    sparql += GenerateWhereStatement(fact, alreadyTyped);
                                                }
                                                if (excludeFactAttributes != null && excludeFactAttributes.Any())
                                                {
                                                    sparql += "  FILTER NOT EXISTS {\n";
                                                    foreach (var excluded in excludeFacts)
                                                    {
                                                        if (!string.IsNullOrEmpty(excluded.SubjectName))
                                                        {
                                                            excluded.SubjectName = ProcessQueryVariable(excluded.SubjectName);
                                                        }
                                                        if (!string.IsNullOrEmpty(excluded.ObjectName))
                                                        {
                                                            excluded.ObjectName = ProcessQueryVariable(excluded.ObjectName);
                                                        }
                                                        sparql += GenerateWhereStatement(excluded, alreadyTyped);
                                                    }
                                                    sparql += "  }";
                                                }
                                                sparql += "}\n";
                                                queries.Add("Query-" + typeName + "-" + propertyName + "-" + count.ToString("000"), new Tuple<int, string>(argCount, sparql));
                                                count++;
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                return queries;
            }
            return null;
        }

        private bool IsUsed(List<SemanticFact> combination, string? variable)
        {
            bool isUsed = false;
            if (!string.IsNullOrEmpty(variable))
            {
                variable = ProcessQueryVariable(variable);
                foreach (var fact in combination)
                {
                    if (!string.IsNullOrEmpty(fact.SubjectName))
                    {
                        string subjectName = ProcessQueryVariable(fact.SubjectName);
                        if (subjectName == variable) { isUsed = true; break; }
                    }
                    if (!string.IsNullOrEmpty(fact.ObjectName))
                    {
                        string objectName = ProcessQueryVariable(fact.ObjectName);
                        if (objectName == variable) { isUsed = true; break; }
                    }
                }
            }
            return isUsed;
        }
        public List<string>? GetMarkDownSparQLFormatting(List<string>? sparqls)
        {
            List<string>? results = null;
            if (sparqls != null)
            {
                results = new List<string>();
                foreach (var sparql in sparqls)
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

        private string ProcessQueryVariable(string variable)
        {
            variable = variable.Trim();
            if (!variable.StartsWith('?'))
            {
                variable = '?' + variable;
            }
            return variable;
        }
        private string ProcessQueryVariable(string variable, List<string> queryVariables)
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

        private string GenerateWhereStatement(SemanticFact fact, List<string> alreadyTyped)
        {
            string result = string.Empty;
            if (fact != null &&
                (fact.Subject != null || !string.IsNullOrEmpty(fact.SubjectName)) &&
                (fact.Object != null || !string.IsNullOrEmpty(fact.ObjectName) || fact.ObjectPhysicalQuantity != null || fact.ObjectDrillingQuantity != null))
            {
                string subject = string.Empty;
                if (!string.IsNullOrEmpty(fact.SubjectName))
                {
                    subject = fact.SubjectName;
                }
                else
                {
                    subject = "ddhub:" + fact.Subject.ToString();
                }
                string obj = string.Empty;
                if (!string.IsNullOrEmpty(fact.ObjectName))
                {
                    obj = fact.ObjectName;
                }
                else if (fact.ObjectPhysicalQuantity != null)
                {
                    obj = "quantity:" + fact.ObjectPhysicalQuantity.ToString();
                }
                else if (fact.ObjectDrillingQuantity != null)
                {
                    obj = "quantity:" + fact.ObjectDrillingQuantity.ToString();
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

        private List<List<SemanticFact>>? GetCombinations(IEnumerable<SemanticFactAttribute>? facts,
                                                          IEnumerable<OptionalFactAttribute>? topLevelOptionalFacts,
                                                          IEnumerable<OptionalFactAttribute>? subLevelOptionalFacts,
                                                          IEnumerable<SemanticExclusiveOrAttribute>? exclusiveOrs)
        {
            List<List<SemanticFact>>? results = null;
            if (facts != null)
            {
                Dictionary<byte, List<OptionalFactAttribute>> topLevelBundlesOfOptionalFacts = new Dictionary<byte, List<OptionalFactAttribute>>();
                if (topLevelOptionalFacts != null)
                {
                    foreach (var fact in topLevelOptionalFacts)
                    {
                        if (fact != null)
                        {
                            if (!topLevelBundlesOfOptionalFacts.ContainsKey(fact.GroupIndex))
                            {
                                topLevelBundlesOfOptionalFacts.Add(fact.GroupIndex, new List<OptionalFactAttribute>());
                            }
                            topLevelBundlesOfOptionalFacts[fact.GroupIndex].Add(fact);
                        }
                    }
                }
                Dictionary<byte, List<OptionalFactAttribute>> subLevelBundlesOfOptionalFacts = new Dictionary<byte, List<OptionalFactAttribute>>();
                if (subLevelOptionalFacts != null)
                {
                    foreach (var fact in subLevelOptionalFacts)
                    {
                        if (fact != null)
                        {
                            if (!subLevelBundlesOfOptionalFacts.ContainsKey(fact.GroupIndex))
                            {
                                subLevelBundlesOfOptionalFacts.Add(fact.GroupIndex, new List<OptionalFactAttribute>());
                            }
                            subLevelBundlesOfOptionalFacts[fact.GroupIndex].Add(fact);
                        }
                    }
                }
                List<byte> bundles = topLevelBundlesOfOptionalFacts.Keys.ToList();
                List<List<byte>>? combinations = GetCombinations(exclusiveOrs, bundles);
                if (combinations != null && subLevelOptionalFacts != null && subLevelBundlesOfOptionalFacts.Count > 0)
                {
                    var originalCombinations = combinations;
                    combinations = new List<List<byte>>();
                    foreach (var combination in originalCombinations)
                    {
                        List<byte> subBundles = new List<byte>();

                        foreach (var idx in combination)
                        {
                            foreach (var sub in subLevelOptionalFacts)
                            {
                                if (sub != null && sub.ParentGroupIndex == idx && !subBundles.Contains(sub.ParentGroupIndex))
                                {
                                    subBundles.Add(sub.GroupIndex);
                                }
                            }
                        }
                        if (subBundles.Count > 0)
                        {
                            List<List<byte>>? subCombinations = GetCombinations(exclusiveOrs, subBundles);
                            if (subCombinations != null && subCombinations.Count > 0)
                            {
                                foreach (var sub in subCombinations)
                                {
                                    List<byte> comb = new List<byte>(combination);
                                    foreach (var idx in sub)
                                    {
                                        comb.Add(idx);
                                    }
                                    combinations.Add(comb);
                                }
                            }
                            else
                            {
                                combinations.Add(combination);
                            }
                        }
                        else
                        {
                            combinations.Add(combination);
                        }
                    }
                }
                results = new List<List<SemanticFact>>();
                List<SemanticFact> rootFacts = new List<SemanticFact>();
                foreach (var f in facts)
                {
                    if (f != null)
                    {
                        rootFacts.Add(f.GetSemanticFact());
                    }
                }
                if (combinations == null || combinations.Count == 0 || topLevelOptionalFacts == null)
                {
                    results.Add(rootFacts);
                }
                else
                {
                    foreach (var combination in combinations)
                    {
                        List<SemanticFact> result = new List<SemanticFact>();
                        foreach (var fact in rootFacts)
                        {
                            result.Add(fact);
                        }
                        foreach (var idx in combination)
                        {
                            foreach (var optionalFact in topLevelOptionalFacts)
                            {
                                if (optionalFact.GroupIndex == idx)
                                {
                                    result.Add(optionalFact.GetSemanticFact());
                                }
                            }
                            if (subLevelOptionalFacts != null)
                            {
                                foreach (var optionalFact in subLevelOptionalFacts)
                                {
                                    if (optionalFact.GroupIndex == idx)
                                    {
                                        result.Add(optionalFact.GetSemanticFact());
                                    }
                                }
                            }
                        }
                        results.Add(result);
                    }
                }
            }
            return results;
        }
        private List<List<byte>>? GetCombinations(IEnumerable<SemanticExclusiveOrAttribute>? exclusiveOrs, List<byte> bundles)
        {
            List<List<byte>>? combinations = GetCombinations(bundles);
            if (combinations != null && exclusiveOrs != null)
            {
                List<List<byte>>? results = new List<List<byte>>();
                foreach (var combination in combinations)
                {
                    bool any = false;
                    foreach (var exclusiveOr in exclusiveOrs)
                    {
                        if (exclusiveOr != null && exclusiveOr.ExclusiveOr != null)
                        {
                            List<byte> founds = new List<byte>();
                            foreach (var bundle in combination)
                            {
                                if (exclusiveOr.ExclusiveOr.Contains(bundle) && !founds.Contains(bundle))
                                {
                                    founds.Add(bundle);
                                }
                            }
                            if (founds.Count > 1)
                            {
                                any = true;
                                break;
                            }
                        }
                    }
                    if (!any)
                    {
                        results.Add(combination);
                    }
                }
                combinations = results;
            }
            return combinations;
        }
        private List<List<byte>>? GetCombinations(List<byte> bundles)
        {
            List<List<byte>> result = new List<List<byte>>();
            List<byte> currentCombination = new List<byte>();

            // Start the recursive call
            GenerateCombinations(bundles, 0, currentCombination, result);

            return result;
        }
        private void GenerateCombinations(List<byte> bundles, int index, List<byte> currentCombination, List<List<byte>> result)
        {
            // Add current combination to the result
            result.Add(new List<byte>(currentCombination));

            // Generate combinations starting from the current index
            for (int i = index; i < bundles.Count; i++)
            {
                // Include the current byte in the combination
                currentCombination.Add(bundles[i]);

                // Recur for the next index
                GenerateCombinations(bundles, i + 1, currentCombination, result);

                // Backtrack by removing the last element
                currentCombination.RemoveAt(currentCombination.Count - 1);
            }
        }
        public ManifestFile? ManageManisfestFile(string name, Assembly assembly, string typeName, string propertyName)
        {
            return null;
        }
    }
}