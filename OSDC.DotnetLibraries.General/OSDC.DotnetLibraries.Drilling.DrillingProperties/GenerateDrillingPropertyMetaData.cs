using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class GenerateDrillingPropertyMetaData
    {

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
                            var physicalQuantityAttribute = property.GetCustomAttribute<PhysicalQuantityAttribute>();
                            var drillingPhysicalQuantityAttribute = property.GetCustomAttribute<DrillingPhysicalQuantityAttribute>();
                            var accessToVariableAttribute = property.GetCustomAttribute<AccessToVariableAttribute>();
                            var mandatoryAttritbute = property.GetCustomAttribute<MandatoryAttribute>();
                            var semanticFactAttributes = property.GetCustomAttributes<SemanticFactAttribute>();
                            var optionalFactAttributes = property.GetCustomAttributes<OptionalFactAttribute>();
                            var semanticDiracVariableAttribute = property.GetCustomAttribute<SemanticDiracVariableAttribute>();
                            var semanticGaussianVariableAttribute = property.GetCustomAttribute<SemanticGaussianVariableAttribute>();
                            var semanticUniformVariableAttribute = property.GetCustomAttribute<SemanticUniformVariableAttribute>();
                            var semanticGeneralDistributionVariableAttribute = property.GetCustomAttribute<SemanticGeneralDistributionVariableAttribute>();
                            var abscissaReferenceAttribute = property.GetCustomAttribute<AbscissaReferenceAttribute>();
                            var anglePositionReferenceAttribute = property.GetCustomAttribute<AnglePositionReferenceAttribute>();
                            var axialPositionReferenceAttribute = property.GetCustomAttribute<AxialPositionReferenceAttribute>();
                            var azimuthReferenceAttribute = property.GetCustomAttribute<AzimuthReferenceAttribute>();
                            var depthReferenceAttribute = property.GetCustomAttribute<DepthReferenceAttribute>();
                            var positionReferenceAttribute = property.GetCustomAttribute<PositionReferenceAttribute>();
                            var pressureReferenceAttribute = property.GetCustomAttribute<PressureReferenceAttribute>();
                            var timeReferenceAttribute = property.GetCustomAttribute<TimeReferenceAttribute>();
                            if (physicalQuantityAttribute != null ||
                                drillingPhysicalQuantityAttribute != null ||
                                accessToVariableAttribute != null ||
                                semanticDiracVariableAttribute != null ||
                                semanticGaussianVariableAttribute != null ||
                                semanticUniformVariableAttribute != null ||
                                semanticGeneralDistributionVariableAttribute != null ||
                                anglePositionReferenceAttribute != null ||
                                axialPositionReferenceAttribute != null ||
                                abscissaReferenceAttribute != null ||
                                azimuthReferenceAttribute != null ||
                                depthReferenceAttribute != null ||
                                timeReferenceAttribute != null ||
                                mandatoryAttritbute != null ||
                                positionReferenceAttribute != null ||
                                pressureReferenceAttribute != null ||
                                (semanticFactAttributes != null && semanticFactAttributes.Any()) ||
                                (optionalFactAttributes != null && optionalFactAttributes.Any()))
                            {
                                MetaDataDrillingProperty metaData = new MetaDataDrillingProperty();
                                metaData.Namespace = (type.Namespace == null) ? string.Empty : type.Namespace;
                                metaData.ClassName = type.Name;
                                metaData.PropertyName = property.Name;
                                if (physicalQuantityAttribute != null)
                                {
                                    metaData.PhysicalQuantity = physicalQuantityAttribute.PhysicalQuantity;
                                }
                                if (drillingPhysicalQuantityAttribute != null)
                                {
                                    metaData.DrillingPhysicalQuantity = drillingPhysicalQuantityAttribute.PhysicalQuantity;
                                }
                                if (accessToVariableAttribute != null)
                                {
                                    metaData.AccessType = accessToVariableAttribute.AccessType;
                                }
                                if (semanticDiracVariableAttribute != null)
                                {
                                    metaData.SemanticDiracVariable = semanticDiracVariableAttribute.Value;
                                }
                                if (semanticGaussianVariableAttribute != null)
                                {
                                    metaData.SemanticGaussianMeanVariable = semanticGaussianVariableAttribute.Mean;
                                    metaData.SemanticGaussianStandardDeviationVariable = semanticGaussianVariableAttribute.StandardDeviation;
                                }
                                if (semanticUniformVariableAttribute != null)
                                {
                                    metaData.SemanticUniformMinVariable = semanticUniformVariableAttribute.MinValue;
                                    metaData.SemanticUniformMaxVariable = semanticUniformVariableAttribute.MaxValue;
                                }
                                if (semanticGeneralDistributionVariableAttribute != null)
                                {
                                    metaData.SemanticGeneralDistributionHistogramVariable = semanticGeneralDistributionVariableAttribute.Histogram;
                                }
                                if (anglePositionReferenceAttribute != null)
                                {
                                    metaData.AnglePositionReferenceType = anglePositionReferenceAttribute.ReferenceType;
                                }
                                if (axialPositionReferenceAttribute != null)
                                {
                                    metaData.AxialPositionReferenceType = axialPositionReferenceAttribute.ReferenceType;
                                }
                                if (abscissaReferenceAttribute != null)
                                {
                                    metaData.AbscissaReferenceType = abscissaReferenceAttribute.ReferenceType;
                                }
                                if (azimuthReferenceAttribute != null)
                                {
                                    metaData.AzimuthReferenceType = azimuthReferenceAttribute.ReferenceType;
                                }
                                if (depthReferenceAttribute != null)
                                {
                                    metaData.DepthReferenceType = depthReferenceAttribute.ReferenceType;
                                }
                                if (timeReferenceAttribute != null)
                                {
                                    metaData.TimeReferenceType = timeReferenceAttribute.ReferenceType;
                                }
                                if (mandatoryAttritbute != null)
                                {
                                    metaData.MandatoryType = mandatoryAttritbute.Mandatory;
                                }
                                if (positionReferenceAttribute != null)
                                {
                                    metaData.PositionReferenceType = positionReferenceAttribute.ReferenceType;
                                }
                                if (pressureReferenceAttribute != null)
                                {
                                    metaData.PressureReferenceType = pressureReferenceAttribute.ReferenceType;
                                }
                                if (semanticFactAttributes != null)
                                {
                                    foreach (var attribute in semanticFactAttributes)
                                    {
                                        if (attribute != null)
                                        {
                                            SemanticFact fact = new SemanticFact();
                                            fact.Subject = attribute.Subject;
                                            fact.SubjectName = attribute.SubjectName;
                                            fact.Verb = attribute.Verb;
                                            fact.Object = attribute.Object;
                                            fact.ObjectName = attribute.ObjectName;
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
                                            OptionalFact fact = new OptionalFact();
                                            fact.GroupIndex = attribute.GroupIndex;
                                            fact.Subject = attribute.Subject;
                                            fact.SubjectName = attribute.SubjectName;
                                            fact.Verb = attribute.Verb;
                                            fact.Object = attribute.Object;
                                            fact.ObjectName = attribute.ObjectName;
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
