using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.DrillingProperties
{
    public class CommonProperty
    {
        public enum AbscissaReferenceType { None, Top, Bottom};
        public enum DepthReferenceType { None, WGS84, WellHead, GroundLevel, TopOfString, EndOfString };
        public enum PositionReferenceType { None, WGS84, WellHead };
        public enum AzimuthReferenceType { None, TrueNorth };
        public enum MandatoryType
        {
            /// <summary>
            /// it is not mandatory
            /// </summary>
            None = 0,
            /// <summary>
            /// it is always mandatory
            /// </summary>
            General = 1,
            /// <summary>
            /// it is mandatory in the mechanical perspective
            /// </summary>
            Mechanical = 2,
            /// <summary>
            /// it is mandatory in the hydraulic perspective
            /// </summary>
            Hydraulic = 4,
            /// <summary>
            /// it is mandatory in the heat transfer perspective
            /// </summary>
            HeatTransfer = 8,
            /// <summary>
            /// it is mandatory in the material transport perspective
            /// </summary>
            MaterialTransport = 16,
            /// <summary>
            /// it is mandatory in the pipe handling perspective
            /// </summary>
            PipeHandling = 32,
            /// <summary>
            /// it is mandatory in the geomechanic perspective
            /// </summary>
            Geomechanic = 64,
            /// <summary>
            /// it is mandatory in the directional drilling perspective
            /// </summary>
            Directional = 128,
            /// <summary>
            /// it is mandatory in the formation evaluation perspective
            /// </summary>
            FormationEvaluation = 256,
            /// <summary>
            /// it is mandatory in the well integrity perspective
            /// </summary>
            WellIntegrity = 512
        }

    }
}
