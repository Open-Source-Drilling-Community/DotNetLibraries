using DWIS.API.DTO;
using DWIS.Vocabulary.Schemas;
using System.Reflection;


namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public static class GeneratorSparQLManifestFile
    {
        public static ManifestFile? GetManifestFile(Assembly? assembly, string? typeName, string manifestName, string companyName, string prefix)
        {
            if (assembly == null || typeName == null)
            {
                return null;
            }
            Type[] types = assembly.GetTypes();
            if (types != null)
            {
                ManifestFile? manifestFile = null;
                foreach (Type type in types)
                {
                    if (type.FullName == typeName)
                    {
                        var workingSemanticFactAttributes = type.GetCustomAttributes<SemanticFactAttribute>();
                        var optionalFactAttributes = type.GetCustomAttributes<OptionalFactAttribute>();
                        var semanticManifestOptionsAttribute = type.GetCustomAttribute<SemanticManifestOptionsAttribute>();
                        // remove the optional facts from the list of facts
                        List<SemanticFactAttribute> semanticFactAttributes = [];
                        foreach (var attr in workingSemanticFactAttributes)
                        {
                            if (!(attr is ExcludeFactAttribute) && !(attr is OptionalFactAttribute) && !optionalFactAttributes.Contains(attr))
                            {
                                semanticFactAttributes.Add(attr);
                            }
                        }
                        if (semanticManifestOptionsAttribute != null ||
                            (semanticFactAttributes != null && semanticFactAttributes.Any()) ||
                            (optionalFactAttributes != null && optionalFactAttributes.Any()))

                        {
                            List<SemanticFact> facts = new List<SemanticFact>();
                            if (semanticFactAttributes != null)
                            {
                                foreach (var attr in semanticFactAttributes)
                                {
                                    SemanticFact fact = new()
                                    {
                                        Subject = attr.Subject,
                                        Object = attr.Object,
                                        Verb = attr.Verb,
                                        SubjectName = attr.SubjectName,
                                        ObjectName = attr.ObjectName,
                                        ObjectDrillingQuantity = attr.ObjectDrillingQuantity,
                                        ObjectPhysicalQuantity = attr.ObjectPhysicalQuantity,
                                        ObjectAttributes = attr.ObjectAttributes
                                    };
                                    facts.Add(fact);
                                }
                            }
                            if (semanticManifestOptionsAttribute != null && semanticManifestOptionsAttribute.Options != null)
                            {
                                foreach (var attr in optionalFactAttributes)
                                {
                                    if (attr != null)
                                    {
                                        foreach (byte groupIdx in semanticManifestOptionsAttribute.Options)
                                        {
                                            if (attr.GroupIndex == groupIdx)
                                            {
                                                SemanticFact fact = new()
                                                {
                                                    Subject = attr.Subject,
                                                    Object = attr.Object,
                                                    Verb = attr.Verb,
                                                    SubjectName = attr.SubjectName,
                                                    ObjectName = attr.ObjectName,
                                                    ObjectDrillingQuantity = attr.ObjectDrillingQuantity,
                                                    ObjectPhysicalQuantity = attr.ObjectPhysicalQuantity,
                                                    ObjectAttributes = attr.ObjectAttributes
                                                };
                                                facts.Add(fact);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(companyName))
                            {
                                companyName = "DWIS";
                            }
                            if (string.IsNullOrEmpty(manifestName))
                            {
                                manifestName = Guid.NewGuid().ToString();
                            }
                            if (string.IsNullOrEmpty(prefix))
                            {
                                prefix = Guid.NewGuid().ToString();
                            }
                            manifestFile = new()
                            {
                                InjectedNodes = new List<InjectedNode>(),
                                InjectedReferences = new List<InjectedReference>(),
                                InjectedVariables = new List<InjectedVariable>(),
                                InjectionInformation = new InjectionInformation()
                                {
                                    EndPointURL = "",
                                    InjectedNodesNamespaceAlias = "nodes",
                                    InjectedVariablesNamespaceAlias = "variables",
                                    ProvidedVariablesNamespaceAlias = "providedNodes",
                                    ServerName = "sourceserver"
                                },
                                ProvidedVariables = new List<ProvidedVariable>(),
                                ManifestName = manifestName,
                                Provider = new InjectionProvider()
                                {
                                    Company = companyName,
                                    Name = manifestName
                                }
                            };
                            string ddhubURL = "http://ddhub.no/";
                            string quantityNameSpace = "http://ddhub.no/UnitAndQuantity";
                            // find the provided variables
                            List<string> providedVariables = new List<string>();
                            var semanticTypeVariableAttribute = type.GetCustomAttribute<SemanticTypeVariableAttribute>();
                            if (semanticTypeVariableAttribute != null && !string.IsNullOrEmpty(semanticTypeVariableAttribute.ValueVariable))
                            {
                                ProvidedVariable providedVariable = new() { DataType = "string", VariableID = ProcessManifestVariable(semanticTypeVariableAttribute.ValueVariable, prefix) };
                                manifestFile.ProvidedVariables.Add(providedVariable);
                                providedVariables.Add(semanticTypeVariableAttribute.ValueVariable);
                            }
                            // find the injected nodes and their types
                            Dictionary<string, List<Nouns.Enum>> injectedNodes = FindInjectedNodes(facts, providedVariables);
                            if (injectedNodes != null)
                            {
                                foreach (var kpv in injectedNodes)
                                {
                                    if (!string.IsNullOrEmpty(kpv.Key) && kpv.Value != null && kpv.Value.Count > 0)
                                    {
                                        InjectedNode injectedNode = new()
                                        {
                                            BrowseName = ProcessManifestVariable(kpv.Key, prefix),
                                            DisplayName = ProcessManifestVariable(kpv.Key, prefix),
                                            UniqueName = ProcessManifestVariable(kpv.Key, prefix),
                                            TypeDictionaryURI = kpv.Value[0].ToString()
                                        };
                                        manifestFile.InjectedNodes.Add(injectedNode);
                                    }
                                }
                                foreach (var kpv in injectedNodes)
                                {
                                    if (!string.IsNullOrEmpty(kpv.Key) && kpv.Value != null && kpv.Value.Count > 1)
                                    {
                                        for (int i = 1; i < kpv.Value.Count; i++)
                                        {
                                            string subjectNameSpace = manifestFile.InjectionInformation.InjectedNodesNamespaceAlias;
                                            if (providedVariables.Contains(kpv.Key))
                                            {
                                                subjectNameSpace = manifestFile.InjectionInformation.ProvidedVariablesNamespaceAlias;
                                            }
                                            InjectedReference injectedReference = new()
                                            {
                                                Subject = new NodeIdentifier() { NameSpace = subjectNameSpace, ID = ProcessManifestVariable(kpv.Key, prefix) },
                                                VerbURI = ddhubURL + Verbs.Enum.BelongsToClass.ToString(),
                                                Object = new NodeIdentifier() { NameSpace = ddhubURL, ID = ProcessManifestVariable(kpv.Value[i].ToString(), ddhubURL) }
                                            };
                                            manifestFile.InjectedReferences.Add(injectedReference);
                                        }
                                    }
                                }
                            }
                            foreach (SemanticFact fact in facts)
                            {
                                if (fact != null && fact.Verb != Verbs.Enum.BelongsToClass && !string.IsNullOrEmpty(fact.SubjectName))
                                {
                                    string subjectNameSpace = manifestFile.InjectionInformation.InjectedNodesNamespaceAlias;
                                    if (providedVariables.Contains(fact.SubjectName))
                                    {
                                        subjectNameSpace = manifestFile.InjectionInformation.ProvidedVariablesNamespaceAlias;
                                    }
                                    if (!string.IsNullOrEmpty(fact.ObjectName))
                                    {
                                        string objectNameSpace = manifestFile.InjectionInformation.InjectedNodesNamespaceAlias;
                                        if (providedVariables.Contains(fact.ObjectName))
                                        {
                                            objectNameSpace = manifestFile.InjectionInformation.ProvidedVariablesNamespaceAlias;
                                        }
                                        InjectedReference injectedReference = new()
                                        {
                                            Subject = new NodeIdentifier() { NameSpace = subjectNameSpace, ID = ProcessManifestVariable(fact.SubjectName, prefix) },
                                            VerbURI = ddhubURL + fact.Verb.ToString(),
                                            Object = new NodeIdentifier() { NameSpace = objectNameSpace, ID = ProcessManifestVariable(fact.ObjectName, prefix) }
                                        };
                                        manifestFile.InjectedReferences.Add(injectedReference);
                                    }
                                    else if (fact.ObjectPhysicalQuantity != null)
                                    {
                                        InjectedReference injectedReference = new()
                                        {
                                            Subject = new NodeIdentifier() { NameSpace = subjectNameSpace, ID = ProcessManifestVariable(fact.SubjectName, prefix) },
                                            VerbURI = ddhubURL + fact.Verb.ToString(),
                                            Object = new NodeIdentifier() { NameSpace = quantityNameSpace, ID = ProcessManifestVariable(fact.ObjectPhysicalQuantity.Value.ToString(), prefix) }
                                        };
                                        manifestFile.InjectedReferences.Add(injectedReference);
                                    }
                                    else if (fact.ObjectDrillingQuantity != null)
                                    {
                                        InjectedReference injectedReference = new()
                                        {
                                            Subject = new NodeIdentifier() { NameSpace = subjectNameSpace, ID = ProcessManifestVariable(fact.SubjectName, prefix) },
                                            VerbURI = ddhubURL + fact.Verb.ToString(),
                                            Object = new NodeIdentifier() { NameSpace = quantityNameSpace, ID = ProcessManifestVariable(fact.ObjectDrillingQuantity.Value.ToString(), prefix) }
                                        };
                                        manifestFile.InjectedReferences.Add(injectedReference);
                                    }
                                }
                            }
                        }
                    }
                }
                return manifestFile;
            }
            return null;
        }

        public static ManifestFile? GetManifestFile(Assembly? assembly, string? typeName, string? propertyName, string manifestName, string companyName, string prefix)
        {
            if (assembly == null || typeName == null || propertyName == null)
            {
                return null;
            }
            Type[] types = assembly.GetTypes();
            if (types != null)
            {
                ManifestFile? manifestFile = null;
                foreach (Type type in types)
                {
                    if (type.FullName == typeName)
                    {
                        PropertyInfo[] properties = type.GetProperties();
                        // Print property information
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.Name == propertyName &&
                                (true || property.PropertyType.IsSubclassOf(typeof(DrillingProperty)) || property.PropertyType.IsAssignableFrom(typeof(DrillingProperty))))
                            {
                                var workingSemanticFactAttributes = property.GetCustomAttributes<SemanticFactAttribute>();
                                var optionalFactAttributes = property.GetCustomAttributes<OptionalFactAttribute>();
                                var semanticManifestOptionsAttribute = property.GetCustomAttribute<SemanticManifestOptionsAttribute>();
                                // remove the optional facts from the list of facts
                                List<SemanticFactAttribute> semanticFactAttributes = [];
                                foreach (var attr in workingSemanticFactAttributes)
                                {
                                    if (!(attr is ExcludeFactAttribute) && !(attr is OptionalFactAttribute) && !optionalFactAttributes.Contains(attr))
                                    {
                                        semanticFactAttributes.Add(attr);
                                    }
                                }
                                if (semanticManifestOptionsAttribute != null ||
                                    (semanticFactAttributes != null && semanticFactAttributes.Any()) ||
                                    (optionalFactAttributes != null && optionalFactAttributes.Any()))

                                {
                                    List<SemanticFact> facts = new List<SemanticFact>();
                                    if (semanticFactAttributes != null)
                                    {
                                        foreach (var attr in semanticFactAttributes)
                                        {
                                            SemanticFact fact = new()
                                            {
                                                Subject = attr.Subject,
                                                Object = attr.Object,
                                                Verb = attr.Verb,
                                                SubjectName = attr.SubjectName,
                                                ObjectName = attr.ObjectName,
                                                ObjectDrillingQuantity = attr.ObjectDrillingQuantity,
                                                ObjectPhysicalQuantity = attr.ObjectPhysicalQuantity,
                                                ObjectAttributes = attr.ObjectAttributes
                                            };
                                            facts.Add(fact);
                                        }
                                    }
                                    if (semanticManifestOptionsAttribute != null && semanticManifestOptionsAttribute.Options != null)
                                    {
                                        foreach (var attr in optionalFactAttributes)
                                        {
                                            if (attr != null)
                                            {
                                                foreach (byte groupIdx in semanticManifestOptionsAttribute.Options)
                                                {
                                                    if (attr.GroupIndex == groupIdx)
                                                    {
                                                        SemanticFact fact = new()
                                                        {
                                                            Subject = attr.Subject,
                                                            Object = attr.Object,
                                                            Verb = attr.Verb,
                                                            SubjectName = attr.SubjectName,
                                                            ObjectName = attr.ObjectName,
                                                            ObjectDrillingQuantity = attr.ObjectDrillingQuantity,
                                                            ObjectPhysicalQuantity = attr.ObjectPhysicalQuantity,
                                                            ObjectAttributes = attr.ObjectAttributes
                                                        };
                                                        facts.Add(fact);
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (string.IsNullOrEmpty(companyName))
                                    {
                                        companyName = "DWIS";
                                    }
                                    if (string.IsNullOrEmpty(manifestName))
                                    {
                                        manifestName = Guid.NewGuid().ToString();
                                    }
                                    if (string.IsNullOrEmpty(prefix))
                                    {
                                        prefix = Guid.NewGuid().ToString();
                                    }
                                    manifestFile = new()
                                    {
                                        InjectedNodes = new List<InjectedNode>(),
                                        InjectedReferences = new List<InjectedReference>(),
                                        InjectedVariables = new List<InjectedVariable>(),
                                        InjectionInformation = new InjectionInformation()
                                        {
                                            EndPointURL = "",
                                            InjectedNodesNamespaceAlias = "nodes",
                                            InjectedVariablesNamespaceAlias = "variables",
                                            ProvidedVariablesNamespaceAlias = "providedNodes",
                                            ServerName = "sourceserver"
                                        },
                                        ProvidedVariables = new List<ProvidedVariable>(),
                                        ManifestName = manifestName,
                                        Provider = new InjectionProvider()
                                        {
                                            Company = companyName,
                                            Name = manifestName
                                        }
                                    };
                                    string ddhubURL = "http://ddhub.no/";
                                    string quantityNameSpace = "http://ddhub.no/UnitAndQuantity";
                                    // find the provided variables
                                    List<string> providedVariables = [];
                                    var semanticDiracVariableAttribute = property.GetCustomAttribute<SemanticDiracVariableAttribute>();
                                    var semanticGaussianVariableAttribute = property.GetCustomAttribute<SemanticGaussianVariableAttribute>();
                                    var semanticSensorVariableAttribute = property.GetCustomAttribute<SemanticSensorVariableAttribute>();
                                    var semanticFullScaleVariableAttribute = property.GetCustomAttribute<SemanticFullScaleVariableAttribute>();
                                    var semanticUniformVariableAttribute = property.GetCustomAttribute<SemanticUniformVariableAttribute>();
                                    var semanticGeneralDistributionVariableAttribute = property.GetCustomAttribute<SemanticGeneralDistributionVariableAttribute>();
                                    var semanticDeterministicCategoricalVariableAttribute = property.GetCustomAttribute<SemanticDeterministicCategoricalVariableAttribute>();
                                    var semanticDeterministicBernoulliVariableAttribute = property.GetCustomAttribute<SemanticDeterministicBernoulliVariableAttribute>();
                                    var semanticBernoulliVariableAttribute = property.GetCustomAttribute<SemanticBernoulliVariableAttribute>();
                                    var semanticCategoricalVariableAttribute = property.GetCustomAttribute<SemanticCategoricalVariableAttribute>();
                                    if (semanticDiracVariableAttribute != null &&
                                        !string.IsNullOrEmpty(semanticDiracVariableAttribute.ValueVariable) &&
                                        IsUsed(facts, semanticDiracVariableAttribute.ValueVariable))
                                    {
                                        ProvidedVariable providedVariable = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticDiracVariableAttribute.ValueVariable, prefix) };
                                        manifestFile.ProvidedVariables.Add(providedVariable);
                                        providedVariables.Add(semanticDiracVariableAttribute.ValueVariable);
                                    }
                                    else if (semanticGaussianVariableAttribute != null &&
                                             !string.IsNullOrEmpty(semanticGaussianVariableAttribute.MeanVariable) &&
                                             IsUsed(facts, semanticGaussianVariableAttribute.MeanVariable) &&
                                             (string.IsNullOrEmpty(semanticGaussianVariableAttribute.StandardDeviationVariable) ||
                                             IsUsed(facts, semanticGaussianVariableAttribute.StandardDeviationVariable)))
                                    {
                                        ProvidedVariable providedVariable = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticGaussianVariableAttribute.MeanVariable, prefix) };
                                        manifestFile.ProvidedVariables.Add(providedVariable);
                                        providedVariables.Add(semanticGaussianVariableAttribute.MeanVariable);
                                        if (!string.IsNullOrEmpty(semanticGaussianVariableAttribute.StandardDeviationVariable))
                                        {
                                            ProvidedVariable providedVariable2 = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticGaussianVariableAttribute.StandardDeviationVariable, prefix) };
                                            manifestFile.ProvidedVariables.Add(providedVariable2);
                                            providedVariables.Add(semanticGaussianVariableAttribute.StandardDeviationVariable);
                                        }
                                    }
                                    else if (semanticSensorVariableAttribute != null &&
                                            !string.IsNullOrEmpty(semanticSensorVariableAttribute.MeanVariable) &&
                                            IsUsed(facts, semanticSensorVariableAttribute.MeanVariable) &&
                                            (string.IsNullOrEmpty(semanticSensorVariableAttribute.AccuracyVariable) ||
                                             IsUsed(facts, semanticSensorVariableAttribute.AccuracyVariable)) &&
                                            (string.IsNullOrEmpty(semanticSensorVariableAttribute.PrecisionVariable) ||
                                             IsUsed(facts, semanticSensorVariableAttribute.PrecisionVariable)))
                                    {
                                        ProvidedVariable providedVariable = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticSensorVariableAttribute.MeanVariable, prefix) };
                                        manifestFile.ProvidedVariables.Add(providedVariable);
                                        providedVariables.Add(semanticSensorVariableAttribute.MeanVariable);
                                        if (!string.IsNullOrEmpty(semanticSensorVariableAttribute.AccuracyVariable))
                                        {
                                            ProvidedVariable providedVariable2 = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticSensorVariableAttribute.AccuracyVariable, prefix) };
                                            manifestFile.ProvidedVariables.Add(providedVariable2);
                                            providedVariables.Add(semanticSensorVariableAttribute.AccuracyVariable);
                                        }
                                        if (!string.IsNullOrEmpty(semanticSensorVariableAttribute.PrecisionVariable))
                                        {
                                            ProvidedVariable providedVariable3 = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticSensorVariableAttribute.PrecisionVariable, prefix) };
                                            manifestFile.ProvidedVariables.Add(providedVariable3);
                                            providedVariables.Add(semanticSensorVariableAttribute.PrecisionVariable);
                                        }
                                    }
                                    else if (semanticFullScaleVariableAttribute != null &&
                                             !string.IsNullOrEmpty(semanticFullScaleVariableAttribute.MeanVariable) &&
                                             IsUsed(facts, semanticFullScaleVariableAttribute.MeanVariable) &&
                                             (string.IsNullOrEmpty(semanticFullScaleVariableAttribute.FullScaleVariable) ||
                                              IsUsed(facts, semanticFullScaleVariableAttribute.FullScaleVariable)) &&
                                             (string.IsNullOrEmpty(semanticFullScaleVariableAttribute.ProportionErrorVariable) ||
                                              IsUsed(facts, semanticFullScaleVariableAttribute.ProportionErrorVariable)))
                                    {
                                        ProvidedVariable providedVariable = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticFullScaleVariableAttribute.MeanVariable, prefix) };
                                        manifestFile.ProvidedVariables.Add(providedVariable);
                                        providedVariables.Add(semanticFullScaleVariableAttribute.MeanVariable);
                                        if (!string.IsNullOrEmpty(semanticFullScaleVariableAttribute.FullScaleVariable))
                                        {
                                            ProvidedVariable providedVariable2 = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticFullScaleVariableAttribute.FullScaleVariable, prefix) };
                                            manifestFile.ProvidedVariables.Add(providedVariable2);
                                            providedVariables.Add(semanticFullScaleVariableAttribute.FullScaleVariable);
                                        }
                                        if (!string.IsNullOrEmpty(semanticFullScaleVariableAttribute.ProportionErrorVariable))
                                        {
                                            ProvidedVariable providedVariable3 = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticFullScaleVariableAttribute.ProportionErrorVariable, prefix) };
                                            manifestFile.ProvidedVariables.Add(providedVariable3);
                                            providedVariables.Add(semanticFullScaleVariableAttribute.ProportionErrorVariable);
                                        }
                                    }
                                    else if (semanticUniformVariableAttribute != null &&
                                             !string.IsNullOrEmpty(semanticUniformVariableAttribute.MinValueVariable) &&
                                             IsUsed(facts, semanticUniformVariableAttribute.MinValueVariable) &&
                                             !string.IsNullOrEmpty(semanticUniformVariableAttribute.MaxValueVariable) &&
                                             IsUsed(facts, semanticUniformVariableAttribute.MaxValueVariable))
                                    {
                                        ProvidedVariable providedVariable1 = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticUniformVariableAttribute.MinValueVariable, prefix) };
                                        manifestFile.ProvidedVariables.Add(providedVariable1);
                                        providedVariables.Add(semanticUniformVariableAttribute.MinValueVariable);
                                        ProvidedVariable providedVariable2 = new() { DataType = "double", VariableID = ProcessManifestVariable(semanticUniformVariableAttribute.MaxValueVariable, prefix) };
                                        manifestFile.ProvidedVariables.Add(providedVariable2);
                                        providedVariables.Add(semanticUniformVariableAttribute.MaxValueVariable);
                                    }
                                    else if (semanticGeneralDistributionVariableAttribute != null &&
                                             !string.IsNullOrEmpty(semanticGeneralDistributionVariableAttribute.HistogramVariable) &&
                                             IsUsed(facts, semanticGeneralDistributionVariableAttribute.HistogramVariable))
                                    {
                                        ProvidedVariable providedVariable = new() { DataType = "double", Rank = 1, Dimensions = [20], VariableID = ProcessManifestVariable(semanticGeneralDistributionVariableAttribute.HistogramVariable, prefix) };
                                        manifestFile.ProvidedVariables.Add(providedVariable);
                                        providedVariables.Add(semanticGeneralDistributionVariableAttribute.HistogramVariable);
                                    }
                                    else if (semanticDeterministicCategoricalVariableAttribute != null &&
                                             !string.IsNullOrEmpty(semanticDeterministicCategoricalVariableAttribute.Variable) &&
                                             IsUsed(facts, semanticDeterministicCategoricalVariableAttribute.Variable) &&
                                             semanticDeterministicCategoricalVariableAttribute.NumberOfStates != null)
                                    {
                                        ProvidedVariable providedVariable = new() { 
                                            DataType = "short", 
                                            Rank = 1, 
                                            Dimensions = [(int)semanticDeterministicCategoricalVariableAttribute.NumberOfStates.Value], 
                                            VariableID = ProcessManifestVariable(semanticDeterministicCategoricalVariableAttribute.Variable, prefix) 
                                        };
                                        manifestFile.ProvidedVariables.Add(providedVariable);
                                        providedVariables.Add(semanticDeterministicCategoricalVariableAttribute.Variable);
                                    }
                                    else if (semanticDeterministicBernoulliVariableAttribute != null &&
                                             !string.IsNullOrEmpty(semanticDeterministicBernoulliVariableAttribute.Variable) &&
                                             IsUsed(facts, semanticDeterministicBernoulliVariableAttribute.Variable))
                                    {
                                        ProvidedVariable providedVariable = new() { 
                                            DataType = "double", 
                                            Rank = 1,
                                            Dimensions = [2],
                                            VariableID = ProcessManifestVariable(semanticDeterministicBernoulliVariableAttribute.Variable, prefix) 
                                        };
                                        manifestFile.ProvidedVariables.Add(providedVariable);
                                        providedVariables.Add(semanticDeterministicBernoulliVariableAttribute.Variable);
                                    }
                                    else if (semanticBernoulliVariableAttribute != null &&
                                             !string.IsNullOrEmpty(semanticBernoulliVariableAttribute.Variable) &&
                                             IsUsed(facts, semanticBernoulliVariableAttribute.Variable))
                                    {
                                        ProvidedVariable providedVariable = new() { 
                                            DataType = "double", 
                                            Rank = 1,
                                            Dimensions = [2],
                                            VariableID = ProcessManifestVariable(semanticBernoulliVariableAttribute.Variable, prefix) 
                                        };
                                        manifestFile.ProvidedVariables.Add(providedVariable);
                                        providedVariables.Add(semanticBernoulliVariableAttribute.Variable);
                                    }
                                    else if (semanticCategoricalVariableAttribute != null &&
                                             !string.IsNullOrEmpty(semanticCategoricalVariableAttribute.Variable) &&
                                             IsUsed(facts, semanticCategoricalVariableAttribute.Variable) &&
                                             semanticCategoricalVariableAttribute.NumberOfStates != null)
                                    {
                                        ProvidedVariable providedVariable = new() { 
                                            DataType = "double", 
                                            Rank = 1, 
                                            Dimensions = [(int)semanticCategoricalVariableAttribute.NumberOfStates.Value], 
                                            VariableID = ProcessManifestVariable(semanticCategoricalVariableAttribute.Variable, prefix) 
                                        };
                                        manifestFile.ProvidedVariables.Add(providedVariable);
                                        providedVariables.Add(semanticCategoricalVariableAttribute.Variable);
                                    }
                                    // find the injected nodes and their types
                                    Dictionary<string, List<Nouns.Enum>> injectedNodes = FindInjectedNodes(facts, providedVariables);
                                    if (injectedNodes != null)
                                    {
                                        foreach (var kpv in injectedNodes)
                                        {
                                            if (!string.IsNullOrEmpty(kpv.Key) && kpv.Value != null && kpv.Value.Count > 0)
                                            {
                                                InjectedNode injectedNode = new()
                                                {
                                                    BrowseName = ProcessManifestVariable(kpv.Key, prefix),
                                                    DisplayName = ProcessManifestVariable(kpv.Key, prefix),
                                                    UniqueName = ProcessManifestVariable(kpv.Key, prefix),
                                                    TypeDictionaryURI = kpv.Value[0].ToString()
                                                };
                                                manifestFile.InjectedNodes.Add(injectedNode);
                                            }
                                        }
                                        foreach (var kpv in injectedNodes)
                                        {
                                            if (!string.IsNullOrEmpty(kpv.Key) && kpv.Value != null && kpv.Value.Count > 1)
                                            {
                                                for (int i = 1; i < kpv.Value.Count; i++)
                                                {
                                                    string subjectNameSpace = manifestFile.InjectionInformation.InjectedNodesNamespaceAlias;
                                                    if (providedVariables.Contains(kpv.Key))
                                                    {
                                                        subjectNameSpace = manifestFile.InjectionInformation.ProvidedVariablesNamespaceAlias;
                                                    }
                                                    InjectedReference injectedReference = new()
                                                    {
                                                        Subject = new NodeIdentifier() { NameSpace = subjectNameSpace, ID = ProcessManifestVariable(kpv.Key, prefix) },
                                                        VerbURI = ddhubURL + Verbs.Enum.BelongsToClass.ToString(),
                                                        Object = new NodeIdentifier() { NameSpace = ddhubURL, ID = ProcessManifestVariable(kpv.Value[i].ToString(), ddhubURL) }
                                                    };
                                                    manifestFile.InjectedReferences.Add(injectedReference);
                                                }
                                            }
                                        }
                                    }
                                    foreach (SemanticFact fact in facts)
                                    {
                                        if (fact != null && fact.Verb != Verbs.Enum.BelongsToClass && !string.IsNullOrEmpty(fact.SubjectName))
                                        {
                                            string subjectNameSpace = manifestFile.InjectionInformation.InjectedNodesNamespaceAlias;
                                            if (providedVariables.Contains(fact.SubjectName))
                                            {
                                                subjectNameSpace = manifestFile.InjectionInformation.ProvidedVariablesNamespaceAlias;
                                            }
                                            if (!string.IsNullOrEmpty(fact.ObjectName))
                                            {
                                                string objectNameSpace = manifestFile.InjectionInformation.InjectedNodesNamespaceAlias;
                                                if (providedVariables.Contains(fact.ObjectName))
                                                {
                                                    objectNameSpace = manifestFile.InjectionInformation.ProvidedVariablesNamespaceAlias;
                                                }
                                                InjectedReference injectedReference = new()
                                                {
                                                    Subject = new NodeIdentifier() { NameSpace = subjectNameSpace, ID = ProcessManifestVariable(fact.SubjectName, prefix) },
                                                    VerbURI = ddhubURL + fact.Verb.ToString(),
                                                    Object = new NodeIdentifier() { NameSpace = objectNameSpace, ID = ProcessManifestVariable(fact.ObjectName, prefix) }
                                                };
                                                manifestFile.InjectedReferences.Add(injectedReference);
                                            }
                                            else if (fact.ObjectPhysicalQuantity != null)
                                            {
                                                InjectedReference injectedReference = new()
                                                {
                                                    Subject = new NodeIdentifier() { NameSpace = subjectNameSpace, ID = ProcessManifestVariable(fact.SubjectName, prefix) },
                                                    VerbURI = ddhubURL + fact.Verb.ToString(),
                                                    Object = new NodeIdentifier() { NameSpace = quantityNameSpace, ID = ProcessManifestVariable(fact.ObjectPhysicalQuantity.Value.ToString(), prefix) }
                                                };
                                                manifestFile.InjectedReferences.Add(injectedReference);
                                            }
                                            else if (fact.ObjectDrillingQuantity != null)
                                            {
                                                InjectedReference injectedReference = new()
                                                {
                                                    Subject = new NodeIdentifier() { NameSpace = subjectNameSpace, ID = ProcessManifestVariable(fact.SubjectName, prefix) },
                                                    VerbURI = ddhubURL + fact.Verb.ToString(),
                                                    Object = new NodeIdentifier() { NameSpace = quantityNameSpace, ID = ProcessManifestVariable(fact.ObjectDrillingQuantity.Value.ToString(), prefix) }
                                                };
                                                manifestFile.InjectedReferences.Add(injectedReference);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                return manifestFile;
            }
            return null;
        }

        public static string? GetMermaid(ManifestFile manifestFile)
        {
            if (manifestFile == null || manifestFile.InjectedNodes == null || manifestFile.ProvidedVariables == null || manifestFile.InjectedReferences == null)
            {
                return null;
            }
            else
            {
                string mermaid = string.Empty;
                mermaid += "```mermaid\n";
                mermaid += "flowchart TD\n";
                mermaid += "\t classDef typeClass fill:#f96;\n";
                mermaid += "\t classDef classClass fill:#9dd0ff;\n";
                mermaid += "\t classDef opcClass fill:#ff9dd0;\n";
                mermaid += "\t classDef quantityClass fill:#d0ff9d;\n";
                foreach (var v in manifestFile.ProvidedVariables)
                {
                    if (v != null && !string.IsNullOrEmpty(v.VariableID) && !string.IsNullOrEmpty(v.DataType))
                    {
                        string opcClass = "opc:";
                        if (v.Dimensions != null)
                        {
                            opcClass += "array_of_";
                            bool first = true;
                            foreach (var k in v.Dimensions)
                            {
                                if (!first)
                                {
                                    opcClass += "_";
                                    first = false;
                                }
                                opcClass += k.ToString();
                            }
                            opcClass += "_" + v.DataType;
                        }
                        else
                        {
                            opcClass += v.DataType;
                        }
                        mermaid += "\t" + ProcessMermaid(v.VariableID) + "([" + ProcessMermaid(v.VariableID) + "]) --> " + ProcessMermaid(opcClass) + "([" + ProcessMermaid(opcClass) + "]):::opcClass\n";
                    }
                }
                foreach (var v in manifestFile.InjectedNodes)
                {
                    if (v != null && !string.IsNullOrEmpty(v.DisplayName) && !string.IsNullOrEmpty(v.TypeDictionaryURI))
                    {
                        mermaid += "\t" + ProcessMermaid(v.DisplayName) + "([" + ProcessMermaid(v.DisplayName) + "]) --> " + ProcessMermaid(v.TypeDictionaryURI) + "([" + ProcessMermaid(v.TypeDictionaryURI) + "]):::typeClass\n";
                    }
                }
                foreach (var r in manifestFile.InjectedReferences)
                {
                    if (r != null && r.Subject != null && !string.IsNullOrEmpty(r.Subject.ID) && !string.IsNullOrEmpty(r.VerbURI) && r.Object != null && !string.IsNullOrEmpty(r.Object.ID))
                    {
                        string klass = "classClass";
                        if (r.Object.NameSpace == "http://ddhub.no/UnitAndQuantity")
                        {
                            klass = "quantityClass";
                        }
                        mermaid += "\t" + ProcessMermaid(r.Subject.ID) + "([" + ProcessMermaid(r.Subject.ID) + "]) -- " + ProcessMermaid(r.VerbURI) + " --> " + ProcessMermaid(r.Object.ID) + "([" + ProcessMermaid(r.Object.ID) + "]):::" + klass + "\n";
                    }
                }
                mermaid += "```\n";
                return mermaid;
            }
        }
        private static string ProcessMermaid(string str)
        {
            return str.Replace('#', '_');
        }
        public static Dictionary<string, QuerySpecification>? GetSparQLQueries(Assembly? assembly, string? typeName)
        {
            if (assembly == null || typeName == null)
            {
                return null;
            }
            Type[] types = assembly.GetTypes();
            if (types != null)
            {
                int count = 0;
                Dictionary<string, QuerySpecification> queries = [];
                foreach (Type type in types)
                {
                    if (type.FullName == typeName)
                    {
                        var workingSemanticFactAttributes = type.GetCustomAttributes<SemanticFactAttribute>();
                        var excludeFactAttributes = type.GetCustomAttributes<ExcludeFactAttribute>();
                        var optionalFactAttributes = type.GetCustomAttributes<OptionalFactAttribute>();
                        var excludeOptionalFactAttributes = type.GetCustomAttributes<OptionalExcludeFactAttribute>();
                        var semanticTypeVariableAttribute = type.GetCustomAttribute<SemanticTypeVariableAttribute>();
                        var semanticExclusiveOrAttributes = type.GetCustomAttributes<SemanticExclusiveOrAttribute>();
                        // remove the optional facts from the list of facts
                        List<SemanticFactAttribute> semanticFactAttributes = new List<SemanticFactAttribute>();
                        foreach (var attr in workingSemanticFactAttributes)
                        {
                            if (!optionalFactAttributes.Contains(attr))
                            {
                                semanticFactAttributes.Add(attr);
                            }
                        }
                        if (semanticTypeVariableAttribute != null ||
                            (semanticExclusiveOrAttributes != null && semanticExclusiveOrAttributes.Any()) ||
                            (semanticFactAttributes != null && semanticFactAttributes.Count > 0) ||
                            (excludeFactAttributes != null && excludeFactAttributes.Any()) ||
                            (optionalFactAttributes != null && optionalFactAttributes.Any()) ||
                            (excludeOptionalFactAttributes != null && excludeOptionalFactAttributes.Any()))
                        {
                            if (semanticTypeVariableAttribute != null)
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
                                    List<ExcludeFact> excludeFacts = [];
                                    if (excludeFactAttributes != null)
                                    {
                                        foreach (var excludedFactAttribute in excludeFactAttributes)
                                        {
                                            if (excludedFactAttribute != null)
                                            {
                                                ExcludeFact excludeFact = new ExcludeFact();
                                                excludeFact.Subject = excludedFactAttribute.Subject;
                                                excludeFact.SubjectName = excludedFactAttribute.SubjectName;
                                                excludeFact.Verb = excludedFactAttribute.Verb;
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
                                        if (combination != null) 
                                        {
                                            List<byte> options = GetOptions(combination, topLevelOptionalFacts, subLevelOptionalFacts);
                                            string soptions = string.Empty;
                                            string sparql = string.Empty;
                                            int argCount = 0;
                                            sparql += "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\n";
                                            sparql += "PREFIX ddhub: <http://ddhub.no/>\n";
                                            sparql += "PREFIX quantity: <http://ddhub.no/UnitAndQuantity>\n\n";
                                            if (semanticTypeVariableAttribute != null &&
                                                !string.IsNullOrEmpty(semanticTypeVariableAttribute.ValueVariable) &&
                                                IsUsed(combination, semanticTypeVariableAttribute.ValueVariable))
                                            {
                                                string var1 = ProcessQueryVariable(semanticTypeVariableAttribute.ValueVariable);
                                                sparql += "SELECT " + var1;
                                                argCount = 1;
                                                if (options.Count > 0)
                                                {
                                                    bool first = true;
                                                    foreach (var option in options)
                                                    {
                                                        if (!first)
                                                        {
                                                            soptions += ",";
                                                        }
                                                        first = false;
                                                        soptions += option;
                                                    }
                                                    if (!string.IsNullOrEmpty(soptions))
                                                    {
                                                        sparql += ", " + "?factOptionSet";
                                                        argCount++;
                                                    }
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
                                                sparql += "  }\n";
                                            }
                                            if (!string.IsNullOrEmpty(soptions))
                                            {
                                                sparql += "  BIND (\"" + soptions + "\" as ?factOptionSet)\n";
                                            }
                                            sparql += "}\n";
                                            queries.Add("Query-" + typeName + "-" + count.ToString("000"), new QuerySpecification() { NumberOfArguments = argCount, Options = options, SparQL = sparql });
                                            count++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return queries;
            }
            return null;
        }
        public static Dictionary<string, QuerySpecification>? GetSparQLQueries(Assembly? assembly, string? typeName, string? propertyName)
        {
            if (assembly == null || typeName == null || propertyName == null)
            {
                return null;
            }
            Type[] types = assembly.GetTypes();
            if (types != null)
            {
                int count = 0;
                Dictionary<string, QuerySpecification> queries = [];
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
                                var semanticDeterministicCategoricalVariableAttribute = property.GetCustomAttribute<SemanticDeterministicCategoricalVariableAttribute>();
                                var semanticDeterministicBernoulliVariableAttribute = property.GetCustomAttribute<SemanticDeterministicBernoulliVariableAttribute>();
                                var semanticBernoulliVariableAttribute = property.GetCustomAttribute<SemanticBernoulliVariableAttribute>();
                                var semanticCategoricalVariableAttribute = property.GetCustomAttribute<SemanticCategoricalVariableAttribute>();
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
                                if (semanticDiracVariableAttribute != null ||
                                    semanticGaussianVariableAttribute != null ||
                                    semanticSensorVariableAttribute != null ||
                                    semanticFullScaleVariableAttribute != null ||
                                    semanticUniformVariableAttribute != null ||
                                    semanticGeneralDistributionVariableAttribute != null ||
                                    semanticDeterministicCategoricalVariableAttribute != null ||
                                    semanticDeterministicBernoulliVariableAttribute != null ||
                                    semanticBernoulliVariableAttribute != null ||
                                    semanticCategoricalVariableAttribute != null ||
                                    (semanticExclusiveOrAttributes != null && semanticExclusiveOrAttributes.Any()) ||
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
                                        semanticDeterministicCategoricalVariableAttribute != null ||
                                        semanticDeterministicBernoulliVariableAttribute != null ||
                                        semanticBernoulliVariableAttribute != null ||
                                        semanticCategoricalVariableAttribute !=  null)
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
                                                        excludeFact.Verb = excludedFactAttribute.Verb;
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
                                                string soptions = string.Empty;
                                                int argCount = 0;
                                                sparql += "PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>\n";
                                                sparql += "PREFIX ddhub: <http://ddhub.no/>\n";
                                                sparql += "PREFIX quantity: <http://ddhub.no/UnitAndQuantity>\n\n";
                                                List<byte> options = GetOptions(combination, topLevelOptionalFacts, subLevelOptionalFacts);
                                                if (semanticDiracVariableAttribute != null &&
                                                    !string.IsNullOrEmpty(semanticDiracVariableAttribute.ValueVariable) &&
                                                    IsUsed(combination, semanticDiracVariableAttribute.ValueVariable))
                                                {
                                                    string var1 = ProcessQueryVariable(semanticDiracVariableAttribute.ValueVariable);
                                                    sparql += "SELECT " + var1;
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
                                                    sparql += "SELECT " + min + ", " + max;
                                                    argCount = 2;
                                                }
                                                else if (semanticSensorVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticSensorVariableAttribute.MeanVariable) &&
                                                         IsUsed(combination, semanticSensorVariableAttribute.MeanVariable) &&
                                                         !string.IsNullOrEmpty(semanticSensorVariableAttribute.AccuracyVariable) &&
                                                         IsUsed(combination, semanticSensorVariableAttribute.AccuracyVariable) &&
                                                         !string.IsNullOrEmpty(semanticSensorVariableAttribute.PrecisionVariable) &&
                                                         IsUsed(combination, semanticSensorVariableAttribute.PrecisionVariable))
                                                {
                                                    string mean = ProcessQueryVariable(semanticSensorVariableAttribute.MeanVariable);
                                                    sparql += "SELECT " + mean;
                                                    argCount = 1;
                                                    string precision = ProcessQueryVariable(semanticSensorVariableAttribute.PrecisionVariable);
                                                    sparql += ", " + precision;
                                                    argCount++;
                                                    string accuracy = ProcessQueryVariable(semanticSensorVariableAttribute.AccuracyVariable);
                                                    sparql += ", " + accuracy;
                                                    argCount++;
                                                }
                                                else if (semanticFullScaleVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticFullScaleVariableAttribute.MeanVariable) &&
                                                         !string.IsNullOrEmpty(semanticFullScaleVariableAttribute.FullScaleVariable) &&
                                                         IsUsed(combination, semanticFullScaleVariableAttribute.FullScaleVariable) &&
                                                         !string.IsNullOrEmpty(semanticFullScaleVariableAttribute.ProportionErrorVariable) &&
                                                         IsUsed(combination, semanticFullScaleVariableAttribute.ProportionErrorVariable))
                                                {
                                                    string mean = ProcessQueryVariable(semanticFullScaleVariableAttribute.MeanVariable);
                                                    sparql += "SELECT " + mean;
                                                    argCount = 1;
                                                    string fullScale = ProcessQueryVariable(semanticFullScaleVariableAttribute.FullScaleVariable);
                                                    sparql += ", " + fullScale;
                                                    argCount++;
                                                    string proportionError = ProcessQueryVariable(semanticFullScaleVariableAttribute.ProportionErrorVariable);
                                                    sparql += ", " + proportionError;
                                                    argCount++;
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
                                                }
                                                else if (semanticGeneralDistributionVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticGeneralDistributionVariableAttribute.HistogramVariable) &&
                                                         IsUsed(combination, semanticGeneralDistributionVariableAttribute.HistogramVariable))
                                                {
                                                    string histoVar = ProcessQueryVariable(semanticGeneralDistributionVariableAttribute.HistogramVariable);
                                                    sparql += "SELECT " + histoVar;
                                                    argCount = 1;
                                                }
                                                else if (semanticDeterministicCategoricalVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticDeterministicCategoricalVariableAttribute.Variable) &&
                                                         IsUsed(combination, semanticDeterministicCategoricalVariableAttribute.Variable))
                                                {
                                                    string variable = ProcessQueryVariable(semanticDeterministicCategoricalVariableAttribute.Variable);
                                                    sparql += "SELECT " + variable;
                                                    argCount = 1;
                                                }
                                                else if (semanticDeterministicBernoulliVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticDeterministicBernoulliVariableAttribute.Variable) &&
                                                         IsUsed(combination, semanticDeterministicBernoulliVariableAttribute.Variable))
                                                {
                                                    string variable = ProcessQueryVariable(semanticDeterministicBernoulliVariableAttribute.Variable);
                                                    sparql += "SELECT " + variable;
                                                    argCount = 1;
                                                }
                                                else if (semanticBernoulliVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticBernoulliVariableAttribute.Variable) &&
                                                         IsUsed(combination, semanticBernoulliVariableAttribute.Variable))
                                                {
                                                    string variable = ProcessQueryVariable(semanticBernoulliVariableAttribute.Variable);
                                                    sparql += "SELECT " + variable;
                                                    argCount = 1;
                                                }
                                                else if (semanticCategoricalVariableAttribute != null &&
                                                         !string.IsNullOrEmpty(semanticCategoricalVariableAttribute.Variable) &&
                                                         IsUsed(combination, semanticCategoricalVariableAttribute.Variable))
                                                {
                                                    sparql += "SELECT " + ProcessQueryVariable(semanticCategoricalVariableAttribute.Variable);
                                                }
                                                if (options.Count > 0)
                                                {
                                                    bool first = true;
                                                    foreach (var option in options)
                                                    {
                                                        if (!first)
                                                        {
                                                            soptions += ",";
                                                        }
                                                        first = false;
                                                        soptions += option;
                                                    }
                                                    if (!string.IsNullOrEmpty(soptions))
                                                    {
                                                        sparql += ", " + "?factOptionSet";
                                                        argCount++;
                                                    }
                                                }
                                                sparql += "\n";
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
                                                    sparql += "  }\n";
                                                }
                                                if (!string.IsNullOrEmpty(soptions))
                                                {
                                                    sparql += "  BIND (\"" + soptions + "\" as ?factOptionSet)\n";
                                                }
                                                sparql += "}\n";
                                                queries.Add("Query-" + typeName + "-" + propertyName + "-" + count.ToString("000"), new QuerySpecification() { NumberOfArguments = argCount, Options = options, SparQL = sparql });
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

        private static List<byte> GetOptions(List<SemanticFact> facts, List<OptionalFactAttribute> topLevelOptionalFacts, List<OptionalFactAttribute> subLevelOptionalFacts)
        {
            List<byte> options = [];
            if (facts != null)
            {
                foreach (var fact in facts)
                {
                    if (fact != null)
                    {
                        byte optionID = 0;
                        if (topLevelOptionalFacts != null)
                        {
                            foreach (var optFact in topLevelOptionalFacts)
                            {
                                if (optFact != null &&
                                    optFact.Object == fact.Object &&
                                    optFact.Subject == fact.Subject &&
                                    optFact.ObjectAttributes == fact.ObjectAttributes &&
                                    optFact.ObjectDrillingQuantity == fact.ObjectDrillingQuantity &&
                                    optFact.ObjectPhysicalQuantity == fact.ObjectPhysicalQuantity &&
                                    optFact.ObjectName == fact.ObjectName &&
                                    optFact.SubjectName == fact.SubjectName)
                                {
                                    byte id = optFact.GroupIndex;
                                    if (!options.Contains(id))
                                    {
                                        options.Add(id);
                                        break;
                                    }
                                }
                            }
                        }
                        if (optionID == 0)
                        {
                            if (topLevelOptionalFacts != null)
                            {
                                foreach (var optFact in subLevelOptionalFacts)
                                {
                                    if (optFact != null &&
                                    optFact.Object == fact.Object &&
                                    optFact.Subject == fact.Subject &&
                                    optFact.ObjectAttributes == fact.ObjectAttributes &&
                                    optFact.ObjectDrillingQuantity == fact.ObjectDrillingQuantity &&
                                    optFact.ObjectPhysicalQuantity == fact.ObjectPhysicalQuantity &&
                                    optFact.ObjectName == fact.ObjectName &&
                                    optFact.SubjectName == fact.SubjectName)
                                    {
                                        byte id = optFact.GroupIndex;
                                        if (!options.Contains(id))
                                        {
                                            options.Add(id);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return options;
        }
        private static bool IsUsed(List<SemanticFact> combination, string? variable)
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
        public static List<string>? GetMarkDownSparQLFormatting(List<string>? sparqls)
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

        private static string ProcessManifestVariable(string variable, string prefix)
        {
            variable = variable.Trim();
            return prefix + variable;
        }
        private static Dictionary<string, List<Nouns.Enum>> FindInjectedNodes(List<SemanticFact> facts, List<string> providedVariables)
        {
            Dictionary<string, List<Nouns.Enum>> results = new();
            if (facts != null)
            {
                if (providedVariables == null)
                {
                    providedVariables = new List<string>();
                }
                foreach (var fact in facts)
                {
                    if (fact != null && !string.IsNullOrEmpty(fact.SubjectName) && fact.Verb == Verbs.Enum.BelongsToClass && fact.Object != null)
                    {
                        if (!providedVariables.Contains(fact.SubjectName))
                        {
                            if (results.ContainsKey(fact.SubjectName))
                            {
                                if (results[fact.SubjectName] == null)
                                {
                                    results[fact.SubjectName] = new();
                                }
                                results[fact.SubjectName].Add(fact.Object.Value);
                            }
                            else
                            {
                                results.Add(fact.SubjectName, [fact.Object.Value]);
                            }
                        }
                    }
                }
            }
            return results;
        }
        private static string ProcessQueryVariable(string variable)
        {
            variable = variable.Trim();
            if (!variable.StartsWith('?'))
            {
                variable = '?' + variable;
            }
            return variable;
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

        private static List<List<SemanticFact>>? GetCombinations(IEnumerable<SemanticFactAttribute>? facts,
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
        private static List<List<byte>>? GetCombinations(IEnumerable<SemanticExclusiveOrAttribute>? exclusiveOrs, List<byte> bundles)
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
        private static List<List<byte>>? GetCombinations(List<byte> bundles)
        {
            List<List<byte>> result = new List<List<byte>>();
            List<byte> currentCombination = new List<byte>();

            // Start the recursive call
            GenerateCombinations(bundles, 0, currentCombination, result);

            return result;
        }
        private static void GenerateCombinations(List<byte> bundles, int index, List<byte> currentCombination, List<List<byte>> result)
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
    }
}
