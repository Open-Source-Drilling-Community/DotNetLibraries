using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    public struct Complex
    {
        private double real_;
        private double imag_;

        /// <summary>
        /// 
        /// </summary>
        public static readonly Complex Zero = new Complex(0, 0);

        /// <summary>
        /// 
        /// </summary>
        public static readonly Complex Unity = new Complex(1, 0);

        /// <summary>
        /// 
        /// </summary>
        public static readonly Complex I = new Complex(0, 1);

        /// <summary>
        /// 
        /// </summary>
        public static readonly Complex Undef = new Complex(Numeric.UNDEF_DOUBLE, Numeric.UNDEF_DOUBLE);
        /// <summary>
        /// 
        /// </summary>
        public static readonly Complex Epsilon = new Complex(Numeric.DOUBLE_ACCURACY, Numeric.DOUBLE_ACCURACY);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="real"></param>
        /// <param name="imag"></param>
        public Complex(double real, double imag)
        {
            real_ = real;
            imag_ = imag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="real"></param>
        /// <param name="imag"></param>
        public void Set(double real, double imag)
        {
            real_ = real;
            imag_ = imag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        public void Set(Complex c)
        {
            real_ = c.real_;
            imag_ = c.imag_;
        }

        /// <summary>
        /// 
        /// </summary>
        public double Real
        {
            get { return real_; }
            set { real_ = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Imaginary
        {
            get { return imag_; }
            set { imag_ = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Conjugate()
        {
            imag_ = -imag_;
        }

        /// <summary>
        /// 
        /// </summary>
        public double Modulus
        {
            get
            {
                return System.Math.Sqrt(real_ * real_ + imag_ * imag_);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double SquaredNorm
        {
            get
            {
                return real_ * real_ + imag_ * imag_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Argument
        {
            get
            {
                if (Numeric.EQ(real_, 0))
                {
                    if (Numeric.EQ(imag_, 0))
                    {
                        throw new DivideByZeroException();
                    }
                    else if (Numeric.LT(imag_, 0))
                    {
                        return -Numeric.PI / 2.0;
                    }
                    else
                    {
                        return Numeric.PI / 2.0;
                    }
                }
                else
                {
                    return System.Math.Atan2(imag_, real_);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double Abs(Complex a)
        {
            return System.Math.Sqrt(a.real_ * a.real_ + a.imag_ * a.imag_);
        }

        #region Arithmetic Operators

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.real_ + b.real_, a.imag_ + b.imag_);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator +(Complex a, double b)
        {
            return new Complex(a.real_ + b, a.imag_);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator +(double b, Complex a)
        {
            return new Complex(a.real_ + b, a.imag_);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Complex operator -(Complex a)
        {
            return new Complex(-a.real_, -a.imag_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.real_ - b.real_, a.imag_ - b.imag_);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator -(Complex a, double b)
        {
            return new Complex(a.real_ - b, a.imag_);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator -(double a, Complex b)
        {
            return new Complex(a - b.real_, -b.imag_);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex(a.real_ * b.real_ - a.imag_ * b.imag_, a.real_ * b.imag_ + a.imag_ * b.real_);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator *(Complex a, double b)
        {
            return new Complex(a.real_ * b, a.imag_ * b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator *(double b, Complex a)
        {
            return new Complex(a.real_ * b, a.imag_ * b);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator /(Complex a, Complex b)
        {
            double tmp = b.real_ * b.real_ + b.imag_ * b.imag_;
            if (Numeric.EQ(tmp, 0))
            {
                throw new DivideByZeroException();
            }
            return new Complex((a.real_ * b.real_ + a.imag_ * b.imag_) / tmp, (a.imag_ * b.real_ - a.real_ * b.imag_) / tmp);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator /(Complex a, double b)
        {
            if (Numeric.EQ(b, 0))
            {
                throw new DivideByZeroException();
            }
            return new Complex(a.real_/ b, a.imag_ / b);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator /(double a, Complex b)
        {
            double tmp = b.real_ * b.real_ + b.imag_ * b.imag_;
            if (Numeric.EQ(tmp, 0))
            {
                throw new DivideByZeroException();
            }
            return new Complex(a * b.real_ / tmp, -a * b.imag_ / tmp);
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Complex a, Complex b)
        {
            return a.Equals(b);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Complex a, Complex b)
        {
            return !a.Equals(b);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Complex a, double b)
        {
            return Numeric.EQ(a.real_, b) && Numeric.EQ(a.imag_, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Complex a, double b)
        {
            return !Numeric.EQ(a.real_, b) || !Numeric.EQ(a.imag_, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator == (double a, Complex b)
        {
            return Numeric.EQ(a, b.real_) && Numeric.EQ(b.imag_, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(double a, Complex b)
        {
            return !Numeric.EQ(a, b.real_) || !Numeric.EQ(b.imag_, 0);
        }
        #region IAddable<Complex> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Complex Add(Complex a)
        {
            return new Complex(real_ + a.real_, imag_ + a.imag_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Complex Negate()
        {
            return new Complex(-real_, -imag_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Complex Substract(Complex a)
        {
            return new Complex(real_ - a.real_, imag_ - a.imag_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Add(Complex a, ref Complex b)
        {
            b.real_ = real_ + a.real_;
            b.imag_ = imag_ + a.imag_;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Negate(ref Complex a)
        {
            a.real_ = -real_;
            a.imag_ = -imag_;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Substract(Complex a, ref Complex b)
        {
            b.real_ = real_ - a.real_;
            b.imag_ = imag_ - a.imag_;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void AddAssign(Complex a)
        {
            real_ = real_ + a.real_;
            imag_ = imag_ + a.imag_;
        }

        /// <summary>
        /// 
        /// </summary>
        public void NegateAssign()
        {
            real_ = -real_;
            imag_ = -imag_;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void SubstractAssign(Complex a)
        {
            real_ = real_ - a.real_;
            imag_ = imag_ - a.imag_;
        }

        #endregion

        #region IInvertableMultipliable<Complex> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Complex Time(Complex a)
        {
            return new Complex(real_ * a.real_ - imag_ * a.imag_, real_ * a.imag_ + imag_ * a.real_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Complex Invert()
        {
            double tmp = real_ * real_ + imag_ * imag_;
            if (Numeric.EQ(tmp, 0))
            {
                throw new DivideByZeroException();
            }
            return new Complex(real_ / tmp, -imag_ / tmp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Complex Divide(Complex a)
        {
            double tmp = a.real_ * a.real_ + a.imag_ * a.imag_;
            if (Numeric.EQ(tmp, 0))
            {
                throw new DivideByZeroException();
            }
            return new Complex((real_ * a.real_ + imag_ * a.imag_) / tmp, (imag_ * a.real_ - real_ * a.imag_) / tmp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void Time(Complex a, ref Complex b)
        {
            b.Set(real_ * a.real_ - imag_ * a.imag_, real_ * a.imag_ + imag_ * a.real_);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Invert(ref Complex a)
        {
            double tmp = real_ * real_ + imag_ * imag_;
            if (Numeric.EQ(tmp, 0))
            {
                throw new DivideByZeroException();
            }
            a.Set(real_ / tmp, -imag_ / tmp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public void Divide(Complex a, ref Complex b)
        {
            double tmp = a.real_ * a.real_ + a.imag_ * a.imag_;
            if (Numeric.EQ(tmp, 0))
            {
                throw new DivideByZeroException();
            }
            b.Set((real_ * a.real_ + imag_ * a.imag_) / tmp, (imag_ * a.real_ - real_ * a.imag_) / tmp);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void TimeAssign(Complex a)
        {
            Set(real_ * a.real_ - imag_ * a.imag_, real_ * a.imag_ + imag_ * a.real_);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InvertAssign()
        {
            double tmp = real_ * real_ + imag_ * imag_;
            if (Numeric.EQ(tmp, 0))
            {
                throw new DivideByZeroException();
            }
            Set(real_ / tmp, -imag_ / tmp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void DivideAssign(Complex a)
        {
            double tmp = a.real_ * a.real_ + a.imag_ * a.imag_;
            if (Numeric.EQ(tmp, 0))
            {
                throw new DivideByZeroException();
            }
            Set((real_ * a.real_ + imag_ * a.imag_) / tmp, (imag_ * a.real_ - real_ * a.imag_) / tmp);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Sqrt()
        {
            this.Set(Sqrt(this));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Complex Sqrt(Complex c)
        {
            double a = c.real_;
            double b = c.imag_;
            double sum = System.Math.Sqrt(a * a + b * b);
            double sign = System.Math.Sign(b);
            if (Numeric.EQ(sign, 0))
            {
                sign = 1;
            }
            return new Complex(System.Math.Sqrt((sum + a) / 2.0), sign * System.Math.Sqrt((sum - a) / 2.0));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Exp()
        {
            this.Set(System.Math.Exp(real_) * System.Math.Cos(imag_), System.Math.Exp(real_) * System.Math.Sin(imag_));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Complex Exp(Complex a)
        {
            return new Complex(System.Math.Exp(a.real_) * System.Math.Cos(a.imag_), System.Math.Exp(a.real_) * System.Math.Sin(a.imag_));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Log()
        {
            this.Set(System.Math.Log(Modulus), Argument);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Complex Log(Complex a)
        {
            return new Complex(System.Math.Log(a.Modulus), a.Argument);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Cos()
        {
            this.Set(System.Math.Cos(real_) * System.Math.Cosh(imag_), -System.Math.Sin(real_) * System.Math.Sinh(imag_));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Complex Cos(Complex a)
        {
            return new Complex(System.Math.Cos(a.real_) * System.Math.Cosh(a.imag_), -System.Math.Sin(a.real_) * System.Math.Sinh(a.imag_));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Sin()
        {
            this.Set(System.Math.Sin(real_) * System.Math.Cosh(imag_), System.Math.Cos(real_) * System.Math.Sinh(imag_));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Complex Sin(Complex a)
        {
            return new Complex(System.Math.Sin(a.real_) * System.Math.Cosh(a.imag_), System.Math.Cos(a.real_) * System.Math.Sinh(a.imag_));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Acos()
        {
            this.Set(Log(this + I * Sqrt(Unity - this * this)) / I);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Complex Acos(Complex a)
        {
            return Log(a + I * Sqrt(Unity - a * a)) / I;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Asin()
        {
            this.Set(Log(I * this + Sqrt(Unity - this * this)) / I);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Complex Asin(Complex a)
        {
            return Log(I * a + Sqrt(Unity - a * a)) / I;
        }

        #region IUndefinable Members

        /// <summary>
        /// 
        /// </summary>
        public void SetUndefined()
        {
            real_ = Numeric.UNDEF_DOUBLE;
            imag_ = Numeric.UNDEF_DOUBLE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            return Numeric.IsUndefined(real_) || Numeric.IsUndefined(imag_);
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Complex(real_, imag_);
        }

        #endregion

        #region ICopyable<Complex> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref Complex item)
        {
            item.real_ = real_;
            item.imag_ = imag_;
        }

        #endregion

        #region IEquatable<Complex> Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Complex other)
        {
            return Numeric.EQ(real_, other.real_, Numeric.DOUBLE_ACCURACY) && Numeric.EQ(imag_, other.imag_, Numeric.DOUBLE_ACCURACY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        public bool Equals(Complex other, Complex acc)
        {
            return Numeric.EQ(real_, other.real_, acc.real_) && Numeric.EQ(imag_, other.imag_, acc.imag_);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="real"></param>
        /// <param name="imag"></param>
        /// <returns></returns>
        public bool Equals(double real, double imag)
        {
            return Numeric.EQ(real_, real) && Numeric.EQ(imag_, imag);
        }

        #region IScalarProduct Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public Complex Time(double x)
        {
            return new Complex(real_ * x, imag_ * x);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="c"></param>
        public void Time(double x, ref Complex c)
        {
            c.real_ = real_ * x;
            c.imag_ = imag_ * x;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        public void TimeAssign(double x)
        {
            real_ = real_ * x;
            imag_ = imag_ * x;
        }

        #endregion

    }
}
