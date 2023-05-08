using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace OSDC.DotnetLibraries.General.Common
{
    public class RandomGenerator
    {
        
        private RNGCryptoServiceProvider random_ = new RNGCryptoServiceProvider();
        private static RandomGenerator instance_ = null;

        /// <summary>
        /// 
        /// </summary>
        public static RandomGenerator Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RandomGenerator();
                }
                return instance_;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double NextDouble()
        {
            byte[] longByteResult = new byte[sizeof(ulong)];
            random_.GetBytes(longByteResult);
            ulong randomLong = BitConverter.ToUInt64(longByteResult, 0);
            return Convert.ToDouble(randomLong) / ulong.MaxValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double NextDouble(double min, double max)
        {
            if (Numeric.LT(min, max))
            {
                double result = min + (max - min) * NextDouble();
                return result;
            }
            else
            {
                return Numeric.UNDEF_DOUBLE;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double NextGaussian(double mean, double standardDeviation)
        {
            if (standardDeviation != 0)
            {
                double x_01;
                double x_ab;
                double u1;
                double u2;

                do
                {
                    u1 = NextDouble();
                    u2 = NextDouble();
                } while (Numeric.EQ(u1, 0));

                x_01 = System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Cos(2 * System.Math.PI * u2);
                x_ab = standardDeviation * x_01 + mean;
                return x_ab;
            }
            else
            { return Numeric.UNDEF_DOUBLE; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="mostProbable"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double NextTriangle(double min, double mostProbable, double max)
        {
            double k;
            double u1;
            double x;
            u1 = NextDouble();
            if ((System.Math.Abs(max - mostProbable) < 0.001) || (System.Math.Abs(mostProbable - min) < 0.001))
            {
                if (System.Math.Abs(max - mostProbable) < 0.001)
                {
                    x = System.Math.Sqrt(u1) * (max - min);
                }
                else
                {
                    x = (1 - System.Math.Sqrt(u1)) * (max - min);
                }
                return (x + min);
            }
            else
            {
                k = (mostProbable - min) / (max - mostProbable);
                if (NextDouble() < 1.0 / (k + 1))
                {
                    x = (1 - System.Math.Sqrt(u1)) * (max - mostProbable);
                    return (x + mostProbable);
                }
                else
                {
                    x = System.Math.Sqrt(u1) * (mostProbable - min);
                    return (x + min);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int Next()
        {
            byte[] byteResult = new byte[sizeof(uint)];
            random_.GetBytes(byteResult);
            return BitConverter.ToInt32(byteResult, 0);
            //System.Security.Cryptography.RNGCryptoServiceProvider random_ = new System.Security.Cryptography.RNGCryptoServiceProvider();
            //return random_.Next();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public uint NextUnsigned()
        {
            byte[] byteResult = new byte[sizeof(uint)];
            random_.GetBytes(byteResult);
            return BitConverter.ToUInt32(byteResult, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public int Next(int maxValue)
        {
            return Next() % maxValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public int Next(int minValue, int maxValue)
        {
            return minValue + (int)NextUnsigned((uint)(maxValue - minValue));// random_.Next(minValue, maxValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public uint NextUnsigned(uint maxValue)
        {
            return NextUnsigned() % maxValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public uint NextUnsigned(uint minValue, uint maxValue)
        {
            return minValue + NextUnsigned(maxValue - minValue);// random_.Next(minValue, maxValue);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        public void NextBytes(Byte[] buffer)
        {
            random_.GetBytes(buffer);// NextBytes(buffer);
        }

    }
}
