//namespace OSDC.DotnetLibraries.Drilling.Surveying
//{
//    public class EnvelopeOfUncertainty
//    {
//        /// <summary>
//        /// The list of survey stations to compute the envelope of uncertainty from
//        /// </summary>
//        public SurveyStationList? SurveyStationList {  get; set; }

//        /// <summary>
//        /// The list of discretized ellipses delineating the envelope of uncertainty
//        /// </summary>
//        public List<EnvelopeOfUncertaintyEllipse>? EllipseList { get; set; }
//        /// <summary>
//        /// the discretization used for the perpendicular projections of the ellipsoids of uncertainty at all survey points
//        /// </summary>
//        public int EllipseVerticesPhi { get; set; } = 32;
        
//        /// <summary>
//        /// Default constructor
//        /// </summary>
//        public EnvelopeOfUncertainty() : base()
//        {
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="confidenceFactor"></param>
//        /// <param name="scalingFactor"></param>
//        /// <param name="minTVD"></param>
//        /// <param name="maxTVD"></param>
//        /// <param name="calculateEllipseAreaCoordinates"></param>
//        /// <returns></returns>
//        public List<EnvelopeOfUncertaintyEllipse> GetUncertaintyEnvelopeTVD(double confidenceFactor, double scalingFactor = 1.0, double? minTVD = null, double? maxTVD = null, int? maxEllipsesCount = null, bool calculateEllipseAreaCoordinates = false)
//        {
//            return GetUncertaintyEnvelope(confidenceFactor, scalingFactor, minTVD, maxTVD, null, null, maxEllipsesCount, calculateEllipseAreaCoordinates);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="confidenceFactor"></param>
//        /// <param name="scalingFactor"></param>
//        /// <param name="minTVD"></param>
//        /// <param name="maxTVD"></param>
//        /// <param name="calculateEllipseAreaCoordinates"></param>
//        /// <returns></returns>
//        public List<EnvelopeOfUncertaintyEllipse> GetUncertaintyEnvelopeMD(double confidenceFactor, double scalingFactor = 1.0, double? minMD = null, double? maxMD = null, bool calculateEllipseAreaCoordinates = false)
//        {
//            return GetUncertaintyEnvelope(confidenceFactor, scalingFactor, null, null, minMD, maxMD, null, calculateEllipseAreaCoordinates);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="confidenceFactor"></param>
//        /// <param name="scalingFactor"></param>
//        /// <param name="calculateEllipseAreaCoordinates"></param>
//        /// <returns></returns>
//        public List<EnvelopeOfUncertaintyEllipse> GetUncertaintyEnvelope(double confidenceFactor, double scalingFactor = 1.0, bool calculateEllipseAreaCoordinates = false)
//        {
//            return GetUncertaintyEnvelope(confidenceFactor, scalingFactor, null, null, null, null, null, calculateEllipseAreaCoordinates);
//        }

//        private List<EnvelopeOfUncertaintyEllipse> GetUncertaintyEnvelope(double confidenceFactor, double scalingFactor = 1.0, double? minTVD = null, double? maxTVD = null, double? minMD = null, double? maxMD = null, int? maxEllipsesCount = null, bool calculateEllipseAreaCoordinates = false)
//        {
//            double[,] A = new double[6, 3];
//            for (int i = 0; i < A.GetLength(0); i++)
//            {
//                for (int j = 0; j < A.GetLength(1); j++)
//                {
//                    A[i, j] = 0.0;
//                }
//            }
//            List<ISCWSAErrorData> ISCWSAErrorDataTmp = new List<ISCWSAErrorData>();
//            List<SurveyStation> surveyList = new List<SurveyStation>();
//            // Start from i = 0 to include the first surveystation. This will typically have radius 0
//            for (int i = 0; i < _surveyList.Count; i++)
//            {
//                bool ok = true;
//                if (minTVD != null && maxTVD != null)
//                {
//                    ok = Numeric.GE(_surveyList[i].TvdWGS84, minTVD) && Numeric.LE(_surveyList[i].TvdWGS84, maxTVD);
//                    // We should also inculde the surveys just outside the tvd range to be able to fill the whole requested range
//                    if (!ok && i < _surveyList.Count - 1)
//                    {
//                        if (Numeric.GE(_surveyList[i + 1].TvdWGS84, minTVD) && Numeric.LE(_surveyList[i + 1].TvdWGS84, maxTVD))
//                        {
//                            // Next survey is ok, then we should also add the current
//                            ok = true;
//                        }
//                    }
//                    if (!ok)
//                    {
//                        if (i > 0 && Numeric.GE(_surveyList[i - 1].TvdWGS84, minTVD) && Numeric.LE(_surveyList[i - 1].TvdWGS84, maxTVD))
//                        {
//                            // Previous survey was ok, then we should also add the current
//                            ok = true;
//                        }
//                    }
//                }
//                else if (minMD != null && maxMD != null)
//                {
//                    ok = Numeric.GE(_surveyList[i].MdWGS84, minMD) && Numeric.LE(_surveyList[i].MdWGS84, maxMD);
//                    // We should also inculde the surveys just outside the tvd range to be able to fill the whole requested range
//                    if (!ok && i < _surveyList.Count - 1)
//                    {
//                        if (Numeric.GE(_surveyList[i + 1].MdWGS84, minMD) && Numeric.LE(_surveyList[i + 1].MdWGS84, maxMD))
//                        {
//                            // Next survey is ok, then we should also add the current
//                            ok = true;
//                        }
//                    }
//                    if (!ok)
//                    {
//                        if (i > 0 && Numeric.GE(_surveyList[i - 1].MdWGS84, minMD) && Numeric.LE(_surveyList[i - 1].MdWGS84, maxMD))
//                        {
//                            // Previous survey was ok, then we should also add the current
//                            ok = true;
//                        }
//                    }
//                }
//                if (ok)
//                {
//                    if (_surveyList[i].Uncertainty == null)
//                    {
//                        WdWSurveyStationUncertainty wdwun = new WdWSurveyStationUncertainty();
//                        SurveyInstrument.Model.SurveyInstrument surveyTool = new SurveyInstrument.Model.SurveyInstrument(SurveyInstrument.Model.SurveyInstrument.WdWGoodMag);
//                        _surveyList[i].SurveyTool = surveyTool;
//                        _surveyList[i].Uncertainty = wdwun;
//                    }
//                    //if (((_useWdwCovariance == _surveyList[i].Uncertainty is WdWSurveyStationUncertainty && i > 0) || (_surveyList.Count>1 && _surveyList[i].Uncertainty.Covariance[0,0]==null )) )
//                    //{
//                    //    WdWSurveyStationUncertainty wdwSurveyStatoinUncertainty = (WdWSurveyStationUncertainty)_surveyList[i].Uncertainty;
//                    //    A = wdwSurveyStatoinUncertainty.CalculateCovariances(_surveyList[i], _surveyList[i - 1], A);
//                    //}
//                    //if (((_surveyList[i].Uncertainty is ISCWSA_SurveyStationUncertainty && i > 0) || (_surveyList.Count > 1 && _surveyList[i].Uncertainty.Covariance[0, 0] == null)))
//                    //{
//                    //    ISCWSA_SurveyStationUncertainty ISCWSASurveyStatoinUncertainty = (ISCWSA_SurveyStationUncertainty)_surveyList[i].Uncertainty;
//                    //    if (i == _surveyList.Count - 1)
//                    //    {
//                    //        SurveyStation surveyStationNext = new SurveyStation();
//                    //        surveyStationNext.NorthOfWellHead  = 0.0;
//                    //        surveyStationNext.EastOfWellHead = 0.0;
//                    //        surveyStationNext.Incl = 0.0;
//                    //        surveyStationNext.AzWGS84 = 0.0;
//                    //        surveyStationNext.MD = 0.0;
//                    //        ISCWSASurveyStatoinUncertainty.CalculateCovariance(_surveyList[i], _surveyList[i - 1], surveyStationNext, ISCWSAErrorDataTmp, i);
//                    //    }
//                    //    else
//                    //    {
//                    //        ISCWSASurveyStatoinUncertainty.CalculateCovariance(_surveyList[i], _surveyList[i - 1], _surveyList[i + 1], ISCWSAErrorDataTmp, i);
//                    //    }

