using OSDC.DotnetLibraries.General.Common;

namespace OSDC.DotnetLibraries.General.Math
{
    /// <summary>
    /// This package is intended to provide routines for special functions such as 
    /// gamma, beta, zeta, binomial coefficients, factorials or special polynomials...
    /// </summary>
    public static class SpecialFunctions
    {
        private const double MACHEP = 1.11022302462515654042E-16;
        private const double MAXLOG = 7.09782712893383996732E2;
        private const double MINLOG = -7.451332191019412076235E2;
        private const double MAXGAM = 171.624376956302725;

        private const int INCOMPLETE_GAMMA_MAX_ITERATIONS = 1000; //Maximum number of iterations allowed in Incomplete Gamma Function calculations
        private const double INCOMPLETE_GAMMA_EPSILON = 1.0E-8; //Tolerance used in terminating series in Incomplete Gamma Function calculations
        private const double FPMIN = 1e-300; //A small number close to the smallest representable floating point number.

        /// <summary>
        /// Method to compute the factorial of an integer, e.g. 4! = 4 * 3 * 2 * 1.
        /// This method returns a double, to avoid overflow for Int32.
        /// Max value for n is 170.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double Factorial(int n)
        {
            if (n > 170) throw new OverflowException("Maxium value for factorial is n = 170");
            else if (n <= 1) return 1;
            else return n * Factorial(n - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static int BinomialCoefficient(int n, int k)
        {
            if (k > n) return 0;
            if (n == k) return 1;
            int n1 = n;
            int k1 = k;
            if (k > n - k) k1 = n - k;
            int c = 1;
            for (int i = 1; i <= k1; i++)
            {
                c *= n1--;
                c /= i;
            }
            return c;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double Beta(double x, double y)
        {
            return Gamma(x) * Gamma(y) / (Gamma(x + y));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Gamma(double x)
        {
            if (Numeric.GT(x, 0, 1e-8))
            {
                return System.Math.Exp(LogGamma(x));
            }
            else return Numeric.UNDEF_DOUBLE;
        }
        /// <summary>
        /// Computes the gamma function, defined by 
        /// 
        /// \Gamma(z)  := \int_0^\infty t^{z-1} \exp^{-t} dt
        /// 
        /// the following routine only computes gamma for real positives values
        /// It is based on Lanczos approximation, as desrcibed in Numerical Recipes in C, p213, on-line edition
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public static double LogGamma(double z)
        {
            if (Numeric.GT(z, 0, 1e-8))
            {
                double x, y, temp, ser;
                double[] cof = {76.18009172947146,
                            -86.50532032941677,
                            24.01409824083091,
                            -1.23173957245055,
                            0.1208650973866179e-2,
                            -0.5395239384953e-5};
                y = x = z;
                temp = x + 5.5;
                temp -= (x + 0.5) * System.Math.Log(temp);
                ser = 1.000000000190015;
                for (int i = 0; i < 6; i++)
                {
                    ser += cof[i] / ++y;
                }
                return -temp + System.Math.Log(2.5066282746310005 * ser / x);
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// Computes the incomplete gamma function, by its series representations
        /// 
        /// The continued fraction approach is supposed to converge faster for x>a+1, 
        /// to do...
        /// Numerical Recipes in C, p217 o fthe on-line edition
        /// </summary>
        /// <param name="a"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double IncompleteGamma(double a, double x, double logGammaa)
        {
            if (Numeric.EQ(x, 0))
            {
                return 0;
            }
            //int maxIterations = 100;
            //double epsilon = 3.0e-7;
            double gln = logGammaa;
            double sum, del, ap;
            double result = Numeric.UNDEF_DOUBLE;
            if (Numeric.GE(x, 0, 1e-8) && !Numeric.EQ(a, 0, 1e-8))
            {
                ap = a;
                del = sum = 1.0 / a;
                int i = 0;
                while (i < INCOMPLETE_GAMMA_MAX_ITERATIONS)
                {
                    ++ap;
                    del *= x / ap;
                    sum += del;
                    if (Numeric.LE(System.Math.Abs(del), System.Math.Abs(sum) * INCOMPLETE_GAMMA_EPSILON))
                    {
                        result = sum * System.Math.Exp(-x + a * System.Math.Log(x) - gln);
                        return result;
                    }
                    i++;
                }
                return result;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }

        /// <summary>
        /// Computes the incomplete gamma function, by its series representations
        /// 
        /// The continued fraction approach is supposed to converge faster for x>a+1, 
        /// to do...
        /// Numerical Recipes in C, p217 o fthe on-line edition
        /// </summary>
        /// <param name="a"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double IncompleteGamma(double a, double x)
        {
            double gln = LogGamma(a);
            return IncompleteGamma(a, x, gln);
        }
        /// <summary>
        /// Computes the complement of the incomplete gamma function(i.e. 1- igf), by its series representations
        /// 
        /// The continued fraction approach is supposed to converge faster for x>a+1, 
        /// to do...
        /// Numerical Recipes in C, p217 o fthe on-line edition
        /// </summary>
        /// <param name="a"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ComplementIncompleteGamma(double a, double x)
        {
            return 1 - IncompleteGamma(a, x);
        }

        /// <summary>
        /// Returns the incomplete beta function evaluated from zero to xx.
        /// Code is taken from http://www.codeproject.com/Articles/11647/Special-Function-s-for-C
        /// </summary>
        /// <param name="aa"></param>
        /// <param name="bb"></param>
        /// <param name="xx"></param>
        /// <returns></returns>
        public static double IncompleteBeta(double aa, double bb, double xx)
        {
            double a, b, t, x, xc, w, y;
            bool flag;

            if (aa <= 0.0 || bb <= 0.0) throw new ArithmeticException("Incomplete Beta: Domain error!");

            if ((xx <= 0.0) || (xx >= 1.0))
            {
                if (xx == 0.0) return 0.0;
                if (xx == 1.0) return 1.0;
                throw new ArithmeticException("Incomplete Beta: Domain error!");
            }

            flag = false;
            if ((bb * xx) <= 1.0 && xx <= 0.95)
            {
                t = PowerSeriesIncompleteBeta(aa, bb, xx);
                return t;
            }

            w = 1.0 - xx;

            /* Reverse a and b if x is greater than the mean. */
            if (xx > (aa / (aa + bb)))
            {
                flag = true;
                a = bb;
                b = aa;
                xc = xx;
                x = w;
            }
            else
            {
                a = aa;
                b = bb;
                xc = w;
                x = xx;
            }

            if (flag && (b * x) <= 1.0 && x <= 0.95)
            {
                t = PowerSeriesIncompleteBeta(a, b, x);
                if (t <= MACHEP) t = 1.0 - MACHEP;
                else t = 1.0 - t;
                return t;
            }

            /* Choose expansion for better convergence. */
            y = x * (a + b - 2.0) - (a - 1.0);
            if (y < 0.0)
                w = ContinuedFractionExpansionIncompleteBeta1(a, b, x);
            else
                w = ContinuedFractionExpansionIncompleteBeta2(a, b, x) / xc;

            /* Multiply w by the factor
                   a      b   _             _     _
                  x  (1-x)   | (a+b) / ( a | (a) | (b) ) .   */

            y = a * System.Math.Log(x);
            t = b * System.Math.Log(xc);
            if ((a + b) < MAXGAM && System.Math.Abs(y) < MAXLOG && System.Math.Abs(t) < MAXLOG)
            {
                t = System.Math.Pow(xc, b);
                t *= System.Math.Pow(x, a);
                t /= a;
                t *= w;
                t *= Gamma(a + b) / (Gamma(a) * Gamma(b));
                if (flag)
                {
                    if (t <= MACHEP) t = 1.0 - MACHEP;
                    else t = 1.0 - t;
                }
                return t;
            }
            /* Resort to logarithms.  */
            y += t + LogGamma(a + b) - LogGamma(a) - LogGamma(b);
            y += System.Math.Log(w / a);
            if (y < MINLOG)
                t = 0.0;
            else
                t = System.Math.Exp(y);

            if (flag)
            {
                if (t <= MACHEP) t = 1.0 - MACHEP;
                else t = 1.0 - t;
            }
            return t;
        }

        /// <summary>
        /// WARNING: DEPRECATED! Use RegularizedIncompleteBeta() instead!
        /// </summary>
        /// <param name="aa"></param>
        /// <param name="bb"></param>
        /// <param name="xx"></param>
        /// <returns></returns>
        public static double IncompleteBetaRegularized(double aa, double bb, double xx)
        {
            return IncompleteBeta(aa, bb, xx) / Beta(aa, bb);
        }

        /// <summary>
        /// Returns the power series for incomplete beta integral. Use when b*x is small and x not too close to 1.
        /// Code is taken from http://www.codeproject.com/Articles/11647/Special-Function-s-for-C
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double PowerSeriesIncompleteBeta(double a, double b, double x)
        {
            double s, t, u, v, n, t1, z, ai;

            ai = 1.0 / a;
            u = (1.0 - b) * x;
            v = u / (a + 1.0);
            t1 = v;
            t = u;
            n = 2.0;
            s = 0.0;
            z = MACHEP * ai;
            while (System.Math.Abs(v) > z)
            {
                u = (n - b) * x / n;
                t *= u;
                v = t / (a + n);
                s += v;
                n += 1.0;
            }
            s += t1;
            s += ai;

            u = a * System.Math.Log(x);
            if ((a + b) < MAXGAM && System.Math.Abs(u) < MAXLOG)
            {
                t = Gamma(a + b) / (Gamma(a) * Gamma(b));
                s = s * t * System.Math.Pow(x, a);
            }
            else
            {
                t = LogGamma(a + b) - LogGamma(a) - LogGamma(b) + u + System.Math.Log(s);
                if (t < MINLOG) s = 0.0;
                else s = System.Math.Exp(t);
            }
            return s;
        }

        /// <summary>
        /// Returns the continued fraction expansion #1 for incomplete beta integral.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double ContinuedFractionExpansionIncompleteBeta1(double a, double b, double x)
        {
            double xk, pk, pkm1, pkm2, qk, qkm1, qkm2;
            double k1, k2, k3, k4, k5, k6, k7, k8;
            double r, t, ans, thresh;
            int n;
            double big = 4.503599627370496e15;
            double biginv = 2.22044604925031308085e-16;

            k1 = a;
            k2 = a + b;
            k3 = a;
            k4 = a + 1.0;
            k5 = 1.0;
            k6 = b - 1.0;
            k7 = k4;
            k8 = a + 2.0;

            pkm2 = 0.0;
            qkm2 = 1.0;
            pkm1 = 1.0;
            qkm1 = 1.0;
            ans = 1.0;
            r = 1.0;
            n = 0;
            thresh = 3.0 * MACHEP;
            do
            {
                xk = -(x * k1 * k2) / (k3 * k4);
                pk = pkm1 + pkm2 * xk;
                qk = qkm1 + qkm2 * xk;
                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;

                xk = (x * k5 * k6) / (k7 * k8);
                pk = pkm1 + pkm2 * xk;
                qk = qkm1 + qkm2 * xk;
                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;

                if (qk != 0) r = pk / qk;
                if (r != 0)
                {
                    t = System.Math.Abs((ans - r) / r);
                    ans = r;
                }
                else
                    t = 1.0;

                if (t < thresh) return ans;

                k1 += 1.0;
                k2 += 1.0;
                k3 += 2.0;
                k4 += 2.0;
                k5 += 1.0;
                k6 -= 1.0;
                k7 += 2.0;
                k8 += 2.0;

                if ((System.Math.Abs(qk) + System.Math.Abs(pk)) > big)
                {
                    pkm2 *= biginv;
                    pkm1 *= biginv;
                    qkm2 *= biginv;
                    qkm1 *= biginv;
                }
                if ((System.Math.Abs(qk) < biginv) || (System.Math.Abs(pk) < biginv))
                {
                    pkm2 *= big;
                    pkm1 *= big;
                    qkm2 *= big;
                    qkm1 *= big;
                }
            } while (++n < 300);

            return ans;
        }

        /// <summary>
        /// Returns the continued fraction expansion #2 for incomplete beta integral.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double ContinuedFractionExpansionIncompleteBeta2(double a, double b, double x)
        {
            double xk, pk, pkm1, pkm2, qk, qkm1, qkm2;
            double k1, k2, k3, k4, k5, k6, k7, k8;
            double r, t, ans, z, thresh;
            int n;
            double big = 4.503599627370496e15;
            double biginv = 2.22044604925031308085e-16;

            k1 = a;
            k2 = b - 1.0;
            k3 = a;
            k4 = a + 1.0;
            k5 = 1.0;
            k6 = a + b;
            k7 = a + 1.0;
            ;
            k8 = a + 2.0;

            pkm2 = 0.0;
            qkm2 = 1.0;
            pkm1 = 1.0;
            qkm1 = 1.0;
            z = x / (1.0 - x);
            ans = 1.0;
            r = 1.0;
            n = 0;
            thresh = 3.0 * MACHEP;
            do
            {
                xk = -(z * k1 * k2) / (k3 * k4);
                pk = pkm1 + pkm2 * xk;
                qk = qkm1 + qkm2 * xk;
                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;

                xk = (z * k5 * k6) / (k7 * k8);
                pk = pkm1 + pkm2 * xk;
                qk = qkm1 + qkm2 * xk;
                pkm2 = pkm1;
                pkm1 = pk;
                qkm2 = qkm1;
                qkm1 = qk;

                if (qk != 0) r = pk / qk;
                if (r != 0)
                {
                    t = System.Math.Abs((ans - r) / r);
                    ans = r;
                }
                else
                    t = 1.0;

                if (t < thresh) return ans;

                k1 += 1.0;
                k2 -= 1.0;
                k3 += 2.0;
                k4 += 2.0;
                k5 += 1.0;
                k6 += 1.0;
                k7 += 2.0;
                k8 += 2.0;

                if ((System.Math.Abs(qk) + System.Math.Abs(pk)) > big)
                {
                    pkm2 *= biginv;
                    pkm1 *= biginv;
                    qkm2 *= biginv;
                    qkm1 *= biginv;
                }
                if ((System.Math.Abs(qk) < biginv) || (System.Math.Abs(pk) < biginv))
                {
                    pkm2 *= big;
                    pkm1 *= big;
                    qkm2 *= big;
                    qkm1 *= big;
                }
            } while (++n < 300);

            return ans;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ErrorFunction(double x)
        {
            if (Numeric.GT(System.Math.Abs(x), 7.696))
            {
                return 1.0 - ErFC(x);
            }
            return x < 0 ? -IncompleteGamma(0.5, x * x) : IncompleteGamma(0.5, x * x);
        }

        /// <summary>
        /// Computes the gamma function, defined by 
        /// 
        /// \Gamma(z)  := \int_0^\infty t^{z-1} \exp^{-t} dt
        /// 
        /// the following routine only computes gamma for real positives values
        /// It is based on Lanczos approximation, as desrcibed in Numerical Recipes in C, p213, on-line edition
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public static float LogGamma(float z)
        {
            if (Numeric.GT(z, 0, 1e-8))
            {
                float x, y, temp, ser;
                float[] cof = {76.18009172947146f,
                            -86.50532032941677f,
                            24.01409824083091f,
                            -1.23173957245055f,
                            0.1208650973866179e-2f,
                            -0.5395239384953e-5f};
                y = x = z;
                temp = x + 5.5f;
                temp -= (x + 0.5f) * (float)System.Math.Log(temp);
                ser = 1.000000000190015f;
                for (int i = 0; i < 6; i++)
                {
                    ser += cof[i] / ++y;
                }
                return -temp + (float)System.Math.Log(2.5066282746310005f * ser / x);
            }
            else
            {
                return Numeric.UNDEF_FLOAT;
            }
        }
        /// <summary>
        /// Computes the incomplete gamma function, by its series representations
        /// 
        /// The continued fraction approach is supposed to converge faster for x>a+1, 
        /// to do...
        /// Numerical Recipes in C, p217 o fthe on-line edition
        /// </summary>
        /// <param name="a"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float IncompleteGamma(float a, float x)
        {
            int maxIterations = 100;
            float epsilon = 3.0e-7f;
            float gln = LogGamma(a);
            float sum, del, ap;
            float result = Numeric.UNDEF_FLOAT;
            if (Numeric.GT(x, 0, 1e-8) && !Numeric.EQ(a, 0, 1e-8))
            {
                ap = a;
                del = sum = 1.0f / a;
                int i = 0;
                while (i < maxIterations)
                {
                    ++ap;
                    del *= x / ap;
                    sum += del;
                    if (Numeric.LE(System.Math.Abs(del), System.Math.Abs(sum) * epsilon))
                    {
                        result = sum * (float)System.Math.Exp(-x + a * System.Math.Log(x) - gln);
                        return result;
                    }
                    i++;
                }
                return result;
            }
            else
            {
                return Numeric.UNDEF_FLOAT;
            }
        }
        /// <summary>
        /// Computes the complement of the incomplete gamma function(i.e. 1- igf), by its series representations
        /// 
        /// The continued fraction approach is supposed to converge faster for x>a+1, 
        /// to do...
        /// Numerical Recipes in C, p217 o fthe on-line edition
        /// </summary>
        /// <param name="a"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float ComplementIncompleteGamma(float a, float x)
        {
            return 1 - IncompleteGamma(a, x);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float ErrorFunction(float x)
        {
            if (Numeric.GT(System.Math.Abs(x), 7.696))
            {
                return 1.0f - ErFC(x);
            }
            return x < 0 ? -IncompleteGamma(0.5f, x * x) : IncompleteGamma(0.5f, x * x);
        }

        /// <summary>
        /// Complementary error function
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double ErFC(double x)
        {
            double t, z, ans;
            z = System.Math.Abs(x);
            t = 1 / (1 + 0.5 * z);
            ans = t * System.Math.Exp(-z * z - 1.26551223 + t * (1.00002368 + t * (0.37409196 + t * (0.09678418 +
                t * (-0.18628806 + t * (0.27886807 + t * (-1.13520398 + t * (1.48851587 + t * (-0.82215223 + t * 0.17087277)))))))));
            return Numeric.GE(x, 0) ? ans : 2 - ans;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float ErFC(float x)
        {
            float t, z, ans;
            z = System.Math.Abs(x);
            t = 1 / (1 + 0.5f * z);
            ans = t * (float)System.Math.Exp(-z * z - 1.26551223f + t * (1.00002368f + t * (0.37409196f + t * (0.09678418f +
                t * (-0.18628806f + t * (0.27886807f + t * (-1.13520398f + t * (1.48851587f + t * (-0.82215223f + t * 0.17087277f)))))))));
            return Numeric.GE(x, 0) ? ans : 2 - ans;
        }

        /// <summary>
        /// Inverse complementary error function
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double InverseErFC(double p)
        {
            double x, err, t, pp;
            if (p >= 2.0)
            {
                return -100.0;
            }
            if (p <= 0.0)
            {
                return 100.0;
            }
            pp = (p < 1.0) ? p : 2.0 - p;
            t = System.Math.Sqrt(-2.0 * System.Math.Log(pp / 2.0));
            x = -0.70711 * ((2.30753 + t * 0.27061) / (1.0 + t * (0.99229 + t * 0.04481)) - t);
            for (int j = 0; j < 2; j++)
            {
                err = ErFC(x) - pp;
                x += err / (1.12837916709551257 * System.Math.Exp(-x * x) - x * err);
            }
            return (p < 1.0 ? x : -x);
        }

        /**
        * Regularized Incomplete Gamma Function P(a,x) = <i><big>&#8747;</big><sub><small>0</small></sub><sup><small>x</small></sup> e<sup>-t</sup> t<sup>(a-1)</sup> dt</i>.
        * Series representation of the function - valid for x < a + 1
         * 
         * Implementation from: https://github.com/haifengl/smile/blob/master/math/src/main/java/smile/math/special/Gamma.java
        */
        private static double RegularizedIncompleteGammaSeries(double a, double x)
        {
            if (a < 0.0 || x < 0.0 || x >= a + 1)
            {
                return Numeric.UNDEF_DOUBLE;//throw new ArgumentException(String.Format("Invalid a = %f, x = %f", a, x));
            }

            int i = 0;
            double igf = 0.0;
            bool check = true;

            double acopy = a;
            double sum = 1.0 / a;
            double incr = sum;
            double loggamma = LogGamma(a);

            while (check)
            {
                ++i;
                ++a;
                incr *= x / a;
                sum += incr;
                if (System.Math.Abs(incr) < System.Math.Abs(sum) * INCOMPLETE_GAMMA_EPSILON)
                {
                    igf = sum * System.Math.Exp(-x + acopy * System.Math.Log(x) - loggamma);
                    check = false;
                }
                if (i >= INCOMPLETE_GAMMA_MAX_ITERATIONS)
                {
                    check = false;
                    igf = sum * System.Math.Exp(-x + acopy * System.Math.Log(x) - loggamma);
                    //logger.error("RegularizedIncompleteGammaSeries: Maximum number of iterations wes exceeded");
                }
            }
            return igf;
        }

        /**
        * Regularized Incomplete Gamma Function P(a,x) = <i><big>&#8747;</big><sub><small>0</small></sub><sup><small>x</small></sup> e<sup>-t</sup> t<sup>(a-1)</sup> dt</i>.
        * Continued Fraction representation of the function - valid for x &ge; a + 1
        * This method follows the general procedure used in Numerical Recipes.
         * 
         * Implementation from: https://github.com/haifengl/smile/blob/master/math/src/main/java/smile/math/special/Gamma.java
        */
        private static double RegularizedIncompleteGammaFraction(double a, double x)
        {
            if (a < 0.0 || x < 0.0 || x < a + 1)
            {
                return Numeric.UNDEF_DOUBLE;//throw new ArgumentException(String.Format("Invalid a = %f, x = %f", a, x));
            }

            int i = 0;
            double ii = 0.0;
            double igf = 0.0;
            bool check = true;

            double loggamma = LogGamma(a);
            double numer = 0.0;
            double incr = 0.0;
            double denom = x - a + 1.0;
            double first = 1.0 / denom;
            double term = 1.0 / FPMIN;
            double prod = first;

            while (check)
            {
                ++i;
                ii = (double)i;
                numer = -ii * (ii - a);
                denom += 2.0D;
                first = numer * first + denom;
                if (System.Math.Abs(first) < FPMIN)
                {
                    first = FPMIN;
                }
                term = denom + numer / term;
                if (System.Math.Abs(term) < FPMIN)
                {
                    term = FPMIN;
                }
                first = 1.0D / first;
                incr = first * term;
                prod *= incr;
                if (System.Math.Abs(incr - 1.0D) < INCOMPLETE_GAMMA_EPSILON)
                {
                    check = false;
                }
                if (i >= INCOMPLETE_GAMMA_MAX_ITERATIONS)
                {
                    check = false;
                    //logger.error("Gamma.regularizedIncompleteGammaFraction: Maximum number of iterations wes exceeded");
                }
            }
            igf = 1.0 - System.Math.Exp(-x + a * System.Math.Log(x) - loggamma) * prod;
            return igf;
        }

        /**
        * Regularized Incomplete Gamma Function
        * P(s,x) = <i><big>&#8747;</big><sub><small>0</small></sub><sup><small>x</small></sup> e<sup>-t</sup> t<sup>(s-1)</sup> dt</i>
         * 
         * Implementation from: https://github.com/haifengl/smile/blob/master/math/src/main/java/smile/math/special/Gamma.java
        */
        public static double RegularizedIncompleteGamma(double s, double x)
        {
            if (s < 0.0)
            {
                return Numeric.UNDEF_DOUBLE;//throw new ArgumentException("Invalid s: " + s);
            }

            if (x < 0.0)
            {
                return Numeric.UNDEF_DOUBLE;//throw new ArgumentException("Invalid x: " + x);
            }

            double igf = 0.0;

            if (x < s + 1.0)
            {
                // Series representation
                igf = RegularizedIncompleteGammaSeries(s, x);
            }
            else
            {
                // Continued fraction representation
                igf = RegularizedIncompleteGammaFraction(s, x);
            }

            return igf;
        }

        /**
        * The inverse of regularized incomplete gamma function.
         * 
         * Implementation from: https://github.com/haifengl/smile/blob/master/math/src/main/java/smile/math/special/Gamma.java
        */
        public static double InverseRegularizedIncompleteGamma(double a, double p)
        {
            if (a <= 0.0)
            {
                return Numeric.UNDEF_DOUBLE;//throw new ArgumentException("a must be pos in invgammap");
            }

            double EPS = 1.0E-8;

            double x, err, t, u, pp;
            double lna1 = 0.0;
            double afac = 0.0;
            double a1 = a - 1;
            double gln = LogGamma(a);
            if (p >= 1.0)
            {
                return System.Math.Max(100.0, a + 100.0 * System.Math.Sqrt(a));
            }

            if (p <= 0.0)
            {
                return 0.0;
            }

            if (a > 1.0)
            {
                lna1 = System.Math.Log(a1);
                afac = System.Math.Exp(a1 * (lna1 - 1.0) - gln);
                pp = (p < 0.5) ? p : 1.0 - p;
                t = System.Math.Sqrt(-2.0 * System.Math.Log(pp));
                x = (2.30753 + t * 0.27061) / (1.0 + t * (0.99229 + t * 0.04481)) - t;
                if (p < 0.5)
                {
                    x = -x;
                }
                x = System.Math.Max(1.0e-3, a * System.Math.Pow(1.0 - 1.0 / (9.0 * a) - x / (3.0 * System.Math.Sqrt(a)), 3));
            }
            else
            {
                t = 1.0 - a * (0.253 + a * 0.12);
                if (p < t)
                {
                    x = System.Math.Pow(p / t, 1.0 / a);
                }
                else
                {
                    x = 1.0 - System.Math.Log(1.0 - (p - t) / (1.0 - t));
                }
            }
            for (int j = 0; j < 12; j++)
            {
                if (x <= 0.0)
                {
                    return 0.0;
                }
                err = RegularizedIncompleteGamma(a, x) - p;
                if (a > 1.0)
                {
                    t = afac * System.Math.Exp(-(x - a1) + a1 * (System.Math.Log(x) - lna1));
                }
                else
                {
                    t = System.Math.Exp(-x + a1 * System.Math.Log(x) - gln);
                }
                u = err / t;
                x -= (t = u / (1.0 - 0.5 * System.Math.Min(1.0, u * ((a - 1.0) / x - 1))));
                if (x <= 0.0)
                {
                    x = 0.5 * (x + t);
                }
                if (System.Math.Abs(t) < EPS * x)
                {
                    break;
                }
            }
            return x;
        }

        /// <summary>
        /// Implementation from https://github.com/haifengl/smile/blob/master/math/src/main/java/smile/math/special/Beta.java
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double InverseRegularizedIncompleteBeta(double alpha, double beta, double p)
        {
            double pp, t, u, err, x, al, h, w, afac;
            double a1 = alpha - 1.0;
            double b1 = beta - 1.0;
            double EPS = 1.0e-8;

            if (p <= 0.0)
            {
                return 0.0;
            }

            if (p >= 1.0)
            {
                return 1.0;
            }

            if (alpha >= 1.0 && beta >= 1.0)
            {
                pp = (p < 0.5) ? p : 1.0 - p;
                t = System.Math.Sqrt(-2.0 * System.Math.Log(pp));
                x = (2.30753 + t * 0.27061) / (1.0 + t * (0.99229 + t * 0.04481)) - t;
                if (p < 0.5)
                {
                    x = -x;
                }
                al = (x * x - 3.0) / 6.0;
                h = 2.0 / (1.0 / (2.0 * alpha - 1.0) + 1.0 / (2.0 * beta - 1.0));
                w = (x * System.Math.Sqrt(al + h) / h) - (1.0 / (2.0 * beta - 1) - 1.0 / (2.0 * alpha - 1.0)) * (al + 5.0 / 6.0 - 2.0 / (3.0 * h));
                x = alpha / (alpha + beta * System.Math.Exp(2.0 * w));
            }
            else
            {
                double lna = System.Math.Log(alpha / (alpha + beta));
                double lnb = System.Math.Log(beta / (alpha + beta));
                t = System.Math.Exp(alpha * lna) / alpha;
                u = System.Math.Exp(beta * lnb) / beta;
                w = t + u;
                if (p < t / w)
                {
                    x = System.Math.Pow(alpha * w * p, 1.0 / alpha);
                }
                else
                {
                    x = 1.0 - System.Math.Pow(beta * w * (1.0 - p), 1.0 / beta);
                }
            }
            afac = -LogGamma(alpha) - LogGamma(beta) + LogGamma(alpha + beta);
            for (int j = 0; j < 10; j++)
            {
                if (x == 0.0 || x == 1.0)
                {
                    return x;
                }
                err = RegularizedIncompleteBeta(alpha, beta, x) - p;
                t = System.Math.Exp(a1 * System.Math.Log(x) + b1 * System.Math.Log(1.0 - x) + afac);
                u = err / t;
                x -= (t = u / (1.0 - 0.5 * System.Math.Min(1.0, u * (a1 / x - b1 / (1.0 - x)))));
                if (x <= 0.0)
                {
                    x = 0.5 * (x + t);
                }
                if (x >= 1.0)
                {
                    x = 0.5 * (x + t + 1.0);
                }
                if (System.Math.Abs(t) < EPS * x && j > 0)
                {
                    break;
                }
            }
            return x;
        }

        /// <summary>
        /// Implementation from https://github.com/haifengl/smile/blob/master/math/src/main/java/smile/math/special/Beta.java
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double RegularizedIncompleteBeta(double alpha, double beta, double x)
        {
            if (x < 0.0 || x > 1.0) return Numeric.UNDEF_DOUBLE;

            double ibeta = 0.0;
            if (x == 0.0)
            {
                ibeta = 0.0;
            }
            else
            {
                if (x == 1.0)
                {
                    ibeta = 1.0;
                }
                else
                {
                    // Term before continued fraction
                    ibeta = System.Math.Exp(LogGamma(alpha + beta) - LogGamma(alpha) - LogGamma(beta) + alpha * System.Math.Log(x) + beta * System.Math.Log(1.0D - x));
                    // Continued fraction
                    if (x < (alpha + 1.0) / (alpha + beta + 2.0))
                    {
                        ibeta = ibeta * IncompleteFractionSummation(alpha, beta, x) / alpha;
                    }
                    else
                    {
                        // Use symmetry relationship
                        ibeta = 1.0 - ibeta * IncompleteFractionSummation(beta, alpha, 1.0 - x) / beta;
                    }
                }
            }
            return ibeta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private static double IncompleteFractionSummation(double alpha, double beta, double x)
        {
            int MAXITER = 500;
            double EPS = 3.0E-7;

            double aplusb = alpha + beta;
            double aplus1 = alpha + 1.0;
            double aminus1 = alpha - 1.0;
            double c = 1.0;
            double d = 1.0 - aplusb * x / aplus1;
            if (System.Math.Abs(d) < FPMIN)
            {
                d = FPMIN;
            }
            d = 1.0 / d;
            double h = d;
            double aa = 0.0;
            double del = 0.0;
            int i = 1, i2 = 0;
            bool test = true;
            while (test)
            {
                i2 = 2 * i;
                aa = i * (beta - i) * x / ((aminus1 + i2) * (alpha + i2));
                d = 1.0 + aa * d;
                if (System.Math.Abs(d) < FPMIN)
                {
                    d = FPMIN;
                }
                c = 1.0 + aa / c;
                if (System.Math.Abs(c) < FPMIN)
                {
                    c = FPMIN;
                }
                d = 1.0 / d;
                h *= d * c;
                aa = -(alpha + i) * (aplusb + i) * x / ((alpha + i2) * (aplus1 + i2));
                d = 1.0 + aa * d;
                if (System.Math.Abs(d) < FPMIN)
                {
                    d = FPMIN;
                }
                c = 1.0 + aa / c;
                if (System.Math.Abs(c) < FPMIN)
                {
                    c = FPMIN;
                }
                d = 1.0 / d;
                del = d * c;
                h *= del;
                i++;
                if (System.Math.Abs(del - 1.0) < EPS)
                {
                    test = false;
                }
                if (i > MAXITER)
                {
                    test = false;
                    //logger.error("Beta.incompleteFractionSummation: Maximum number of iterations wes exceeded");
                }
            }
            return h;
        }

    }
}
