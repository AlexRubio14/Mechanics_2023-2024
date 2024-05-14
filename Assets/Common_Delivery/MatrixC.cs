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
        return new Vector3C(
            /* X */ point.x * matrix.data[0, 0] + point.y * matrix.data[0, 1] + point.z * matrix.data[0, 2],
            /* Y */ point.x * matrix.data[1, 0] + point.y * matrix.data[1, 1] + point.z * matrix.data[1, 2],
            /* Z */ point.x * matrix.data[2, 0] + point.y * matrix.data[2, 1] + point.z * matrix.data[2, 2]
            );
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
        MatrixC matrixRotateX = new MatrixC(new float[,] {
                  { 1.0f, 0.0f            , 0.0f              },
                  { 0.0f, MathF.Cos(angle), -MathF.Sin(angle) },
                  { 0.0f, MathF.Sin(angle),  MathF.Cos(angle) }
                });

        return matrixRotateX * point;
    }
    public static Vector3C RotateY(float angle, Vector3C point)
    {
        MatrixC matrixRotateY = new MatrixC(new float[,] {
                  {  MathF.Cos(angle), 0.0f, MathF.Sin(angle) },
                  { 0.0f             , 1.0f, 0.0f             },
                  { -MathF.Sin(angle), 0.0f, MathF.Cos(angle) }
                });

        return matrixRotateY * point;
    }
    public static Vector3C RotateZ(float angle, Vector3C point)
    {
        MatrixC matrixRotateZ = new MatrixC(new float[,] {
                  { MathF.Cos(angle), -MathF.Sin(angle), 0.0f },
                  { MathF.Sin(angle),  MathF.Cos(angle), 0.0f },
                  { 0.0f            , 0.0f             , 1.0f }
                });

        return matrixRotateZ * point;
    }
    public static Vector3C Rotation(Vector3C euler, Vector3C point)
    {
        return RotateX(euler.x, point) * RotateY(euler.y, point) * RotateZ(euler.z, point);
    }
        #endregion
    }