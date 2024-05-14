using System.Drawing;
using UnityEngine;

[System.Serializable]
public class AA2_Cloth
{
    [System.Serializable]
    public struct Settings
    {
        public Vector3C gravity;
        public float dampingCoef;
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
        public float maxSpringL;

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
        // ATTRIBUTES
        public Vector3C lastPosition;
        public Vector3C actualPosition;
        public Vector3C velocity;

        // CONSTRUCTOR
        public Vertex(Vector3C _position)
        {
            actualPosition = _position;
            lastPosition = _position;
            velocity = Vector3C.zero;
        }

        // FORCE METHODS
        public void Euler(Vector3C force, float dt)
        {
            lastPosition = actualPosition;
            velocity += force * dt;
            actualPosition += velocity * dt;
        }
        public Vector3C StructuralSpring(Vertex otherPoint, ClothSettings clothSettings)
        {
            float structuralMagnitude = (otherPoint.actualPosition - this.actualPosition).magnitude - clothSettings.structuralSpringL;

            if (structuralMagnitude >= clothSettings.maxSpringL)
                structuralMagnitude = clothSettings.maxSpringL;

            Vector3C structuralForceVector = (otherPoint.actualPosition
                                - this.actualPosition).normalized * structuralMagnitude * clothSettings.structuralElasticCoef;

            Vector3C dampingForce = (this.velocity - otherPoint.velocity) * clothSettings.structuralDampCoef;
            Vector3C structuralSpringForce = (structuralForceVector * clothSettings.structuralElasticCoef) - dampingForce;

            return structuralSpringForce; 
        }
        public Vector3C ShearSpring(Vertex otherPoint, ClothSettings clothSettings)
        {
            float shearMagnitude = (otherPoint.actualPosition - this.actualPosition).magnitude - clothSettings.shearSpringL;

            if (shearMagnitude >= clothSettings.maxSpringL)
                shearMagnitude = clothSettings.maxSpringL;

            Vector3C shearForceVector = (otherPoint.actualPosition
                                - this.actualPosition).normalized * shearMagnitude * clothSettings.shearElasticCoef;

            Vector3C shearDampingForce = (-otherPoint.velocity + this.velocity) * clothSettings.shearDampCoef;
            Vector3C shearSpringForce = shearForceVector * clothSettings.shearElasticCoef - shearDampingForce;

            return shearSpringForce;
        }
        public Vector3C BendingSpring(Vertex otherPoint, ClothSettings clothSettings)
        {
            float bendMagnitudeY = (otherPoint.actualPosition - this.actualPosition).magnitude - clothSettings.bendingSpringL;

            if (bendMagnitudeY >= clothSettings.maxSpringL * 2)
                bendMagnitudeY = clothSettings.maxSpringL * 2;

            Vector3C bendForceVector = (otherPoint.actualPosition
                                - this.actualPosition).normalized * bendMagnitudeY * clothSettings.bendingElasticCoef;

            Vector3C bendDampingForce = (-otherPoint.velocity + this.velocity) * clothSettings.bendingDampCoef;
            Vector3C bendSpringForce = bendForceVector * clothSettings.bendingElasticCoef - bendDampingForce;

            return bendSpringForce;
        }

        // COLLISION METHODS
        public bool CheckSphere(SphereC sphere, Settings settings, SettingsCollision settingsCollision)
        {
            if (settingsCollision.sphere.IsInside(actualPosition))
            {
                Vector3C sphereToPos = Vector3C.CreateVector3(sphere.position, actualPosition);
                actualPosition = sphere.position + sphereToPos.normalized * sphere.radius;
                Vector3C normalizedDistance = (actualPosition - sphere.position).normalized;
                PlaneC plane = new PlaneC(sphere.position + normalizedDistance * sphere.radius, normalizedDistance);
                Vector3C Vn = plane.normal.normalized * Vector3C.Dot(velocity, plane.normal);

                velocity = (velocity - Vn) * settings.dampingCoef;

                return true;
            }
            return false;
        }
        public bool CollisionPlane(PlaneC plane, Settings settings, SphereC sphere)
        {
            // 1. Distance
            double distance = plane.DistanceToPoint(actualPosition);

            if (distance < 0.0f)
            {
                // 2. Recolocamos la particula 
                Vector3C sphereToPos = Vector3C.CreateVector3(sphere.position, actualPosition);
                actualPosition = sphere.position + sphereToPos.normalized * sphere.radius;

                // 3. Colision
                return true;
            }
            return false;
        }
    }

    public Vertex[] points;
    public Vector3C[] CalculateClothForces(Vertex[] points, int xVertices)
    {
        Vector3C[] clothForces = new Vector3C[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            // STRUCTURAL VERTICAL
            if (i > xVertices - 1)
            {
                Vector3C structuralSpringForce = points[i].StructuralSpring(points[i - xVertices], clothSettings);

                clothForces[i] += structuralSpringForce;
                clothForces[i - xVertices] += -structuralSpringForce;
            }
            //STRUCTURAL HORIZONTAL
            if (i % xVertices != 0)
            {
                Vector3C structuralSpringForce = points[i].StructuralSpring(points[i - 1], clothSettings);

                clothForces[i] += structuralSpringForce;
                clothForces[i - 1] += -structuralSpringForce;
            }

            //SHEAR
            if (i > xVertices - 1 && i % xVertices - 1 != 0)
            {
                Vector3C shearSpringForce = points[i].ShearSpring(points[i - xVertices + 1], clothSettings);

                clothForces[i] += shearSpringForce;
                clothForces[i - xVertices + 1] += -shearSpringForce;
            }

            //BENDING VERTICAL
            if (i > xVertices * 2 - 1)
            {
                Vector3C bendSpringForce = points[i].BendingSpring(points[i - xVertices * 2], clothSettings);

                clothForces[i] += bendSpringForce;
                clothForces[i - xVertices * 2] += -bendSpringForce;
            }
            //BENDING HORIZONTAL
            if (i % xVertices != 0 && i % xVertices != 1)
            {
                Vector3C bendSpringForce = points[i].BendingSpring(points[i - 2], clothSettings);

                clothForces[i] += bendSpringForce;
                clothForces[i - 2] += -bendSpringForce;
            }
        }

        return clothForces;
    }
    public void VertexBehaviour(Vertex[] points, Vector3C[] clothForces, float dt)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (i != 0 && i != settings.xPartSize)
            {
                // CHECK COLLISION
                points[i].CheckSphere(settingsCollision.sphere, settings, settingsCollision);
                // APPLY FORCES
                points[i].Euler(settings.gravity + clothForces[i], dt);
            }
        }
    }

    public void Update(float dt)
    {
        Vector3C[] clothForces = CalculateClothForces(points, settings.xPartSize + 1);

        VertexBehaviour(points, clothForces, dt);
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