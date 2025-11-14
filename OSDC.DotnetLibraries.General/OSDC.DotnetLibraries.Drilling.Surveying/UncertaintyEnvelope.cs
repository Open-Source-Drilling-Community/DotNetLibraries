using OSDC.DotnetLibraries.General.Math;

namespace OSDC.DotnetLibraries.Drilling.Surveying
{
    public class UncertaintyEnvelope
    {
        public enum ErrorModelType { WolffAndDeWardt, ISCWSA }

        /// <summary>
        /// The error model used to compute the uncertainty envelope
        /// </summary>
        public ErrorModelType ErrorModel { get; set; }
        /// <summary>
        /// The list of survey stations to compute the envelope of uncertainty from
        /// </summary>
        public SurveyStationList? SurveyStationList { get; set; }
        /// <summary>
        /// The confidence factor used to compute the ellipsoid of uncertainty
        /// </summary>
        public double? ConfidenceFactor { get; set; } = 0.95;
        /// <summary>
        /// The scaling factor used to compute the ellipsoid of uncertainty
        /// </summary>
        public double? ScalingFactor { get; set; } = 1.0;
        /// <summary>
        /// The borehole radius at the survey station
        /// </summary>
        public double? BoreholeRadius { get; set; } = 0.0;
        /// <summary>
        /// The delegate function used to select the type of depth range (in TVD or MD) on which the envelope of uncertainty is computed
        /// </summary>
        public Func<SurveyStation, double>? DepthTypeSelector { get; set; } = null;
        /// <summary>
        /// The lower bound of the depth range (in TVD or MD) on which the evelope of uncertainty is computed
        /// </summary>
        public double? MinDepth { get; set; } = null;
        /// <summary>
        /// The upper bound of the depth range (in TVD or MD) on which the evelope of uncertainty is computed
        /// </summary>
        public double? MaxDepth { get; set; } = null;
        /// <summary>
        /// The list of ellipsoids of uncertainty at all survey stations
        /// </summary>
        public List<UncertaintyEllipsoid>? UncertaintyEllipsoidList { get; set; }
        /// <summary>
        /// The list of meshed ellipses of uncertainty defining the envelope of uncertainty
        ///     - includes the perpendicular ellipses of uncertainty at all survey stations
        ///     - and intermediary ellipses of uncertainty interpolated between the survey stations
        /// </summary>
        public List<UncertaintyEllipse>? MeshedEllipseList { get; set; }
        /// <summary>
        /// The number of sectors into which ellipses of uncertainty (perpendicular projection) are decomposed
        /// The same count is used at all survey stations
        /// </summary>
        public int? MeshSectorCount { get; set; } = 32;
        /// <summary>
        /// The maximum length [SI] of one mesh element of the envelope of uncertainty between two consecutive survey stations
        /// The same density is used at all survey stations
        /// </summary>
        public double? MeshLongitudinalLength { get; set; } = 3;
        /// <summary>
        /// The maximum number of ellipses used to discretize the envelope of uncertainty
        /// </summary>
        public int? MaxEllipseCount { get; set; }
        /// <summary>
        /// Default constructor
        /// </summary>
        public UncertaintyEnvelope() : base()
        {
        }

