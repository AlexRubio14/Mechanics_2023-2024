using System;
using System.Data;
using System.Security.Cryptography;
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
    public static MatrixC operator *(MatrixC m1, float product)
    {
        for(int i = 0; i < m1.size; i++)
        {
            for(int j = 0; j < m1.size; j++)
            {
                m1.data[i,j] *= product;
            }
        }
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
    public MatrixC Transposed()
    {
        MatrixC transposed = new MatrixC();

        for(int i = 0; i < this.size; i++)
        {
            for(int j = 0; j < this.size; j++)
            {
                transposed.data[i,j] = data[j,i];
            }
        }
        return transposed;
    }

    private MatrixC Cofactor3x3()
{
        MatrixC inverse = new MatrixC();

        inverse.data[0, 0] = (data[1, 1] * data[2, 2] - data[1, 2] * data[2, 1]);
        inverse.data[0, 1] = (data[0, 2] * data[2, 1] - data[0, 1] * data[2, 2]) * -1;
        inverse.data[0, 2] = (data[0, 1] * data[1, 2] - data[0, 2] * data[1, 1]);
        inverse.data[1, 0] = (data[1, 2] * data[2, 0] - data[1, 0] * data[2, 2]) * -1;
        inverse.data[1, 1] = (data[0, 0] * data[2, 2] - data[0, 2] * data[2, 0]);
        inverse.data[2, 2] = (data[0, 0] * data[1, 1] - data[0, 1] * data[1, 0]);
        inverse.data[1, 2] = (data[0, 2] * data[1, 0] - data[0, 0] * data[1, 2]) * -1;
        inverse.data[2, 0] = (data[1, 0] * data[2, 1] - data[1, 1] * data[2, 0]);
        inverse.data[2, 1] = (data[0, 1] * data[2, 0] - data[0, 0] * data[2, 1]) * -1;

        return inverse;
    }
    #endregion

    #region FUNCTIONS
    public static MatrixC Inverse(MatrixC matrix)
    {
        float determinant = matrix.Determinant3x3();

        // 1. if det() = 0 --> no inverse
        if (determinant == 0)
            return MatrixC.identity3x3;

        float invDet = 1 / determinant;

        // 2. Make the transposed
        MatrixC transposed = matrix.Transposed();

        // 3. Coef
        MatrixC result = transposed.Cofactor3x3() * invDet;

        return result;
    }
    public static Vector3C RotateX(float angle, Vector3C point)
    {
        Vector3C newPoint = new Vector3C();

        newPoint.x = point.x;
        newPoint.y = point.y * MathF.Cos(angle) + point.z * (-MathF.Sin(angle));
        newPoint.z = point.y * MathF.Sin(angle) + point.z * (MathF.Cos(angle));

        return newPoint;
    }
    public static Vector3C RotateY(float angle, Vector3C point)
    {
        Vector3C newPoint = new Vector3C();

        newPoint.x = point.x * MathF.Cos(angle) + point.z * (MathF.Sin(angle));
        newPoint.y = point.y;
        newPoint.z = point.x * (-MathF.Sin(angle)) + point.z * MathF.Cos(angle);

        return newPoint;
    }
    public static Vector3C RotateZ(float angle, Vector3C point)
    {
        Vector3C newPoint = new Vector3C(); 

        newPoint.x = point.x * MathF.Cos(angle) + point.y * (-MathF.Sin(angle));
        newPoint.y = point.x * MathF.Sin(angle) + point.y * (MathF.Cos(angle));
        newPoint.z = point.z;

        return newPoint;
    }

        #endregion
    }