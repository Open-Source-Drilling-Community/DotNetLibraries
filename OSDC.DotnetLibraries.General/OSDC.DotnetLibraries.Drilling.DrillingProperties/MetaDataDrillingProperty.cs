using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class MetaDataDrillingProperty
    {
        public string Namespace { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommonProperty.VariableAccessType? AccessType { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticDiracVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticGaussianMeanVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticGaussianStandardDeviationVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SemanticDefaultStandardDeviation { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticSensorPrecisionVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SemanticSensorDefaultPrecision { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticSensorAccuracyVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SemanticSensorDefaultAccuracy { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticFullScaleVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SemanticDefaultFullScale { get; set; } = null;
        public string? SemanticProportionErrorVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SemanticDefaultProportionError { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticUniformMinVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticUniformMaxVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticGeneralDistributionHistogramVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticBernoulliProbabilistVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SemanticBernoulliDeterministVariable { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? SemanticBernoulliDeterministDefaultUncertainty { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<byte[]>? SemanticExclusiveOrs {  get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommonProperty.MandatoryType? MandatoryType { get; set; } = null;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<SemanticFact>? SemanticFacts { get; set; } = null;
        public List<OptionalFact>? OptionalFacts { get; set; } = null;

        public static Dictionary<Tuple<string, string>, MetaDataDrillingProperty>? GetDrillingPropertyMetaData(Assembly assembly)
        {
            if (assembly == null)
            {
                return null;
            }
            Type[] types = assembly.GetTypes();
            Dictionary<Tuple<string, string>, MetaDataDrillingProperty> results = new Dictionary<Tuple<string, string>, MetaDataDrillingProperty>();
            // Filter and print only the classes
            foreach (Type type in types)
            {
                if (type.IsClass)
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
                            var semanticBernoulliVariableAttribute = property.GetCustomAttribute<SemanticBernoulliVariableAttribute>();
                            var semanticExclusiveOrAttributes = property.GetCustomAttributes<SemanticExclusiveOrAttribute>();
                            if (accessToVariableAttribute != null ||
                                semanticDiracVariableAttribute != null ||
                                semanticGaussianVariableAttribute != null ||
                                semanticSensorVariableAttribute != null ||
                                semanticFullScaleVariableAttribute != null ||
                                semanticUniformVariableAttribute != null ||
                                semanticGeneralDistributionVariableAttribute != null ||
                                semanticBernoulliVariableAttribute != null ||
                                (semanticExclusiveOrAttributes != null && semanticExclusiveOrAttributes.Any()) ||
                                mandatoryAttritbute != null ||
                                (semanticFactAttributes != null && semanticFactAttributes.Any()) ||
                                (optionalFactAttributes != null && optionalFactAttributes.Any()))
                            {
                                MetaDataDrillingProperty metaData = new MetaDataDrillingProperty();
                                metaData.Namespace = (type.Namespace == null) ? string.Empty : type.Namespace;
                                metaData.ClassName = type.Name;
                                metaData.PropertyName = property.Name;
                                if (accessToVariableAttribute != null)
                                {
                                    metaData.AccessType = accessToVariableAttribute.AccessType;
                                }
                                if (semanticDiracVariableAttribute != null)
                                {
                                    metaData.SemanticDiracVariable = semanticDiracVariableAttribute.ValueVariable;
                                }
                                if (semanticGaussianVariableAttribute != null)
                                {
                                    metaData.SemanticGaussianMeanVariable = semanticGaussianVariableAttribute.MeanVariable;
                                    metaData.SemanticGaussianStandardDeviationVariable = semanticGaussianVariableAttribute.StandardDeviationVariable;
                                    metaData.SemanticDefaultStandardDeviation = semanticGaussianVariableAttribute.DefaultStandardDeviation;
                                }
                                if (semanticSensorVariableAttribute != null)
                                {
                                    metaData.SemanticSensorPrecisionVariable = semanticSensorVariableAttribute.PrecisionVariable;
                                    metaData.SemanticSensorDefaultPrecision = semanticSensorVariableAttribute.DefaultPrecision;
                                    metaData.SemanticSensorAccuracyVariable = semanticSensorVariableAttribute.AccuracyVariable;
                                    metaData.SemanticSensorDefaultAccuracy = semanticSensorVariableAttribute.DefaultAccuracy;
                                }
                                if (semanticFullScaleVariableAttribute != null)
                                {
                                    metaData.SemanticFullScaleVariable = semanticFullScaleVariableAttribute.FullScaleVariable;
                                    metaData.SemanticDefaultFullScale = semanticFullScaleVariableAttribute.DefaultFullScale;
                                    metaData.SemanticProportionErrorVariable = semanticFullScaleVariableAttribute.ProportionErrorVariable;
                                    metaData.SemanticDefaultProportionError = semanticFullScaleVariableAttribute.DefaultProportionError;
                                }
                                if (semanticUniformVariableAttribute != null)
                                {
                                    metaData.SemanticUniformMinVariable = semanticUniformVariableAttribute.MinValueVariable;
                                    metaData.SemanticUniformMaxVariable = semanticUniformVariableAttribute.MaxValueVariable;
                                }
                                if (semanticGeneralDistributionVariableAttribute != null)
                                {
                                    metaData.SemanticGeneralDistributionHistogramVariable = semanticGeneralDistributionVariableAttribute.HistogramVariable;
                                }
                                if (semanticBernoulliVariableAttribute != null)
                                {
                                    metaData.SemanticBernoulliProbabilistVariable = semanticBernoulliVariableAttribute.ProbabilistVariable;
                                    metaData.SemanticBernoulliDeterministVariable = semanticBernoulliVariableAttribute.DeterministVariable;
                                    metaData.SemanticBernoulliDeterministDefaultUncertainty = semanticBernoulliVariableAttribute.DeterministDefaultUncertainty;
                                }
                                if (semanticExclusiveOrAttributes != null)
                                {
                                    metaData.SemanticExclusiveOrs = new List<byte[]>();
                                    foreach (var attribute in semanticExclusiveOrAttributes)
                                    {
                                        if (attribute.ExclusiveOr != null)
                                        {
                                            metaData.SemanticExclusiveOrs.Add(attribute.ExclusiveOr);
                                        }
                                    }
                                }
                                if (mandatoryAttritbute != null)
                                {
                                    metaData.MandatoryType = mandatoryAttritbute.Mandatory;
                                }
                                if (semanticFactAttributes != null)
                                {
                                    foreach (var attribute in semanticFactAttributes)
                                    {
                                        if (attribute != null)
                                        {
                                            SemanticFact? fact = null;
                                            if (attribute is ExcludeFactAttribute)
                                            {
                                                fact = new ExcludeFact();

                                            }
                                            else
                                            {
                                                fact = new SemanticFact();
                                            }
                                            fact.Subject = attribute.Subject;
                                            fact.SubjectName = attribute.SubjectName;
                                            fact.Verb = attribute.Verb;
                                            fact.Object = attribute.Object;
                                            fact.ObjectName = attribute.ObjectName;
                                            fact.ObjectPhysicalQuantity = attribute.ObjectPhysicalQuantity;
                                            fact.ObjectDrillingQuantity = attribute.ObjectDrillingQuantity;
                                            fact.ObjectAttributes = attribute.ObjectAttributes;
                                            if (metaData.SemanticFacts == null)
                                            {
                                                metaData.SemanticFacts = new List<SemanticFact>();
                                            }
                                            metaData.SemanticFacts.Add(fact);
                                        }
                                    }
                                }
                                if (optionalFactAttributes != null)
                                {
                                    foreach (var attribute in optionalFactAttributes)
                                    {
                                        if (attribute != null)
                                        {
                                            OptionalFact? fact = null;
                                            if (attribute is OptionalExcludeFactAttribute)
                                            {
                                                fact = new OptionalExcludeFact();
                                            }
                                            else
                                            {
                                                fact = new OptionalFact();
                                            }
                                            fact.ParentGroupIndex = attribute.ParentGroupIndex;
                                            fact.GroupIndex = attribute.GroupIndex;
                                            fact.Subject = attribute.Subject;
                                            fact.SubjectName = attribute.SubjectName;
                                            fact.Verb = attribute.Verb;
                                            fact.Object = attribute.Object;
                                            fact.ObjectName = attribute.ObjectName;
                                            fact.ObjectPhysicalQuantity = attribute.ObjectPhysicalQuantity;
                                            fact.ObjectDrillingQuantity = attribute.ObjectDrillingQuantity;
                                            fact.ObjectAttributes = attribute.ObjectAttributes;
                                            if (metaData.OptionalFacts == null)
                                            {
                                                metaData.OptionalFacts = new List<OptionalFact>();
                                            }
                                            metaData.OptionalFacts.Add(fact);
                                        }
                                    }
                                }
                                results.Add(new Tuple<string, string>(type.Name, property.Name), metaData);
                            }
                        }
                    }
                }
            }
            return results;
        }
    }
}
