using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Statistics
{
    public class GeneralContinuousDistribution : ContinuousDistribution
    {
        private Tuple<double, double>[]? function_;
        private List<double> data_;
        private double resolution_;
        private bool isFunctionUpdated_ = false;
        private int numberOfHistrogramPoints_ = 20;
        private bool isDataSorted_ = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public GeneralContinuousDistribution() : base()
        {
            data_ = new List<double>();
        }

        /// <summary>
        /// Constructor with initialisation
        /// </summary>
        /// <param name="data"></param>
        public GeneralContinuousDistribution(List<double> data)
            : base()
        {
            data_ = new List<double>();
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    data_.Add(data[i]);
                }
            }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source"></param>
        public GeneralContinuousDistribution(GeneralContinuousDistribution source)
            : base()
        {
            data_ = new List<double>();
            if (source != null)
            {
                if (source.data_ != null)
                {
                    for (int i = 0; i < source.data_.Count; i++)
                    {
                        data_.Add(source.data_[i]);
                    }
                }
                if (source.function_ != null)
                {
                    function_ = new Tuple<double, double>[source.function_.Length];
                    for (int i = 0; i < source.function_.Length; i++)
                    {
                        function_[i] = new Tuple<double, double>(source.function_[i].Item1, source.function_[i].Item2);
                    }
                }
                resolution_ = source.resolution_;
                isFunctionUpdated_ = source.isFunctionUpdated_;
                numberOfHistrogramPoints_ = source.numberOfHistrogramPoints_;
                isDataSorted_ = source.isDataSorted_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Tuple<double, double>[]? Function
        {
            get
            {
                if (!isFunctionUpdated_)
                {
                    ComputeFunction();
                }
                return function_;
            }
            set
            {
                function_ = value;
                isFunctionUpdated_ = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int NumberOfHistrogramPoints
        {
            get { return numberOfHistrogramPoints_; }
            set
            {
                numberOfHistrogramPoints_ = value;
                isFunctionUpdated_ = false;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        [XmlArray("dataList")]
        [XmlArrayItem("data", typeof(double))]
        public List<double> Data
        {
            get { return data_; }
            set
            {
                if (value != null)
                {
                    data_ = value;
                    isFunctionUpdated_ = false;
                    isDataSorted_ = false;
                }
            }
        }

        private void ComputeFunction()
        {
            Tuple<double,double>[]? temp = GetHistogram();
            if (temp != null)
            {
                function_ = new Tuple<double,double>[numberOfHistrogramPoints_ + 2];

                double range = temp[temp.Length - 1].Item1 - temp[0].Item1;
                resolution_ = range / (numberOfHistrogramPoints_ - 1);
                if (function_ != null && function_.Length > 0)
                {
                    double total_ = 0;
                    for (int i = 0; i < temp.Length; i++)
                    {
                        total_ += temp[i].Item2;
                        function_[i + 1] = new Tuple<double, double>(temp[i].Item1 + resolution_ / 2, 0);
                    }
                    function_[0] = new Tuple<double, double>(temp[0].Item1 - resolution_ / 2, 0);
                    function_[function_.Length - 1] = new Tuple<double, double>(temp[temp.Length - 1].Item1 + 3 * resolution_ / 2, 0);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        function_[i + 1] = new Tuple<double, double>(function_[i+1].Item1, temp[i].Item2 / total_);
                    }
                }
            }
            isFunctionUpdated_ = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? Realize()
        {
            if (IsValid())
            {
                if (!isFunctionUpdated_)
                {
                    ComputeFunction();
                }
                if (function_ != null)
                {
                    double minValueX = function_[0].Item1;
                    double maxValueX = function_[function_.Length - 1].Item1;
                    double maxValueY = function_[0].Item2;
                    for (int i = 0; i < function_.Length; i++)
                    {
                        if (maxValueY < function_[i].Item2)
                        {
                            maxValueY = function_[i].Item2;
                        }
                    }

                    int accept = 0;
                    double draw = 0;
                    long index;
                    double yValue;

                    while (accept == 0)
                    {
                        draw = minValueX + (maxValueX - minValueX) * RandomGenerator.Instance.NextDouble();
                        index = 0;
                        if (draw > function_[0].Item1)
                        {
                            while (function_[index].Item1 < draw)
                            {
                                index = index + 1;
                            }
                            yValue = function_[index - 1].Item2 + (draw - function_[index - 1].Item1) / (function_[index].Item1 - function_[index - 1].Item1) * (function_[index].Item2 - function_[index - 1].Item2);
                        }
                        else
                        {
                            yValue = function_[0].Item2;
                        }
                        if (Numeric.EQ(minValueX, maxValueX) || RandomGenerator.Instance.NextDouble() < (yValue / maxValueY))
                        {
                            accept = 1;
                        }
                    }

                    return draw;
                }
                else
                {
                    return null;
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
        /// <param name="cumulative"></param>
        /// <returns></returns>
        public override double? Quantile(double cumulative)
        {
            return GetPercentile(cumulative * 100.0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Tuple<double, double>[]? GetCurve()
        {
            if (!isFunctionUpdated_)
            {
                ComputeFunction();
            }
            Tuple<double, double>[] result = new Tuple<double, double>[function_.Length];
            for (int i = 0; i < function_.Length; i++)
            {
                result[i] = new Tuple<double, double>(function_[i].Item1, function_[i].Item2);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public override double? GetCumulativeProbability(double target)
        {
            double result = Numeric.UNDEF_DOUBLE;
            if (data_ != null && data_.Count > 0)
            {
                if (!isDataSorted_)
                {
                    {
                        data_.Sort();
                        isDataSorted_ = true;
                    }
                }
                bool stop = false;
                int index = 0;
                do
                {
                    stop = data_[index] > target;
                }
                while (!stop && index++ < data_.Count - 1);

                if (index > 0 && index < data_.Count)
                {
                    result = Numeric.Interpolate(data_[index - 1], data_[index], (double)(index - 1) / (double)data_.Count, (double)index / (double)data_.Count, target);
                }
                else if (index == 0)
                {
                    result = 0;
                }
                else
                {
                    result = 1;
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Tuple<double, double>[]? GetHistogram()
        {
            CleanData();

            if (data_ != null && data_.Count > 0)
            {
                if (data_.Count >= 2)
                {
                    numberOfHistrogramPoints_ = (int)System.Math.Sqrt(data_.Count);
                    if (numberOfHistrogramPoints_ > data_.Count / 2)
                    {
                        numberOfHistrogramPoints_ = data_.Count / 2;
                    }
                    double min, max;
                    min = max = data_[0];
                    for (int i = 0; i < data_.Count; i++)
                    {
                        if (data_[i] < min)
                        {
                            min = data_[i];
                        }
                        if (data_[i] > max)
                        {
                            max = data_[i];
                        }
                    }

                    double range = max - min;
                    if (Numeric.EQ(range, 0)) //Deterministic value, only need 2 points
                    {
                        Tuple<double, double>[] histo = new Tuple<double, double>[2];
                        histo[0] = new Tuple<double,double>(data_[0] - data_[0] * 0.01, 0);
                        histo[1] = new Tuple<double, double>(data_[0], 1.0);
                        return histo;
                    }
                    else
                    {
                        resolution_ = range / numberOfHistrogramPoints_;
                        Tuple<double, double>[] histogram = new Tuple<double, double>[numberOfHistrogramPoints_];

                        for (int i = 0; i < numberOfHistrogramPoints_; i++)
                        {
                            histogram[i] = new Tuple<double, double>(min + i * resolution_ + resolution_ / 2, 0);
                        }
                        for (int i = 0; i < data_.Count; i++)
                        {
                            if (data_[i] != max)
                            {
                                int index = System.Math.Min((int)((data_[i] - min) / resolution_), numberOfHistrogramPoints_ - 1);
                                histogram[index] = new Tuple<double, double>(histogram[index].Item1, histogram[index].Item2 + 1);
                            }
                            else
                            {
                                histogram[numberOfHistrogramPoints_ - 1] = new Tuple<double, double>(histogram[numberOfHistrogramPoints_ - 1].Item1, histogram[numberOfHistrogramPoints_ - 1].Item2 + 1);
                            }
                        }
                        if (data_.Count > 0)
                        {
                            for (int i = 0; i < numberOfHistrogramPoints_; i++)
                            {
                                histogram[i] = new Tuple<double, double>(histogram[i].Item1, (histogram[i].Item2 / data_.Count));
                            }
                        }
                        return histogram;
                    }
                }
                else
                {
                    Tuple<double, double>[] result = new Tuple<double, double>[1];

                    result[0] = new Tuple<double, double>(data_[0], 1);
                    return result;
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
        /// <param name="normalized"></param>
        /// <returns></returns>
        public Tuple<double, double>[]? GetHistogram(bool normalized)
        {
            if (normalized)
            {
                CleanData();

                if (data_ != null && data_.Count > 0)
                {
                    if (numberOfHistrogramPoints_ > data_.Count / 2)
                    {
                        numberOfHistrogramPoints_ = Convert.ToInt32(System.Math.Sqrt(data_.Count - 1) - 1);
                        if (numberOfHistrogramPoints_ <= 0) numberOfHistrogramPoints_ = 1;
                    }
                    double min, max;
                    min = max = data_[0];
                    for (int i = 0; i < data_.Count; i++)
                    {
                        if (data_[i] < min)
                        {
                            min = data_[i];
                        }
                        if (data_[i] > max)
                        {
                            max = data_[i];
                        }
                    }

                    double range = max - min;
                    if (Numeric.EQ(range, 0))
                    {
                        Tuple<double, double>[] histo = new Tuple<double, double>[2];
                        histo[0] = new Tuple<double, double>(data_[0] - data_[0] * 0.01, 0);
                        histo[1] = new Tuple<double, double>(data_[0], data_.Count);

                        return histo;
                    }
                    else
                    {
                        resolution_ = range / numberOfHistrogramPoints_;
                        Tuple<double, double>[] histogram = new Tuple<double, double>[numberOfHistrogramPoints_];

                        for (int i = 0; i < numberOfHistrogramPoints_; i++)
                        {
                            histogram[i] = new Tuple<double, double>(min + i * resolution_ + resolution_ / 2, 0);
                        }
                        for (int i = 0; i < data_.Count; i++)
                        {
                            if (data_[i] != max)
                            {
                                int index = System.Math.Min((int)((data_[i] - min) / resolution_), numberOfHistrogramPoints_ - 1);
                                histogram[index] = new Tuple<double, double>(histogram[index].Item1, histogram[index].Item2 + 1);
                            }
                            else
                            {
                                histogram[numberOfHistrogramPoints_ - 1] = new Tuple<double, double>(histogram[numberOfHistrogramPoints_ - 1].Item1, histogram[numberOfHistrogramPoints_ - 1].Item2 + 1);
                            }
                        }
                        double area = 0;
                        for (int i = 0; i < histogram.Length - 1; i++)
                        {
                            area += (histogram[i + 1].Item1 - histogram[i].Item1) * histogram[i].Item2;
                        }
                        if (Numeric.GT(area, 0))
                        {
                            for (int i = 0; i < histogram.Length; i++)
                            {
                                histogram[i] = new Tuple<double, double>(histogram[i].Item1, histogram[i].Item2 / area);
                            }
                        }
                        return histogram;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return GetHistogram();
            }
        }

        private void CleanData()
        {
            if (data_ != null && data_.Count > 0)
            {
                for (int i = 0; i < data_.Count; i++)
                {
                    if (!Numeric.IsDefined(data_[i]))
                    {
                        data_.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsValid()
        {
            if (!isFunctionUpdated_)
            {
                ComputeFunction();
            }
            if (function_ != null && function_.Length > 0)
            {
                double total = 0;
                for (int i = 0; i < function_.Length; i++)
                {
                    if (function_[i].Item2 < 0)
                    {
                        return false;
                    }
                    total += function_[i].Item2;
                }
                if (!Numeric.EQ(total, 1.0, 1e-3))
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public override void Copy(ContinuousDistribution? from)
        {
            if (from != null)
            {
                if (from is GeneralContinuousDistribution)
                {
                    resolution_ = ((GeneralContinuousDistribution)from).resolution_;
                    numberOfHistrogramPoints_ = ((GeneralContinuousDistribution)from).numberOfHistrogramPoints_;
                    function_ = new Tuple<double, double>[((GeneralContinuousDistribution)from).Function.Length];
                    if (from is GeneralContinuousDistribution)
                    {
                        for (int i = 0; i < ((GeneralContinuousDistribution)from).Function.Length; i++)
                        {
                            function_[i] = new Tuple<double, double>(((GeneralContinuousDistribution)from).Function[i].Item1, ((GeneralContinuousDistribution)from).Function[i].Item2);
                        }
                    }
                    data_.Clear();
                    for (int i = 0; i < ((GeneralContinuousDistribution)from).Data.Count; i++)
                    {
                        data_.Add(((GeneralContinuousDistribution)from).Data[i]);
                    }
                }
                CopyExtraData(from);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetMean()
        {
            double total = 0;
            double nbOfElmt = 0;
            if (data_ != null && data_.Count > 0)
            {
                foreach (double val in data_)
                {
                    if (Numeric.IsDefined(val))
                    {
                        total += val;
                        nbOfElmt++;
                    }
                }
                return nbOfElmt > 0 ? total / nbOfElmt : null;
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
        public override double? GetStandardDeviation()
        {
            double total = 0;
            double nbOfElmnts = 0;
            double? mean = GetMean();
            if (mean != null && data_ != null && data_.Count > 0)
            {
                foreach (double val in data_)
                {
                    if (Numeric.IsDefined(val))
                    {
                        total += val * val;
                        nbOfElmnts++;
                    }
                }
                double? result = nbOfElmnts > 0 ? System.Math.Sqrt(total / nbOfElmnts - mean.Value * mean.Value) : null;
                result *= System.Math.Sqrt((double)(nbOfElmnts) / (double)(nbOfElmnts - 1));
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
        public double? Variance()
        {
            if (!isFunctionUpdated_)
            {
                ComputeFunction();
            }
            if (function_ == null)
            {
                return null;
            }
            else
            {
                double[] xVec = new double[function_.Length];
                double[] yVec = new double[function_.Length];
                for (int i = 0; i < xVec.Length; i++)
                {
                    xVec[i] = function_[i].Item1;
                    yVec[i] = function_[i].Item2;
                }
                double Ex2 = 0;
                double variance;
                double a;
                double b;
                for (long i = 1; i < function_.Length; i++)
                {
                    a = (yVec[i] - yVec[i - 1]) / (xVec[i] - xVec[i - 1]);
                    b = yVec[i] - a * xVec[i];
                    Ex2 += 1 / 4f * a * System.Math.Pow(xVec[i], 4) + 1 / 3f * b * System.Math.Pow(xVec[i], 3) - 1 / 4f * a * System.Math.Pow(xVec[i - 1], 4) - 1 / 3f * b * System.Math.Pow(xVec[i - 1], 3);
                }
                double? Ex = GetMean();
                if (Ex != null)
                {
                    variance = Ex2 - System.Math.Pow(Ex.Value, 2);

                    return variance;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public override double? GetPercentile(double x)
        {

            if (data_ != null && data_.Count > 0 && x >= 0 && x <= 100)
            {
                if (!isDataSorted_)
                {
                    data_.Sort();
                    isDataSorted_ = true;
                }
                if (x == 0) return data_[0];
                if (x == 100) return data_[data_.Count - 1];
                int target = (int)x * (data_.Count - 1) / 100;
                double f = x / 100 - target / data_.Count;
                f /= data_.Count;
                return data_.Count == 1 ? data_[0] : data_[target] + f * (data_[target + 1] - data_[target]);
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
        public double[]? Get5thPercentiles()
        {
            if (!isFunctionUpdated_)
            {
                ComputeFunction();
            }
            if (function_ != null)
            {
                double[] result = new double[20];
                double temp = 0;
                double previousTemp = 0;
                double currentX = 0.05;
                int index = 0;
                double max = 0;
                for (int i = 0; i < function_.Length; i++)
                {
                    if (function_[i].Item2 != 0)
                    {
                        max = function_[i].Item1;
                    }
                    previousTemp = temp;
                    temp += function_[i].Item2;
                    if (Numeric.GT(temp, 1, 0.001))
                    {
                        return null;
                    }
                    if (temp >= currentX)
                    {
                        while (temp >= currentX && index < result.Length && i < function_.GetLength(0) - 1)
                        {
                            double prevValue = i >= 1 ? function_[i - 1].Item1 : 0;
                            result[index] = i >= 1 ? (function_[i].Item1 - function_[i - 1].Item1) * (currentX - previousTemp) / (temp - previousTemp) + function_[i - 1].Item1 : function_[i].Item1;
                            index++;
                            currentX += 0.05;
                        }
                    }
                }

                result[19] = max;
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
        public override double? GetDataMin()
        {
            double? result = null;
            if (data_ != null && data_.Count > 0)
            {
                if (!isDataSorted_)
                {
                    data_.Sort();
                    isDataSorted_ = true;
                }
                int index = 0;
                while (Numeric.IsUndefined(result) && index < data_.Count)
                {
                    result = data_[index];
                    index++;
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override double? GetDataMax()
        {
            double? result = null;
            if (data_ != null && data_.Count > 0)
            {
                if (!isDataSorted_)
                {
                    data_.Sort();
                    isDataSorted_ = true;
                }
                int index = data_.Count - 1;
                while (Numeric.IsUndefined(result) && index >= 0)
                {
                    result = data_[index];
                    index--;
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string GetDistributionTypeName()
        {
            return "Data Distribution";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override ContinuousDistribution Clone()
        {
            ContinuousDistribution result = new GeneralContinuousDistribution(data_);
            CloneExtraData(result);
            return result;

        }
    }
}
