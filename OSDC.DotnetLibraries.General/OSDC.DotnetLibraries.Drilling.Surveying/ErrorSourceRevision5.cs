namespace OSDC.DotnetLibraries.Drilling.Surveying;

/// <summary>Validation and classification rules from the ISCWSA Error Model, Revision 5.13.</summary>
public static class ErrorSourceRevision5
{
    public static bool IsHistoric(ErrorCode code) => code is
        ErrorCode.AMID or
        // Early OSDC Revision 5 work used simplified, non-ISCWSA names. Keep
        // these enum values readable, but require the exact codes in new models.
        ErrorCode.ABXY_TI1 or ErrorCode.ABXY_TI2 or
        ErrorCode.ASXY_TI1 or ErrorCode.ASXY_TI2 or ErrorCode.ASXY_TI3 or
        ErrorCode.ABIXY_TI1 or ErrorCode.ABIXY_TI2 or
        ErrorCode.ASIXY_TI1 or ErrorCode.ASIXY_TI2 or ErrorCode.ASIXY_TI3 or
        ErrorCode.MBIXY_TI1 or ErrorCode.MBIXY_TI2 or
        ErrorCode.MSIXY_TI1 or ErrorCode.MSIXY_TI2 or ErrorCode.MSIXY_TI3 or
        ErrorCode.DEC or ErrorCode.DBH or ErrorCode.MFI or ErrorCode.MDI;

    public static bool TryValidate(ErrorSource? source, bool requireCurrentCode, out string? error)
    {
        if (source == null)
        {
            error = "Error source is required.";
            return false;
        }
        if (requireCurrentCode && IsHistoric(source.ErrorCode))
        {
            error = $"{source.ErrorCode} is a historic or legacy compatibility code and cannot be used in a new ISCWSA Revision 5 model.";
            return false;
        }
        if (source.Magnitude is double magnitude && (!double.IsFinite(magnitude) || magnitude < 0.0))
        {
            error = "Magnitude must be a finite, nonnegative one-sigma standard uncertainty.";
            return false;
        }
        if (source.PropagationMode == null && !HasUnambiguousLegacyPropagation(source))
        {
            error = "Specify exactly one ISCWSA propagation mode.";
            return false;
        }
        if (source.PropagationMode is ErrorPropagationMode explicitMode && !LegacyFlagsMatch(source, explicitMode))
        {
            error = "Legacy propagation flags contradict the explicit ISCWSA propagation mode.";
            return false;
        }
        if (source.UseInclinationInterval &&
            (source.StartInclination is not double start || source.EndInclination is not double end ||
             !double.IsFinite(start) || !double.IsFinite(end) || start > end))
        {
            error = "An enabled inclination interval requires finite start and end values with start <= end.";
            return false;
        }
        if (source.MagnitudeQuantity is string quantity &&
            ExpectedMagnitudeQuantity(source.ErrorCode) is string expected &&
            !string.Equals(quantity, expected, StringComparison.Ordinal))
        {
            error = $"{source.ErrorCode} requires magnitude quantity {expected}, not {quantity}.";
            return false;
        }
        error = null;
        return true;
    }

