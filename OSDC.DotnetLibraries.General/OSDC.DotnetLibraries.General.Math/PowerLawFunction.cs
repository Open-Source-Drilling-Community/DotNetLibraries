using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// Implements a function of the form 
    /// 
    ///     f(x) :=  a0 + a1 * x ^ a2
    /// 
    /// </summary>
    public struct PowerLawFunction : IValuable, IValuable<double>, IUndefinable, ICopyable<PowerLawFunction>, IEquivalent<PowerLawFunction>

    {
        private double a0_;
        private double a1_;
        private double a2_;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a0"></param>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        public PowerLawFunction(double a0, double a1, double a2)
        {
            a0_ = a0;
            a1_ = a1;
            a2_ = a2;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a0"></param>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        public void Set(double a0, double a1, double a2)
        {
            a0_ = a0;
            a1_ = a1;
            a2_ = a2;
        }

        #region IValuable Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Eval(double x)
        {
            if (Numeric.GT(x, 0))
            {
                return a0_ + a1_ * System.Math.Pow(x, a2_);
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        #endregion

        #region IDerivable Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Derivate(double x)
        {
            if (Numeric.GT(x, 0))
            {
                return a1_ * a2_ * System.Math.Pow(x, a2_ - 1);
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        #endregion

        #region IUndefinable Members
        /// <summary>
        /// 
        /// </summary>
        public void SetUndefined()
        {
            a0_ = a1_ = a2_ = Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            return Numeric.IsUndefined(a0_) || Numeric.IsUndefined(a1_) || Numeric.IsUndefined(a2_);
        }

        #endregion

        #region ICopyable<PowerLawFunction> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref PowerLawFunction item)
        {
            item.a0_ = a0_;
            item.a1_ = a1_;
            item.a2_ = a2_;
        }

        #endregion

        #region IEquivalent<PowerLawFunction> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(PowerLawFunction other)
        {
            return EQ(other, Numeric.DOUBLE_ACCURACY);
        }

        public bool EQ(PowerLawFunction other, double precision)
        { 
            return Numeric.EQ(a0_, other.a0_, precision) && 
                Numeric.EQ(a1_, other.a1_, precision) && 
                Numeric.EQ(a2_, other.a2_, precision);
        }

        #endregion
    }
}