//                    //    ISCWSAErrorDataTmp = ISCWSASurveyStatoinUncertainty.ISCWSAErrorDataTmp;
//                    //}
//                    //Always calculate new Covariances
//                    if ((_useWdwCovariance == _surveyList[i].Uncertainty is WdWSurveyStationUncertainty && i > 0))
//                    {
//                        WdWSurveyStationUncertainty wdwSurveyStatoinUncertainty = (WdWSurveyStationUncertainty)_surveyList[i].Uncertainty;
//                        A = wdwSurveyStatoinUncertainty.CalculateCovariances(_surveyList[i], _surveyList[i - 1], A);
//                    }
//                    if ((_surveyList[i].Uncertainty is ISCWSA_SurveyStationUncertainty && i > 0))
//                    {
//                        ISCWSA_SurveyStationUncertainty ISCWSASurveyStatoinUncertainty = (ISCWSA_SurveyStationUncertainty)_surveyList[i].Uncertainty;
//                        if (i == _surveyList.Count - 1)
//                        {
//                            SurveyStation surveyStationNext = new SurveyStation();
//                            surveyStationNext.NorthOfWellHead = 0.0;
//                            surveyStationNext.EastOfWellHead = 0.0;
//                            surveyStationNext.Incl = 0.0;
//                            surveyStationNext.AzWGS84 = 0.0;
//                            surveyStationNext.MdWGS84 = 0.0;
//                            ISCWSASurveyStatoinUncertainty.CalculateCovariance(_surveyList[i], _surveyList[i - 1], surveyStationNext, ISCWSAErrorDataTmp, i);
//                        }
//                        else
//                        {
//                            ISCWSASurveyStatoinUncertainty.CalculateCovariance(_surveyList[i], _surveyList[i - 1], _surveyList[i + 1], ISCWSAErrorDataTmp, i);
//                        }

//                        ISCWSAErrorDataTmp = ISCWSASurveyStatoinUncertainty.ISCWSAErrorDataTmp;
//                    }
//                    _surveyList[i].Uncertainty.Calculate(_surveyList[i], confidenceFactor, scalingFactor);
//                    surveyList.Add(_surveyList[i]);
//                    if (UseUncertaintyCylinder)
//                    {
//                        CalculateUncertaintyCylinder(_surveyList[i], confidenceFactor);
//                    }
//                }
//            }

//            List<EnvelopeOfUncertaintyEllipse> uncertaintyEnvelope = new List<EnvelopeOfUncertaintyEllipse>();

//            for (int i = 0; i < surveyList.Count - 1; i++)
//            {
//                EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipse = new EnvelopeOfUncertaintyEllipse();

//                Vector2D ellipseRadius = surveyList[i].Uncertainty.EllipseRadius;
//                Vector2D ellipseRadiusNext = surveyList[i + 1].Uncertainty.EllipseRadius;
//                double distance = (double)surveyList[i + 1].MdWGS84 - (double)surveyList[i].MdWGS84;
//                _intermediateEllipseNumbers = (int)(distance / MaxDistanceEllipse);

//                uncertaintyEnvelopeEllipse.Azimuth = surveyList[i].AzWGS84;
//                uncertaintyEnvelopeEllipse.Inclination = surveyList[i].Incl;
//                uncertaintyEnvelopeEllipse.X = surveyList[i].NorthOfWellHead;
//                uncertaintyEnvelopeEllipse.Y = surveyList[i].EastOfWellHead;
//                uncertaintyEnvelopeEllipse.Z = surveyList[i].TvdWGS84;
//                uncertaintyEnvelopeEllipse.MD = surveyList[i].MdWGS84;
//                uncertaintyEnvelopeEllipse.LatitudeWGS84 = surveyList[i].LatitudeWGS84;
//                uncertaintyEnvelopeEllipse.LongitudeWGS84 = surveyList[i].LongitudeWGS84;
//                uncertaintyEnvelopeEllipse.EllipseRadius = ellipseRadius;
//                uncertaintyEnvelopeEllipse.PerpendicularDirection = surveyList[i].Uncertainty.PerpendicularDirection;
//                List<GlobalCoordinatePoint3D> ellipseCoordinates = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipse);
//                uncertaintyEnvelopeEllipse.EllipseCoordinates = ellipseCoordinates;
//                if (calculateEllipseAreaCoordinates)
//                {
//                    List<GlobalCoordinatePoint3D> ellipseAreaCoordinates = GetUncertaintyEllipseAreaCoordinates(uncertaintyEnvelopeEllipse);
//                    uncertaintyEnvelopeEllipse.EllipseAreaCoordinates = ellipseAreaCoordinates;
//                }
//                bool ok = true;
//                if (minTVD != null && maxTVD != null)
//                {
//                    ok = Numeric.GE(uncertaintyEnvelopeEllipse.Z, minTVD) && Numeric.LE(uncertaintyEnvelopeEllipse.Z, maxTVD);
//                }
//                else if (minMD != null && maxMD != null)
//                {
//                    ok = Numeric.GE(uncertaintyEnvelopeEllipse.MD, minMD) && Numeric.LE(uncertaintyEnvelopeEllipse.MD, maxMD);
//                }
//                if (ok)
//                {
//                    uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipse);
//                }

//                bool skipLast = false;
//                // n = 0 corresponds to the uncertaintyEnvelopeEllipse added above
//                for (int n = 1; n < _intermediateEllipseNumbers; n++)
//                {
//                    Vector2D ellipseR = new Vector2D();
//                    ellipseR[0] = ellipseRadius[0] + (double)n * (ellipseRadiusNext[0] - ellipseRadius[0]) / (double)_intermediateEllipseNumbers;
//                    ellipseR[1] = ellipseRadius[1] + (double)n * (ellipseRadiusNext[1] - ellipseRadius[1]) / (double)_intermediateEllipseNumbers;
//                    double inclination = ((double)surveyList[i].Incl + (double)n * ((double)surveyList[i + 1].Incl - (double)surveyList[i].Incl) / (double)_intermediateEllipseNumbers);
//                    double azimuth = ((double)surveyList[i].AzWGS84 + (double)n * ((double)surveyList[i + 1].AzWGS84 - (double)surveyList[i].AzWGS84) / (double)_intermediateEllipseNumbers);
//                    double north = (double)surveyList[i].NorthOfWellHead + (double)n * ((double)surveyList[i + 1].NorthOfWellHead - (double)surveyList[i].NorthOfWellHead) / (double)_intermediateEllipseNumbers;
//                    double east = (double)surveyList[i].EastOfWellHead + (double)n * ((double)surveyList[i + 1].EastOfWellHead - (double)surveyList[i].EastOfWellHead) / (double)_intermediateEllipseNumbers;
//                    double tvd = (double)surveyList[i].TvdWGS84 + (double)n * ((double)surveyList[i + 1].TvdWGS84 - (double)surveyList[i].TvdWGS84) / (double)_intermediateEllipseNumbers;
//                    double md = (double)surveyList[i].MdWGS84 + (double)n * ((double)surveyList[i + 1].MdWGS84 - (double)surveyList[i].MdWGS84) / (double)_intermediateEllipseNumbers;
//                    double perpendicularDirection = surveyList[i].Uncertainty.PerpendicularDirection + (double)n * (surveyList[i + 1].Uncertainty.PerpendicularDirection - surveyList[i + 1].Uncertainty.PerpendicularDirection) / (double)_intermediateEllipseNumbers;

//                    //ellipseR[0] = ellipseRadius[0] + (double)_intermediateEllipseNumbers * (ellipseRadiusNext[0] - ellipseRadius[0]) / (double)_intermediateEllipseNumbers;
//                    //ellipseR[1] = ellipseRadius[1] + (double)_intermediateEllipseNumbers * (ellipseRadiusNext[1] - ellipseRadius[1]) / (double)_intermediateEllipseNumbers;
//                    ////double inclination = ((double)surveyList[i].Incl + (double)_intermediateEllipseNumbers * ((double)surveyList[i + 1].Incl - (double)surveyList[i].Incl) / (double)_intermediateEllipseNumbers);
//                    ////double azimuth = ((double)surveyList[i].AzWGS84 + (double)_intermediateEllipseNumbers * ((double)surveyList[i + 1].AzWGS84 - (double)surveyList[i].AzWGS84) / (double)_intermediateEllipseNumbers);
//                    ////double north = (double)surveyList[i].NorthOfWellHead  + (double)_intermediateEllipseNumbers * ((double)surveyList[i + 1].NorthOfWellHead  - (double)surveyList[i].NorthOfWellHead ) / (double)_intermediateEllipseNumbers;
//                    ////double east = (double)surveyList[i].EastOfWellHead + (double)_intermediateEllipseNumbers * ((double)surveyList[i + 1].EastOfWellHead - (double)surveyList[i].EastOfWellHead) / (double)_intermediateEllipseNumbers;
//                    ////double tvd = (double)surveyList[i].TvdWGS84 + (double)_intermediateEllipseNumbers * ((double)surveyList[i + 1].TvdWGS84 - (double)surveyList[i].TvdWGS84) / (double)_intermediateEllipseNumbers;
//                    ////double md = (double)surveyList[i].MD + (double)_intermediateEllipseNumbers * ((double)surveyList[i + 1].MD - (double)surveyList[i].MD) / (double)_intermediateEllipseNumbers;
//                    //double perpendicularDirection = surveyList[i].Uncertainty.PerpendicularDirection + (double)_intermediateEllipseNumbers * (surveyList[i + 1].Uncertainty.PerpendicularDirection - surveyList[i + 1].Uncertainty.PerpendicularDirection) / (double)_intermediateEllipseNumbers;


