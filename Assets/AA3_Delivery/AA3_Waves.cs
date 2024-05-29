using JetBrains.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Android.Types;
using UnityEngine;
using static AA3_Waves;

[System.Serializable]
public class AA3_Waves
{
    [System.Serializable]
    public struct Settings
    {

    }
    public Settings settings;

    [System.Serializable]
    public struct BuoySettings
    {
        public float buoyancyCoefficient;
        public float buoyVelocity;
        public float mass;
        public float waterDensity;
        public float gravity;
    }

    public BuoySettings buoySettings;

    [System.Serializable]
    public struct WavesSettings
    {
        public float amplitude;
        public float frequency;
        public float phase;

        public Vector3C direction;
        public float speed;
    }
    public WavesSettings[] wavesSettings;
    public struct Vertex
    {
        public Vector3C originalposition;
        public Vector3C position;
        public Vertex(Vector3C _position)
        {
            this.position = _position;
            this.originalposition = _position;
        }
    }
    public Vertex[] points;
    private float elapsedTime = 0;

    public SphereC buoy;


    public void Update(float dt)
    {
        elapsedTime += dt;

        for (int i = 0; i < points.Length; i++)
        {
            points[i].position = points[i].originalposition;

            for (int j = 0; j < wavesSettings.Length; j++)
            {
                float k = 2 * (float)Math.PI / wavesSettings[j].frequency;

                points[i].position.x += points[i].originalposition.x + wavesSettings[j].amplitude * k
                    * (float)Math.Cos(k * (Vector3C.Dot(points[i].originalposition, wavesSettings[j].direction) + (elapsedTime * wavesSettings[j].speed))
                    + wavesSettings[j].phase) * wavesSettings[j].direction.x;

                points[i].position.z += points[i].originalposition.z + wavesSettings[j].amplitude * k
                    * (float)Math.Cos(k * (Vector3C.Dot(points[i].originalposition, wavesSettings[j].direction) + (elapsedTime * wavesSettings[j].speed))
                    + wavesSettings[j].phase) * wavesSettings[j].direction.z;

                points[i].position.y += wavesSettings[j].amplitude
                    * (float)Math.Sin(k * (Vector3C.Dot(points[i].originalposition, wavesSettings[j].direction) + (elapsedTime * wavesSettings[j].speed))
                    + wavesSettings[j].phase);

                wavesSettings[j].phase += dt * wavesSettings[j].speed;
            }
            
        }

        BuoyEuler(CalculateBuoyForce(), dt);
    }


public float GetWaveHeight(float x, float z)
{
    float buoyHeight = 0;

    for (int j = 0; j < wavesSettings.Length; j++)
    {
        float k = 2 * (float)Math.PI / wavesSettings[j].frequency;  

        buoyHeight += wavesSettings[j].amplitude
                * (float)Math.Sin(k * (Vector3C.Dot(new Vector3C(x, 0, z), wavesSettings[j].direction) + (elapsedTime * wavesSettings[j].speed))
                + wavesSettings[j].phase);
    }

    return buoyHeight;
}
public float CalculateBuoyForce()
{
    float waveHeight = GetWaveHeight(buoy.position.x, buoy.position.z);

    float immerseHeight = waveHeight - buoy.position.y - buoy.radius;

    float volume = ((float)Math.PI * (float)Math.Pow(immerseHeight, 2) / 3) * (3 * buoy.radius - immerseHeight);
    float force = buoySettings.waterDensity * buoySettings.gravity * volume;
    return (force - (buoySettings.mass * buoySettings.gravity)) * buoySettings.buoyancyCoefficient;
}
public void BuoyEuler(float force, float dt)
{
    float acceleration = force / buoySettings.mass;
    buoySettings.buoyVelocity += acceleration * dt;

    buoy.position.y += buoySettings.buoyVelocity * dt;
}


public void Debug()
    {
        if(points != null)
        foreach (var item in points)
        {
            item.originalposition.Print(0.05f);
            item.position.Print(0.05f);
        }

        buoy.Print(Vector3C.blue);
    }
}
