using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.Drilling.DrillingProperties
{
    public class CommonProperty
    {
        public enum TimeReferenceType { None, GPS, UTC, LocalTime};
        public enum AbscissaReferenceType { None, BottomOfString, TopOfString };
        public enum DepthReferenceType { 
            None, 
            WGS84, 
            CasingFlange, 
            CrownValve, 
            DerrickFloor, 
            KellyBushing, 
            RotaryTable, 
            WellHead, 
            GroundLevel, 
            SeaFloor, 
            LowestAstronomicalTide, 
            MeanSeaLevel, 
            MeanHigherHighWater, 
            MeanHighWater,
            MeanLowerLowWater,
            MeanLowWater,
            MeanTideLevel,
            KickOff};
        public enum PositionReferenceType { None, WGS84, WellHead };
        public enum AxialPositionReferenceType { None, BottomShoulder, TopShoulder};
        public enum AnglePositionReferenceType { None, ScribeLine, GravityHighSide, MagneticHighSide, UpperSide };
        public enum AzimuthReferenceType { None, TrueNorth, MagneticNorth, GridNorth };
        public enum PressureReferenceType { None, Absolute, Relative, Gauge };

        public enum VariableAccessType { None, Readable, Assignable };
        public enum MandatoryType
        {
            /// <summary>
            /// it is not mandatory
            /// </summary>
            None = 0,
            /// <summary>
            /// it is always mandatory
            /// </summary>
            General = 65535, // meaning 1111111111111111 in binary
            /// <summary>
            /// it is mandatory in the mechanical perspective
            /// </summary>
            Mechanical = 1,
            /// <summary>
            /// it is mandatory in the hydraulic perspective
            /// </summary>
            Hydraulic = 2,
            /// <summary>
            /// it is mandatory in the heat transfer perspective
            /// </summary>
            HeatTransfer = 4,
            /// <summary>
            /// it is mandatory in the material transport perspective
            /// </summary>
            MaterialTransport = 8,
            /// <summary>
            /// it is mandatory in the pipe handling perspective
            /// </summary>
            PipeHandling = 16,
            /// <summary>
            /// it is mandatory in the geomechanic perspective
            /// </summary>
            Geomechanic = 32,
            /// <summary>
            /// it is mandatory in the directional drilling perspective
            /// </summary>
            Directional = 64,
            /// <summary>
            /// it is mandatory in the formation evaluation perspective
            /// </summary>
            FormationEvaluation = 128,
            /// <summary>
            /// it is mandatory in the well integrity perspective
            /// </summary>
            WellIntegrity = 256
        }

    }
}
