using System.Diagnostics;
using static AA2_Cloth;

[System.Serializable]
public class AA2_Rigidbody
{
    [System.Serializable]
    public struct Settings
    {
        public Vector3C gravity;
        public float bounce;
    }
    public Settings settings;

    [System.Serializable]
    public struct SettingsCollision
    {
        public PlaneC[] planes;
    }
    public SettingsCollision settingsCollision;

    [System.Serializable]
    public struct CubeRigidbody
    {
        public Vector3C position;
        public Vector3C lastPosition;
        public Vector3C size;
        public Vector3C euler;
        public Vector3C linearVelocity;
        public Vector3C angularVelocity;
        public float inertialTension;
        public float density;
        public Vector3C[] vertexs;

        public CubeRigidbody(Vector3C _position, Vector3C _size, Vector3C _euler)
        {
            lastPosition = _position;
            position = _position;
            size = _size;
            euler = _euler;
            linearVelocity = Vector3C.zero;
            angularVelocity = Vector3C.zero;
            inertialTension = 0;
            density = 0;
            vertexs = new Vector3C[] {
                new Vector3C(-(size.x/2), (size.y/2), (-size.z/2)),
                new Vector3C(-(size.x/2), (-size.y/2), (-size.z/2)),
                new Vector3C(-(size.x/2), (size.y/2), (size.z/2)),
                new Vector3C(-(size.x/2), (-size.y/2), (size.z/2)),

                new Vector3C((size.x/2), (size.y/2), (-size.z/2)),
                new Vector3C((size.x/2), (-size.y/2), (-size.z/2)),
                new Vector3C((size.x/2), (size.y/2), (size.z/2)),
                new Vector3C((size.x/2), (-size.y/2), (size.z/2))
            };
        }

        public void Euler(Vector3C force, float dt)
        {
            lastPosition = position;
            linearVelocity += force * dt;
            euler += angularVelocity * dt;
            position += linearVelocity * dt;
        }
        public bool CheckPlanes(PlaneC[] _planes, Settings _settings)
        {
            foreach (PlaneC plane in _planes)
            {
                foreach(Vector3C vertex in vertexs)
                {
                    if (CollisionPlane(vertex + position, plane, _settings))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool CollisionPlane(Vector3C _vertex, PlaneC _plane, Settings _settings)
        {
            // 1. Distance
            double distance = _plane.DistanceToPoint(_vertex);

            if (distance < 0.0f)
            {
                // 2. Recolocamos la particula 
                UnityEngine.Debug.Break();




                position = _plane.IntersectionWithLine(new LineC(lastPosition, position));
                CollisionPlaneReaction(_plane, _settings);
                return true;
            }
            return false;
        }
        public void CollisionPlaneReaction(PlaneC _plane, Settings _settings)
        {
            Vector3C Vn = _plane.normal.normalized * Vector3C.Dot(linearVelocity, _plane.normal);
            linearVelocity = ((linearVelocity - Vn) - Vn) * _settings.bounce;
        }
    }
    public CubeRigidbody crb = new CubeRigidbody(Vector3C.zero, new(.1f,.1f,.1f), Vector3C.zero);
    public void Update(float dt)
    {
        crb.CheckPlanes(settingsCollision.planes, settings);

        crb.Euler(settings.gravity, dt);
    }

    public void Debug()
    {
        foreach (var item in settingsCollision.planes)
        {
            item.Print(Vector3C.red);
        }
    }
}
