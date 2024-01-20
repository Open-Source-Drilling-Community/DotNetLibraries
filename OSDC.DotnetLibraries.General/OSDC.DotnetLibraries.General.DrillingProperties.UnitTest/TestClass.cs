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
        [MetaDataID("bc51b872-cf4d-4474-86a9-9f4a83efe905")]
        public ScalarDrillingProperty ScalarValue { get; set; } = new ScalarDrillingProperty("bc51b872-cf4d-4474-86a9-9f4a83efe905");

        [PhysicalQuantity(PhysicalQuantity.QuantityEnum.SmallLength)]
        [AbscissaReference(CommonProperty.AbscissaReferenceType.Top)]
        [Mandatory(CommonProperty.MandatoryType.PipeHandling | CommonProperty.MandatoryType.Mechanical)]
        [MetaDataID("52539cdd-1918-4f7d-8cdd-b9bc9bb818c3")]
        public UniformDrillingProperty UniformValue { get; set; } = new UniformDrillingProperty("52539cdd-1918-4f7d-8cdd-b9bc9bb818c3");

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingDensity)]
        [Mandatory(CommonProperty.MandatoryType.Hydraulic | CommonProperty.MandatoryType.Mechanical | CommonProperty.MandatoryType.MaterialTransport)]
        [MetaDataID("cc46f945-c9f5-4605-b1b3-c391239416fb")]
        public GaussianDrillingProperty GaussianValue { get; set; } = new GaussianDrillingProperty("cc46f945-c9f5-4605-b1b3-c391239416fb");

        [DrillingPhysicalQuantity(DrillingPhysicalQuantity.QuantityEnum.DrillingPlaneAngle)]
        [Mandatory(CommonProperty.MandatoryType.Directional)]
        [AzimuthReference(CommonProperty.AzimuthReferenceType.TrueNorth)]
        [MetaDataID("ae4ed40a-e7d2-486d-8368-61528bc95cee")]
        public GeneralDistributionDrillingProperty GeneralDistributionValue { get; set; } = new GeneralDistributionDrillingProperty("ae4ed40a-e7d2-486d-8368-61528bc95cee");
    }
}
