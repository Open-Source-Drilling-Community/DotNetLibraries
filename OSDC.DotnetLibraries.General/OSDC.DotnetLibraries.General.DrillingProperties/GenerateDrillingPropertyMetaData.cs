using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class GenerateDrillingPropertyMetaData
    {

        public static Dictionary<Guid, MetaDataDrillingProperty>? GetDrillingPropertyMetaData(Assembly assembly)
        {
            if (assembly == null)
            {
                return null;
            }
            Type[] types = assembly.GetTypes();
            Dictionary<Guid, MetaDataDrillingProperty> results = new Dictionary<Guid, MetaDataDrillingProperty>();
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
                            var metaDataIDAttribute = (MetaDataIDAttribute?)property.GetCustomAttribute(typeof(MetaDataIDAttribute));
                            if (metaDataIDAttribute != null && metaDataIDAttribute.MetaDataID != Guid.Empty)
                            {
                                Guid ID = metaDataIDAttribute.MetaDataID;
                                var physicalQuantityAttribute = property.GetCustomAttribute<PhysicalQuantityAttribute>();
                                var drillingPhysicalQuantityAttribute = property.GetCustomAttribute<DrillingPhysicalQuantityAttribute>();
                                var abscissaReferenceAttribute = property.GetCustomAttribute<AbscissaReferenceAttribute>();
                                var azimuthReferenceAttribute = property.GetCustomAttribute<AzimuthReferenceAttribute>();
                                var depthReferenceAttribute = property.GetCustomAttribute<DepthReferenceAttribute>();
                                var mandatoryAttritbute = property.GetCustomAttribute<MandatoryAttribute>();
                                var positionReferenceAttribute = property.GetCustomAttribute<PositionReferenceAttribute>();
                                var pressureReferenceAttribute = property.GetCustomAttribute<PressureReferenceAttribute>();
                                var semanticFactsAttributes = property.GetCustomAttributes<SemanticFactAttribute>();
                                MetaDataDrillingProperty metaData = new MetaDataDrillingProperty();
                                if (physicalQuantityAttribute != null)
                                {
                                    metaData.PhysicalQuantity = physicalQuantityAttribute.PhysicalQuantity;
                                }
                                if (drillingPhysicalQuantityAttribute != null)
                                {
                                    metaData.DrillingPhysicalQuantity = drillingPhysicalQuantityAttribute.PhysicalQuantity;
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
                                if (semanticFactsAttributes != null)
                                {
                                    foreach (var attribute in semanticFactsAttributes)
                                    {
                                        if (attribute != null)
                                        {
                                            SemanticFact fact = new SemanticFact();
                                            fact.Subject = attribute.Subject;
                                            fact.SubjectName = attribute.SubjectName;
                                            fact.Verb = attribute.Verb;
                                            fact.Object = attribute.Object;
                                            fact.ObjectName = attribute.ObjectName;
                                            if (metaData.SemanticFacts == null)
                                            {
                                                metaData.SemanticFacts = new List<SemanticFact>();
                                            }
                                            metaData.SemanticFacts.Add(fact);
                                        }
                                    }
                                }
                                results.Add(ID, metaData);
                            }
                        }
                    }
                }
            }
            return results;
        }
    }
}
