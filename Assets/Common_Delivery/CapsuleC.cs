using System;

[System.Serializable]
public struct CapsuleC
{
    #region FIELDS
    public Vector3C positionA;
    public Vector3C positionB;
    public float radius;
    #endregion

    #region PROPIERTIES
    #endregion

    #region CONSTRUCTORS
    public CapsuleC(Vector3C postionA, Vector3C positionB, float radius)
    {
        this.positionA = postionA; this.positionB = positionB; this.radius = radius;
    }
    #endregion

    #region OPERATORS
    #endregion

    #region METHODS

   
    #endregion

    #region FUNCTIONS
    #endregion

}