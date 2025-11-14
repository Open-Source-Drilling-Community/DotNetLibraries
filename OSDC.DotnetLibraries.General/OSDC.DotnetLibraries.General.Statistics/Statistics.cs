using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public static class Statistics
    {
        public static readonly (double Probability, double ChiSquare)[] ChiSquare3D =
        {
            (0.05, 0.35), (0.10, 0.58), (0.20, 1.01), (0.30, 1.42), (0.50, 2.37),
            (0.70, 3.66), (0.80, 4.64), (0.90, 6.25), (0.95, 7.82), (0.99, 11.34), (0.999, 16.27)
        };

        /// <summary>
        /// The arithmetic mean of a list of double
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Average(IList<double> data)
        {
            if (data != null && data.Count > 0)
            {
                double sum = 0;
                foreach (double val in data)
                {
                    sum += val;
                }
                return sum / data.Count;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// the min value of a list of double
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Min(IList<double> data)
        {
            if (data != null && data.Count > 0)
            {
                double min = Numeric.MAX_DOUBLE;
                foreach (double val in data)
                {
                    if (val < min)
                    {
                        min = val;
                    }
                }
                return min;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// the max value of a list of double
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Max(IList<double> data)
        {
            if (data != null && data.Count > 0)
            {
                double max = Numeric.MIN_DOUBLE;
                foreach (double val in data)
                {
                    if (val > max)
                    {
                        max = val;
                    }
                }
                return max;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// The standard deviation of the list of double
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double StandardDeviation(IList<double> data)
        {
            if (data != null && data.Count > 0)
            {
                double total, sum;
                total = sum = 0;
                foreach (double val in data)
                {
                    sum += val;
                    total += val * val;
                }
                sum /= data.Count;
                return Numeric.SqrtEqual(total / data.Count - sum * sum);
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// Returns the median of the list. 
        /// 
        /// Sorts the list and returns element number n/2
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double Median(List<double> data)
        {
            if (data != null && data.Count >= 2)
            {
                data.Sort();
                if (data.Count % 2 == 0)
                {
                    return (data[data.Count / 2 - 1] + data[data.Count / 2]) / 2;
                }
                else
                {
                    return data[data.Count / 2];
                }
            }
            return Numeric.UNDEF_DOUBLE;
        }
        /// <summary>
        /// Returns the median of the list, taking weights into account
        /// </summary>
        /// <param name="weightedData">List of pairs (value, weight)</param>
        /// <returns></returns>
        public static double Median(List<Pair<double, double>> weightedData)
        {
            if (weightedData != null && weightedData.Count > 1)
            {
                double medianWeight = 0;
                for (int i = 0; i < weightedData.Count; i++)
                {
                    medianWeight += weightedData[i].Right;
                }
                medianWeight /= 2;
                weightedData.Sort(PairComparer);
                double temp = 0;
                for (int i = 0; i < weightedData.Count; i++)
                {
                    temp += weightedData[i].Right;
                    if (Numeric.GE(temp, medianWeight))
                    {
                        return weightedData[i].Left;
                    }
                }
                return weightedData[weightedData.Count - 1].Left;
            }
            return Numeric.UNDEF_DOUBLE;
        }
        private static int PairComparer(Pair<double, double> p1, Pair<double, double> p2)
        {
            return Comparer<double>.Default.Compare(p1.Left, p2.Left);
        }

        /// <summary>
        /// Calculate the chi square for a fit y(i) = func(x[i]), with standard
        /// deviations std[i]
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="std"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double ChiSquare(double[] x, double[] y, double[] std, IValuable obj)
        {
            double chi2 = 0;
            if (x != null && y != null && std != null && x.Length == y.Length && x.Length == std.Length)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    chi2 += Numeric.Pow((y[i] - obj.Eval(x[i])) / std[i], 2);
                }
            }
            return chi2;
        }
        /// <summary>
        /// Calculate the chi square for a fit y(i) = func(x[i]) where x is the left element of the pair and y is the right element of the pair
        /// The standard deviation is supposed to be equal to 1
        /// </summary>
        /// <param name="data"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double ChiSquare(IList<Pair<double, double>> data, IValuable obj)
        {
            if (data != null)
            {
                double chi2 = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    double temp = obj.Eval(data[i].Left);
                    chi2 += (temp - data[i].Right) * (temp - data[i].Right);
                }
                return chi2;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }
        /// <summary>
        /// calculate the chi square of lists of estimated and measured values.
        /// The standard deviation is supposed to be equal to 1.
        /// </summary>
        /// <param name="yEstimated"></param>
        /// <param name="yMeasured"></param>
        /// <returns></returns>
        public static double ChiSquare(double[] yEstimated, double[] yMeasured)
        {
            double chi2 = 0;
            if (yEstimated != null && yMeasured != null && yEstimated.Length == yMeasured.Length)
            {
                for (int i = 0; i < yMeasured.Length; i++)
                {
                    chi2 += (yEstimated[i] - yMeasured[i]) * (yEstimated[i] - yMeasured[i]);
                }
            }
            return chi2;
        }
        /// <summary>
        /// calculate the chi square of lists of estimated and measured values.
        /// </summary>
        /// <param name="yEstimated"></param>
        /// <param name="yMeasured"></param>
        /// <param name="std"></param>
        /// <returns></returns>
        public static double ChiSquare(double[] yEstimated, double[] yMeasured, double std)
        {
            if (!Numeric.EQ(std, 0))
            {
                double chi2 = 0;
                if (yEstimated != null && yMeasured != null && yEstimated.Length == yMeasured.Length)
                {
                    for (int i = 0; i < yMeasured.Length; i++)
                    {
                        chi2 += (yEstimated[i] - yMeasured[i]) * (yEstimated[i] - yMeasured[i]) / (std * std);
                    }
                }
                return chi2;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }
        /// <summary>
        /// calculate the chi square of lists of estimated and measured values.
        /// </summary>
        /// <param name="yEstimated"></param>
        /// <param name="yMeasured"></param>
        /// <param name="std"></param>
        /// <returns></returns>
        public static double ChiSquare(double[] yEstimated, double[] yMeasured, double[] std)
        {
            if (std == null)
            {
                return ChiSquare(yEstimated, yMeasured);
            }
            else
            {
                double chi2 = 0;
                if (yEstimated != null && yMeasured != null && yEstimated.Length == yMeasured.Length)
                {
                    for (int i = 0; i < yMeasured.Length; i++)
                    {
                        if (!Numeric.EQ(std[i], 0))
                        {
                            chi2 += (yEstimated[i] - yMeasured[i]) * (yEstimated[i] - yMeasured[i]) / (std[i] * std[i]);
                        }
                    }
                }
                return chi2;
            }
        }

        /// <summary>
        /// Calculate the confidence factor corresponding to the given ChiSquare3D (linear interpolation)
        /// </summary>
        /// <param name="chiSquare3D"></param>
        /// <returns>the linearly interpolated confidence factor corresponding to the given ChiSquare3D value</returns>
        public static double GetConfidenceFactor(double chiSquare3D)
        {
            if (Numeric.IsUndefined(chiSquare3D))
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else
            {
                int last = ChiSquare3D.GetLength(1) - 1;
                if (chiSquare3D < ChiSquare3D[0].ChiSquare)
                {
                    double factor = (chiSquare3D - ChiSquare3D[0].ChiSquare) / (ChiSquare3D[1].ChiSquare - ChiSquare3D[0].ChiSquare);
                    return ChiSquare3D[0].Probability + factor * (ChiSquare3D[1].Probability - ChiSquare3D[0].Probability);
                }
                else if (chiSquare3D >= ChiSquare3D[last].ChiSquare)
                {
                    double factor = (chiSquare3D - ChiSquare3D[last - 1].ChiSquare) / (ChiSquare3D[last].ChiSquare - ChiSquare3D[last - 1].ChiSquare);
                    return ChiSquare3D[last - 1].Probability + factor * (ChiSquare3D[last].Probability - ChiSquare3D[last - 1].Probability);
                }
                else
                {
                    for (int i = 0; i < last; i++)
                    {
                        if (chiSquare3D >= ChiSquare3D[i].ChiSquare && chiSquare3D < ChiSquare3D[i + 1].ChiSquare)
                        {
                            double factor = (chiSquare3D - ChiSquare3D[i].ChiSquare) / (ChiSquare3D[i + 1].ChiSquare - ChiSquare3D[i].ChiSquare);
                            return ChiSquare3D[i].Probability + factor * (ChiSquare3D[i + 1].Probability - ChiSquare3D[i].Probability);
                        }
                    }
                    return Numeric.UNDEF_DOUBLE;
                }
            }
        }

        /// <summary>
        /// Calculate the ChiSquare3D corresponding to the given confidence factor (probability)
        /// </summary>
        /// <param name="probability"></param>
        /// <returns>the linearly interpolated ChiSquare3D corresponding to the given confidence factor (probability)</returns>
        public static double GetChiSquare3D(double probability)
        {
            if (Numeric.IsUndefined(probability))
            {
                return Numeric.UNDEF_DOUBLE;
            }
            else
            {
                int last = ChiSquare3D.GetLength(1) - 1;
                if (probability < ChiSquare3D[0].Probability)
                {
                    double factor = (probability - ChiSquare3D[0].Probability) / (ChiSquare3D[1].Probability - ChiSquare3D[0].Probability);
                    return ChiSquare3D[0].ChiSquare + factor * (ChiSquare3D[1].ChiSquare - ChiSquare3D[0].ChiSquare);
                }
                else if (probability >= ChiSquare3D[last].Probability)
                {
                    double factor = (probability - ChiSquare3D[last - 1].Probability) / (ChiSquare3D[last].Probability - ChiSquare3D[last - 1].Probability);
                    return ChiSquare3D[last - 1].ChiSquare + factor * (ChiSquare3D[last].ChiSquare - ChiSquare3D[last - 1].ChiSquare);
                }
                else
                {
                    for (int i = 0; i < last; i++)
                    {
                        if (probability >= ChiSquare3D[i].Probability && probability < ChiSquare3D[i + 1].Probability)
                        {
                            double factor = (probability - ChiSquare3D[i].Probability) / (ChiSquare3D[i + 1].Probability - ChiSquare3D[i].Probability);
                            return ChiSquare3D[i].ChiSquare + factor * (ChiSquare3D[i + 1].ChiSquare - ChiSquare3D[i].ChiSquare);
                        }
                    }
                    return Numeric.UNDEF_DOUBLE;
                }
            }
        }
    }
}