    public static string? ExpectedMagnitudeQuantity(ErrorCode code) => code switch
    {
        ErrorCode.DRFR or ErrorCode.DRFS or ErrorCode.XCLL or ErrorCode.XCLI1 or ErrorCode.XCLI2 => "DepthDrilling",
        ErrorCode.DSFS or ErrorCode.XCLH or ErrorCode.XCLA => "ProportionSmall",
        ErrorCode.DSTG or ErrorCode.DSTS => "ReciprocalLengthSurveyInstrumentDrilling",
        ErrorCode.AMIL or ErrorCode.MBXY_TI1 or ErrorCode.MBXY_TI2 or ErrorCode.MBZ or
        ErrorCode.MBIXY_TI1 or ErrorCode.MBIXY_TI2 or ErrorCode.MBIXY_TI1S or ErrorCode.MBIXY_TI2S or
        ErrorCode.MFI or ErrorCode.MFIR or ErrorCode.MFI_U or ErrorCode.MFI_OS or ErrorCode.MFI_OH or ErrorCode.MFI_OI =>
            "EarthMagneticFluxDensity",
        ErrorCode.MSXY_TI1 or ErrorCode.MSXY_TI2 or ErrorCode.MSXY_TI3 or ErrorCode.MSZ or
        ErrorCode.MSIXY_TI1 or ErrorCode.MSIXY_TI2 or ErrorCode.MSIXY_TI3 or
        ErrorCode.ASXY_TI1S or ErrorCode.ASXY_TI2S or ErrorCode.ASXY_TI3S or ErrorCode.ASXY_TI1 or
        ErrorCode.ASXY_TI2 or ErrorCode.ASXY_TI3 or ErrorCode.ASZ or
        ErrorCode.ASIXY_TI1 or ErrorCode.ASIXY_TI2 or ErrorCode.ASIXY_TI3 or
        ErrorCode.ASIXY_TI1S or ErrorCode.ASIXY_TI2S or ErrorCode.ASIXY_TI3S or ErrorCode.ASIZ or
        ErrorCode.MSIXY_TI1S or ErrorCode.MSIXY_TI2S or ErrorCode.MSIXY_TI3S => "ProportionSmall",
        ErrorCode.ABXY_TI1S or ErrorCode.ABXY_TI2S or ErrorCode.ABIXY_TI1S or ErrorCode.ABIXY_TI2S or
        ErrorCode.ABZ or ErrorCode.ABXY_TI1 or ErrorCode.ABXY_TI2 or ErrorCode.ABIXY_TI1 or ErrorCode.ABIXY_TI2 or ErrorCode.ABIZ or
        ErrorCode.AXYZ_XYB or ErrorCode.AXYZ_ZB or ErrorCode.AXY_B or ErrorCode.AXY_GB => "AccelerationDrilling",
        ErrorCode.DBH or ErrorCode.DBH_U or ErrorCode.DBH_OS or ErrorCode.DBH_OH or ErrorCode.DBH_OI or ErrorCode.DBHR =>
            "AngleMagneticFluxDensitySurveyInstrumentDrilling",
        ErrorCode.GXYZ_RW or ErrorCode.GXY_RW or ErrorCode.GZ_RW => "RandomWalkDrilling",
        ErrorCode.GXYZ_XYB1 or ErrorCode.GXYZ_XYB2 or ErrorCode.GXYZ_XYRN or ErrorCode.GXYZ_XYG1 or
        ErrorCode.GXYZ_XYG2 or ErrorCode.GXYZ_XYG3 or ErrorCode.GXYZ_XYG4 or ErrorCode.GXYZ_ZB or
        ErrorCode.GXYZ_ZRN or ErrorCode.GXYZ_ZG1 or ErrorCode.GXYZ_ZG2 or ErrorCode.GXY_B1 or
        ErrorCode.GXY_B2 or ErrorCode.GXY_RN or ErrorCode.GXY_G1 or ErrorCode.GXY_G2 or ErrorCode.GXY_G3 or
        ErrorCode.GXY_G4 or ErrorCode.GXYZ_GD or ErrorCode.GXY_GD or ErrorCode.GZ_GD =>
            "AngularVelocitySurveyInstrumentDrilling",
        ErrorCode.AMID or ErrorCode.XYM1 or ErrorCode.XYM2 or ErrorCode.XYM3 or ErrorCode.XYM4 or
        ErrorCode.XYM3E or ErrorCode.XYM4E or ErrorCode.SAG or ErrorCode.SAGE or ErrorCode.DEC or ErrorCode.DEC_U or
        ErrorCode.DEC_OS or ErrorCode.DEC_OH or ErrorCode.DEC_OI or ErrorCode.DECR or ErrorCode.MDI or ErrorCode.MDIR or
        ErrorCode.MDI_U or ErrorCode.MDI_OS or ErrorCode.MDI_OH or ErrorCode.MDI_OI or
        ErrorCode.AXYZ_MIS or ErrorCode.AXY_MS or ErrorCode.GXYZ_MIS or ErrorCode.GXY_MIS or
        ErrorCode.EXT_REF or ErrorCode.EXT_TIE or ErrorCode.EXT_MIS => "PlaneAngleDrilling",
        _ => null
    };

    private static bool HasUnambiguousLegacyPropagation(ErrorSource source)
    {
        if (source.IsGlobal)
            return !source.IsRandom;
        return source.IsRandom ^ source.IsSystematic;
    }

    private static bool LegacyFlagsMatch(ErrorSource source, ErrorPropagationMode mode)
    {
        if (!source.IsRandom && !source.IsSystematic && !source.IsGlobal)
            return true;

        return mode switch
        {
            ErrorPropagationMode.Random => source.IsRandom && !source.IsSystematic && !source.IsGlobal,
            ErrorPropagationMode.Systematic => !source.IsRandom && source.IsSystematic && !source.IsGlobal,
            ErrorPropagationMode.WellByWell => !source.IsRandom && source.IsSystematic && !source.IsGlobal,
            ErrorPropagationMode.Global => !source.IsRandom && source.IsGlobal,
            _ => false
        };
    }
}
