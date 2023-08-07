using System;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface ISquaredMatrix : IMatrix, IInvertableMultipliable<ISquaredMatrix>
    {
        /// <summary>
        /// 
        /// </summary>
        int Dim { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        void GetDiagonal(ref IVector v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        void SetDiagonal(IVector v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startCol"></param>
        /// <param name="startRow"></param>
        /// <param name="m"></param>
        void GetSubMatrix(int startCol, int startRow, ref ISquaredMatrix m);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startCol"></param>
        /// <param name="startRow"></param>
        /// <param name="m"></param>
        void SetSubMatrix(int startCol, int startRow, ISquaredMatrix m);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double Determinant();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double Trace();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int Rank();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double FrobeniusNorm();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double InfinityNorm();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double OneNorm();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double TwoNorm();
    }
}
