using OSDC.DotnetLibraries.General.DataManagement;
using System.Text.Json.Serialization;
using OSDC.DotnetLibraries.Drilling.SemanticCatalogue;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    /// <summary>
    /// ISCWSA correlation mode. This is one closed classification, not a set of independent flags.
    /// </summary>
    public enum ErrorPropagationMode
    {
        Random,
        Systematic,
        WellByWell,
        Global
    }

    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public sealed class ErrorSource
    {
        // Keys used to map the values passed to the weighting functions
        public enum ParameterType
        {
            Inclination, InclinationPrev, Azimuth, MD, MDPrev, TVD, Magnitude, Declination, Dip, BField, GField,
            Convergence, EarthRotRate, Latitude, h_gyroPrev, c_gyro, DeltaMD, KOperator, CantAngle
        };

        /// <summary>
        /// a MetaInfo for the ErrorSource
        /// </summary>
        public MetaInfo? MetaInfo { get; init; }
        /// <summary>
        /// error code
        /// </summary>
        [Semantic(Concepts.SurveyErrorSourceCode)]
        public ErrorCode ErrorCode { get; init; }
        /// <summary>
        /// a description of the data
        /// </summary>
        public string? Description { get; init; }
        /// <summary
        /// 
        /// </summary>
        [Semantic(Concepts.ErrorSourceOrderingIndex)]
        public int Index { get; init; }
        /// <summary>
        /// Authoritative ISCWSA propagation mode. Null is accepted only for persisted legacy records,
        /// whose effective mode is derived from the deprecated boolean fields below.
        /// </summary>
        [Semantic(Concepts.ErrorPropagationMode)]
        public ErrorPropagationMode? PropagationMode { get; init; }
        /// <summary
        /// 
        /// </summary>
        public bool IsSystematic { get; init; }
        /// <summary
        /// 
        /// </summary>
        public bool IsRandom { get; init; }
        /// <summary
        /// 
        /// </summary>
        public bool IsGlobal { get; init; }
        /// <summary
        /// 
        /// </summary>
        public bool SingularIssues { get; init; }
        /// <summary
        /// 
        /// </summary>
        public bool IsContinuous { get; init; }
        /// <summary
        /// 
        /// </summary>
        public bool IsStationary { get; init; }
        /// <summary
        /// TODO: (rev Gilles 14.11.2025): unsure of the purpose of this parameter
        /// ISCWSA states that kOperator (change from 1 to -1 binary state) is used to switch the cant angle of XY accelerometers when inclination reaches 90°
        /// This parameter seems to be defined to prevent this change, but did not find reference to force this behavior
        /// </summary>
        [Semantic(Concepts.KOperatorImposed)]
        public bool KOperatorImposed { get; init; }
        /// <summary
        /// 
        /// </summary>
        [Semantic(Concepts.SurveyErrorMagnitude)]
        public double? Magnitude { get; init; }
        /// <summary
        /// 
        /// </summary>
        [Semantic(Concepts.ErrorMagnitudeQuantityIdentifier)]
        public string? MagnitudeQuantity { get; init; }
        /// <summary
        /// 
        /// </summary>
        public bool UseInclinationInterval { get; init; }
        /// <summary
        /// 
        /// </summary>
        public double? StartInclination { get; set; }
        /// <summary
        /// 
        /// </summary>
        public double? EndInclination { get; set; }
        /// <summary
        /// 
        /// </summary>
        public double? InitInclination { get; set; }

        /// <summary>
        /// Gets the explicit Revision 5 propagation mode, or derives the unambiguous legacy meaning.
        /// </summary>
        [JsonIgnore]
        public ErrorPropagationMode EffectivePropagationMode => PropagationMode ??
            (IsGlobal ? ErrorPropagationMode.Global :
             IsRandom && !IsSystematic ? ErrorPropagationMode.Random :
             ErrorPropagationMode.Systematic);

        /// <summary>
        /// Calculate the MD component of the weighting function associated to the error source
        /// </summary>
        [JsonIgnore] // delegate functions are irrelevant to OpenAPI schema generation
        public Func<KeyValuePair<ParameterType, double>?[], double?>? WeightingFunctionMD { get; set; }
        /// <summary>
        /// Calculate the inclination component of the weighting function associated to the error source
        /// </summary>
        [JsonIgnore] // delegate functions are irrelevant to OpenAPI schema generation
        public Func<KeyValuePair<ParameterType, double>?[], double?>? WeightingFunctionIncl { get; set; }
        /// <summary>
        /// Calculate the azimuth component of the weighting function associated to the error source
        /// </summary>
        [JsonIgnore] // delegate functions are irrelevant to OpenAPI schema generation
        public Func<KeyValuePair<ParameterType, double>?[], double?>? WeightingFunctionAzim { get; set; }
        /// <summary>
        /// Calculate the North component of the weighting function associated to the error source, in case of a vertical hole
        /// </summary>
        [JsonIgnore] // delegate functions are irrelevant to OpenAPI schema generation
        public Func<KeyValuePair<ParameterType, double>?[], double?>? VerticalHoleWeightingFunctionNorth { get; set; }
        /// <summary>
        /// Calculate the East component of the weighting function associated to the error source, in case of a vertical hole
        /// </summary>
        [JsonIgnore] // delegate functions are irrelevant to OpenAPI schema generation
        public Func<KeyValuePair<ParameterType, double>?[], double?>? VerticalHoleWeightingFunctionEast { get; set; }
        /// <summary>
        /// Calculate the vertical component of the weighting function associated to the error source, in case of a vertical hole
        /// </summary>
        [JsonIgnore] // delegate functions are irrelevant to OpenAPI schema generation
        public Func<KeyValuePair<ParameterType, double>?[], double?>? VerticalHoleWeightingFunctionVertical { get; set; }
        /// <summary>
        /// Calculate the depth component of the weighting function associated to the error source
        /// </summary>
        [JsonIgnore] // delegate functions are irrelevant to OpenAPI schema generation
        public Func<KeyValuePair<ParameterType, double>?[], double?>? WeightingFunctionDepthGyro { get; set; }


        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public ErrorSource() : base()
        {
        }
    }
}