//                    EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipseInter = new EnvelopeOfUncertaintyEllipse();
//                    uncertaintyEnvelopeEllipseInter.Azimuth = azimuth;
//                    uncertaintyEnvelopeEllipseInter.Inclination = inclination;
//                    uncertaintyEnvelopeEllipseInter.X = north;
//                    uncertaintyEnvelopeEllipseInter.Y = east;
//                    uncertaintyEnvelopeEllipseInter.Z = tvd;
//                    uncertaintyEnvelopeEllipseInter.EllipseRadius = ellipseR;
//                    uncertaintyEnvelopeEllipseInter.PerpendicularDirection = perpendicularDirection;

//                    ok = true;
//                    if (minTVD != null && maxTVD != null)
//                    {
//                        ok = Numeric.GE(uncertaintyEnvelopeEllipseInter.Z, minTVD) && Numeric.LE(uncertaintyEnvelopeEllipseInter.Z, maxTVD);
//                    }
//                    else if (minMD != null && maxMD != null)
//                    {
//                        ok = Numeric.GE(md, minMD) && Numeric.LE(md, maxMD);
//                    }
//                    if (ok)
//                    {
//                        List<GlobalCoordinatePoint3D> ellipseCoordinatesInter = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipseInter);
//                        uncertaintyEnvelopeEllipseInter.EllipseCoordinates = ellipseCoordinatesInter;
//                        if (calculateEllipseAreaCoordinates)
//                        {
//                            List<GlobalCoordinatePoint3D> ellipseAreaCoordinatesInter = GetUncertaintyEllipseAreaCoordinates(uncertaintyEnvelopeEllipseInter);
//                            uncertaintyEnvelopeEllipseInter.EllipseAreaCoordinates = ellipseAreaCoordinatesInter;
//                        }
//                        uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipseInter);
//                        if (maxEllipsesCount != null && uncertaintyEnvelope.Count == maxEllipsesCount)
//                        {
//                            skipLast = true;
//                            break;
//                        }
//                    }

//                }
//                if (skipLast)
//                {
//                    break;
//                }
//                if (i == surveyList.Count - 2)
//                {
//                    uncertaintyEnvelopeEllipse = new EnvelopeOfUncertaintyEllipse();
//                    //Vector2D ellipseRadius = new Vector2D();
//                    ellipseRadius = surveyList[surveyList.Count - 1].Uncertainty.EllipseRadius;

//                    uncertaintyEnvelopeEllipse.Azimuth = surveyList[surveyList.Count - 1].AzWGS84;
//                    uncertaintyEnvelopeEllipse.Inclination = surveyList[surveyList.Count - 1].Incl;
//                    uncertaintyEnvelopeEllipse.X = surveyList[surveyList.Count - 1].NorthOfWellHead;
//                    uncertaintyEnvelopeEllipse.Y = surveyList[surveyList.Count - 1].EastOfWellHead;
//                    uncertaintyEnvelopeEllipse.Z = surveyList[surveyList.Count - 1].TvdWGS84;
//                    uncertaintyEnvelopeEllipse.MD = surveyList[surveyList.Count - 1].MdWGS84;
//                    uncertaintyEnvelopeEllipse.EllipseRadius = ellipseRadius;

//                    uncertaintyEnvelopeEllipse.PerpendicularDirection = surveyList[surveyList.Count - 1].Uncertainty.PerpendicularDirection;


//                    ok = true;
//                    if (minTVD != null && maxTVD != null)
//                    {
//                        ok = Numeric.GE(uncertaintyEnvelopeEllipse.Z, minTVD) && Numeric.LE(uncertaintyEnvelopeEllipse.Z, maxTVD);
//                    }
//                    else if (minMD != null && maxMD != null)
//                    {
//                        ok = Numeric.GE(uncertaintyEnvelopeEllipse.MD, minMD) && Numeric.LE(uncertaintyEnvelopeEllipse.MD, maxMD);
//                    }
//                    if (ok)
//                    {
//                        ellipseCoordinates = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipse);
//                        uncertaintyEnvelopeEllipse.EllipseCoordinates = ellipseCoordinates;
//                        if (calculateEllipseAreaCoordinates)
//                        {
//                            List<GlobalCoordinatePoint3D> ellipseAreaCoordinates = GetUncertaintyEllipseAreaCoordinates(uncertaintyEnvelopeEllipse);
//                            uncertaintyEnvelopeEllipse.EllipseAreaCoordinates = ellipseAreaCoordinates;
//                        }
//                        uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipse);
//                    }

//                }
//            }
//            if (cluster != null && well != null && cluster.Slots != null && !string.IsNullOrEmpty(well.SlotID))
//            {

//                WellCoordinateConversionSet conversionSet = new WellCoordinateConversionSet();
//                conversionSet.Field = cluster.Field;
//                conversionSet.Cluster = cluster;
//                conversionSet.Well = well;
//                conversionSet.WellCoordinates = new List<WellCoordinate>();

//                foreach (EnvelopeOfUncertaintyEllipse ellipse in uncertaintyEnvelope)
//                {
//                    foreach (GlobalCoordinatePoint3D gc in ellipse.EllipseCoordinates)
//                    {
//                        WellCoordinate coordinate = new WellCoordinate();
//                        coordinate.NorthOfWellHead = gc.NorthOfWellHead;
//                        coordinate.EastOfWellHead = gc.EastOfWellHead;
//                        coordinate.TVDWGS84 = gc.TvdWGS84;
//                        conversionSet.WellCoordinates.Add(coordinate);
//                    }
//                }
//                conversionSet.Calculate();

//                int index = 0;
//                for (int i = 0; i < uncertaintyEnvelope.Count; i++)
//                {
//                    index = i * uncertaintyEnvelope[i].EllipseCoordinates.Count;
//                    for (int j = 0; j < uncertaintyEnvelope[i].EllipseCoordinates.Count; j++)
//                    {
//                        uncertaintyEnvelope[i].EllipseCoordinates[j].LatitudeWGS84 = conversionSet.WellCoordinates[index + j].LatitudeWGS84;
//                        uncertaintyEnvelope[i].EllipseCoordinates[j].LongitudeWGS84 = conversionSet.WellCoordinates[index + j].LongitudeWGS84;
//                        uncertaintyEnvelope[i].EllipseCoordinates[j].TvdWGS84 = conversionSet.WellCoordinates[index + j].TVDWGS84; // This one is not required, but in case we mess up somewhere, it is better that all points are consistent
//                    }
//                }
//            }

//            UncertaintyEnvelope = uncertaintyEnvelope;
//            return uncertaintyEnvelope;
//        }

//        public List<EnvelopeOfUncertaintyEllipse> GetPlainUncertaintyEnvelope(double confidenceFactor, double scalingFactor, double boreholeRadius, int intermediateEllipseNumbers = 0, double? minimumDistanceMD = null)
//        {
//            double[,] A = new double[6, 3];
//            for (int i = 0; i < A.GetLength(0); i++)
//            {
//                for (int j = 0; j < A.GetLength(1); j++)
//                {
//                    A[i, j] = 0.0;
//                }
//            }

//            // Start from i = 0 to include the first surveystation. This will typically have radius 0
//            for (int i = 0; i < _surveyList.Count; i++)
//            {
//                if (_surveyList[i].Uncertainty == null)
//                {
//                    WdWSurveyStationUncertainty wdwun = new WdWSurveyStationUncertainty();
//                    SurveyInstrument.Model.SurveyInstrument surveyTool = new SurveyInstrument.Model.SurveyInstrument(SurveyInstrument.Model.SurveyInstrument.WdWGoodMag);
//                    _surveyList[i].SurveyTool = surveyTool;
//                    _surveyList[i].Uncertainty = wdwun;
//                }
//                if (((_useWdwCovariance == _surveyList[i].Uncertainty is WdWSurveyStationUncertainty && i > 0) || (_surveyList.Count > 1 && _surveyList[i].Uncertainty.Covariance[0, 0] == null)))
//                {
//                    WdWSurveyStationUncertainty wdwSurveyStatoinUncertainty = (WdWSurveyStationUncertainty)_surveyList[i].Uncertainty;
//                    A = wdwSurveyStatoinUncertainty.CalculateCovariances(_surveyList[i], _surveyList[i - 1], A);
//                }
//                if (_useWdwCovariance == _surveyList[i].Uncertainty is WdWSurveyStationUncertainty && i > 0)
//                {
//                    WdWSurveyStationUncertainty wdwSurveyStatoinUncertainty = (WdWSurveyStationUncertainty)_surveyList[i].Uncertainty;
//                    A = wdwSurveyStatoinUncertainty.CalculateCovariances(_surveyList[i], _surveyList[i - 1], A);
//                }
//                _surveyList[i].Uncertainty.Calculate(_surveyList[i], confidenceFactor, scalingFactor, boreholeRadius);
//            }

//            List<EnvelopeOfUncertaintyEllipse> uncertaintyEnvelope = new List<EnvelopeOfUncertaintyEllipse>();

//            for (int i = 0; i < _surveyList.Count - 1; i++)
//            {
//                EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipse = new EnvelopeOfUncertaintyEllipse();

//                Vector2D ellipseRadius = new Vector2D();
//                ellipseRadius = _surveyList[i].Uncertainty.EllipseRadius;
//                Vector2D ellipseRadiusNext = new Vector2D();
//                ellipseRadiusNext = _surveyList[i + 1].Uncertainty.EllipseRadius;
//                double distance = (double)_surveyList[i + 1].MdWGS84 - (double)_surveyList[i].MdWGS84;

