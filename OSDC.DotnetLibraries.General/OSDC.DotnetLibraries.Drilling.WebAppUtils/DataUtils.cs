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

        public class LeaseLinePositionReferenceSource : ILeaseLinePositionReferenceSource
        {
            public double? LeaseLineNorthPositionReference { get; set; }
            public double? LeaseLineEastPositionReference { get; set; }
        }

        public class ClusterPositionReferenceSource : IClusterPositionReferenceSource
        {
            public double? ClusterNorthPositionReference { get; set; }
            public double? ClusterEastPositionReference { get; set; }
        }

    }
}
