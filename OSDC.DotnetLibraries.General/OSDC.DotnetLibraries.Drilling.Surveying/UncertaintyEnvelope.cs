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
        public List<SurveyStation>? SurveyStationList { get; set; }
        /// <summary>
        /// The confidence factor used to compute the ellipsoid of uncertainty
        /// </summary>
        public double? ConfidenceFactor { get; set; } = 0.95;
        /// <summary>
        /// The scaling factor used to compute the ellipsoid of uncertainty
        /// </summary>
        public double? ScalingFactor { get; set; } = 1.0;
        /// <summary>
        /// The list of ellipsoids of uncertainty at all survey stations
        /// </summary>
        public List<UncertaintyEllipsoid>? UncertaintyEllipsoidList { get; set; }
        /// <summary>
        /// Controls whether the horizontal ellipse projection should be calculated for each ellipsoid.
        /// </summary>
        public bool CalculateHorizontalEllipse { get; set; } = true;
        /// <summary>
        /// Controls whether the vertical ellipse projection should be calculated for each ellipsoid.
        /// </summary>
        public bool CalculateVerticalEllipse { get; set; } = true;
        /// <summary>
        /// Controls whether the perpendicular ellipse projection should be calculated for each ellipsoid.
        /// </summary>
        public bool CalculatePerpendicularEllipse { get; set; } = true;
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
        /// The same density is used at all survey stations - unless the MeshLongitudinalCount is specified, in which case the MeshLongitudinalLength is used instead
        /// </summary>
        public double MeshLongitudinalLength { get; set; } = 3.0;
        /// <summary>
        /// If the MeshLongitudinalCount is null, the MeshLongitudinalLength is used to define the number of mesh elements of the envelope of uncertainty between two consecutive survey stations
        /// </summary>
        public int? MeshLongitudinalCount { get; set; } = null;
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

        public bool Calculate()
        {
            bool ok = false;
            if (SurveyStationList is { Count: >= 3 } && // 3 survey stations needed for an envelope
                ErrorModel is { } errorModel &&
                ConfidenceFactor is double confidenceFactor &&
                ScalingFactor is double scalingFactor
                ) 
            {
                // If required, collect survey stations between specified depth intervals (TVD or MD)
                List<int> surveyStationsIndices = Enumerable.Range(0, SurveyStationList.Count).ToList();

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
                            CalculateHorizontalEllipse = CalculateHorizontalEllipse,
                            CalculateVerticalEllipse = CalculateVerticalEllipse,
                            CalculatePerpendicularEllipse = CalculatePerpendicularEllipse,
                        };
                        ok &= uncertaintyEnvelopeEllipsoid.Calculate();
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
                            int meshLongitudinalCount = MeshLongitudinalCount.HasValue ? MeshLongitudinalCount.Value : (int)Math.Ceiling(distance / MeshLongitudinalLength);
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
                    ok &= DiscretizeEllipses();
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
    }
}