//                uncertaintyEnvelopeEllipse.Azimuth = _surveyList[i].AzWGS84;
//                uncertaintyEnvelopeEllipse.Inclination = _surveyList[i].Incl;
//                uncertaintyEnvelopeEllipse.X = _surveyList[i].NorthOfWellHead;
//                uncertaintyEnvelopeEllipse.Y = _surveyList[i].EastOfWellHead;
//                uncertaintyEnvelopeEllipse.Z = _surveyList[i].TvdWGS84;
//                uncertaintyEnvelopeEllipse.MD = _surveyList[i].MdWGS84;
//                uncertaintyEnvelopeEllipse.EllipseRadius = ellipseRadius;
//                List<GlobalCoordinatePoint3D> ellipseCoordinates = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipse);
//                uncertaintyEnvelopeEllipse.EllipseCoordinates = ellipseCoordinates;
//                uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipse);

//                if (minimumDistanceMD != null)
//                {
//                    intermediateEllipseNumbers = (int)System.Math.Ceiling(distance / (double)minimumDistanceMD);
//                }
//                for (int n = 1; n < intermediateEllipseNumbers; n++)
//                {
//                    Vector2D ellipseR = new Vector2D();
//                    ellipseR[0] = ellipseRadius[0] + (double)n * (ellipseRadiusNext[0] - ellipseRadius[0]) / (double)intermediateEllipseNumbers;
//                    ellipseR[1] = ellipseRadius[1] + (double)n * (ellipseRadiusNext[1] - ellipseRadius[1]) / (double)intermediateEllipseNumbers;

//                    double inclination = ((double)_surveyList[i].Incl + (double)n * ((double)_surveyList[i + 1].Incl - (double)_surveyList[i].Incl) / (double)intermediateEllipseNumbers);
//                    double azimuth = ((double)_surveyList[i].AzWGS84 + (double)n * ((double)_surveyList[i + 1].AzWGS84 - (double)_surveyList[i].AzWGS84) / (double)intermediateEllipseNumbers);
//                    double north = (double)_surveyList[i].NorthOfWellHead + (double)n * ((double)_surveyList[i + 1].NorthOfWellHead - (double)_surveyList[i].NorthOfWellHead) / (double)intermediateEllipseNumbers;
//                    double east = (double)_surveyList[i].EastOfWellHead + (double)n * ((double)_surveyList[i + 1].EastOfWellHead - (double)_surveyList[i].EastOfWellHead) / (double)intermediateEllipseNumbers;
//                    double tvd = (double)_surveyList[i].TvdWGS84 + (double)n * ((double)_surveyList[i + 1].TvdWGS84 - (double)_surveyList[i].TvdWGS84) / (double)intermediateEllipseNumbers;
//                    double md = (double)_surveyList[i].MdWGS84 + (double)n * ((double)_surveyList[i + 1].MdWGS84 - (double)_surveyList[i].MdWGS84) / (double)intermediateEllipseNumbers;
//                    double perpendicularDirection = _surveyList[i].Uncertainty.PerpendicularDirection + (double)n * (_surveyList[i + 1].Uncertainty.PerpendicularDirection - _surveyList[i + 1].Uncertainty.PerpendicularDirection) / (double)intermediateEllipseNumbers;

//                    EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipseInter = new EnvelopeOfUncertaintyEllipse();
//                    uncertaintyEnvelopeEllipseInter.Azimuth = azimuth;
//                    uncertaintyEnvelopeEllipseInter.Inclination = inclination;
//                    uncertaintyEnvelopeEllipseInter.X = north;
//                    uncertaintyEnvelopeEllipseInter.Y = east;
//                    uncertaintyEnvelopeEllipseInter.Z = tvd;
//                    uncertaintyEnvelopeEllipseInter.MD = md;
//                    uncertaintyEnvelopeEllipseInter.EllipseRadius = ellipseR;

//                    uncertaintyEnvelopeEllipseInter.PerpendicularDirection = perpendicularDirection;

//                    List<GlobalCoordinatePoint3D> ellipseCoordinatesInter = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipseInter);
//                    uncertaintyEnvelopeEllipseInter.EllipseCoordinates = ellipseCoordinatesInter;
//                    uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipseInter);
//                }

//                if (i == _surveyList.Count - 2)
//                {
//                    uncertaintyEnvelopeEllipse = new EnvelopeOfUncertaintyEllipse();
//                    //Vector2D ellipseRadius = new Vector2D();
//                    ellipseRadius = _surveyList[_surveyList.Count - 1].Uncertainty.EllipseRadius;

//                    uncertaintyEnvelopeEllipse.Azimuth = _surveyList[_surveyList.Count - 1].AzWGS84;
//                    uncertaintyEnvelopeEllipse.Inclination = _surveyList[_surveyList.Count - 1].Incl;
//                    uncertaintyEnvelopeEllipse.X = _surveyList[_surveyList.Count - 1].NorthOfWellHead;
//                    uncertaintyEnvelopeEllipse.Y = _surveyList[_surveyList.Count - 1].EastOfWellHead;
//                    uncertaintyEnvelopeEllipse.Z = _surveyList[_surveyList.Count - 1].TvdWGS84;
//                    uncertaintyEnvelopeEllipse.MD = _surveyList[_surveyList.Count - 1].MdWGS84;
//                    uncertaintyEnvelopeEllipse.EllipseRadius = ellipseRadius;
//                    ellipseCoordinates = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipse);
//                    uncertaintyEnvelopeEllipse.EllipseCoordinates = ellipseCoordinates;
//                    uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipse);
//                }
//            }
//            UncertaintyEnvelope = uncertaintyEnvelope;
//            return uncertaintyEnvelope;
//        }

//        private void CalculateUncertaintyCylinder(SurveyStation surveyStation, double confidenceFactor)
//        {
//            double xMinEllipsoid = 0.0;
//            double xMaxEllipsoid = 0.0;
//            double yMinEllipsoid = 0.0;
//            double yMaxEllipsoid = 0.0;
//            double zMinEllipsoid = 0.0;
//            double zMaxEllipsoid = 0.0;
//            CalculateExtremumInDepths(surveyStation, confidenceFactor, ref xMinEllipsoid, ref xMaxEllipsoid, ref yMinEllipsoid, ref yMaxEllipsoid, ref zMinEllipsoid, ref zMaxEllipsoid);

//            List<EnvelopeOfUncertaintyEllipse> uncertaintyEnvelope = new List<EnvelopeOfUncertaintyEllipse>();

//            //for (int i = 0; i < surveyList.Count - 1; i++)
//            {

//                double xMinEllipse = Numeric.MAX_DOUBLE;
//                double xMaxEllipse = Numeric.MIN_DOUBLE;
//                double yMinEllipse = Numeric.MAX_DOUBLE;
//                double yMaxEllipse = Numeric.MIN_DOUBLE;
//                double zMinEllipse = Numeric.MAX_DOUBLE;
//                double zMaxEllipse = Numeric.MIN_DOUBLE;

//                EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipse = new EnvelopeOfUncertaintyEllipse();

//                Vector2D ellipseRadius = surveyStation.Uncertainty.EllipseRadius;
//                Vector2D ellipseRadiusNext = surveyStation.Uncertainty.EllipseRadius;
//                double distance = 0;// MD - MD;
//                double _intermediateEllipseNumbers = 10;

//                uncertaintyEnvelopeEllipse.Azimuth = surveyStation.AzWGS84;
//                uncertaintyEnvelopeEllipse.Inclination = surveyStation.Incl;
//                uncertaintyEnvelopeEllipse.X = surveyStation.NorthOfWellHead;
//                uncertaintyEnvelopeEllipse.Y = surveyStation.EastOfWellHead;
//                uncertaintyEnvelopeEllipse.Z = surveyStation.TvdWGS84;
//                uncertaintyEnvelopeEllipse.MD = (double)surveyStation.MdWGS84;
//                uncertaintyEnvelopeEllipse.EllipseRadius = ellipseRadius;
//                uncertaintyEnvelopeEllipse.PerpendicularDirection = surveyStation.Uncertainty.PerpendicularDirection;
//                List<GlobalCoordinatePoint3D> ellipseCoordinates = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipse, 0, ref xMinEllipse, ref xMaxEllipse, ref yMinEllipse, ref yMaxEllipse, ref zMinEllipse, ref zMaxEllipse);
//                uncertaintyEnvelopeEllipse.EllipseCoordinates = ellipseCoordinates;
//                uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipse);