        private bool Calculate()
        {
            bool ok = false;
            if (SurveyStationList is { Count: >= 3 } && // 3 survey stations needed for an envelope
                ErrorModel is { } errorModel &&
                ConfidenceFactor is double confidenceFactor &&
                ScalingFactor is double scalingFactor &&
                BoreholeRadius is double boreholeRadius &&
                MeshLongitudinalLength is double
                ) 
            {
                double[,] AAAAA = new double[6, 3];
                for (int i = 0; i < AAAAA.GetLength(0); i++)
                {
                    for (int j = 0; j < AAAAA.GetLength(1); j++)
                    {
                        AAAAA[i, j] = 0.0;
                    }
                }
                // If required, collect survey stations between specified depth intervals (TVD or MD)
                List<int> surveyStationsIndices = Enumerable.Range(0, SurveyStationList.Count).ToList();
                if (DepthTypeSelector is Func<SurveyStation, double> selector &&
                    MinDepth is double minD &&
                    MaxDepth is double maxD)
                    surveyStationsIndices = (List<int>)FilterSurveyStationsInDepthInterval(DepthTypeSelector, minD, maxD);

                /////////////////////////////////////////////////////////////////////////////////////////////
                // Calculate covariance matrices at each survey station according to specified error model //
                /////////////////////////////////////////////////////////////////////////////////////////////
                if (surveyStationsIndices != null && surveyStationsIndices.Count >= 3) // 3 survey stations needed for an envelope
                {
                    if (errorModel is ErrorModelType.WolffAndDeWardt)
                    {
                        ok = CovarianceCalculatorWolffDeWardt.Calculate(SurveyStationList, surveyStationsIndices);
                    }
                    else if (errorModel is ErrorModelType.ISCWSA)
                    {
                        ok = CovarianceCalculatorISCWSA.Calculate(SurveyStationList, surveyStationsIndices);
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    // Calculate the list of Ellipsoids (incl.ellipsoid and ellipses' parameters) //
                    ////////////////////////////////////////////////////////////////////////////////
                    UncertaintyEllipsoidList = new();
                    foreach (int i in surveyStationsIndices)
                    {
                        UncertaintyEllipsoid uncertaintyEnvelopeEllipsoid = new()
                        {
                            EllipsoidSurveyStation = SurveyStationList[i],
                            ConfidenceFactor = confidenceFactor,
                            ScalingFactor = scalingFactor,
                            BoreholeRadius = boreholeRadius
                        };
                        ok = uncertaintyEnvelopeEllipsoid.Calculate();
                        if (!ok) break;
                        UncertaintyEllipsoidList.Add(uncertaintyEnvelopeEllipsoid);
                    }
                    ////////////////////////////////////////////////////////////////////////////
                    // Interpolate intermediate ellipses of uncertainty from current and next //
                    ////////////////////////////////////////////////////////////////////////////
                    MeshedEllipseList = new();
                    for (int i = 0; ok && i < UncertaintyEllipsoidList.Count - 1; ++i)
                    {
                        if (UncertaintyEllipsoidList[i].PerpendicularEllipse is { } ellipse &&
                            UncertaintyEllipsoidList[i + 1].PerpendicularEllipse is { } ellipseNext &&
                            ellipse.EllipseRadii is { } ellipseRadii &&
                            ellipse.EllipseOrientationAngle is double angle &&
                            ellipseNext.EllipseRadii is { } ellipseRadiiNext &&
                            ellipseNext.EllipseOrientationAngle is double angleNext &&
                            ellipse.EllipseCenter is { } ellipseCenter &&
                            ellipseCenter.Inclination is double inclination &&
                            ellipseCenter.Azimuth is double azimuth &&
                            ellipseCenter.RiemannianNorth is double north &&
                            ellipseCenter.RiemannianEast is double east &&
                            ellipseCenter.TVD is double tvd &&
                            ellipseCenter.MD is double md &&
                            ellipseNext.EllipseCenter is { } ellipseCenterNext &&
                            ellipseCenterNext.Inclination is double inclinationNext &&
                            ellipseCenterNext.Azimuth is double azimuthNext &&
                            ellipseCenterNext.RiemannianNorth is double northNext &&
                            ellipseCenterNext.RiemannianEast is double eastNext &&
                            ellipseCenterNext.TVD is double tvdNext &&
                            ellipseCenterNext.MD is double mdNext)
                        {
                            // Add current ellipse
                            MeshedEllipseList.Add(ellipse);
                            double distance = mdNext - md;
                            int meshLongitudinalCount = (int)(distance / MeshLongitudinalLength);
                            bool skipLast = false;
                            // Construct intermediate ellipses
                            for (int n = 1; n < meshLongitudinalCount; n++)
                            {
                                Vector2D ellipseR = new();
                                ellipseR[0] = ellipseRadii[0] + (double)n * (ellipseRadiiNext[0] - ellipseRadii[0]) / (double)meshLongitudinalCount;
                                ellipseR[1] = ellipseRadii[1] + (double)n * (ellipseRadiiNext[1] - ellipseRadii[1]) / (double)meshLongitudinalCount;
                                double inclination_n = (inclination + (double)n * (inclinationNext - inclination) / (double)meshLongitudinalCount);
                                double azimuth_n = (azimuth + (double)n * (azimuthNext - azimuth) / (double)meshLongitudinalCount);
                                double north_n = north + (double)n * (northNext - north) / (double)meshLongitudinalCount;
                                double east_n = east + (double)n * (eastNext - east) / (double)meshLongitudinalCount;
                                double tvd_n = tvd + (double)n * (tvdNext - tvd) / (double)meshLongitudinalCount;
                                double md_n = md + (double)n * (mdNext - md) / (double)meshLongitudinalCount;
                                double angle_n = angle + (double)n * (angleNext - angle) / (double)meshLongitudinalCount;

                                SurveyPoint ellipseCenterInter = new()
                                {
                                    Inclination = inclination_n,
                                    Azimuth = azimuth_n,
                                    RiemannianNorth = north_n,
                                    RiemannianEast = east_n,
                                    TVD = tvd_n,
                                    MD = md_n,
                                };
                                UncertaintyEllipse ellipseInter = new()
                                {
                                    EllipseCenter = ellipseCenterInter,
                                    EllipseOrientationAngle = angle_n,
                                    EllipseRadii = ellipseR
                                };
                                // Add intermediate ellipse
                                MeshedEllipseList.Add(ellipseInter);
                                if (MaxEllipseCount is int && MeshedEllipseList.Count == MaxEllipseCount)
                                {
                                    skipLast = true;
                                    break;
                                }
                            }
                            if (skipLast)
                            {
                                break;
                            }
                            // Add last ellipse
                            if (i == UncertaintyEllipsoidList.Count - 2)
                                MeshedEllipseList.Add(ellipseNext);
                        }
                    }
                    // Finally, discretize all ellipses (at survey stations and intermediate locations)
                    DiscretizeEllipses();
                }
            }
            return ok;
        }

        /// <summary>
        /// Discretize the perpendicular ellipses of uncertainty associated to the list of ellipsoids
        /// </summary>
        /// <param name="meshSectorCount">the number of sectors into which ellipses of uncertainty (perpendicular projection) are decomposed</param>
        /// <returns>true if calculation went ok, false otherwise</returns>
        public bool DiscretizeEllipses()
        {
            if (MeshedEllipseList is { Count: > 1 } list)
            {
                foreach (var item in list)
                    if (item == null ||
                        MeshSectorCount is not int count ||
                        !item.DiscretizeEllipse(count)
                        )
                        return false;
                return true;
            }
            return false;
        }


        //public List<UncertaintyEllipsoid> GetPlainUncertaintyEnvelope(double confidenceFactor, double scalingFactor, double boreholeRadius, int intermediateEllipseNumbers = 0, double? minimumDistanceMD = null)
        //{
        //    double[,] A = new double[6, 3];
        //    for (int i = 0; i < A.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < A.GetLength(1); j++)
        //        {
        //            A[i, j] = 0.0;
        //        }
        //    }

        //    // Start from i = 0 to include the first surveystation. This will typically have radius 0
        //    for (int i = 0; i < _surveyList.Count; i++)
        //    {
        //        if (_surveyList[i].Uncertainty == null)
        //        {
        //            WdWSurveyStationUncertainty wdwun = new WdWSurveyStationUncertainty();
        //            SurveyInstrument.Model.SurveyInstrument surveyTool = new SurveyInstrument.Model.SurveyInstrument(SurveyInstrument.Model.SurveyInstrument.WdWGoodMag);
        //            _surveyList[i].SurveyTool = surveyTool;
        //            _surveyList[i].Uncertainty = wdwun;
        //        }
        //        if (((_useWdwCovariance == _surveyList[i].Uncertainty is WdWSurveyStationUncertainty && i > 0) || (_surveyList.Count > 1 && _surveyList[i].Uncertainty.Covariance[0, 0] == null)))
        //        {
        //            WdWSurveyStationUncertainty wdwSurveyStatoinUncertainty = (WdWSurveyStationUncertainty)_surveyList[i].Uncertainty;
        //            A = wdwSurveyStatoinUncertainty.CalculateCovariances(_surveyList[i], _surveyList[i - 1], A);
        //        }
        //        if (_useWdwCovariance == _surveyList[i].Uncertainty is WdWSurveyStationUncertainty && i > 0)
        //        {
        //            WdWSurveyStationUncertainty wdwSurveyStatoinUncertainty = (WdWSurveyStationUncertainty)_surveyList[i].Uncertainty;
        //            A = wdwSurveyStatoinUncertainty.CalculateCovariances(_surveyList[i], _surveyList[i - 1], A);
        //        }
        //        _surveyList[i].Uncertainty.Calculate(_surveyList[i], confidenceFactor, scalingFactor, boreholeRadius);
        //    }

        //    List<UncertaintyEllipsoid> uncertaintyEnvelope = new List<UncertaintyEllipsoid>();

        //    for (int i = 0; i < _surveyList.Count - 1; i++)
        //    {
        //        UncertaintyEllipsoid uncertaintyEnvelopeEllipse = new UncertaintyEllipsoid();

        //        Vector2D ellipseRadius = new Vector2D();
        //        ellipseRadius = _surveyList[i].Uncertainty.EllipseRadius;
        //        Vector2D ellipseRadiusNext = new Vector2D();
        //        ellipseRadiusNext = _surveyList[i + 1].Uncertainty.EllipseRadius;
        //        double distance = (double)_surveyList[i + 1].MdWGS84 - (double)_surveyList[i].MdWGS84;

        //        uncertaintyEnvelopeEllipse.Azimuth = _surveyList[i].AzWGS84;
        //        uncertaintyEnvelopeEllipse.Inclination = _surveyList[i].Incl;
        //        uncertaintyEnvelopeEllipse.X = _surveyList[i].NorthOfWellHead;
        //        uncertaintyEnvelopeEllipse.Y = _surveyList[i].EastOfWellHead;
        //        uncertaintyEnvelopeEllipse.Z = _surveyList[i].TvdWGS84;
        //        uncertaintyEnvelopeEllipse.MD = _surveyList[i].MdWGS84;
        //        uncertaintyEnvelopeEllipse.EllipseRadius = ellipseRadius;
        //        List<GlobalCoordinatePoint3D> ellipseCoordinates = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipse);
        //        uncertaintyEnvelopeEllipse.EllipseCoordinates = ellipseCoordinates;
        //        uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipse);

        //        if (minimumDistanceMD != null)
        //        {
        //            intermediateEllipseNumbers = (int)System.Math.Ceiling(distance / (double)minimumDistanceMD);
        //        }
        //        for (int n = 1; n < intermediateEllipseNumbers; n++)
        //        {
        //            Vector2D ellipseR = new Vector2D();
        //            ellipseR[0] = ellipseRadius[0] + (double)n * (ellipseRadiusNext[0] - ellipseRadius[0]) / (double)intermediateEllipseNumbers;
        //            ellipseR[1] = ellipseRadius[1] + (double)n * (ellipseRadiusNext[1] - ellipseRadius[1]) / (double)intermediateEllipseNumbers;

        //            double inclination = ((double)_surveyList[i].Incl + (double)n * ((double)_surveyList[i + 1].Incl - (double)_surveyList[i].Incl) / (double)intermediateEllipseNumbers);
        //            double azimuth = ((double)_surveyList[i].AzWGS84 + (double)n * ((double)_surveyList[i + 1].AzWGS84 - (double)_surveyList[i].AzWGS84) / (double)intermediateEllipseNumbers);
        //            double north = (double)_surveyList[i].NorthOfWellHead + (double)n * ((double)_surveyList[i + 1].NorthOfWellHead - (double)_surveyList[i].NorthOfWellHead) / (double)intermediateEllipseNumbers;
        //            double east = (double)_surveyList[i].EastOfWellHead + (double)n * ((double)_surveyList[i + 1].EastOfWellHead - (double)_surveyList[i].EastOfWellHead) / (double)intermediateEllipseNumbers;
        //            double tvd = (double)_surveyList[i].TvdWGS84 + (double)n * ((double)_surveyList[i + 1].TvdWGS84 - (double)_surveyList[i].TvdWGS84) / (double)intermediateEllipseNumbers;
        //            double md = (double)_surveyList[i].MdWGS84 + (double)n * ((double)_surveyList[i + 1].MdWGS84 - (double)_surveyList[i].MdWGS84) / (double)intermediateEllipseNumbers;
        //            double perpendicularDirection = _surveyList[i].Uncertainty.PerpendicularDirection + (double)n * (_surveyList[i + 1].Uncertainty.PerpendicularDirection - _surveyList[i + 1].Uncertainty.PerpendicularDirection) / (double)intermediateEllipseNumbers;

        //            UncertaintyEllipsoid uncertaintyEnvelopeEllipseInter = new UncertaintyEllipsoid();
        //            uncertaintyEnvelopeEllipseInter.Azimuth = azimuth;
        //            uncertaintyEnvelopeEllipseInter.Inclination = inclination;
        //            uncertaintyEnvelopeEllipseInter.X = north;
        //            uncertaintyEnvelopeEllipseInter.Y = east;
        //            uncertaintyEnvelopeEllipseInter.Z = tvd;
        //            uncertaintyEnvelopeEllipseInter.MD = md;
        //            uncertaintyEnvelopeEllipseInter.EllipseRadius = ellipseR;

        //            uncertaintyEnvelopeEllipseInter.CrossSectionOrientation = perpendicularDirection;

        //            List<GlobalCoordinatePoint3D> ellipseCoordinatesInter = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipseInter);
        //            uncertaintyEnvelopeEllipseInter.EllipseCoordinates = ellipseCoordinatesInter;
        //            uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipseInter);
        //        }

        //        if (i == _surveyList.Count - 2)
        //        {
        //            uncertaintyEnvelopeEllipse = new UncertaintyEllipsoid();
        //            //Vector2D ellipseRadius = new Vector2D();
        //            ellipseRadius = _surveyList[_surveyList.Count - 1].Uncertainty.EllipseRadius;

        //            uncertaintyEnvelopeEllipse.Azimuth = _surveyList[_surveyList.Count - 1].AzWGS84;
        //            uncertaintyEnvelopeEllipse.Inclination = _surveyList[_surveyList.Count - 1].Incl;
        //            uncertaintyEnvelopeEllipse.X = _surveyList[_surveyList.Count - 1].NorthOfWellHead;
        //            uncertaintyEnvelopeEllipse.Y = _surveyList[_surveyList.Count - 1].EastOfWellHead;
        //            uncertaintyEnvelopeEllipse.Z = _surveyList[_surveyList.Count - 1].TvdWGS84;
        //            uncertaintyEnvelopeEllipse.MD = _surveyList[_surveyList.Count - 1].MdWGS84;
        //            uncertaintyEnvelopeEllipse.EllipseRadius = ellipseRadius;
        //            ellipseCoordinates = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipse);
        //            uncertaintyEnvelopeEllipse.EllipseCoordinates = ellipseCoordinates;
        //            uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipse);
        //        }
        //    }
        //    UncertaintyEnvelope = uncertaintyEnvelope;
        //    return uncertaintyEnvelope;
        //}

        //private void CalculateUncertaintyCylinder(SurveyStation surveyStation, double confidenceFactor)
        //{
        //    double xMinEllipsoid = 0.0;
        //    double xMaxEllipsoid = 0.0;
        //    double yMinEllipsoid = 0.0;
        //    double yMaxEllipsoid = 0.0;
        //    double zMinEllipsoid = 0.0;
        //    double zMaxEllipsoid = 0.0;
        //    CalculateExtremumInDepths(surveyStation, confidenceFactor, ref xMinEllipsoid, ref xMaxEllipsoid, ref yMinEllipsoid, ref yMaxEllipsoid, ref zMinEllipsoid, ref zMaxEllipsoid);

        //    List<UncertaintyEllipsoid> uncertaintyEnvelope = new List<UncertaintyEllipsoid>();

        //    //for (int i = 0; i < surveyList.Count - 1; i++)
        //    {

        //        double xMinEllipse = Numeric.MAX_DOUBLE;
        //        double xMaxEllipse = Numeric.MIN_DOUBLE;
        //        double yMinEllipse = Numeric.MAX_DOUBLE;
        //        double yMaxEllipse = Numeric.MIN_DOUBLE;
        //        double zMinEllipse = Numeric.MAX_DOUBLE;
        //        double zMaxEllipse = Numeric.MIN_DOUBLE;

        //        UncertaintyEllipsoid uncertaintyEnvelopeEllipse = new UncertaintyEllipsoid();

        //        Vector2D ellipseRadius = surveyStation.Uncertainty.EllipseRadius;
        //        Vector2D ellipseRadiusNext = surveyStation.Uncertainty.EllipseRadius;
        //        double distance = 0;// MD - MD;
        //        double LongitudinalMeshDensity = 10;

        //        uncertaintyEnvelopeEllipse.Azimuth = surveyStation.AzWGS84;
        //        uncertaintyEnvelopeEllipse.Inclination = surveyStation.Incl;
        //        uncertaintyEnvelopeEllipse.X = surveyStation.NorthOfWellHead;
        //        uncertaintyEnvelopeEllipse.Y = surveyStation.EastOfWellHead;
        //        uncertaintyEnvelopeEllipse.Z = surveyStation.TvdWGS84;
        //        uncertaintyEnvelopeEllipse.MD = (double)surveyStation.MdWGS84;
        //        uncertaintyEnvelopeEllipse.EllipseRadius = ellipseRadius;
        //        uncertaintyEnvelopeEllipse.CrossSectionOrientation = surveyStation.Uncertainty.PerpendicularDirection;
        //        List<GlobalCoordinatePoint3D> ellipseCoordinates = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipse, 0, ref xMinEllipse, ref xMaxEllipse, ref yMinEllipse, ref yMaxEllipse, ref zMinEllipse, ref zMaxEllipse);
        //        uncertaintyEnvelopeEllipse.EllipseCoordinates = ellipseCoordinates;
        //        uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipse);

        //        double z = 0;
        //        double zAdd = 2;
        //        bool increasingZ = false;
        //        bool increasingX = false;
        //        bool increasingY = false;
        //        bool stop = false;
        //        double zMinPrev = zMinEllipse;
        //        double xMinPrev = xMinEllipse;
        //        double yMinPrev = yMinEllipse;
        //        if (surveyStation.Incl > Numeric.PI / 2)
        //        {
        //            stop = true;
        //        }
        //        if (xMinEllipsoid < xMinEllipse || yMinEllipsoid < yMinEllipse || zMinEllipsoid < zMinEllipse)
        //        {
        //            z -= zAdd;
        //            UncertaintyEllipsoid uncertaintyEnvelopeEllipseInter = new UncertaintyEllipsoid();
        //            uncertaintyEnvelopeEllipseInter.Azimuth = surveyStation.AzWGS84;
        //            uncertaintyEnvelopeEllipseInter.Inclination = surveyStation.Incl;
        //            uncertaintyEnvelopeEllipseInter.X = surveyStation.NorthOfWellHead;
        //            uncertaintyEnvelopeEllipseInter.Y = surveyStation.EastOfWellHead;
        //            uncertaintyEnvelopeEllipseInter.Z = surveyStation.TvdWGS84;
        //            uncertaintyEnvelopeEllipseInter.MD = (double)surveyStation.MdWGS84;
        //            uncertaintyEnvelopeEllipseInter.EllipseRadius = ellipseRadius;
        //            uncertaintyEnvelopeEllipseInter.CrossSectionOrientation = surveyStation.Uncertainty.PerpendicularDirection;
        //            List<GlobalCoordinatePoint3D> ellipseCoordinatesInter = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipseInter, z, ref xMinEllipse, ref xMaxEllipse, ref yMinEllipse, ref yMaxEllipse, ref zMinEllipse, ref zMaxEllipse);
        //            if (zMinPrev < zMinEllipse)
        //            {
        //                increasingZ = true;
        //            }
        //            if (xMinPrev <= xMinEllipse)
        //            {
        //                increasingX = true;
        //            }
        //            if (yMinPrev <= yMinEllipse)
        //            {
        //                increasingY = true;
        //            }
        //            if (zMinPrev == zMinEllipse || surveyStation.Incl > Numeric.PI / 2 || Numeric.EQ(surveyStation.AzWGS84, Numeric.PI, 0.01))
        //            {
        //                stop = true;
        //            }
        //            uncertaintyEnvelopeEllipseInter.EllipseCoordinates = ellipseCoordinatesInter;
        //            uncertaintyEnvelope.Insert(0, uncertaintyEnvelopeEllipseInter);
        //        }
        //        //if (!stop)
        //        {
        //            while ((!stop && (zMinEllipsoid < zMinEllipse)) || ((!increasingX && Numeric.LT(xMinEllipsoid, xMinEllipse, 0.01)) || (increasingX && Numeric.GT(xMaxEllipsoid, xMaxEllipse, 0.01))) || ((!increasingY && Numeric.LT(yMinEllipsoid, yMinEllipse, 0.01)) || (increasingY && Numeric.GT(yMaxEllipsoid, yMaxEllipse, 0.01))))
        //            {
        //                z -= zAdd;
        //                UncertaintyEllipsoid uncertaintyEnvelopeEllipseInter = new UncertaintyEllipsoid();
        //                uncertaintyEnvelopeEllipseInter.Azimuth = surveyStation.AzWGS84;
        //                uncertaintyEnvelopeEllipseInter.Inclination = surveyStation.Incl;
        //                uncertaintyEnvelopeEllipseInter.X = surveyStation.NorthOfWellHead;
        //                uncertaintyEnvelopeEllipseInter.Y = surveyStation.EastOfWellHead;
        //                uncertaintyEnvelopeEllipseInter.Z = surveyStation.TvdWGS84;
        //                uncertaintyEnvelopeEllipseInter.MD = (double)surveyStation.MdWGS84;
        //                uncertaintyEnvelopeEllipseInter.EllipseRadius = ellipseRadius;
        //                uncertaintyEnvelopeEllipseInter.CrossSectionOrientation = surveyStation.Uncertainty.PerpendicularDirection;
        //                List<GlobalCoordinatePoint3D> ellipseCoordinatesInter = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipseInter, z, ref xMinEllipse, ref xMaxEllipse, ref yMinEllipse, ref yMaxEllipse, ref zMinEllipse, ref zMaxEllipse);
        //                uncertaintyEnvelopeEllipseInter.EllipseCoordinates = ellipseCoordinatesInter;
        //                uncertaintyEnvelope.Insert(0, uncertaintyEnvelopeEllipseInter);
        //            }
        //            z = 0;
        //            while (((!increasingX && Numeric.GT(xMaxEllipsoid, xMaxEllipse, 0.01)) || (increasingX && Numeric.LT(xMinEllipsoid, xMinEllipse, 0.01))) || ((!increasingY && Numeric.GT(yMaxEllipsoid, yMaxEllipse, 0.01)) || (increasingY && Numeric.LT(yMinEllipsoid, yMinEllipse, 0.01))) || (!stop && zMaxEllipsoid > zMaxEllipse))
        //            //while ( zMax > zMaxEllipse)
        //            {
        //                z += zAdd;
        //                UncertaintyEllipsoid uncertaintyEnvelopeEllipseInter = new UncertaintyEllipsoid();
        //                uncertaintyEnvelopeEllipseInter.Azimuth = surveyStation.AzWGS84;
        //                uncertaintyEnvelopeEllipseInter.Inclination = surveyStation.Incl;
        //                uncertaintyEnvelopeEllipseInter.X = surveyStation.NorthOfWellHead;
        //                uncertaintyEnvelopeEllipseInter.Y = surveyStation.EastOfWellHead;
        //                uncertaintyEnvelopeEllipseInter.Z = surveyStation.TvdWGS84;
        //                uncertaintyEnvelopeEllipseInter.MD = (double)surveyStation.MdWGS84;
        //                uncertaintyEnvelopeEllipseInter.EllipseRadius = ellipseRadius;
        //                uncertaintyEnvelopeEllipseInter.CrossSectionOrientation = surveyStation.Uncertainty.PerpendicularDirection;
        //                List<GlobalCoordinatePoint3D> ellipseCoordinatesInter = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipseInter, z, ref xMinEllipse, ref xMaxEllipse, ref yMinEllipse, ref yMaxEllipse, ref zMinEllipse, ref zMaxEllipse);
        //                uncertaintyEnvelopeEllipseInter.EllipseCoordinates = ellipseCoordinatesInter;
        //                uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipseInter);

        //                CalculateExtremumInDepths(surveyStation, confidenceFactor, ref xMinEllipsoid, ref xMaxEllipsoid, ref yMinEllipsoid, ref yMaxEllipsoid, ref zMinEllipsoid, ref zMaxEllipsoid);
        //            }
        //        }
        //    }
        //    surveyStation.Uncertainty.UncertaintyCylinder = uncertaintyEnvelope;
        //}

        //public bool IsPartOfUncertaintyEnvelope(CoordinateConverter.WGS84Coordinate wGS84Coordinate, double tvd)
        //{
        //    bool isPartOf = false;
        //    CoordinateConverter converter = new CoordinateConverter();
        //    CoordinateConverter.UTMCoordinate utmCoordinate = converter.WGStoUTM(wGS84Coordinate);

        //    for (int i = 0; i < UncertaintyEnvelope.Count - 1; i++)
        //    {
        //        UncertaintyEllipsoid uncertaintyEnvelopeEllipse = UncertaintyEnvelope[i];
        //        if (uncertaintyEnvelopeEllipse.EllipseRadius != null && uncertaintyEnvelopeEllipse.EllipseRadius[0] != null && Numeric.IsDefined(uncertaintyEnvelopeEllipse.EllipseRadius[0]))
        //        {
        //            double azimuth = (double)uncertaintyEnvelopeEllipse.Azimuth;
        //            double inclination = (double)uncertaintyEnvelopeEllipse.Inclination;
        //            double xc = (double)uncertaintyEnvelopeEllipse.X;
        //            double yc = (double)uncertaintyEnvelopeEllipse.Y;
        //            double zc = (double)uncertaintyEnvelopeEllipse.Z;
        //            Vector2D ellipseR = uncertaintyEnvelopeEllipse.EllipseRadius;
        //            double[,] T = new double[3, 3];
        //            double sinI = System.Math.Sin(inclination);
        //            double cosI = System.Math.Cos(inclination);
        //            double sinA = System.Math.Sin(azimuth);
        //            double cosA = System.Math.Cos(azimuth);
        //            T[0, 0] = cosI * cosA;
        //            T[1, 0] = -sinA;
        //            T[2, 0] = sinI * cosA;
        //            T[0, 1] = cosI * sinA;
        //            T[1, 1] = cosA;
        //            T[2, 1] = sinI * sinA;
        //            T[0, 2] = -sinI;
        //            T[1, 2] = 0;
        //            T[2, 2] = cosI;
        //            double xutm = utmCoordinate.X - WellUTMCoordinate.X;
        //            double yutm = utmCoordinate.Y - WellUTMCoordinate.Y;
        //            double x = T[0, 0] * xutm + T[0, 1] * yutm + T[0, 2] * tvd;
        //            double y = T[1, 0] * xutm + T[1, 1] * yutm + T[1, 2] * tvd;
        //            double z = T[2, 0] * xutm + T[2, 1] * yutm + T[2, 2] * tvd;
        //            double val = Math.Pow((x - xc), 2) / Math.Pow((double)ellipseR[0], 2) + Math.Pow((y - yc), 2) / Math.Pow((double)ellipseR[1], 2);
        //            if (val <= 1)
        //            {
        //                isPartOf = true;
        //                break;
        //            }
        //            else
        //            {
        //                isPartOf = false;
        //            }
        //        }
        //    }
        //    return isPartOf;
        //}

        /// <summary>
        /// A utility function used to filter survey stations located in a depth interval, associated to given member selector (TVD or MD).
        ///     - a survey station is selected if it, or its predecessor, or its successor lie in the given interval
        ///     - list ordering is preserved
        ///     - non-contiguous survey stations intervals are kept
        /// </summary>
        /// <param name="stations">the list of survey stations</param>
        /// <param name="memberSelector">the delegate function to select the queried member variable</param>
        /// <param name="minDepth">the min depth to satisfy</param>
        /// <param name="maxDepth">the max depth to satisfy</param>
        /// <returns>The list of indices of the survey stations located in a given depth interval (TDV or MD)</returns>
        private IEnumerable<int> FilterSurveyStationsInDepthInterval(
            Func<SurveyStation, double> memberSelector,
            double minDepth,
            double maxDepth)
        {
            bool InRange(int k)
            {
                if (k < 0 || k >= SurveyStationList!.Count) return false;
                double v = memberSelector(SurveyStationList![k]);
                return v >= minDepth && v <= maxDepth;
            }
            for (int i = 0; i < SurveyStationList!.Count; i++)
                if (InRange(i) || InRange(i - 1) || InRange(i + 1))
                    yield return i; // note that cases of non-contiguous depth intervals are preserved
        }
    }
}
