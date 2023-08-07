using System;

namespace OSDC.DotnetLibraries.General.Math
{
    public interface IMatrix : IAddable<IMatrix>, IProductable<IVector>, IProductable<IMatrix>, IEquivalent<IMatrix>

    {
        /// <summary>
        /// 
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// 
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        double? this[int col, int row] { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="v"></param>
        void GetColumn(int col, ref IVector v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="v"></param>
        void GetRow(int row, ref IVector v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        /// <param name="v"></param>
        void SetColumn(int col, IVector v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="v"></param>
        void SetRow(int row, IVector v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startCol"></param>
        /// <param name="startRow"></param>
        /// <param name="m"></param>
        void GetSubMatrix(int startCol, int startRow, ref IMatrix m);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startCol"></param>
        /// <param name="startRow"></param>
        /// <param name="m"></param>
        void SetSubMatrix(int startCol, int startRow, IMatrix m);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        void Transpose(ref IMatrix result);



        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        void Time(double a);

    }
}