//                double z = 0;
//                double zAdd = 2;
//                bool increasingZ = false;
//                bool increasingX = false;
//                bool increasingY = false;
//                bool stop = false;
//                double zMinPrev = zMinEllipse;
//                double xMinPrev = xMinEllipse;
//                double yMinPrev = yMinEllipse;
//                if (surveyStation.Incl > Numeric.PI / 2)
//                {
//                    stop = true;
//                }
//                if (xMinEllipsoid < xMinEllipse || yMinEllipsoid < yMinEllipse || zMinEllipsoid < zMinEllipse)
//                {
//                    z -= zAdd;
//                    EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipseInter = new EnvelopeOfUncertaintyEllipse();
//                    uncertaintyEnvelopeEllipseInter.Azimuth = surveyStation.AzWGS84;
//                    uncertaintyEnvelopeEllipseInter.Inclination = surveyStation.Incl;
//                    uncertaintyEnvelopeEllipseInter.X = surveyStation.NorthOfWellHead;
//                    uncertaintyEnvelopeEllipseInter.Y = surveyStation.EastOfWellHead;
//                    uncertaintyEnvelopeEllipseInter.Z = surveyStation.TvdWGS84;
//                    uncertaintyEnvelopeEllipseInter.MD = (double)surveyStation.MdWGS84;
//                    uncertaintyEnvelopeEllipseInter.EllipseRadius = ellipseRadius;
//                    uncertaintyEnvelopeEllipseInter.PerpendicularDirection = surveyStation.Uncertainty.PerpendicularDirection;
//                    List<GlobalCoordinatePoint3D> ellipseCoordinatesInter = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipseInter, z, ref xMinEllipse, ref xMaxEllipse, ref yMinEllipse, ref yMaxEllipse, ref zMinEllipse, ref zMaxEllipse);
//                    if (zMinPrev < zMinEllipse)
//                    {
//                        increasingZ = true;
//                    }
//                    if (xMinPrev <= xMinEllipse)
//                    {
//                        increasingX = true;
//                    }
//                    if (yMinPrev <= yMinEllipse)
//                    {
//                        increasingY = true;
//                    }
//                    if (zMinPrev == zMinEllipse || surveyStation.Incl > Numeric.PI / 2 || Numeric.EQ(surveyStation.AzWGS84, Numeric.PI, 0.01))
//                    {
//                        stop = true;
//                    }
//                    uncertaintyEnvelopeEllipseInter.EllipseCoordinates = ellipseCoordinatesInter;
//                    uncertaintyEnvelope.Insert(0, uncertaintyEnvelopeEllipseInter);
//                }
//                //if (!stop)
//                {
//                    while ((!stop && (zMinEllipsoid < zMinEllipse)) || ((!increasingX && Numeric.LT(xMinEllipsoid, xMinEllipse, 0.01)) || (increasingX && Numeric.GT(xMaxEllipsoid, xMaxEllipse, 0.01))) || ((!increasingY && Numeric.LT(yMinEllipsoid, yMinEllipse, 0.01)) || (increasingY && Numeric.GT(yMaxEllipsoid, yMaxEllipse, 0.01))))
//                    {
//                        z -= zAdd;
//                        EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipseInter = new EnvelopeOfUncertaintyEllipse();
//                        uncertaintyEnvelopeEllipseInter.Azimuth = surveyStation.AzWGS84;
//                        uncertaintyEnvelopeEllipseInter.Inclination = surveyStation.Incl;
//                        uncertaintyEnvelopeEllipseInter.X = surveyStation.NorthOfWellHead;
//                        uncertaintyEnvelopeEllipseInter.Y = surveyStation.EastOfWellHead;
//                        uncertaintyEnvelopeEllipseInter.Z = surveyStation.TvdWGS84;
//                        uncertaintyEnvelopeEllipseInter.MD = (double)surveyStation.MdWGS84;
//                        uncertaintyEnvelopeEllipseInter.EllipseRadius = ellipseRadius;
//                        uncertaintyEnvelopeEllipseInter.PerpendicularDirection = surveyStation.Uncertainty.PerpendicularDirection;
//                        List<GlobalCoordinatePoint3D> ellipseCoordinatesInter = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipseInter, z, ref xMinEllipse, ref xMaxEllipse, ref yMinEllipse, ref yMaxEllipse, ref zMinEllipse, ref zMaxEllipse);
//                        uncertaintyEnvelopeEllipseInter.EllipseCoordinates = ellipseCoordinatesInter;
//                        uncertaintyEnvelope.Insert(0, uncertaintyEnvelopeEllipseInter);
//                    }
//                    z = 0;
//                    while (((!increasingX && Numeric.GT(xMaxEllipsoid, xMaxEllipse, 0.01)) || (increasingX && Numeric.LT(xMinEllipsoid, xMinEllipse, 0.01))) || ((!increasingY && Numeric.GT(yMaxEllipsoid, yMaxEllipse, 0.01)) || (increasingY && Numeric.LT(yMinEllipsoid, yMinEllipse, 0.01))) || (!stop && zMaxEllipsoid > zMaxEllipse))
//                    //while ( zMax > zMaxEllipse)
//                    {
//                        z += zAdd;
//                        EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipseInter = new EnvelopeOfUncertaintyEllipse();
//                        uncertaintyEnvelopeEllipseInter.Azimuth = surveyStation.AzWGS84;
//                        uncertaintyEnvelopeEllipseInter.Inclination = surveyStation.Incl;
//                        uncertaintyEnvelopeEllipseInter.X = surveyStation.NorthOfWellHead;
//                        uncertaintyEnvelopeEllipseInter.Y = surveyStation.EastOfWellHead;
//                        uncertaintyEnvelopeEllipseInter.Z = surveyStation.TvdWGS84;
//                        uncertaintyEnvelopeEllipseInter.MD = (double)surveyStation.MdWGS84;
//                        uncertaintyEnvelopeEllipseInter.EllipseRadius = ellipseRadius;
//                        uncertaintyEnvelopeEllipseInter.PerpendicularDirection = surveyStation.Uncertainty.PerpendicularDirection;
//                        List<GlobalCoordinatePoint3D> ellipseCoordinatesInter = GetUncertaintyEllipseCoordinates(uncertaintyEnvelopeEllipseInter, z, ref xMinEllipse, ref xMaxEllipse, ref yMinEllipse, ref yMaxEllipse, ref zMinEllipse, ref zMaxEllipse);
//                        uncertaintyEnvelopeEllipseInter.EllipseCoordinates = ellipseCoordinatesInter;
//                        uncertaintyEnvelope.Add(uncertaintyEnvelopeEllipseInter);

//                        CalculateExtremumInDepths(surveyStation, confidenceFactor, ref xMinEllipsoid, ref xMaxEllipsoid, ref yMinEllipsoid, ref yMaxEllipsoid, ref zMinEllipsoid, ref zMaxEllipsoid);
//                    }
//                }
//            }
//            surveyStation.Uncertainty.UncertaintyCylinder = uncertaintyEnvelope;
//        }

//        public void CalculateExtremumInDepths(SurveyStation surveyStation, double p, ref double xMin, ref double xMax, ref double yMin, ref double yMax, ref double zMin, ref double zMax)
//        {
//            // calculate the parameters of the ellipsoid
//            double chiSquare = GetChiSquare3D(p);
//            // inverse the matrix
//            double h11 = (double)surveyStation.Uncertainty.Covariance[0, 0];
//            double h12 = (double)surveyStation.Uncertainty.Covariance[0, 1];
//            double h13 = (double)surveyStation.Uncertainty.Covariance[0, 2];
//            double h22 = (double)surveyStation.Uncertainty.Covariance[1, 1];
//            double h23 = (double)surveyStation.Uncertainty.Covariance[1, 2];
//            double h33 = (double)surveyStation.Uncertainty.Covariance[2, 2];
//            double determinant = (h11 * h22 - h12 * h12) * h33 - h11 * h23 * h23 + 2 * h12 * h13 * h23 - h13 * h13 * h22;
//            double H11 = (h22 * h33 - h23 * h23) / determinant;
//            double H21 = -(h12 * h33 - h13 * h23) / determinant;
//            double H31 = (h12 * h23 - h13 * h22) / determinant;
//            double H12 = H21;
//            double H22 = (h11 * h33 - h13 * h13) / determinant;
//            double H32 = -(h11 * h23 - h12 * h13) / determinant;
//            double H13 = H31;
//            double H23 = H32;
//            double H33 = (h11 * h22 - h12 * h12) / determinant;

//            // calculate extremum in Z
//            double denominator = H11 * H22 - H12 * H12;
//            double dl = Numeric.SqrtEqual(chiSquare * ((H11 * H22 * H33) / denominator - (H12 * H12 * H33) / denominator - (H11 * H23 * H23) / denominator + (2.0 * H12 * H13 * H23) / denominator - (H13 * H13 * H22) / denominator));
//            determinant = (H11 * H22 - H12 * H12) * H33 - H11 * H23 * H23 + 2 * H12 * H13 * H23 - H13 * H13 * H22;
//            xMin = dl * (H13 * H22 - H12 * H23) / determinant;
//            yMin = dl * (-H12 * H13 + H11 * H23) / determinant;
//            zMin = dl * (H12 * H12 - H11 * H22) / determinant;
//            xMax = -xMin;
//            yMax = -yMin;
//            zMax = -zMin;

