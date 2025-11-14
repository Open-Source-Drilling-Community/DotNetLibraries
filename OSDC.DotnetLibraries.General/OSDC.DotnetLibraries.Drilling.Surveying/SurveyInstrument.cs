using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.DotnetLibraries.General.Common;
using System;
using System.Collections.Generic;

namespace NORCE.Drilling.SurveyInstrument.Model
{
    public class SurveyInstrument
    {
        public static readonly double DEFAULT_DIP = 72 * Math.PI / 180.0;
        public static readonly double DEFAULT_DECLINATION = -4 * Math.PI / 180.0;
        public static readonly double DEFAULT_BFIELD = 31e-6;
        public static readonly double DEFAULT_GYRO_SWITCHING = 1.0;
        public static readonly double DEFAULT_CANT_ANGLE = 0.0;
        public static readonly double DEFAULT_CONVERGENCE = 0.0;
        public static readonly double DEFAULT_LATITUDE = 0.0;
        public static readonly double DEFAULT_GFIELD = Constants.EarthStandardSurfaceGravitationalAcceleration;
        public static readonly double DEFAULT_EARTH_ROT_RATE = Constants.EarthStandardAngularVelocity;

        /// <summary>
        /// a MetaInfo for the SurveyInstrument
        /// </summary>
        public MetaInfo? MetaInfo { get; set; }

        /// <summary>
        /// name of the data
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// a description of the data
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// the date when the data was created
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset? LastModificationDate { get; set; }

        /// <summary>
        /// the type of SurveyInstrument model
        /// </summary>
        public SurveyInstrumentModelType ModelType { get; set; }

        /// <summary>
        /// an input list of ErrorSource
        /// </summary>
        public List<ErrorSource>? ErrorSourceList { get; set; }

        /// <summary>
        /// Magnetic Dip angle [rad]
        /// Legacy comment: the magnetic dip at the equator is theoretically 0, old value corresponds to Scandinavian regions: 72 * Math.PI / 180.0
        /// </summary>
        public double Dip { get; set; } = DEFAULT_DIP;

        /// <summary>
        /// Declination angle [rad]
        /// Legacy comment: the magnetic declination at the equator is very close to 0, the old value corresponds to Scandinavian regions: -4 * Math.PI / 180.0
        /// </summary>
        public double Declination { get; set; } = DEFAULT_DECLINATION;

        /// <summary>
        /// Earth's Gravity [m/s2]
        /// </summary>
        public double Gravity { get; set; } = DEFAULT_GFIELD;

        /// <summary>
        /// Magnetic Total Field [T]
        /// Legacy comment: the magnetic strength at the equator is around 31-32 microT. The previous value corresponds to Scandinavian regions: 50000e-9
        /// </summary>
        public double BField { get; set; } = DEFAULT_BFIELD;

        /// <summary>
        /// Convergence [rad]
        /// </summary>
        public double Convergence { get; set; } = DEFAULT_CONVERGENCE;

        /// <summary>
        /// Latitude [rad]
        /// Legacy comment: i.e., at the equator
        /// </summary>
        public double Latitude { get; set; } = DEFAULT_LATITUDE;

        /// <summary>
        /// Earthc standard angular velocity
        /// </summary>
        public double EarthRotRate { get; set; } = DEFAULT_EARTH_ROT_RATE;

        /// <summary>
        /// Cant Angle
        /// </summary>
        public double CantAngle { get; set; } = DEFAULT_CANT_ANGLE;

        /// <summary>
        /// Gyro Running Speed
        /// </summary>
        public double? GyroRunningSpeed { get; set; }

        /// <summary>
        /// Gyro External Reference Init inclination
        /// </summary>
        public double? ExtRefInitInc { get; set; }

        /// <summary>
        /// Gyro Switching
        /// </summary>
        public double? GyroSwitching { get; set; } = DEFAULT_GYRO_SWITCHING;

        /// <summary>
        /// Gyro Minimum Distance between initialization
        /// </summary>
        public double? GyroMinDist { get; set; }

        /// <summary>
        /// Gyro Noise Reduction Factor at initialization
        /// </summary>
        public double? GyroNoiseRed { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: indicate whether the Relative Depth Error shall be used or not
        /// </summary>
        public bool UseRelDepthError { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: Relative Depth Error
        /// </summary>
        public double? RelDepthError { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: indicate whether the Misalignment shall be used or not
        /// </summary>
        public bool UseMisalignment { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: Misalignment
        /// </summary>
        public double? Misalignment { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: indicate whether the True Inclination shall be used or not
        /// </summary>
        public bool UseTrueInclination { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: True Inclination
        /// </summary>
        public double? TrueInclination { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: indicate whether the Reference Error shall be used or not
        /// </summary>
        public bool UseReferenceError { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: Reference Error
        /// </summary>
        public double? ReferenceError { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: indicate whether the DrillString Magnetism shall be used or not
        /// </summary>
        public bool UseDrillStringMag { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: DrillString Magnetism
        /// </summary>
        public double? DrillStringMag { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: indicate whether the Gyro Compass Error shall be used or not
        /// </summary>
        public bool UseGyroCompassError { get; set; }

        /// <summary>
        /// WolffDeWardt model specific: Gyro Compass Error
        /// </summary>
        public double? GyroCompassError { get; set; }

        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public SurveyInstrument() : base()
        {
        }
    }
}
