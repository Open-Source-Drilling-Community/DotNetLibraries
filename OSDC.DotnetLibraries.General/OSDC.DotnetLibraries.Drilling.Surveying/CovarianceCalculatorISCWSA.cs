using System.Text.Json;
using static OSDC.DotnetLibraries.Drilling.Surveying.ErrorSource;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public static class CovarianceCalculatorISCWSA
    {
        // Default tortuosity for long course length terms (XCL terms in ISCWSA notations)
        public static readonly double DEFAULT_TORTUOSITY = 0.000572615; //[rad/m]

        /// <summary>
        /// Implementation of the ISCWSA error model rev. 5
        ///      - "Error Model Definition Document, updated 20/1/23" https://www.iscwsa.net/error-model-documentation/
        /// </summary>
        /// <param name="surveyStationList"></param>
        /// <param name="surveyStationsIndices"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool Calculate(List<SurveyStation> surveyStationList, List<int>? surveyStationsIndices = null)
        {
            bool ok;
            if (surveyStationList is { } && surveyStationList.Count > 0)
            { 
                ok = SurveyStation.CompleteSurvey(surveyStationList); // make sure that all survey stations member variables are complete
                if (ok) {
                    List<ISCWSAErrorAccumulator> errorSourcesAccumulator = [];
                    // Start from i = 0 to include the first surveystation. This will typically have radius 0
                    // TODO: deal with case i=0
                    // Start from i = 1 to exclude the first surveystation
                    int startIdx = surveyStationsIndices is null ? 0 : surveyStationsIndices[0];
                    int endIdx = surveyStationsIndices is null ? surveyStationList.Count - 1 : surveyStationsIndices[^1];
                    // The error at first survey station is assumed to be 0
                    surveyStationList[startIdx].Covariance = new();
                    // The ISCWSA error accumulator is initialized
                    foreach (var errorSource in surveyStationList[startIdx]!.SurveyTool!.ErrorSourceList!)
                        errorSourcesAccumulator.Add(new());
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            surveyStationList[startIdx].Covariance![j, k] = 0.0;
                        }
                    }
                    for (int i =  1 + startIdx; i <= endIdx; i++)
                    {
                        if (surveyStationList[i].SurveyTool is { } tool &&
                            tool.ErrorSourceList is { } errorSources &&
                            errorSources.Count > 0)
                        {
                            if (i == surveyStationList.Count - 1)
                            {
                                SurveyStation surveyStationNext = new()
                                {
                                    RiemannianNorth = 0.0,
                                    RiemannianEast = 0.0,
                                    Inclination = 0.0,
                                    Azimuth = 0.0,
                                    MD = 0.0
                                };
                                ok = CalculateCovariance(surveyStationList[i], surveyStationList[i - 1], surveyStationNext, errorSourcesAccumulator, i);
                            }
                            else
                            {
                                ok = CalculateCovariance(surveyStationList[i], surveyStationList[i - 1], surveyStationList[i + 1], errorSourcesAccumulator, i);
                            }
                            if (!ok)
                                throw new Exception($"Calling CalculateCovariance() failed at iteration {i}");
                        }
                        //}
                        //SurveyStationList[i].Uncertainty.Calculate(SurveyStationList[i], confidenceFactor, scalingFactor);
                        //surveyList.Add(SurveyStationList[i]);
                        //if (UseUncertaintyCylinder)
                        //{
                        //    CalculateUncertaintyCylinder(SurveyStationList[i], confidenceFactor);
                        //}
                    }
                }
            }
            else
            {
                System.Console.WriteLine($"Problem while calculating covariance matrices");
                return false;
            }
            return ok;
        }

        /// <summary>
        /// Calculate the covariance matrix at the given survey station
        /// From p.19 in "Error Model Definition Document, updated 20/1/23" https://www.iscwsa.net/error-model-documentation/
        /// </summary>
        /// <param name="surveyStation">the current survey station</param>
        /// <param name="surveyStationPrev">the previous survey station</param>
        /// <param name="surveyStationNext"></param>
        /// <param name="errorSourcesAccumulator"
        /// <param name="stationIdx"></param>
        /// <returns></returns>
        public static bool CalculateCovariance(
            SurveyStation surveyStation, SurveyStation surveyStationPrev, SurveyStation surveyStationNext,
            List<ISCWSAErrorAccumulator> errorSourcesAccumulator, int stationIdx)
        {
            try
            {
                double[,] drdp = new double[3, 3];
                if (stationIdx > 0)
                    drdp = CalculateDisplacementErrorMatrix(surveyStation, surveyStationPrev, stationIdx);
                double[,] drdpNext = new double[3, 3];
                drdpNext = CalculateDisplacementErrorMatrixNext(surveyStation, surveyStationNext, stationIdx);
                // initializing the covariance matrix
                surveyStation.Covariance = new();
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        surveyStation.Covariance[j, k] = 0.0;
                    }
                }
                return CalculateAllCovariance(surveyStation, surveyStationPrev, surveyStationNext, drdp, drdpNext, stationIdx, errorSourcesAccumulator);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Problem while calculating covariance matrix for survey station {stationIdx}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Effect of the errors in the survey measurements at station k, on the position vector from survey station k-1 to survey station k
        /// From p.19 in "Error Model Definition Document, updated 20/1/23" https://www.iscwsa.net/error-model-documentation/
        /// </summary>
        /// <param name="station_k"></param>
        /// <param name="station_kprev"></param>
        /// <returns></returns>
        public static double[,] CalculateDisplacementErrorMatrix(SurveyStation station_k, SurveyStation station_kprev, int k)
        {
            // drk = the displacement between survey station k-1 and k
            double[] drk_dDepth = new double[3];
            double[] drk_dInc = new double[3];
            double[] drk_dAz = new double[3];
            double[,] A = new double[3, 3];
            if (station_k != null && station_k.SurveyTool != null && station_kprev != null && A != null)
            {
                double sinI = System.Math.Sin((double)station_k.Inclination!);
                double cosI = System.Math.Cos((double)station_k.Inclination!);
                double sinA = System.Math.Sin((double)station_k.Azimuth!);
                double cosA = System.Math.Cos((double)station_k.Azimuth!);
                double sinIp = System.Math.Sin((double)station_kprev.Inclination!);
                double cosIp = System.Math.Cos((double)station_kprev.Inclination!);
                double sinAp = System.Math.Sin((double)station_kprev.Azimuth!);
                double cosAp = System.Math.Cos((double)station_kprev.Azimuth!);
                double deltaSk = (double)station_k.MD! - (double)station_kprev.MD!;
                drk_dDepth[0] = 0.5 * (sinIp * cosAp + sinI * cosA);
                drk_dDepth[1] = 0.5 * (sinIp * sinAp + sinI * sinA);
                drk_dDepth[2] = 0.5 * (cosIp + cosI);
                // rev Gilles (03.11.2025): dont see any reason why first survey station derivative would be counted twice for incl and az
                //if (k == 1)
                //{
                //    //NB Check why multiplied by 2
                //    drk_dInc[0] = 0.5 * deltaSk * cosI * cosA * 2;
                //    drk_dInc[1] = 0.5 * deltaSk * cosI * sinA * 2;
                //    drk_dInc[2] = -0.5 * deltaSk * sinI * 2;
                //    drk_dAz[0] = -0.5 * deltaSk * sinI * sinA * 2;
                //    drk_dAz[1] = 0.5 * deltaSk * sinI * cosA * 2;
                //}
                //else
                //{
                drk_dInc[0] = 0.5 * deltaSk * cosI * cosA;
                drk_dInc[1] = 0.5 * deltaSk * cosI * sinA;
                drk_dInc[2] = -0.5 * deltaSk * sinI;
                drk_dAz[0] = -0.5 * deltaSk * sinI * sinA;
                drk_dAz[1] = 0.5 * deltaSk * sinI * cosA;
                //}
                drk_dAz[2] = 0.0;  // =0
                A[0, 0] = drk_dDepth[0];
                A[1, 0] = drk_dDepth[1];
                A[2, 0] = drk_dDepth[2];
                A[0, 1] = drk_dInc[0];
                A[1, 1] = drk_dInc[1];
                A[2, 1] = drk_dInc[2];
                A[0, 2] = drk_dAz[0];
                A[1, 2] = drk_dAz[1];
                A[2, 2] = drk_dAz[2];
            }
            return A!;
        }

        /// <summary>
        /// Effect of the errors in the survey measurements at station k, on the position vector from survey station k to survey station k+1
        /// From p.19 in "Error Model Definition Document, updated 20/1/23" https://www.iscwsa.net/error-model-documentation/
        /// TODO: check why the sign change only applies to the derivative with respect to Depth and not to Incl and Azim.
        /// </summary>
        /// <param name="station_k"></param>
        /// <param name="station_knext"></param>
        /// <returns></returns>
        public static double[,] CalculateDisplacementErrorMatrixNext(SurveyStation station_k, SurveyStation station_knext, int k)
        {
            // drk = the displacement between survey station k and k+1
            double[] drk_dDepth = new double[3];
            double[] drk_dInc = new double[3];
            double[] drk_dAz = new double[3];
            double[,] A = new double[3, 3];
            if (station_k != null && station_k.SurveyTool != null && station_knext != null && A != null)
            {
                double sinI = System.Math.Sin((double)station_k.Inclination!);
                double cosI = System.Math.Cos((double)station_k.Inclination!);
                double sinA = System.Math.Sin((double)station_k.Azimuth!);
                double cosA = System.Math.Cos((double)station_k.Azimuth!);
                double sinIn = System.Math.Sin((double)station_knext.Inclination!);
                double cosIn = System.Math.Cos((double)station_knext.Inclination!);
                double sinAn = System.Math.Sin((double)station_knext.Azimuth!);
                double cosAn = System.Math.Cos((double)station_knext.Azimuth!);
                double deltaSk = (double)station_knext.MD! - (double)station_k.MD!;
                drk_dDepth[0] = -0.5 * (sinI * cosA + sinIn * cosAn);
                drk_dDepth[1] = -0.5 * (sinI * sinA + sinIn * sinAn);
                // rev Gilles (03.11.2025): dont understand the exception on k==0 on the Depth and Incl derivatives (strangely not Azim)
                //if (k == 0)
                //{
                //    drk_dDepth[2] = 0.0;
                //}
                //else
                //{
                drk_dDepth[2] = -0.5 * (cosI + cosIn);  // 
                                                        //}
                                                        //if (k == 0)
                                                        //{
                                                        //    drk_dInc[0] = 0.0;
                                                        //}
                                                        //else
                                                        //{
                drk_dInc[0] = 0.5 * deltaSk * cosI * cosA;
                //}
                drk_dInc[1] = 0.5 * deltaSk * cosI * sinA;
                drk_dInc[2] = -0.5 * deltaSk * sinI;
                drk_dAz[0] = -0.5 * deltaSk * sinI * sinA;
                drk_dAz[1] = 0.5 * deltaSk * sinI * cosA;
                drk_dAz[2] = 0.0;  // =0
                A[0, 0] = drk_dDepth[0];
                A[1, 0] = drk_dDepth[1];
                A[2, 0] = drk_dDepth[2];
                A[0, 1] = drk_dInc[0];
                A[1, 1] = drk_dInc[1];
                A[2, 1] = drk_dInc[2];
                A[0, 2] = drk_dAz[0];
                A[1, 2] = drk_dAz[1];
                A[2, 2] = drk_dAz[2];
            }
            return A!;
        }

        public static ErrorSource? ErrorSourceConverter(object eSourceJsonString)
        {
            try
            {
                if (eSourceJsonString is string s)
                {
                    var er = JsonSerializer.Deserialize<ErrorSource>(s);
                    //ErrorSourceASXY_TI1S err = JsonSerializer.Deserialize<ErrorSourceASXY_TI1S>(s);
                    //ErrorSource errorSource = err;
                    return er;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        private static List<ErrorSource> ConvertToErrorSource(List<object> eSources)
        {
            //         List<ErrorSource> errorSources = new List<ErrorSource>();
            //         for(int i=0;i<errorSources.Count;i++)
            //{
            //             double m = eSources.ConvertAll(Converter<object ErrorSources> converter)
            //}

            List<ErrorSource> lp = eSources.ConvertAll(
            new Converter<object, ErrorSource>(ErrorSourceConverter));

            return lp;


        }

        /// <summary>
        /// Calculate Total Covariance matrix for all error sources for a survey station
        /// From p.19 in "Error Model Definition Document, updated 20/1/23" https://www.iscwsa.net/error-model-documentation/
        /// </summary>
        /// <param name="surveyStation"></param>
        /// <param name="nextStation"></param>
        /// <returns></returns>
        public static bool CalculateAllCovariance(
            SurveyStation surveyStation, SurveyStation surveyStationPrev, SurveyStation surveyStationNext,
            double[,] drdp, double[,] drdpNext, int c, List<ISCWSAErrorAccumulator> errorSourcesAccumulator)
        {
            if (surveyStation is { } &&
                surveyStation.SurveyTool is { } surveyTool &&
                surveyTool.ErrorSourceList is List<ErrorSource> errorSources &&
                errorSources.Count > 0 &&
                surveyStation.MD is double MD &&
                surveyStation.TVD is double TVD &&
                surveyStation.Inclination is double inclination &&
                surveyStation.Azimuth is double azimuth &&
                surveyStationPrev is { } &&
                surveyStationPrev.MD is double MDPrev &&
                surveyStationPrev.Inclination is double inclinationPrev
                )
            {
                double[,] covarianceSum = new double[3, 3];
                double[,] sigmaerandom = new double[3, 3];
                bool allSystematic = false;
                for (int e = 0; e < errorSourcesAccumulator.Count; e++)
                {
                    if (errorSourcesAccumulator[e].IsInitialized)
                    {
                        allSystematic = true;
                    }
                }
                for (int i = 0; i < errorSources.Count; i++)
                {
                    ErrorSource eSource = errorSources[i];
                    if (eSource is { })
                    {
                        /////////////////////////////
                        // Rev Gilles 11.11.2025): //
                        /////////////////////////////
                        //      - below is commented legacy code 
                        //      - geomagnetic parameters are now carried by the SurveyInstrument
                        //      - and not associated with the ErrorSource anymore
                        //#region Set GeoMagnetic data
                        //double latitude = Numeric.UNDEF_DOUBLE;
                        //double longitude = Numeric.UNDEF_DOUBLE;
                        //double radius = Numeric.UNDEF_DOUBLE;
                        //GeodeticDatum geodeticDatum = new GeodeticDatum();
                        //geodeticDatum.Spheroid = Spheroid.WGS84;
                        //geodeticDatum.DeltaX = 0.0;
                        //geodeticDatum.DeltaY = 0.0;
                        //geodeticDatum.DeltaZ = 0.0;
                        //geodeticDatum.RotationX = 0.0;
                        //geodeticDatum.RotationY = 0.0;
                        //geodeticDatum.RotationZ = 0.0;
                        //geodeticDatum.ScaleFactor = 1;

                        //geodeticDatum.ToGeocentric(
                        //    (double)surveyStation.Latitude, (double)surveyStation.Longitude, (double)surveyStation.TVD,
                        //    out latitude, out longitude, out radius);
                        //DateTime date = DateTime.Now;
                        //double declination = Numeric.UNDEF_DOUBLE;
                        //double dip = Numeric.UNDEF_DOUBLE;
                        //double hStrength = Numeric.UNDEF_DOUBLE;
                        //double tStrength = Numeric.UNDEF_DOUBLE;
                        //GeoMagneticFieldModel igrf = GeoMagneticFieldModel.IGRF;
                        //igrf.GeoMagnetism(date, latitude, longitude, radius,
                        //                 out declination, out dip,
                        //                 out hStrength, out tStrength);
                        //if (Numeric.IsDefined(dip))
                        //{
                        //    if (eSource is NORCE.Drilling.SurveyInstrument.Model.ErrorSourceXYM2)
                        //    {
                        //    }
                        //    eSource.Dip = dip;
                        //}
                        //if (Numeric.IsDefined(declination))
                        //{
                        //    eSource.Declination = declination;
                        //}
                        //if (Numeric.IsDefined(tStrength))
                        //{
                        //    eSource.BField = tStrength;
                        //}
                        //#endregion
                        bool isInitialized = errorSourcesAccumulator[i].IsInitialized;
                        sigmaerandom = errorSourcesAccumulator[i].SigmaErrorRandom!;
                        bool singular = false;

                        ////////////////////////
                        /// dpde calculation ///
                        ////////////////////////

                        /// dpde represents the normalized effect of the ith error source (epsilon in ISCWSA model notations) on the survey measurement vector p
                        double[] dpde = new double[3];

                        #region Setting the depth component of the dpde term
                        var args = new KeyValuePair<ParameterType, double>?[]
                        {
                            new KeyValuePair<ParameterType, double>(ParameterType.MD, MD),
                            new KeyValuePair<ParameterType, double>(ParameterType.TVD, TVD),
                        };
                        double? depth = eSource?.WeightingFunctionMD?.Invoke(args);
                        if (false && eSource.ErrorCode is ErrorCode.DSFS)
                        {
                            args =
                            [
                                new KeyValuePair<ParameterType, double>(ParameterType.MD, MD),
                                new KeyValuePair<ParameterType, double>(ParameterType.TVD, TVD),
                                new KeyValuePair<ParameterType, double>(ParameterType.MDPrev, MDPrev),
                                new KeyValuePair<ParameterType, double>(ParameterType.Inclination, inclination),
                            ];
                            depth = eSource?.WeightingFunctionDepthGyro?.Invoke(args);
                        }
                        if (false && eSource.ErrorCode is ErrorCode.DSTG)
                        {
                            args =
                            [
                                new KeyValuePair<ParameterType, double>(ParameterType.MD, MD),
                                new KeyValuePair<ParameterType, double>(ParameterType.TVD, TVD),
                                new KeyValuePair<ParameterType, double>(ParameterType.MDPrev, MDPrev),
                                new KeyValuePair<ParameterType, double>(ParameterType.Inclination, inclination),
                            ];
                            depth = eSource?.WeightingFunctionDepthGyro?.Invoke(args);
                        }
                        if (depth is double d)
                        {
                            dpde[0] = d;
                        }
                        else
                        {
                            throw new Exception($"Error while computing depth component of the weighting function for errorSource {eSource!.ErrorCode}");
                        }
                        #endregion

                        #region Setting the inclination component of the dpde term
                        args =
                            [
                                new KeyValuePair<ParameterType, double>(ParameterType.Inclination, inclination),
                                new KeyValuePair<ParameterType, double>(ParameterType.Azimuth, azimuth),
                            ];
                        double? incl = eSource?.WeightingFunctionIncl?.Invoke(args);
                        if (incl is double inc)
                        {
                            dpde[1] = inc;
                        }
                        else
                        {
                            throw new Exception($"Error while computing inclination component of the weighting function for errorSource {eSource!.ErrorCode}");
                        }
                        #endregion

                        #region Setting the azimuth component of the dpde term
                        double wf_azim = 0.0;
                        double initializationMD = errorSourcesAccumulator[i].InitializationMD;
                        double minDistance = 9999;
                        if (surveyStation.SurveyTool.GyroMinDist != null)
                            minDistance = (double)surveyStation.SurveyTool.GyroMinDist;// 99999.0; //Minimum distance between initializations. 
                        // Evaluates whether some error sources need to be re-initialize: TODO inactive code to activate
                        bool toReInitialize = ToReInitialize(inclination, inclinationPrev, errorSources, MD - errorSourcesAccumulator[i].InitializationMD, minDistance);

                        #region Case: continuous gyro
                        if (eSource!.IsContinuous)
                        {
                            double deltaD = MD - MDPrev;
                            double c_gyro = 0.6;
                            if (surveyStation.SurveyTool.GyroRunningSpeed != null)
                            {
                                c_gyro = (double)surveyStation.SurveyTool.GyroRunningSpeed; //Running speed. 
                            }
                            double h = errorSourcesAccumulator[i].GyroH;

                            // Case: continuous gyro is not initialized or has been initialized for too long (not sure I understand why a continuous hyro initialization status is relevant)
                            if ((!isInitialized && inclination >= eSource.StartInclination && inclination <= eSource.EndInclination) ||
                                (isInitialized && (MD - errorSourcesAccumulator[i].InitializationMD) > minDistance)) //NB! include initialization inclination code
                            {
                                isInitialized = true;
                                errorSourcesAccumulator[i].GyroH = 0.0;
                                initializationMD = MD;
                                h = errorSourcesAccumulator[i].GyroH;
                            }
                            else if (isInitialized && (inclination < eSource.InitInclination)) //NB! include initialization inclination code
                            {
                                ////isInitialized = false;            //New
                                //ISCWSAErrorDataTmp[i].GyroH = 0.0;
                                //initializationDepth = surveyStation.MD;
                                //h = ISCWSAErrorDataTmp[i].GyroH;
                            }
                            else
                            {
                                h = errorSourcesAccumulator[i].GyroH;
                                if (eSource.ErrorCode is ErrorCode.GXYZ_GD || eSource.ErrorCode is ErrorCode.GXYZ_RW)
                                {
                                    args =
                                    [
                                        new KeyValuePair<ParameterType, double>(ParameterType.Inclination, inclination),
                                        new KeyValuePair<ParameterType, double>(ParameterType.h_gyroPrev, h),
                                        new KeyValuePair<ParameterType, double>(ParameterType.DeltaMD, deltaD),
                                        new KeyValuePair<ParameterType, double>(ParameterType.c_gyro, c_gyro),
                                    ];
                                    if (eSource.WeightingFunctionAzim?.Invoke(args) is { } h_tmp)
                                    {
                                        h = h_tmp;
                                    }
                                    else
                                    {
                                        throw new Exception($"Error while computing azimuth component of the weighting function for ErrorCode {eSource.ErrorCode}");
                                    }
                                }
                                if (eSource.ErrorCode is ErrorCode.GXY_GD || eSource.ErrorCode is ErrorCode.GXY_RW ||
                                    eSource.ErrorCode is ErrorCode.GZ_GD || eSource.ErrorCode is ErrorCode.GZ_RW)
                                {
                                    args =
                                    [
                                        new KeyValuePair<ParameterType, double>(ParameterType.Inclination, inclination),
                                        new KeyValuePair<ParameterType, double>(ParameterType.h_gyroPrev, h),
                                        new KeyValuePair<ParameterType, double>(ParameterType.DeltaMD, deltaD),
                                        new KeyValuePair<ParameterType, double>(ParameterType.c_gyro, c_gyro),
                                        new KeyValuePair<ParameterType, double>(ParameterType.InclinationPrev, inclinationPrev),
                                    ];
                                    if (eSource.WeightingFunctionAzim?.Invoke(args) is { } h_tmp)
                                    {
                                        h = h_tmp;
                                    }
                                    else
                                    {
                                        throw new Exception($"Error while computing azimuth component of the weighting function for ErrorCode {eSource.ErrorCode}");
                                    }
                                    if (h == 0 && errorSourcesAccumulator[i].GyroH != 0)
                                    {
                                        h = errorSourcesAccumulator[i].GyroH;
                                    }
                                }
                            }
                            wf_azim = h; // finalized later
                            errorSourcesAccumulator[i].IsInitialized = isInitialized; // inform accumulator about initialization status
                        }
                        #endregion

                        bool continuousMode = IsContinuousMode(errorSources, inclination, errorSourcesAccumulator);

                        #region Case: stationary gyro
                        // Case: stationary gyro has been initialized for too long
                        if (IsStationary(eSource) && isInitialized &&
                            (MD - errorSourcesAccumulator[i].InitializationMD) > minDistance &&
                            eSource.InitInclination is double initIncl)
                        {
                            // weighting function is evaluated at initialization inclination
                            args =
                            [
                                new KeyValuePair<ParameterType, double>(ParameterType.Inclination, initIncl),
                                new KeyValuePair<ParameterType, double>(ParameterType.Azimuth, azimuth),
                            ];
                            if (eSource?.WeightingFunctionAzim?.Invoke(args) is double azim_tmp)
                            {
                                wf_azim = azim_tmp;
                                errorSourcesAccumulator[i].GyroH = wf_azim;
                                initializationMD = MD; // gyro re-initialization
                            }
                        }
                        // Case: stationary gyro re-initialization (TODO rev Gilles 13.11.2025: not sure I understand all conditions, especially referring back to continuous mode here...)
                        if (IsStationary(eSource!) &&
                            (isInitialized || (!isInitialized && inclination > eSource!.EndInclination) || continuousMode) &&
                            eSource!.InitInclination is double initIncl2)
                        {
                            if (false && toReInitialize) //TODO: code not reached
                            {
                                // weighting function is evaluated at initialization inclination
                                args =
                                [
                                    new KeyValuePair<ParameterType, double>(ParameterType.Inclination, initIncl),
                                    new KeyValuePair<ParameterType, double>(ParameterType.Azimuth, azimuth),
                                ];
                                if (eSource?.WeightingFunctionAzim?.Invoke(args) is double azim_tmp)
                                {
                                    wf_azim = azim_tmp;
                                    initializationMD = MD; // gyro re-initialization
                                }
                            }
                            else
                            {
                                wf_azim = errorSourcesAccumulator[i].GyroH;
                            }
                            if (!isInitialized &&
                                eSource.InitInclination is double initIncl3 &&
                                (inclination < initIncl3 || inclination > eSource.EndInclination || initIncl3 < 0))
                            {
                                //azim = eSource.FunctionAz(inclination, azimuth); //Azimuth NB! Unsure
                                //azim = ISCWSAErrorDataTmp[i].GyroH + (azim- ISCWSAErrorDataTmp[i].GyroH)/2;
                                if (initIncl3 > 0)
                                {
                                    // weighting function is evaluated at initialization inclination
                                    args =
                                    [
                                        new KeyValuePair<ParameterType, double>(ParameterType.Inclination, initIncl3),
                                        new KeyValuePair<ParameterType, double>(ParameterType.Azimuth, azimuth),
                                    ];
                                    if (eSource?.WeightingFunctionAzim?.Invoke(args) is double azim_tmp)
                                        wf_azim = azim_tmp;
                                }
                                if ((eSource!.ErrorCode is ErrorCode.GXY_RN || eSource.ErrorCode is ErrorCode.GXYZ_XYRN) &&
                                    surveyStation.SurveyTool.GyroNoiseRed is double noiseRedFactor)
                                {
                                    wf_azim = noiseRedFactor * wf_azim;// noiseredFactor * ISCWSAErrorDataTmp[i].GyroH;
                                }
                                // gyro re-initialization
                                initializationMD = MD;
                            }
                            isInitialized = true;
                            errorSourcesAccumulator[i].IsInitialized = true;
                        }
                        errorSourcesAccumulator[i].IsInitialized = isInitialized;//New
                        #endregion

                        // Finalize
                        errorSourcesAccumulator[i].GyroH = wf_azim;
                        errorSourcesAccumulator[i].InitializationMD = initializationMD;
                        dpde[2] = wf_azim;

                        #endregion

                        /////////////////////////////
                        /// Setting the magnitude ///
                        /////////////////////////////
                        /// magnitude symbol is sigma_i on p.17 in "Error Model Definition Document, updated 20/1/23" https://www.iscwsa.net/error-model-documentation/
                        /// sigma*dpde represents the size of the effect of the ith error source (epsilon in ISCWSA model notations) on the survey measurement vector p
                        double magnitude = eSource!.Magnitude ?? 0.0;
                        if (eSource.SingularIssues && (depth == null || incl == null)) // (rev Gilles 13.11.2025: removed following irrelevant condition, double-check) || azim == null
                        {
                            singular = true;
                        }

                        ////////////////////////////////
                        /// Assembling error vectors ///
                        /// ////////////////////////////
                        // Goal: compute the error at all intermediate survey stations (notation e) and at the last survey station (e* notation)
                        double[] e = new double[3]; //the error due to the ith error source at the kth survey station in the lth survey leg
                        double[] eStar = new double[3]; //the error due to the ith error source at the kth survey stations in the lth survey leg, where k is the last survey of interest
                        // Error at first survey station is assumed to be zero
                        if (c == 0)
                        {
                            e[0] = 0;
                            e[1] = 0;
                            e[2] = 0;
                            eStar[0] = 0;
                            eStar[1] = 0;
                            eStar[2] = 0;
                        }
                        else
                        {
                            if (eSource.ErrorCode is ErrorCode.XCLA && surveyStationPrev.Azimuth is double azimPrev && surveyStationPrev.Inclination is double inclPrev)
                            {
                                double azT = azimuth + surveyTool.Convergence;
                                double azTPrev = azimPrev + surveyTool.Convergence;
                                double mod = (azT - azTPrev + Math.PI) % (2 * Math.PI);
                                double val1 = mod - Math.PI;
                                double val2 = 0;
                                if (inclPrev >= 0.0001 * Math.PI / 180.0)
                                {
                                    val2 = val1;
                                }
                                double val3 = Math.Abs(Math.Sin(inclination) * val2);
                                double defaultTortuosity = 0.000572615; //[rad/m]
                                double val4 = Math.Max(val3, defaultTortuosity * (MD - MDPrev));
                                double val5 = magnitude * (MD - MDPrev) * val4;
                                e[0] = val5 * (-Math.Sin(azT));
                                e[1] = val5 * (Math.Cos(azT));
                                e[2] = 0.0;
                                eStar[0] = e[0];
                                eStar[1] = e[1];
                                eStar[2] = e[2];
                            }
                            else if (eSource.ErrorCode is ErrorCode.XCLH && surveyStationPrev.Azimuth is double azimPrev2 && surveyStationPrev.Inclination is double inclPrev2)
                            {
                                double azT = azimuth + surveyTool.Convergence;
                                double azTPrev = azimPrev2 + surveyTool.Convergence;
                                //=Model!$W$37*(Wellpath!$K4-Wellpath!$K3)*MAX(ABS(Wellpath!$L4-Wellpath!$L3);Model!$B$24*(Wellpath!$K4-Wellpath!$K3))*COS(Wellpath!$L4)*COS(Wellpath!$M4)
                                double mod = (azT - azTPrev + Math.PI) % (2 * Math.PI);
                                double val1 = mod - Math.PI;
                                double val2;
                                if (inclPrev2 >= 0.0001 * Math.PI / 180.0)
                                    val2 = val1; // TODO: unnecessary assignent? Are we missing something? see case above
                                double val3 = Math.Abs(inclination - inclinationPrev);
                                double defaultTortuosity = 0.000572615; //[rad/m]
                                double val4 = Math.Max(val3, defaultTortuosity * (MD - MDPrev));
                                double val5 = magnitude * (MD - MDPrev) * val4;
                                e[0] = val5 * Math.Cos(inclination) * Math.Cos(azT);
                                e[1] = val5 * Math.Cos(inclination) * Math.Sin(azT);
                                e[2] = val5 * (-Math.Sin(inclination));
                                eStar[0] = e[0];
                                eStar[1] = e[1];
                                eStar[2] = e[2];
                            }
                            else if (eSource.SingularIssues && singular && surveyStationNext.MD is double md)
                            {
                                if (c == 1)
                                {
                                    args =
                                        [
                                        new KeyValuePair<ParameterType, double>(ParameterType.Azimuth, azimuth)
                                        ];
                                    if (eSource?.VerticalHoleWeightingFunctionNorth?.Invoke(args) is double wfNorth)
                                    {
                                        e[0] = magnitude * (md + MD - 2 * MDPrev) / 2 * wfNorth;
                                        eStar[0] = magnitude * (MD - MDPrev) * wfNorth;

                                    }
                                    else
                                    {
                                        throw new Exception($"Error while computing singularity North component of the weighting function for errorSource {eSource!.ErrorCode}");
                                    }
                                    args =
                                        [
                                        new KeyValuePair<ParameterType, double>(ParameterType.Azimuth, azimuth)
                                        ];
                                    if (eSource?.VerticalHoleWeightingFunctionEast?.Invoke(args) is double wfEast)
                                    {
                                        e[1] = magnitude * (md + MD - 2 * MDPrev) / 2 * wfEast;
                                        eStar[1] = magnitude * (MD - MDPrev) * wfEast;
                                    }
                                    else
                                    {
                                        throw new Exception($"Error while computing singularity East component of the weighting function for errorSource {eSource!.ErrorCode}");
                                    }
                                    e[2] = 0.0;
                                    eStar[2] = 0.0;
                                }
                                else
                                {
                                    args =
                                        [
                                        new KeyValuePair<ParameterType, double>(ParameterType.Azimuth, azimuth)
                                        ];
                                    if (eSource?.VerticalHoleWeightingFunctionNorth?.Invoke(args) is double wfNorth)
                                    {
                                        e[0] = magnitude * (md - MDPrev) / 2 * wfNorth;
                                        eStar[0] = magnitude * (MD - MDPrev) / 2 * wfNorth;

                                    }
                                    else
                                    {
                                        throw new Exception($"Error while computing singularity North component of the weighting function for errorSource {eSource!.ErrorCode}");
                                    }
                                    args =
                                        [
                                        new KeyValuePair<ParameterType, double>(ParameterType.Azimuth, azimuth)
                                        ];
                                    if (eSource?.VerticalHoleWeightingFunctionEast?.Invoke(args) is double wfEast)
                                    {
                                        e[1] = magnitude * (md - MDPrev) / 2 * wfEast;
                                        eStar[1] = magnitude * (MD - MDPrev) / 2 * wfEast;
                                    }
                                    else
                                    {
                                        throw new Exception($"Error while computing singularity East component of the weighting function for errorSource {eSource!.ErrorCode}");
                                    }
                                    e[2] = 0.0;
                                    eStar[2] = 0.0;
                                }
                            }
                            else
                            {
                                e[0] = magnitude * ((drdp[0, 0] + drdpNext[0, 0]) * dpde[0] + (drdp[0, 1] + drdpNext[0, 1]) * dpde[1] + (drdp[0, 2] + drdpNext[0, 2]) * dpde[2]);
                                e[1] = magnitude * ((drdp[1, 0] + drdpNext[1, 0]) * dpde[0] + (drdp[1, 1] + drdpNext[1, 1]) * dpde[1] + (drdp[1, 2] + drdpNext[1, 2]) * dpde[2]);
                                e[2] = magnitude * ((drdp[2, 0] + drdpNext[2, 0]) * dpde[0] + (drdp[2, 1] + drdpNext[2, 1]) * dpde[1] + (drdp[2, 2] + drdpNext[2, 2]) * dpde[2]);
                                eStar[0] = magnitude * ((drdp[0, 0]) * dpde[0] + (drdp[0, 1]) * dpde[1] + (drdp[0, 2]) * dpde[2]);
                                eStar[1] = magnitude * ((drdp[1, 0]) * dpde[0] + (drdp[1, 1]) * dpde[1] + (drdp[1, 2]) * dpde[2]);
                                eStar[2] = magnitude * ((drdp[2, 0]) * dpde[0] + (drdp[2, 1]) * dpde[1] + (drdp[2, 2]) * dpde[2]);
                            }
                        }

                        ////////////////////////////////////////
                        /// Assembling the covariance matrix ///
                        ////////////////////////////////////////
                        double[,] CovarianceI = new double[3, 3];
                        if (eSource.IsRandom && !allSystematic)
                        {
                            if (c == 0)
                            {
                                CovarianceI[0, 0] = eStar[0] * eStar[0];
                                CovarianceI[1, 1] = eStar[1] * eStar[1];
                                CovarianceI[2, 2] = eStar[2] * eStar[2];
                                CovarianceI[1, 0] = eStar[0] * eStar[1];
                                CovarianceI[0, 1] = CovarianceI[1, 0];
                                CovarianceI[2, 0] = eStar[0] * eStar[2];
                                CovarianceI[0, 2] = CovarianceI[2, 0];
                                CovarianceI[1, 2] = eStar[1] * eStar[2];
                                CovarianceI[2, 1] = CovarianceI[2, 1];
                            }
                            else
                            {
                                CovarianceI[0, 0] = sigmaerandom[0, 0] + eStar[0] * eStar[0];
                                CovarianceI[1, 1] = sigmaerandom[1, 1] + eStar[1] * eStar[1];
                                CovarianceI[2, 2] = sigmaerandom[2, 2] + eStar[2] * eStar[2];
                                CovarianceI[1, 0] = eStar[0] * eStar[1] + sigmaerandom[1, 0];
                                CovarianceI[0, 1] = CovarianceI[1, 0];
                                CovarianceI[2, 0] = eStar[0] * eStar[2] + sigmaerandom[2, 0];
                                CovarianceI[0, 2] = CovarianceI[2, 0];
                                CovarianceI[1, 2] = eStar[1] * eStar[2] + sigmaerandom[1, 2];
                                CovarianceI[2, 1] = CovarianceI[1, 2];
                            }
                            // error accumulation
                            sigmaerandom[0, 0] = e[0] * e[0] + sigmaerandom[0, 0];
                            sigmaerandom[1, 1] = e[1] * e[1] + sigmaerandom[1, 1];
                            sigmaerandom[2, 2] = e[2] * e[2] + sigmaerandom[2, 2];
                            sigmaerandom[1, 0] = e[0] * e[1] + sigmaerandom[1, 0];
                            sigmaerandom[0, 1] = sigmaerandom[1, 0];
                            sigmaerandom[2, 0] = e[0] * e[2] + sigmaerandom[2, 0];
                            sigmaerandom[0, 2] = sigmaerandom[2, 0];
                            sigmaerandom[1, 2] = e[1] * e[2] + sigmaerandom[1, 2];
                            sigmaerandom[2, 1] = sigmaerandom[1, 2];
                            // inform the error accumulation container
                            errorSourcesAccumulator[i].SigmaErrorRandom = sigmaerandom;
                        }
                        else
                        {
                            if (errorSourcesAccumulator[i] is { } esAcc &&
                                esAcc.ErrorSum is double[] es &&
                                es[0] is double es0 &&
                                es[1] is double es1 &&
                                es[2] is double es2)
                            {

                                double[] sigmaesystematic = new double[3];
                                sigmaesystematic[0] = es0 + eStar[0];
                                sigmaesystematic[1] = es1 + eStar[1];
                                sigmaesystematic[2] = es2 + eStar[2];
                                if (c == 0)
                                {
                                    CovarianceI[0, 0] = eStar[0] * eStar[0];
                                    CovarianceI[1, 1] = eStar[1] * eStar[1];
                                    CovarianceI[2, 2] = eStar[2] * eStar[2];
                                    CovarianceI[1, 0] = eStar[0] * eStar[1];
                                    CovarianceI[0, 1] = CovarianceI[1, 0];
                                    CovarianceI[2, 0] = eStar[0] * eStar[2];
                                    CovarianceI[0, 2] = CovarianceI[2, 0];
                                    CovarianceI[1, 2] = eStar[1] * eStar[2];
                                    CovarianceI[2, 1] = CovarianceI[2, 1];
                                }
                                else
                                {
                                    CovarianceI[0, 0] = sigmaesystematic[0] * sigmaesystematic[0];
                                    CovarianceI[1, 1] = sigmaesystematic[1] * sigmaesystematic[1];
                                    CovarianceI[2, 2] = sigmaesystematic[2] * sigmaesystematic[2];
                                    CovarianceI[1, 0] = sigmaesystematic[1] * sigmaesystematic[0];
                                    CovarianceI[0, 1] = CovarianceI[1, 0];
                                    CovarianceI[2, 0] = sigmaesystematic[2] * sigmaesystematic[0];
                                    CovarianceI[0, 2] = CovarianceI[2, 0];
                                    CovarianceI[1, 2] = sigmaesystematic[1] * sigmaesystematic[2];
                                    CovarianceI[2, 1] = CovarianceI[1, 2];
                                }
                                errorSourcesAccumulator[i]!.ErrorSum![0] = es0 + e[0];
                                errorSourcesAccumulator[i]!.ErrorSum![1] = es1 + e[1];
                                errorSourcesAccumulator[i]!.ErrorSum![2] = es2 + e[2];
                            }
                        }
                        if (errorSourcesAccumulator[i] is { } esAcc2 &&
                            esAcc2.Covariance is { })
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                for (int k = 0; k < 3; k++)
                                {
                                    //surveyStation.Uncertainty.Covariance[j, k] += CovarianceI[j, k];
                                    esAcc2.Covariance[j, k] += CovarianceI[j, k];
                                    covarianceSum[j, k] += CovarianceI[j, k];
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"Error: Covariance matrix of the error sources accumulator not instantiated for errorSource {eSource!.ErrorCode}");
                        }

                        errorSourcesAccumulator[i].Covariance = CovarianceI;
                    }
                }
                return true;
            }
            return false;
        }

        private static bool IsStationary(ErrorSource errorSource)
        {
            if (errorSource.ErrorCode is ErrorCode.GXY_B1 || errorSource.ErrorCode is ErrorCode.GXY_B2 || errorSource.ErrorCode is ErrorCode.GXY_RN ||
                errorSource.ErrorCode is ErrorCode.GXY_G1 || errorSource.ErrorCode is ErrorCode.GXY_G2 || errorSource.ErrorCode is ErrorCode.GXY_G3 ||
                errorSource.ErrorCode is ErrorCode.GXY_G4 || errorSource.ErrorCode is ErrorCode.GXY_SF || errorSource.ErrorCode is ErrorCode.GXY_MIS ||
                errorSource.ErrorCode is ErrorCode.GXYZ_XYB1 || errorSource.ErrorCode is ErrorCode.GXYZ_XYB2 || errorSource.ErrorCode is ErrorCode.GXYZ_XYRN ||
                errorSource.ErrorCode is ErrorCode.GXYZ_XYG1 || errorSource.ErrorCode is ErrorCode.GXYZ_XYG2 || errorSource.ErrorCode is ErrorCode.GXYZ_XYG3 ||
                errorSource.ErrorCode is ErrorCode.GXYZ_XYG4 || errorSource.ErrorCode is ErrorCode.GXYZ_ZB || errorSource.ErrorCode is ErrorCode.GXYZ_ZRN ||
                errorSource.ErrorCode is ErrorCode.GXYZ_ZG1 || errorSource.ErrorCode is ErrorCode.GXYZ_ZG2 || errorSource.ErrorCode is ErrorCode.GXYZ_SF ||
                errorSource.ErrorCode is ErrorCode.GXYZ_MIS)
            {
                return true;
            }
            else { return false; }
        }

        /// <summary>
        /// Evaluates whether at least an error source is continuous and has been initialized
        /// </summary>
        /// <param name="errorSources">the list of error sources</param>
        /// <param name="incl">the inclination at current survey station</param>
        /// <param name="errorSourcesAccumulator">the list of error sources accumulators</param>
        /// <returns></returns>
        private static bool IsContinuousMode(List<ErrorSource> errorSources, double incl, List<ISCWSAErrorAccumulator> errorSourcesAccumulator)
        {
            bool isContinuous = false;
            for (int i = 0; i < errorSourcesAccumulator.Count; i++)
            {
                if (errorSourcesAccumulator[i].IsInitialized && errorSources[i].IsContinuous)
                {
                    isContinuous = true;
                }
                if (errorSources[i].IsContinuous && incl >= errorSources[i].StartInclination) //New
                {
                    //isContinuous = true;
                }
            }
            return isContinuous;
        }

        /// <summary>
        /// Evaluates whether at least an error source needs to be re-initialized when:
        ///     - either when inclination is lower than the initialization inclination, whereas it was higher at previous survey station
        ///     - or when last re-initialization occurred too long ago
        /// </summary>
        /// <param name="incl">the inclination at current survey station</param>
        /// <param name="inclPrev">the inclination at previous survey station</param>
        /// <param name="errorSources">the list of error sources</param>
        /// <param name="dist">the distance since last initialization depth (understood measured depth)</param>
        /// <param name="minDist"></param>
        /// <returns></returns>
        private static bool ToReInitialize(double incl, double inclPrev, List<ErrorSource> errorSources, double dist, double minDist)
        {
            bool reInitialize = false;
            for (int i = 0; i < errorSources.Count; i++)
            {
                ErrorSource eSource = (ErrorSource)errorSources[i];
                if ((IsStationary(eSource) && incl <= eSource.InitInclination && inclPrev > eSource.InitInclination) || dist > minDist)
                {
                    reInitialize = true;
                }
            }
            return reInitialize;
        }

        /// <summary>
        /// ISCWSAErrorData
        /// </summary>
        public class ISCWSAErrorAccumulator
        {
            public double[,]? Covariance { get; set; }
            public double[]? ErrorSum { get; set; }
            public double[,]? SigmaErrorRandom { get; set; }
            public double GyroH { get; set; }
            public bool IsInitialized = false;
            public double InitializationMD = 0.0;

            public ISCWSAErrorAccumulator()
            {
                Covariance = new double[3, 3];
                SigmaErrorRandom = new double[3, 3];
                ErrorSum = new double[3];
            }
        }
    }
}
