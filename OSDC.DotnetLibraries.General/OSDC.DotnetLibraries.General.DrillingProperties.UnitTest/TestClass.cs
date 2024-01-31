using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.DrillingProperties;
using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using DWIS.Vocabulary.Schemas;

namespace OSDC.DotnetLibraries.General.DrillingProperties.UnitTest
{
    internal class TestClass
    {
        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.Depth)]
        [DepthReference(CommonProperty.DepthReferenceType.WGS84)]
        [Mandatory(CommonProperty.MandatoryType.General)]
        [SemanticFact(Nouns.Enum.DynamicDrillingSignal, Verbs.Enum.IsProcessedBy, Nouns.Enum.ComputationUnit)]
        [SemanticFact(Nouns.Enum.DataProvider, Verbs.Enum.BelongsToClass, Nouns.Enum.DrillingContractor)]
        public ScalarDrillingProperty ScalarValue { get; set; } = new ScalarDrillingProperty();

        [PhysicalQuantity(PhysicalQuantity.QuantityEnum.SmallLength)]
        [AbscissaReference(CommonProperty.AbscissaReferenceType.Top)]
        [Mandatory(CommonProperty.MandatoryType.PipeHandling | CommonProperty.MandatoryType.Mechanical)]
        public UniformDrillingProperty UniformValue { get; set; } = new UniformDrillingProperty();

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        [Mandatory(CommonProperty.MandatoryType.Hydraulic | CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.MaterialTransport)]
        public GaussianDrillingProperty GaussianValue { get; set; } = new GaussianDrillingProperty();

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [Mandatory(CommonProperty.MandatoryType.Directional)]
        [AzimuthReference(CommonProperty.AzimuthReferenceType.TrueNorth)]
        public GeneralDistributionDrillingProperty GeneralDistributionValue { get; set; } = new GeneralDistributionDrillingProperty();
    }
}