//            if (zMin < zMax)
//            {
//                //swap
//                double tt = xMin;
//                xMin = xMax;
//                xMax = tt;
//                tt = yMin;
//                yMin = yMax;
//                yMax = tt;
//                tt = zMin;
//                //zMin = zMax;
//                //zMax = tt;
//            }
//            //add bias and survey position
//            xMin += (double)surveyStation.NorthOfWellHead;
//            yMin += (double)surveyStation.EastOfWellHead;
//            zMin += (double)surveyStation.TvdWGS84;
//            xMax += (double)surveyStation.NorthOfWellHead;
//            yMax += (double)surveyStation.EastOfWellHead;
//            zMax += (double)surveyStation.TvdWGS84;

//            double ellipseVerticesPhi_ = 32;
//            double ellipseVerticesTheta_ = 32;
//            if (surveyStation.Uncertainty.EllipsoidRadius[0] > 0)
//            {
//                for (int j = 0; j <= ellipseVerticesPhi_; j++)
//                {
//                    double phi = (double)j * 2.0 * Math.PI / (double)ellipseVerticesPhi_;
//                    for (int k = 0; k <= ellipseVerticesTheta_; k++)
//                    {
//                        double theta = (double)k * 1.0 * Math.PI / (double)ellipseVerticesTheta_;
//                        double UEllipsoid = (double)surveyStation.Uncertainty.EllipsoidRadius[0] * System.Math.Sin(theta) * System.Math.Cos(phi);
//                        double VEllipsoid = (double)surveyStation.Uncertainty.EllipsoidRadius[1] * System.Math.Sin(theta) * System.Math.Sin(phi);
//                        double WEllipsoid = (double)surveyStation.Uncertainty.EllipsoidRadius[2] * System.Math.Cos(theta);

//                        double xEllipsoid = (double)surveyStation.Uncertainty.EigenVectors[0, 0] * UEllipsoid + (double)surveyStation.Uncertainty.EigenVectors[0, 1] * VEllipsoid + (double)surveyStation.Uncertainty.EigenVectors[0, 2] * WEllipsoid;
//                        double yEllipsoid = (double)surveyStation.Uncertainty.EigenVectors[1, 0] * UEllipsoid + (double)surveyStation.Uncertainty.EigenVectors[1, 1] * VEllipsoid + (double)surveyStation.Uncertainty.EigenVectors[1, 2] * WEllipsoid;
//                        double zEllipsoid = (double)surveyStation.Uncertainty.EigenVectors[2, 0] * UEllipsoid + (double)surveyStation.Uncertainty.EigenVectors[2, 1] * VEllipsoid + (double)surveyStation.Uncertainty.EigenVectors[2, 2] * WEllipsoid;

//                        xEllipsoid += (double)surveyStation.NorthOfWellHead;
//                        yEllipsoid += (double)surveyStation.EastOfWellHead;
//                        zEllipsoid += (double)surveyStation.TvdWGS84;

//                        if (xEllipsoid < xMin)
//                        {
//                            xMin = xEllipsoid;
//                        }
//                        if (xEllipsoid > xMax)
//                        {
//                            xMax = xEllipsoid;
//                        }
//                        if (yEllipsoid < yMin)
//                        {
//                            yMin = yEllipsoid;
//                        }
//                        if (yEllipsoid > yMax)
//                        {
//                            yMax = yEllipsoid;
//                        }
//                        if (zEllipsoid < zMin)
//                        {
//                            zMin = zEllipsoid;
//                        }
//                        if (zEllipsoid > zMax)
//                        {
//                            zMax = zEllipsoid;
//                        }

//                        //double xCyl = (double)uncertainty.EllipsoidRadius[0] * System.Math.Sin(theta) *  System.Math.Cos(Math.PI/2);
//                        //double yCyl = (double)uncertainty.EllipsoidRadius[1] * System.Math.Sin(theta) *  System.Math.Sin(Math.PI / 2);
//                        //double zCyl = (double)uncertainty.EllipsoidRadius[2] * System.Math.Cos(theta);
//                        //double xNEH = (double)H[0, 0] * xCyl + (double)H[0, 1] * yCyl + (double)H[0, 2] * zCyl;
//                        //double yNEH = (double)H[1, 0] * xCyl + (double)H[1, 1] * yCyl + (double)H[1, 2] * zCyl;
//                        //double zNEH = (double)H[2, 0] * xCyl + (double)H[2, 1] * yCyl + (double)H[2, 2] * zCyl;

//                        //xNEH += (double)surveys[i].NorthOfWellHead ;
//                        //yNEH += (double)surveys[i].EastOfWellHead;
//                        //zNEH += (double)surveys[i].TvdWGS84;
//                        //var pEll = new System.Windows.Media.Media3D.Point3D(xNEH - _minNorth, yNEH - _minEast, -zNEH - _minTVD);
//                        //pointsEll.Add(pEll);

//                    }
//                }
//            }
//        }

//        private List<GlobalCoordinatePoint3D> GetUncertaintyEllipseCoordinates(EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipse, double z, ref double xMin, ref double xMax, ref double yMin, ref double yMax, ref double zMin, ref double zMax)
//        {
//            List<GlobalCoordinatePoint3D> ellipseCoordinates = new List<GlobalCoordinatePoint3D>();

//            double sinI = System.Math.Sin((double)uncertaintyEnvelopeEllipse.Inclination);
//            double cosI = System.Math.Cos((double)uncertaintyEnvelopeEllipse.Inclination);
//            double sinA = System.Math.Sin((double)uncertaintyEnvelopeEllipse.Azimuth);
//            double cosA = System.Math.Cos((double)uncertaintyEnvelopeEllipse.Azimuth);
//            double xNEH = 0.0;
//            double yNEH = 0.0;
//            double zNEH = 0.0;
//            double xNEHt = 0.0;
//            double yNEHt = 0.0;
//            double zNEHt = 0.0;

//            bool useInclAz = false;
//            bool usePhi = true;
//            if (useInclAz)
//            {
//                if (uncertaintyEnvelopeEllipse.EllipseRadius[0] != null && uncertaintyEnvelopeEllipse.EllipseRadius[1] != null)
//                {
//                    //_ellipseVerticesPhi = (int)Numeric.Max(Numeric.Max(uncertaintyEnvelopeEllipse.EllipseRadius[0], uncertaintyEnvelopeEllipse.EllipseRadius[1]), _ellipseVerticesPhi);
//                    double[,] Rz = new double[3, 3];
//                    RotationMatrix(ref Rz, (double)uncertaintyEnvelopeEllipse.Azimuth, 3);
//                    double[,] Ry = new double[3, 3];
//                    RotationMatrix(ref Ry, (double)uncertaintyEnvelopeEllipse.Inclination, 2);
//                    double[,] R = new double[3, 3];
//                    R = MatrixMuliprication(Rz, Ry);
//                    for (int j = 0; j <= 64; j++)
//                    {
//                        double phi = (double)j * 2.0 * Math.PI / (double)32;
//                        double xCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[0] * System.Math.Cos(phi);
//                        double yCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[1] * System.Math.Sin(phi);
//                        double zCyl = z;
//                        xNEH = cosI * cosA * xCyl - sinA * yCyl + sinI * cosA * zCyl;
//                        yNEH = cosI * sinA * xCyl + cosA * yCyl + sinI * sinA * zCyl;
//                        zNEH = -sinI * xCyl + cosI * zCyl;
//                        xNEH = R[0, 0] * xCyl + R[0, 1] * yCyl + R[0, 2] * zCyl;
//                        yNEH = R[1, 0] * xCyl + R[1, 1] * yCyl + R[1, 2] * zCyl;
//                        zNEH = R[2, 0] * xCyl + R[2, 1] * yCyl + R[2, 2] * zCyl;
//                        xNEH += (double)uncertaintyEnvelopeEllipse.X;
//                        yNEH += (double)uncertaintyEnvelopeEllipse.Y;
//                        zNEH += (double)uncertaintyEnvelopeEllipse.Z;
//                        GlobalCoordinatePoint3D point = new GlobalCoordinatePoint3D(xNEH, yNEH, zNEH);
//                        ellipseCoordinates.Add(point);
//                    }
//                }
//            }
//            else if (usePhi)
//            {
//                if (uncertaintyEnvelopeEllipse.EllipseRadius[0] != null && uncertaintyEnvelopeEllipse.EllipseRadius[1] != null)
//                {
//                    //_ellipseVerticesPhi = (int)Numeric.Max(Numeric.Max(uncertaintyEnvelopeEllipse.EllipseRadius[0], uncertaintyEnvelopeEllipse.EllipseRadius[1]), _ellipseVerticesPhi);
//                    double sinP = System.Math.Sin((double)uncertaintyEnvelopeEllipse.PerpendicularDirection);
//                    double cosP = System.Math.Cos((double)uncertaintyEnvelopeEllipse.PerpendicularDirection);

