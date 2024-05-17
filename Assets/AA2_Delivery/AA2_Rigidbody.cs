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
        // ATTRIBUTES
        public Vector3C size;
        private Vector3C[] offsetVertex;
        private Vector3C[] vertexs;

        public Vector3C position;
        public Vector3C lastPosition;
        public Vector3C linearVelocity;

        public Vector3C euler;
        public Vector3C angularVelocity;
        
        public float inertialTension;
        public float density;
        
        // CONSTRUCTOR
        public CubeRigidbody(Vector3C _position, Vector3C _size, Vector3C _euler)
        {
            size = _size;
            euler = _euler;

            vertexs = new Vector3C[8];
            offsetVertex = new Vector3C[8]
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
            angularVelocity = Vector3C.zero;
            
            inertialTension = 0;
            density = 0;
        }

        public Vector3C[] GetVertexs()
        {
            return vertexs; 
        }

        // FORCES METHODS
        public void Euler(Vector3C force, float dt)
        {
            lastPosition = position;

            linearVelocity += force * dt;

            euler += angularVelocity * dt;
            position += linearVelocity * dt;
        }
        public void VertexSet(float _dt)
        {
            for (int i = 0; i < vertexs.Length; i++)
                vertexs[i] = MatrixC.Rotation(euler, offsetVertex[i]) + position;
        }

        // COLLISIONS METHODS
        public bool CheckPlanes(PlaneC[] _planes, Settings _settings)
        {
            foreach (PlaneC plane in _planes)
            {
                // Check each vertex of the cube with all the planes
                for(int i = 0; i < vertexs.Length; i++)
                {
                    //Check collision with the coord.world
                    if (CollisionPlane(vertexs[i], plane, _settings))
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
        crb.VertexSet(dt);

        // 1. Collisions
        crb.CheckPlanes(settingsCollision.planes, settings);

        // 2. Forces
        crb.Euler(settings.gravity, dt);
    }

    public void GetOthersRigidbodysArray(AA2_Rigidbody[] allRigidbodies)
    {
        AA2_Rigidbody[] othersRigidbodys = new AA2_Rigidbody[allRigidbodies.Length - 1];
        int index = 0;
        for (int i = 0; i < allRigidbodies.Length; i++)
        {
            if (allRigidbodies[i] != this)
            {
                othersRigidbodys[index++] = allRigidbodies[i];
            }
        }
        // Aquest array conté els altres rigidbodys amb els quals podreu interactuar.
    }

    public void Debug()
    {
        //foreach (var item in settingsCollision.planes)
            //item.Print(Vector3C.red);
        
        foreach(var vertex in crb.GetVertexs())
        {
            SphereC sphere = new SphereC(vertex, 0.01f);
            sphere.Print(Vector3C.green);
        }
    }
}
