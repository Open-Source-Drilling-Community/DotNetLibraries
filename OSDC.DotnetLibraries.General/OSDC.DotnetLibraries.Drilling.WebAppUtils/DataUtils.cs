using OSDC.UnitConversion.DrillingRazorMudComponents;

namespace OSDC.DotnetLibraries.Drilling.WebAppUtils
{
    public class DataUtils
    {
        public static class UnitAndReferenceParameters
        {
            public static string? UnitSystemName { get; set; } = "Metric";
            public static string? DepthReferenceName { get; set; } = "Rotary table";
            public static string? PositionReferenceName { get; set; } = "Well-head";
            public static string? GeodeticReferenceName { get; set; }
            public static string? AzimuthReferenceName { get; set; }
            public static string? PressureReferenceName { get; set; }
            public static string? DateReferenceName { get; set; }
        }

        public class GroundMudLineDepthReferenceSource : IGroundMudLineDepthReferenceSource
        {
            public double? GroundMudLineDepthReference { get; set; }
        }

        public class RotaryTableDepthReferenceSource : IRotaryTableDepthReferenceSource
        {
            public double? RotaryTableDepthReference { get; set; }
        }

        public class SeaWaterLevelDepthReferenceSource : ISeaWaterLevelDepthReferenceSource
        {
            public double? SeaWaterLevelDepthReference { get; set; }
        }

        public class WellHeadPositionReferenceSource : IWellHeadPositionReferenceSource
        {
            public double? WellHeadNorthPositionReference { get; set; }
            public double? WellHeadEastPositionReference { get; set; }
        }

        public class CartographicGridPositionReferenceSource : ICartographicGridPositionReferenceSource
        {
            public double? CartographicGridNorthPositionReference { get; set; }
            public double? CartographicGridEastPositionReference { get; set; }
        }

        public class FieldPositionReferenceSource : IFieldPositionReferenceSource
        {
            public double? FieldNorthPositionReference { get; set; }
            public double? FieldEastPositionReference { get; set; }
        }

        public class CartographicProjectionDatumGeodeticReferenceSource : ICartographicProjectionDatumGeodeticReferenceSource
        {
            public double? CartographicProjectionDatumLatitudeReference { get; set; }
            public double? CartographicProjectionDatumLongitudeReference { get; set; }
        }

        public class ClusterPositionReferenceSource : IClusterPositionReferenceSource
        {
            public double? ClusterNorthPositionReference { get; set; }
            public double? ClusterEastPositionReference { get; set; }
        }

    }
}
