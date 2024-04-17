using System;
using System.Data;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine.Rendering;

[System.Serializable]
public struct MatrixC
{
    #region FIELDS
    public float[,] data;
    public int size;
    #endregion

    #region PROPIERTIES
    public static MatrixC identity3x3 
    {  
        get 
        { 
            return new MatrixC( new float[,] {
                  { 1.0f, 0.0f, 0.0f },
                  { 0.0f, 1.0f, 0.0f },
                  { 0.0f, 0.0f, 1.0f }
                }); 
        } 
    }
    #endregion

    #region CONSTRUCTORS
    public MatrixC(float[,] _data)
    {
        data = _data;
        size =  (int)MathF.Sqrt(data.Length);
    }
    #endregion

    #region OPERATORS
    public static Vector3C operator *(MatrixC matrix, Vector3C point)
    {
        if (matrix.size != 3)
            return Vector3C.zero;

        Vector3C newPoint = new Vector3C();

        newPoint.x = point.x * matrix.data[0,0] + point.y * matrix.data[0,1] + point.z * matrix.data[0,2];
        newPoint.y = point.x * matrix.data[1,0] + point.y * matrix.data[1,1] + point.z * matrix.data[1,2];
        newPoint.z = point.x * matrix.data[2,0] + point.y * matrix.data[2,1] + point.z * matrix.data[2,2];

        return newPoint;
    }
    public static MatrixC operator *(MatrixC m1, MatrixC m2)
    {
        
        return MatrixC.identity3x3;
    }
    #endregion

    #region METHODS
    public float Determinant3x3()
    {
        if(this.size != 3) 
            return -1;

        return data[0,0] * data[1,1] * data[2,2] + data[0,1] * data[1,2] * data[2,0] + data[0,2] * data[1,0] * data[2,1] -
               data[0,2] * data[1,1] * data[2,0] - data[0,1] * data[1,0] * data[2,2] - data[0,0] * data[1,2] * data[2,1];
    }
    public MatrixC Transposed3x3()
    {
        if (this.size != 3)
            return MatrixC.identity3x3;

        return MatrixC.identity3x3;
    }
    #endregion

    #region FUNCTIONS
    public static MatrixC Inverse(MatrixC matrix)
    {
        float determinant = matrix.Determinant3x3();

        if (determinant == 0)
            return MatrixC.identity3x3;

        return MatrixC.identity3x3;
    }
    #endregion

}