//                    double[,] Rz = new double[3, 3];
//                    RotationMatrix(ref Rz, (double)uncertaintyEnvelopeEllipse.Azimuth, 3);
//                    double[,] Ry = new double[3, 3];
//                    RotationMatrix(ref Ry, (double)uncertaintyEnvelopeEllipse.Inclination, 2);
//                    double[,] R0 = new double[3, 3];
//                    R0 = MatrixMuliprication(Rz, Ry);
//                    double[,] Rz2 = new double[3, 3];
//                    RotationMatrix(ref Rz2, (double)uncertaintyEnvelopeEllipse.PerpendicularDirection, 3);
//                    //RotationMatrix(ref Rz2, Math.PI/2, 3);
//                    double[,] R = new double[3, 3];
//                    R = MatrixMuliprication(R0, Rz2);
//                    for (int j = 0; j <= 96; j++)
//                    {
//                        double phi = (double)j * 2.0 * Math.PI / (double)96;
//                        double xCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[0] * System.Math.Cos(phi);
//                        double yCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[1] * System.Math.Sin(phi);
//                        double zCyl = z;
//                        double xNEH0 = (cosP * cosI * cosA - sinP * sinA * cosI) * xCyl - (cosP * sinA + sinP * cosA) * yCyl + (cosP * sinI * cosA - sinP * sinA * sinA) * zCyl;
//                        double yNEH0 = (sinP * cosI * sinA - cosP * sinA * cosI) * xCyl + (cosP * cosA - sinP * sinA) * yCyl + (cosP * sinI * sinA + sinP * cosA * sinI) * zCyl;
//                        double zNEH0 = -sinI * xCyl + cosI * zCyl;
//                        xNEH = R[0, 0] * xCyl + R[0, 1] * yCyl + R[0, 2] * zCyl;
//                        yNEH = R[1, 0] * xCyl + R[1, 1] * yCyl + R[1, 2] * zCyl;
//                        zNEH = R[2, 0] * xCyl + R[2, 1] * yCyl + R[2, 2] * zCyl;
//                        if (uncertaintyEnvelopeEllipse.PerpendicularDirection > 0.5)
//                        {
//                            bool ok = false;
//                        }
//                        xNEH += (double)uncertaintyEnvelopeEllipse.X;
//                        yNEH += (double)uncertaintyEnvelopeEllipse.Y;
//                        zNEH += (double)uncertaintyEnvelopeEllipse.Z;
//                        GlobalCoordinatePoint3D point = new GlobalCoordinatePoint3D(xNEH, yNEH, zNEH);
//                        ellipseCoordinates.Add(point);
//                        if (xNEH < xMin) xMin = xNEH;
//                        if (xNEH > xMax) xMax = xNEH;
//                        if (yNEH < yMin) yMin = yNEH;
//                        if (yNEH > yMax) yMax = yNEH;
//                        if (zNEH < zMin) zMin = zNEH;
//                        if (zNEH > zMax) zMax = zNEH;

//                    }
//                }
//            }
//            else
//            {
//                //_surveyList[9].Uncertainty.EigenVectors
//                for (int j = 0; j <= 32; j++)
//                {
//                    //double phi = (double)j * 2.0 * Math.PI / (double)_ellipseVerticesPhi;
//                    //double xCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[0] * System.Math.Cos(phi);
//                    //double yCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[1] * System.Math.Sin(phi);
//                    //double zCyl = 0.0;

//                    //xNEH = H[0, 0] * xCyl + H[0, 1] * yCyl + H[0, 2] * zCyl;
//                    //yNEH = H[1, 0] * xCyl + H[1, 1] * yCyl + H[1, 2] * zCyl;
//                    //zNEH = H[2, 0] * xCyl + H[2, 1] * yCyl +H[2, 2] * zCyl;
//                    //xNEH += (double)uncertaintyEnvelopeEllipse.X;
//                    //yNEH += (double)uncertaintyEnvelopeEllipse.Y;
//                    //zNEH += (double)uncertaintyEnvelopeEllipse.Z;
//                    //Point3D point = new Point3D(xNEH, yNEH, zNEH);
//                    //ellipseCoordinates.Add(point);
//                }


//            }


//            return ellipseCoordinates;
//        }
//        private List<GlobalCoordinatePoint3D> GetUncertaintyEllipseCoordinates(EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipse)
//        {
//            List<GlobalCoordinatePoint3D> ellipseCoordinates = new List<GlobalCoordinatePoint3D>();

//            double sinI = System.Math.Sin((double)uncertaintyEnvelopeEllipse.Inclination);
//            double cosI = System.Math.Cos((double)uncertaintyEnvelopeEllipse.Inclination);
//            double sinA = System.Math.Sin((double)uncertaintyEnvelopeEllipse.Azimuth);
//            double cosA = System.Math.Cos((double)uncertaintyEnvelopeEllipse.Azimuth);
//            double xNEH = 0.0;
//            double yNEH = 0.0;
//            double zNEH = 0.0;
//            double xNEHt = 0.0;
//            double yNEHt = 0.0;
//            double zNEHt = 0.0;

//            bool useInclAz = false;
//            bool usePhi = true;
//            if (useInclAz)
//            {
//                if (uncertaintyEnvelopeEllipse.EllipseRadius[0] != null && uncertaintyEnvelopeEllipse.EllipseRadius[1] != null)
//                {
//                    //_ellipseVerticesPhi = (int)Numeric.Max(Numeric.Max(uncertaintyEnvelopeEllipse.EllipseRadius[0], uncertaintyEnvelopeEllipse.EllipseRadius[1]), _ellipseVerticesPhi);
//                    double[,] Rz = new double[3, 3];
//                    RotationMatrix(ref Rz, (double)uncertaintyEnvelopeEllipse.Azimuth, 3);
//                    double[,] Ry = new double[3, 3];
//                    RotationMatrix(ref Ry, (double)uncertaintyEnvelopeEllipse.Inclination, 2);
//                    double[,] R = new double[3, 3];
//                    R = MatrixMuliprication(Rz, Ry);
//                    for (int j = 0; j <= _ellipseVerticesPhi; j++)
//                    {
//                        double phi = (double)j * 2.0 * Math.PI / (double)_ellipseVerticesPhi;
//                        double xCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[0] * System.Math.Cos(phi);
//                        double yCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[1] * System.Math.Sin(phi);
//                        double zCyl = 0.0;
//                        xNEH = cosI * cosA * xCyl - sinA * yCyl + sinI * cosA * zCyl;
//                        yNEH = cosI * sinA * xCyl + cosA * yCyl + sinI * sinA * zCyl;
//                        zNEH = -sinI * xCyl + cosI * zCyl;
//                        xNEH = R[0, 0] * xCyl + R[0, 1] * yCyl + R[0, 2] * zCyl;
//                        yNEH = R[1, 0] * xCyl + R[1, 1] * yCyl + R[1, 2] * zCyl;
//                        zNEH = R[2, 0] * xCyl + R[2, 1] * yCyl + R[2, 2] * zCyl;
//                        xNEH += (double)uncertaintyEnvelopeEllipse.X;
//                        yNEH += (double)uncertaintyEnvelopeEllipse.Y;
//                        zNEH += (double)uncertaintyEnvelopeEllipse.Z;
//                        GlobalCoordinatePoint3D point = new GlobalCoordinatePoint3D(xNEH, yNEH, zNEH);
//                        ellipseCoordinates.Add(point);
//                    }
//                }
//            }
//            else if (usePhi)
//            {
//                if (uncertaintyEnvelopeEllipse.EllipseRadius[0] != null && uncertaintyEnvelopeEllipse.EllipseRadius[1] != null)
//                {
//                    //_ellipseVerticesPhi = (int)Numeric.Max(Numeric.Max(uncertaintyEnvelopeEllipse.EllipseRadius[0], uncertaintyEnvelopeEllipse.EllipseRadius[1]), _ellipseVerticesPhi);
//                    double sinP = System.Math.Sin((double)uncertaintyEnvelopeEllipse.PerpendicularDirection);
//                    double cosP = System.Math.Cos((double)uncertaintyEnvelopeEllipse.PerpendicularDirection);

