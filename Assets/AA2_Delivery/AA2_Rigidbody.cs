using JetBrains.Annotations;
using System.Diagnostics;
using System.Drawing;
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
        public Vector3C size;
        private Vector3C[] vertexs;

        public Vector3C position;
        public Vector3C lastPosition;
        public Vector3C linearVelocity;

        public Vector3C euler;
        public Vector3C angularVelocity;
        
        public float inertialTension;
        public float density;
        
        public CubeRigidbody(Vector3C _position, Vector3C _size, Vector3C _euler)
        {
            size = _size;
            vertexs = new Vector3C[8]
            {
                new Vector3C(-(_size.x),  (_size.y), -(_size.z)),
                new Vector3C(-(_size.x),  (_size.y),  (_size.z)),
                new Vector3C(-(_size.x), -(_size.y), -(_size.z)),
                new Vector3C(-(_size.x), -(_size.y),  (_size.z)),
                                       
                new Vector3C((_size.x),  (_size.y), -(_size.z)),
                new Vector3C((_size.x), -(_size.y), -(_size.z)),
                new Vector3C((_size.x),  (_size.y),  (_size.z)),
                new Vector3C((_size.x), -(_size.y),  (_size.z))
            }; 

            lastPosition = _position;
            position = _position;
            linearVelocity = Vector3C.zero;

            euler = _euler;
            angularVelocity = Vector3C.zero;
            
            inertialTension = 0;
            density = 0;
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
                // Check each vertex of the cube with all the planes
                foreach(Vector3C vertex in vertexs)
                {
                    // Check collision with the coord.world
                    if (CollisionPlane(vertex + position, plane, _settings))
                        return true;
                }
            }
            return false;
        }
        public bool CollisionPlane(Vector3C _point, PlaneC _plane, Settings _settings)
        {
            // 1. Distance
            double distance = _plane.DistanceToPoint(_point);

            if (distance < 0.0f)
            {
                // 2. Relocate the particle position
                float dot = Vector3C.Dot(_plane.normal, lastPosition - _plane.position);
                position = _plane.IntersectionWithLine(new LineC(lastPosition, position)) + _plane.normal * dot;
                
                // 3. Reaction
                CollisionPlaneReaction(_plane, _settings);

                // 4. Collision
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
    public CubeRigidbody crb = new CubeRigidbody(Vector3C.zero, new Vector3C(0.1f,0.1f,0.1f), Vector3C.zero);
    public void Update(float dt)
    {
        // 1. Collisions
        crb.CheckPlanes(settingsCollision.planes, settings);

        // 2. Forces
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
