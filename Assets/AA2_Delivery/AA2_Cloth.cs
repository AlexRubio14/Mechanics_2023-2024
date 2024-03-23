using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

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
        public float structuralElasticCoef;
        public float structuraDamptCoef;

        public float structuralSpringLength;
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

        public Vector3C restLenght;
        public Vertex(Vector3C _position)
        {
            this.actualPosition = _position;
            this.lastPosition = _position;
            this.velocity = new Vector3C(0, 0, 0);
            this.restLenght = Vector3C.zero;
        }

        public void Euler(Vector3C force, float dt)
        {
            lastPosition = actualPosition;
            velocity += force * dt;
            actualPosition += velocity * dt;
        }
    }
    public Vertex[] points;

    public bool start;
    public void Update(float dt)
    {
        int totalVertexs = settings.xPartSize + 1;

        for (int i = settings.yPartSize + 1; i < points.Length; i++)
        {
            float magnitudeY = (points[i - totalVertexs].actualPosition - points[i].actualPosition).magnitude - clothSettings.structuralSpringLength;

            Vector3C forceVector = (points[i - totalVertexs].actualPosition - points[i].actualPosition).normalized * magnitudeY;

            Vector3C damptingForce = (points[i].velocity - points[i - totalVertexs].velocity) * clothSettings.structuraDamptCoef;

            Vector3C structuralSpringForce = (forceVector * clothSettings.structuralElasticCoef) - damptingForce;
            points[i].Euler(settings.gravity + structuralSpringForce, dt);
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