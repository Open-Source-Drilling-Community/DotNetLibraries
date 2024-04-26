
namespace OSDC.DotnetLibraries.General.Statistics
{
    public class DiscreteDistribution : IDistribution
    {

        protected DiscreteDistribution()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public bool Equals(DiscreteDistribution? cmp)
        {
            return cmp != null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        public void CopyTo(DiscreteDistribution? dest)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual double? GetProbability(int target)
        {
            throw new Exception("The method or operation cannot be implemented.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual double? GetCumulativeProbability(int target)
        {
            throw new Exception("The method or operation cannot be implemented.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual int? Realize()
        {
            throw new Exception("The method or operation cannot be implemented.");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        public virtual void Copy(DiscreteDistribution from)
        {
            throw new Exception("The method or operation cannot be implemented.");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual DiscreteDistribution Clone()
        {
            throw new Exception("The method or operation cannot be implemented.");
        }
        #region IDistribution Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Tuple<double, double>[]? GetCurve()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetMean()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetStandardDeviation()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual double? GetMostLikely()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        #endregion

        #region IDistribution Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        /// <summary>
        /// Invertion of CDF by bisection numeric root finding of CDF(x) = p for discrete distribution
        /// </summary>
        /// <param name="p"></param>
        /// <param name="xmin"></param>
        /// <param name="xmax"></param>
        /// <returns></returns>
        protected int Quantile(double p, int xmin, int xmax)
        {
            while (xmax - xmin > 1)
            {
                int xmed = (xmax + xmin) / 2;
                if (GetCumulativeProbability(xmed) > p)
                {
                    xmax = xmed;
                }
                else
                {
                    xmin = xmed;
                }
            }
            if (GetCumulativeProbability(xmin) >= p) return xmin;
            else return xmax;
        }
    }
}
