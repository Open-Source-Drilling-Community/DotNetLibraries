using OSDC.DotnetLibraries.General.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class GaussianDistribution : ContinuousDistribution, IEquatable<GaussianDistribution>
    {

        /// <summary>
        /// 
        /// </summary>
        public double? Mean { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double? StandardDeviation { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public GaussianDistribution()
            : base()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source"></param>
        public GaussianDistribution(GaussianDistribution source)
            : base()
        {
            if (source != null)
            {
                Mean = source.Mean;
                StandardDeviation = source.StandardDeviation;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        public GaussianDistribution(double? mean, double? stdDev)
            : base()
        {
            Mean = mean;
            StandardDeviation = stdDev;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetDataMin()
        {
            return double.NegativeInfinity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetDataMax()
        {
            return double.PositiveInfinity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            return Mean;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetStandardDeviation()
        {
            return StandardDeviation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(GaussianDistribution? cmp)
        {
            return (cmp != null) && Numeric.EQ(Mean, cmp.Mean) && Numeric.EQ(StandardDeviation, cmp.StandardDeviation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetCumulativeProbability(double target)
        {
            if (Mean != null && StandardDeviation != null)
            {
                if (!Numeric.EQ(StandardDeviation, 0))
                {
                    return 0.5 * (1 + Numeric.ErrorFunction((target - Mean.Value) / (StandardDeviation.Value * System.Math.Sqrt(2))));

                }
                return base.GetCumulativeProbability(target);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetProbability(double target)
        {
            if (StandardDeviation != null && Mean != null)
            {
                double result = 1 / (StandardDeviation.Value * System.Math.Sqrt(2 * System.Math.PI));
                result *= System.Math.Exp(-((target - Mean.Value) * (target - Mean.Value) / (2 * StandardDeviation.Value * StandardDeviation.Value)));
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? Realize()
        {
            if (StandardDeviation != null && Mean != null)
            {
                if (Numeric.EQ(StandardDeviation.Value, 0))
                {
                    return Mean;
                }
                else
                {
                    double x_01;
                    double x_ab;
                    double u1;
                    double u2;

                    do
                    {
                        u1 = RandomGenerator.Instance.NextDouble();
                        u2 = RandomGenerator.Instance.NextDouble();
                    } while (u1 == 0);

                    x_01 = System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Cos(2 * System.Math.PI * u2);
                    x_ab = StandardDeviation.Value * x_01 + Mean.Value;
                    return x_ab;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override double? Quantile(double p)
        {
            if (Mean == null || StandardDeviation == null || Numeric.IsUndefined(Mean) || Numeric.IsUndefined(StandardDeviation) || Numeric.EQ(StandardDeviation, 0))
            {
                return null;
            }
            else
            {
                if (Numeric.EQ(Mean, 0) && Numeric.EQ(StandardDeviation, 1))
                {
                    double a1 = -39.69683028665376;
                    double a2 = 220.9460984245205;
                    double a3 = -275.9285104469687;
                    double a4 = 138.3577518672690;
                    double a5 = -30.66479806614716;
                    double a6 = 2.506628277459239;

                    double b1 = -54.47609879822406;
                    double b2 = 161.5858368580409;
                    double b3 = -155.6989798598866;
                    double b4 = 66.80131188771972;
                    double b5 = -13.28068155288572;

                    double c1 = -0.007784894002430293;
                    double c2 = -0.3223964580411365;
                    double c3 = -2.400758277161838;
                    double c4 = -2.549732539343734;
                    double c5 = 4.374664141464968;
                    double c6 = 2.938163982698783;

                    double d1 = 0.007784695709041462;
                    double d2 = 0.3224671290700398;
                    double d3 = 2.445134137142996;
                    double d4 = 3.754408661907416;

                    //Define break-points.

                    double p_low = 0.02425;
                    double p_high = 1 - p_low;
                    double q, x, r;


                    x = Numeric.UNDEF_DOUBLE;


                    //Rational approximation for lower region.

                    if (0 < p && p < p_low)
                    {
                        q = System.Math.Sqrt(-2 * System.Math.Log(p));
                        x = (((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) / ((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
                    }

                    //Rational approximation for central region.

                    if (p_low <= p && p <= p_high)
                    {
                        q = p - 0.5;
                        r = q * q;
                        x = (((((a1 * r + a2) * r + a3) * r + a4) * r + a5) * r + a6) * q / (((((b1 * r + b2) * r + b3) * r + b4) * r + b5) * r + 1);
                    }

                    //Rational approximation for upper region.

                    if (p_high < p && p < 1)
                    {
                        q = System.Math.Sqrt(-2 * System.Math.Log(1 - p));
                        x = -(((((c1 * q + c2) * q + c3) * q + c4) * q + c5) * q + c6) / ((((d1 * q + d2) * q + d3) * q + d4) * q + 1);
                    }
                    return x;
                }
                else
                {
                    if (Numeric.EQ(p, 0.5))
                    {
                        return Mean;
                    }

                    double low, up;
                    low = up = Mean.Value;
                    double cpLow, cpUp;
                    cpLow = cpUp = 0.5;
                    bool isIntervalFound;
                    do
                    {
                        if (p > cpUp)//  Numeric.GT(p, cpUp))
                        {
                            up += StandardDeviation.Value;
                            double? x = GetCumulativeProbability(up);
                            if (x == null)
                            {
                                return null;
                            }
                            else
                            {
                                cpUp = x.Value;
                            }
                        }
                        else if (p < cpLow)// Numeric.LT(p, cpLow))
                        {
                            low -= StandardDeviation.Value;
                            double? x = GetCumulativeProbability(low);
                            if (x == null)
                            {
                                return null;
                            }
                            else
                            {
                                cpLow = x.Value;
                            }
                        }
                        isIntervalFound = Numeric.IsBetween(p, cpLow, cpUp);
                    }
                    while (!isIntervalFound);

                    double middleValue = (low + up) / 2;
                    double? currentCP = GetCumulativeProbability(middleValue);
                    if (currentCP == null)
                    {
                        return null;
                    }
                    else
                    {
                        bool stop = Numeric.EQ(p, currentCP.Value, 1e-9);
                        int iterationNumber = 0;
                        while (!stop && iterationNumber++ < 20)
                        {
                            if (Numeric.LT(currentCP.Value, p))
                            {
                                low = middleValue;
                            }
                            else if (Numeric.GT(currentCP.Value, p))
                            {
                                up = middleValue;
                            }
                            middleValue = (low + up) / 2;
                            currentCP = GetCumulativeProbability(middleValue);
                            if (currentCP == null)
                            {

                            }
                            else
                            {
                                stop = Numeric.EQ(p, currentCP.Value, 1e-9);
                            }
                        }
                        return middleValue;
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double[,]? GetCurve()
        {
            if (Mean != null && StandardDeviation != null)
            {
                double min, max, x, step;
                min = Mean.Value - 4 * StandardDeviation.Value;
                max = Mean.Value + 4 * StandardDeviation.Value;
                double[,] result = new double[100, 2];
                step = (max - min) / 100;
                x = min;

                for (int i = 0; i < 100; i++)
                {
                    result[i, 0] = x;
                    result[i, 1] = (1 / (StandardDeviation.Value * System.Math.Sqrt(2 * System.Math.PI))) * System.Math.Exp(-((x - Mean.Value) * (x - Mean.Value) / (2 * StandardDeviation.Value * StandardDeviation.Value)));
                    x += step;
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool isValid()
        {
            return Numeric.IsDefined(Mean) && Numeric.IsDefined(StandardDeviation) && Numeric.GT(StandardDeviation, 0) && Numeric.GE(Mean, minValue_) && Numeric.LE(Mean, maxValue_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public override void Copy(ContinuousDistribution from)
        {
            if (from != null)
            {
                if (from is GaussianDistribution)
                {
                    Mean = ((GaussianDistribution)from).Mean;
                    StandardDeviation = ((GaussianDistribution)from).StandardDeviation;
                }
                CopyExtraData(from);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetInfo()
        {
            return GetDistributionTypeName() + " [" + Mean + "," + StandardDeviation + "]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetDistributionTypeName()
        {
            return "Gaussian Distribution";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ContinuousDistribution Clone()
        {
            ContinuousDistribution result = new GaussianDistribution(Mean, StandardDeviation);
            CloneExtraData(result);
            return result;
        }
    }
}