//                    double[,] Rz = new double[3, 3];
//                    RotationMatrix(ref Rz, (double)uncertaintyEnvelopeEllipse.Azimuth, 3);
//                    double[,] Ry = new double[3, 3];
//                    RotationMatrix(ref Ry, (double)uncertaintyEnvelopeEllipse.Inclination, 2);
//                    double[,] R0 = new double[3, 3];
//                    R0 = MatrixMuliprication(Rz, Ry);
//                    double[,] Rz2 = new double[3, 3];
//                    RotationMatrix(ref Rz2, (double)uncertaintyEnvelopeEllipse.PerpendicularDirection, 3);
//                    //RotationMatrix(ref Rz2, Math.PI/2, 3);
//                    double[,] R = new double[3, 3];
//                    R = MatrixMuliprication(R0, Rz2);
//                    for (int j = 0; j <= _ellipseVerticesPhi; j++)
//                    {
//                        double phi = (double)j * 2.0 * Math.PI / (double)_ellipseVerticesPhi;
//                        double xCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[0] * System.Math.Cos(phi);
//                        double yCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[1] * System.Math.Sin(phi);
//                        double zCyl = 0.0;
//                        double xNEH0 = (cosP * cosI * cosA - sinP * sinA * cosI) * xCyl - (cosP * sinA + sinP * cosA) * yCyl + (cosP * sinI * cosA - sinP * sinA * sinA) * zCyl;
//                        double yNEH0 = (sinP * cosI * sinA - cosP * sinA * cosI) * xCyl + (cosP * cosA - sinP * sinA) * yCyl + (cosP * sinI * sinA + sinP * cosA * sinI) * zCyl;
//                        double zNEH0 = -sinI * xCyl + cosI * zCyl;
//                        xNEH = R[0, 0] * xCyl + R[0, 1] * yCyl + R[0, 2] * zCyl;
//                        yNEH = R[1, 0] * xCyl + R[1, 1] * yCyl + R[1, 2] * zCyl;
//                        zNEH = R[2, 0] * xCyl + R[2, 1] * yCyl + R[2, 2] * zCyl;
//                        if (uncertaintyEnvelopeEllipse.PerpendicularDirection > 0.5)
//                        {
//                            bool ok = false;
//                        }
//                        xNEH += (double)uncertaintyEnvelopeEllipse.X;
//                        yNEH += (double)uncertaintyEnvelopeEllipse.Y;
//                        zNEH += (double)uncertaintyEnvelopeEllipse.Z;
//                        GlobalCoordinatePoint3D point = new GlobalCoordinatePoint3D(xNEH, yNEH, zNEH);
//                        if (phi == 0)
//                        {
//                            pointx.Add(point);

//                        }
//                        if (phi == Math.PI / 2 + Math.PI)
//                        {
//                            pointy.Add(point);

//                        }
//                        ellipseCoordinates.Add(point);
//                    }
//                }
//            }
//            else
//            {
//                //_surveyList[9].Uncertainty.EigenVectors
//                for (int j = 0; j <= _ellipseVerticesPhi; j++)
//                {
//                    //double phi = (double)j * 2.0 * Math.PI / (double)_ellipseVerticesPhi;
//                    //double xCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[0] * System.Math.Cos(phi);
//                    //double yCyl = (double)uncertaintyEnvelopeEllipse.EllipseRadius[1] * System.Math.Sin(phi);
//                    //double zCyl = 0.0;

//                    //xNEH = H[0, 0] * xCyl + H[0, 1] * yCyl + H[0, 2] * zCyl;
//                    //yNEH = H[1, 0] * xCyl + H[1, 1] * yCyl + H[1, 2] * zCyl;
//                    //zNEH = H[2, 0] * xCyl + H[2, 1] * yCyl +H[2, 2] * zCyl;
//                    //xNEH += (double)uncertaintyEnvelopeEllipse.X;
//                    //yNEH += (double)uncertaintyEnvelopeEllipse.Y;
//                    //zNEH += (double)uncertaintyEnvelopeEllipse.Z;
//                    //Point3D point = new Point3D(xNEH, yNEH, zNEH);
//                    //ellipseCoordinates.Add(point);
//                }


//            }


//            return ellipseCoordinates;
//        }

//        private void RotationMatrix(ref double[,] A, double angle, int rot)
//        {
//            double sinA = System.Math.Sin(angle);
//            double cosA = System.Math.Cos(angle);
//            // x rotation
//            if (rot == 1)
//            {
//                A[0, 0] = 1;
//                A[0, 1] = 0;
//                A[0, 2] = 0;
//                A[1, 0] = 0;
//                A[1, 1] = cosA;
//                A[1, 2] = -sinA;
//                A[2, 0] = 0;
//                A[2, 1] = sinA;
//                A[2, 2] = cosA;
//            }
//            // y rotation
//            else if (rot == 2)
//            {
//                A[0, 0] = cosA;
//                A[0, 1] = 0;
//                A[0, 2] = sinA;
//                A[1, 0] = 0;
//                A[1, 1] = 1;
//                A[1, 2] = 0;
//                A[2, 0] = -sinA;
//                A[2, 1] = 0;
//                A[2, 2] = cosA;
//            }
//            // z rotation
//            else
//            {
//                A[0, 0] = cosA;
//                A[0, 1] = -sinA;
//                A[0, 2] = 0;
//                A[1, 0] = sinA;
//                A[1, 1] = cosA;
//                A[1, 2] = 0;
//                A[2, 0] = 0;
//                A[2, 1] = 0;
//                A[2, 2] = 1;
//            }
//        }
//        private List<GlobalCoordinatePoint3D> GetUncertaintyEllipseAreaCoordinates(EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipse)
//        {
//            List<GlobalCoordinatePoint3D> ellipseAreaCoordinates = new List<GlobalCoordinatePoint3D>();
//            /*
//            EnvelopeOfUncertaintyEllipse ellipse = new EnvelopeOfUncertaintyEllipse();
//            ellipse.Azimuth = uncertaintyEnvelopeEllipse.Azimuth;
//            ellipse.Inclination = uncertaintyEnvelopeEllipse.Inclination;
//            ellipse.X = uncertaintyEnvelopeEllipse.X;
//            ellipse.Y = uncertaintyEnvelopeEllipse.Y;
//            ellipse.Z = uncertaintyEnvelopeEllipse.Z;
//            double minScalingfactor = 0.1;
//            int scalingLevels = 9;
//            double factor = (1 - minScalingfactor) / scalingLevels;
//            if (uncertaintyEnvelopeEllipse.EllipseRadius != null && uncertaintyEnvelopeEllipse.EllipseRadius[0] != null && uncertaintyEnvelopeEllipse.EllipseRadius[1] != null)
//            {
//                for (int i = 0; i <= scalingLevels; i++)
//                {
//                    ellipse.EllipseRadius = new Vector2D();
//                    ellipse.EllipseRadius[0] = uncertaintyEnvelopeEllipse.EllipseRadius[0] * (1 - i * factor);
//                    ellipse.EllipseRadius[1] = uncertaintyEnvelopeEllipse.EllipseRadius[1] * (1 - i * factor);
//                    List<Point3D> ellipseCoordinates = GetUncertaintyEllipseCoordinates(ellipse);
//                    for (int j = 0; j < ellipseCoordinates.Count; j++)
//                    {
//                        ellipseAreaCoordinates.Add(ellipseCoordinates[j]);
//                    }
//                }
//            }
//            */
//            return ellipseAreaCoordinates;
//        }
//        public bool IsPartOfUncertaintyEnvelope(CoordinateConverter.WGS84Coordinate wGS84Coordinate, double tvd)
//        {
//            bool isPartOf = false;
//            CoordinateConverter converter = new CoordinateConverter();
//            CoordinateConverter.UTMCoordinate utmCoordinate = converter.WGStoUTM(wGS84Coordinate);

//            for (int i = 0; i < UncertaintyEnvelope.Count - 1; i++)
//            {
//                EnvelopeOfUncertaintyEllipse uncertaintyEnvelopeEllipse = UncertaintyEnvelope[i];
//                if (uncertaintyEnvelopeEllipse.EllipseRadius != null && uncertaintyEnvelopeEllipse.EllipseRadius[0] != null && Numeric.IsDefined(uncertaintyEnvelopeEllipse.EllipseRadius[0]))
//                {
//                    double azimuth = (double)uncertaintyEnvelopeEllipse.Azimuth;
//                    double inclination = (double)uncertaintyEnvelopeEllipse.Inclination;
//                    double xc = (double)uncertaintyEnvelopeEllipse.X;
//                    double yc = (double)uncertaintyEnvelopeEllipse.Y;
//                    double zc = (double)uncertaintyEnvelopeEllipse.Z;
//                    Vector2D ellipseR = uncertaintyEnvelopeEllipse.EllipseRadius;
//                    double[,] T = new double[3, 3];
//                    double sinI = System.Math.Sin(inclination);
//                    double cosI = System.Math.Cos(inclination);
//                    double sinA = System.Math.Sin(azimuth);
//                    double cosA = System.Math.Cos(azimuth);
//                    T[0, 0] = cosI * cosA;
//                    T[1, 0] = -sinA;
//                    T[2, 0] = sinI * cosA;
//                    T[0, 1] = cosI * sinA;
//                    T[1, 1] = cosA;
//                    T[2, 1] = sinI * sinA;
//                    T[0, 2] = -sinI;
//                    T[1, 2] = 0;
//                    T[2, 2] = cosI;
//                    double xutm = utmCoordinate.X - WellUTMCoordinate.X;
//                    double yutm = utmCoordinate.Y - WellUTMCoordinate.Y;
//                    double x = T[0, 0] * xutm + T[0, 1] * yutm + T[0, 2] * tvd;
//                    double y = T[1, 0] * xutm + T[1, 1] * yutm + T[1, 2] * tvd;
//                    double z = T[2, 0] * xutm + T[2, 1] * yutm + T[2, 2] * tvd;
//                    double val = Math.Pow((x - xc), 2) / Math.Pow((double)ellipseR[0], 2) + Math.Pow((y - yc), 2) / Math.Pow((double)ellipseR[1], 2);
//                    if (val <= 1)
//                    {
//                        isPartOf = true;
//                        break;
//                    }
//                    else
//                    {
//                        isPartOf = false;
//                    }
//                }
//            }
//            return isPartOf;
//        }
//    }
//}
