using OSDC.DotnetLibraries.General.Common;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace OSDC.DotnetLibraries.General.Math
{
    public class Polynom : IPolynom, IEquivalent<IPolynom>, IEquivalent<Polynom>, IUndefinable, ICloneable, ICopyable<IPolynom>, ICopyable<Polynom>, IZero
    {
        /// <summary>
        /// coefficients: coeffs_[0] + coeffs_[1]*x + ... coeffs_[n]*x^n
        /// </summary>
        private double[] coeffs_ { get; set; } = null;

        /// <summary>
        /// the number of coefficients is the degree of the polynom plus 1
        /// </summary>
        /// <param name="degree"></param>
        public Polynom(int degree)
        {
            if (degree >= 0)
            {
                coeffs_ = new double[degree+1];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public Polynom(double[] a)
        {
            if (a != null && a.Length >= 1)
            {
                coeffs_= new double[a.Length];
                Array.Copy(a, coeffs_, a.Length);
            }
            else
            {
                coeffs_ = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public Polynom(IPolynom p)
        {
            if (p != null && p.Degree >= 0)
            {
                coeffs_ = new double[p.Degree+1];
                for (int i = 0; i < p.Degree+1; i++)
                {
                    coeffs_[i] = p[i];
                }
            }
            else
            {
                coeffs_ = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public Polynom(Polynom p)
        {
            if (p != null && p.coeffs_ != null )
            {
                coeffs_ = new double[p.coeffs_.Length];
                Array.Copy(p.coeffs_, coeffs_, p.coeffs_.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (coeffs_ != null && coeffs_.Length > 0)
            {
                string s = coeffs_[0].ToString();
                for (int i = 1; i < coeffs_.Length; i++)
                {
                    s += "+" + coeffs_[i].ToString() + "*x^" + i.ToString();
                }
                return s;
            }
            else
            {
                return "null";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(Polynom p)
        {
            if (p != null && p.coeffs_ != null && p.coeffs_.Length > 0)
            {
                if (coeffs_ == null || coeffs_.Length != p.coeffs_.Length) 
                {
                    coeffs_ = new double[p.coeffs_.Length];
                }
                Array.Copy(p.coeffs_, coeffs_, p.coeffs_.Length);   
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Set(double[] a)
        {
            if (a != null && a.Length >= 1)
            {
                if (coeffs_ == null || coeffs_.Length != a.Length)
                {
                    coeffs_ = new double[a.Length];
                }
                Array.Copy(a, coeffs_, a.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Set(IPolynom p)
        {
            if (p != null && p.Degree >= 0)
            {
                if (coeffs_ == null || coeffs_.Length != p.Degree+1)
                {
                    coeffs_ = new double[p.Degree + 1];
                }
                for (int i = 0; i < coeffs_.Length; i++)
                {
                    coeffs_[i] = p[i];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public int FindRoots(List<Complex> roots)
        {
            List<double> coeffs = new List<double>();
            foreach (var c in coeffs_)
            {
                coeffs.Add(c);
            }
            return FindRoots(coeffs, roots);
        }

        private int FindRoots(List<double> coeffs, List<Complex> roots)
        {
            if (coeffs == null || coeffs.Count == 0 || roots == null)
            {
                return 0;
            }
            if (coeffs.Count == 1)
            {
                if (Numeric.EQ(coeffs[0], 0))
                {
                    roots.Clear();
                    roots[0] = Complex.Zero;
                    return 1;
                }
                else
                {
                    roots.Clear();
                    return 0;
                }
            }
            if (Numeric.EQ(coeffs[coeffs.Count-1], 0))
            {
                List<double> cs = new List<double>();
                for (int i = 0; i < coeffs.Count-1; i++)
                {
                    cs.Add(coeffs[i]);
                }
                return FindRoots(cs, roots);
            }
            else
            {
                int degree = coeffs.Count -1;
                // create the companion matrix for the polynomial
                Matrix<double> matrix = Matrix<double>.Build.Dense(degree, degree);
                for (int i = 0; i < degree; i++)
                {
                    matrix[i, degree - 1] = -coeffs[i] / coeffs[degree];
                }
                for (int i = 1; i < degree; i++)
                {
                    for (int j = 0; j < degree-1; j++)
                    {
                        if (j == i - 1)
                        {
                            matrix[i, j] = 1;
                        }
                        else
                        {
                            matrix[i, j] = 0;
                        }
                    }
                }

                // compute the eigenvalues of the companion matrix using a linear algebra library
                Evd<double> eigen = matrix.Evd();
                if (eigen != null)
                {
                    var eigenvalues = eigen.EigenValues;

                    // create a list of the complex roots found
                    roots.Clear();
                    foreach (var eigenvalue in eigenvalues)
                    {
                        roots.Add(new Complex(eigenvalue.Real, eigenvalue.Imaginary));
                    }

                    return roots.Count;
                }
                else
                {
                    return 0;
                }

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double FindRoot(double min, double max)
        {
            throw new NotImplementedException();
        }

        #region IPolynom Members
        /// <summary>
        /// 
        /// </summary>
        public int Degree
        {
            get 
            { 
                if (coeffs_ == null)
                {
                    return -1;
                }
                else
                {
                    return coeffs_.Length - 1;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double this[int index]
        {
            get
            {
                if (coeffs_ != null && index >= 0 && index < coeffs_.Length)
                {
                    return coeffs_[index];
                }
                else
                {
                    return Numeric.UNDEF_DOUBLE;
                }
            }
            set
            {
                if (coeffs_ != null && index >= 0 && index < coeffs_.Length)
                {
                    coeffs_[index] = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Eval(double x)
        {
            if (coeffs_ != null && coeffs_.Length > 0)
            {
                double xn = 1;
                double sum = coeffs_[0];
                for (int i = 1; i < coeffs_.Length; i++)
                {
                    xn *= x;
                    sum += coeffs_[i] * xn;
                }
                return sum;
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
            if (coeffs_ != null)
            {
                for (int i = 0; i < coeffs_.Length; i++)
                {
                    coeffs_[i] = Numeric.UNDEF_DOUBLE;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUndefined()
        {
            if (coeffs_ == null)
            {
                return true;
            }
            else
            {
                foreach (double v in coeffs_)
                {
                    if (Numeric.IsUndefined(v))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion

        #region ICloneable Members
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new Polynom(this);
        }

        #endregion

        #region ICopyable<IPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref IPolynom item)
        {
            if (item != null && item.Degree == Degree)
            {
                for (int i = 0; i < Degree+1; i++)
                {
                    item[i] = coeffs_[i];
                }
            }
        }

        #endregion

        #region IEquivalent<IPolynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(IPolynom other)
        {
            if (other != null && other.Degree == Degree)
            {
                if (coeffs_ != null)
                {
                    for (int i = 0; i < coeffs_.Length; i++)
                    {
                        if (!Numeric.EQ(coeffs_[i], other[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return Degree < 0;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(IPolynom other, double precision)
        {
            if (other != null && other.Degree == Degree)
            {
                if (coeffs_ != null)
                {
                    for (int i = 0; i < coeffs_.Length; i++)
                    {
                        if (!Numeric.EQ(coeffs_[i], other[i], precision))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return Degree < 0;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region ICopyable<Polynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Copy(ref Polynom item)
        {
            if (item != null && coeffs_ != null)
            {
                if (item.coeffs_ == null || item.coeffs_.Length != coeffs_.Length)
                {
                    item.coeffs_ = new double[coeffs_.Length];
                }
                Array.Copy(coeffs_, item.coeffs_, coeffs_.Length);
            }
       }

        #endregion

        #region IEquivalent<Polynom> Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(Polynom other)
        {
            if (other != null && Degree == other.Degree)
            {
                if (coeffs_ != null)
                {
                    for (int i = 0; i < coeffs_.Length; i++)
                    {
                        if (!Numeric.EQ(coeffs_[i], other.coeffs_[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return other.coeffs_ == null || other.coeffs_.Length == 0;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool EQ(Polynom other, double precision)
        {
            if (other != null && Degree == other.Degree)
            {
                if (coeffs_ != null)
                {
                    for (int i = 0; i < coeffs_.Length; i++)
                    {
                        if (!Numeric.EQ(coeffs_[i], other.coeffs_[i], precision))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return other.coeffs_ == null || other.coeffs_.Length == 0;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region IZero Members
        /// <summary>
        /// 
        /// </summary>
        public void SetZero()
        {
            if (coeffs_ != null)
            {
                for (int i = 0; i < coeffs_.Length; i++)
                {
                    coeffs_[i] = 0;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsZero()
        {
            if (coeffs_ != null)
            {
                for (int i = 0; i < coeffs_.Length; i++)
                {
                    if (!Numeric.EQ(coeffs_[i], 0))
                    {
                         return false;   
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double Derive(double x)
        {
            if (coeffs_ != null && coeffs_.Length > 0)
            {
                double sum = 0;
                double xn = 1.0;
                for (int i = 1; i < coeffs_.Length; i++)
                {
                    sum += i * coeffs_[i] * xn;
                    xn *= x;
                }
                return sum;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double DeriveSecond(double x)
        {
            if (coeffs_ != null && coeffs_.Length > 0)
            {
                double sum = 0;
                double xn = 1.0;
                for (int i = 2; i < coeffs_.Length; i++)
                {
                    sum += i * (i - 1) * coeffs_[i] * xn;
                    xn *= x;
                }
                return sum;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public double Integrate(double a, double b)
        {
            if (coeffs_ != null)
            {
                double sum = 0;
                double an = 1.0;
                double bn = 1.0;
                for (int i = 0; i < coeffs_.Length; i++)
                {
                    an *= a;
                    bn *= b;
                    sum += coeffs_[i] * (bn - an) / (i + 1);
                }
                return sum;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Derivate(ref IPolynom p)
        {
            if (coeffs_ != null && p != null && p.Degree >= Degree-1)
            {
                for (int i = 1; i < coeffs_.Length; i++)
                {
                    p[i - 1] = i * coeffs_[i];
                }
                for (int i = coeffs_.Length-1; i < p.Degree+1; i++)
                {
                    p[i] = 0;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public void Primitive(ref IPolynom p)
        {
            if (coeffs_ != null && p != null && p.Degree >= Degree + 1)
            {
                p[0] = 0.0;
                for (int i = 0; i < coeffs_.Length; i++)
                {
                    p[i + 1] = coeffs_[i] / (i + 1);
                }
                for (int i = coeffs_.Length+1; i < p.Degree+1; i++)
                {
                    p[i] = 0;
                }
            }
        }

    }
}
