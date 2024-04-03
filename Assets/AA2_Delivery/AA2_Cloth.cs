
using System.Drawing;
using UnityEngine;

[System.Serializable]
public class AA2_Cloth
{
    [System.Serializable]
    public struct Settings
    {
        public Vector3C gravity;
        [Min(1)]
        public float width;
        [Min(1)]
        public float height;
        [Min(2)]
        public int xPartSize;
        [Min(2)]
        public int yPartSize;
    }
    public Settings settings;
    [System.Serializable]
    public struct ClothSettings
    {
        [Header("Structual Spring")]
        public float structuralElasticCoef;
        public float structuralDampCoef;
        public float structuralSpringL;

        [Header("Shear Spring")]
        public float shearElasticCoef;
        public float shearDampCoef;
        public float shearSpringL;

        [Header("Bending Spring")]
        public float bendingElasticCoef;
        public float bendingDampCoef;
        public float bendingSpringL;
    }
    public ClothSettings clothSettings;

    [System.Serializable]
    public struct SettingsCollision
    {
        public SphereC sphere;
    }
    public SettingsCollision settingsCollision;
    public struct Vertex
    {
        public Vector3C lastPosition;
        public Vector3C actualPosition;
        public Vector3C velocity;
        public Vertex(Vector3C _position)
        {
            this.actualPosition = _position;
            this.lastPosition = _position;
            this.velocity = new Vector3C(0, 0, 0);
        }
        public void Euler(Vector3C force, float dt)
        {
            lastPosition = actualPosition;
            velocity += force * dt;
            actualPosition += velocity * dt;
        }

        public bool CollisionPlane(PlaneC plane, Settings settings)
        {
            // 1. Distance
            double distance = plane.DistanceToPoint(actualPosition);

            if (distance < 0.0f)
            {
                // 2. Recolocamos la particula 
                actualPosition = plane.IntersectionWithLine(new LineC(lastPosition, actualPosition));

                // 3. Colision
                return true;
            }
            return false;
        }


        public bool CheckSphere(SphereC sphere, Settings settings)
        {
            // Find the plane
            Vector3C normalizedDistance = (actualPosition - sphere.position).normalized;
            PlaneC plane = new PlaneC(sphere.position + normalizedDistance * sphere.radius, normalizedDistance);

            if (CollisionPlane(plane, settings))
                return true;

            return false;
        }
    }
    public Vertex[] points;
    public void Update(float dt)
    {
        int xVertices = settings.xPartSize + 1;
        int yVertices = settings.yPartSize + 1;

        Vector3C[] structuralForces = new Vector3C[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            // STRUCTURAL VERTICAL
            if( i > xVertices-1)
            {
                float structuralMagnitudeY = (points[i - xVertices].actualPosition -
                                points[i].actualPosition).magnitude - clothSettings.structuralSpringL;

                Vector3C structuralForceVector = (points[i - xVertices].actualPosition
                                    - points[i].actualPosition).normalized * structuralMagnitudeY * clothSettings.structuralElasticCoef;

                Vector3C dampingForce = (points[i].velocity - points[i - xVertices].velocity) * clothSettings.structuralDampCoef;
                Vector3C structuralSpringForce = (structuralForceVector * clothSettings.structuralElasticCoef) - dampingForce;

                structuralForces[i] += structuralSpringForce;
                structuralForces[i - xVertices] += -structuralSpringForce;

            }

            //STRUCTURAL HORIZONTAL
            if (i % xVertices != 0)
            {
                float structuralMagnitudeX = (points[i - 1].actualPosition - points[i].actualPosition).magnitude
                                                 - clothSettings.structuralSpringL;
                Vector3C structuralForceVector = (points[i - 1].actualPosition
                                    - points[i].actualPosition).normalized * structuralMagnitudeX * clothSettings.structuralElasticCoef;

                Vector3C dampingForce = (points[i].velocity - points[i - 1].velocity) * clothSettings.structuralDampCoef;
                Vector3C structuralSpringForce = (structuralForceVector * clothSettings.structuralElasticCoef) - dampingForce;

                structuralForces[i] += structuralSpringForce;
                structuralForces[i - 1] += -structuralSpringForce;
            }

            //SHEAR
            if (i > xVertices - 1 && i % xVertices - 1 != 0)
            {
                float shearMagnitude = (points[i - xVertices + 1].actualPosition - points[i].actualPosition).magnitude
                                                 - clothSettings.shearSpringL;
                Vector3C shearForceVector = (points[i - xVertices + 1].actualPosition
                                    - points[i].actualPosition).normalized * shearMagnitude * clothSettings.shearElasticCoef;


                Vector3C shearDampingForce = (points[i - xVertices + 1].actualPosition - points[i].actualPosition) * clothSettings.shearDampCoef;
                Vector3C shearSpringForce = shearForceVector * clothSettings.shearElasticCoef - shearDampingForce;

                structuralForces[i] += shearSpringForce;
                structuralForces[i - xVertices + 1] += -shearSpringForce;
            }

            //BENDING VERTICAL
            if (i > xVertices * 2 - 1)
            {
                float bendMagnitudeY = (points[i - xVertices * 2].actualPosition - points[i].actualPosition).magnitude
                                                 - clothSettings.bendingSpringL;
                Vector3C bendForceVector = (points[i - xVertices * 2].actualPosition
                                    - points[i].actualPosition).normalized * bendMagnitudeY * clothSettings.bendingElasticCoef;

                Vector3C bendDampingForce = (points[i - xVertices * 2].actualPosition - points[i].actualPosition) * clothSettings.bendingDampCoef;
                Vector3C bendSpringForce = bendForceVector * clothSettings.bendingElasticCoef - bendDampingForce;


                structuralForces[i] += bendSpringForce;
                structuralForces[i - xVertices * 2] += -bendSpringForce;
            }

            //BENDING HORIZONTAL
            if (i % xVertices != 0 && i % xVertices != 1)
            {
                float bendMagnitudeX = (points[i - 2].actualPosition - points[i].actualPosition).magnitude
                                                 - clothSettings.bendingSpringL;
                Vector3C bendForceVector = (points[i - 2].actualPosition
                                    - points[i].actualPosition).normalized * bendMagnitudeX * clothSettings.bendingElasticCoef;

                Vector3C bendDampingForce = (points[i - 2].actualPosition - points[i].actualPosition) * clothSettings.bendingDampCoef;
                Vector3C bendSpringForce = bendForceVector * clothSettings.bendingElasticCoef - bendDampingForce;


                structuralForces[i] += bendSpringForce;
                structuralForces[i - 2] += -bendSpringForce;
            }
        }

        for(int i = 0; i < points.Length; i++)
        {
            if (i != 0 && i != xVertices - 1)
            {
                points[i].Euler(settings.gravity + structuralForces[i], dt);

                //if (points[i].CheckSphere(settingsCollision.sphere, settings)) { }
            }
        }
    }



    public void Debug()
    {
        settingsCollision.sphere.Print(Vector3C.blue);

        if (points != null)
            foreach (var item in points)
            {
                item.lastPosition.Print(0.05f);
                item.actualPosition.Print(0.05f);
            }
    }
}