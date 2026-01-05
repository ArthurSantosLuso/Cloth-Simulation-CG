using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CustomClothSimulator : MonoBehaviour
{
    [Header("Mesh Settings")]
    [Range(2, 50)]
    [SerializeField] private int segmentsX = 20;
    [Range(2, 50)]
    [SerializeField] private int segmentsY = 20;
    [SerializeField] private float width = 4f;
    [SerializeField] private float height = 4f;
    [SerializeField] private Material clothMaterial;

    [Header("Physics Settings")]
    [SerializeField] private Vector3 gravity = new Vector3(0, -9.81f, 0);
    [Range(0f, 1f)] public float damping = 0.05f;       // Simulates air resistance
    [Range(0.1f, 1f)] public float springStiffness = 1f; // How rigid the connections are
    [Range(1, 10)] public int solverIterations = 3;     // More iterations = better stability and collision

    [Header("Collisions")]
    [SerializeField] private List<SphereCollider> sphereColliders;
    [SerializeField] private bool enableSelfCollision = true;
    [Range(0.01f, 0.2f)] public float selfCollisionRadius = 0.1f; // Minimum distance between cloth particles

    private Mesh _mesh;
    private Vector3[] _meshVertices;
    private List<ClothParticle> _particles;
    private List<ClothSpring> _springs;

    // Spatial hashing for optimized self collision
    // Maps a grid coordinate to a list of particles in that cell
    private Dictionary<Vector3Int, List<ClothParticle>> _spatialHash;
    private float _hashCellSize;

    void Start()
    {
        InitializeCloth();

        // Initialize hash map.
        // The cell size should be at least the collision diameter (2 * radius)
        _hashCellSize = selfCollisionRadius * 2f;
        _spatialHash = new Dictionary<Vector3Int, List<ClothParticle>>();
    }

    void Update()
    {
        UpdateMesh();
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Integrate forces (gravity + verlet movement)
        foreach (var p in _particles)
        {
            p.AddForce(gravity);
            p.TimeStep(dt, damping);
        }

        // Handle the constraints
        // This should run multiple times per frame to ensure stiffness and prevent tunneling (still happens, idk why)
        for (int i = 0; i < solverIterations; i++)
        {
            // Resolve springs
            foreach (var spring in _springs)
            {
                spring.Resolve(springStiffness);
            }

            // Resolve sphere collisions
            ResolveSphereCollisions();

            // Resolve self collisions
            if (enableSelfCollision)
            {
                ResolveSelfCollisions();
            }
        }
    }

    private void InitializeCloth()
    {
        _particles = new List<ClothParticle>();
        _springs = new List<ClothSpring>();

        // Generate particles
        float dx = width / (segmentsX - 1);
        float dy = height / (segmentsY - 1);

        for (int y = 0; y < segmentsY; y++)
        {
            for (int x = 0; x < segmentsX; x++)
            {
                Vector3 pos = transform.position + new Vector3(x * dx - width / 2, -y * dy, 0);

                // Pin the top row (y == 0) so the cloth hangs
                bool isPinned = (y == 0);

                _particles.Add(new ClothParticle(pos, isPinned));
            }
        }

        // Generate the springs
        for (int y = 0; y < segmentsY; y++)
        {
            for (int x = 0; x < segmentsX; x++)
            {
                int i = y * segmentsX + x;

                // Structural spring right
                if (x < segmentsX - 1)
                    _springs.Add(new ClothSpring(_particles[i], _particles[i + 1]));

                // Structural spring down
                if (y < segmentsY - 1)
                    _springs.Add(new ClothSpring(_particles[i], _particles[i + segmentsX]));

                // Shear spring diagonal - Adds structural integrity
                if (x < segmentsX - 1 && y < segmentsY - 1)
                    _springs.Add(new ClothSpring(_particles[i], _particles[i + segmentsX + 1]));
                if (x > 0 && y < segmentsY - 1)
                    _springs.Add(new ClothSpring(_particles[i], _particles[i + segmentsX - 1]));
            }
        }

        // Mesh setup
        _mesh = new Mesh();
        _mesh.name = "ClothMesh";
        _mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = _mesh;
        if (clothMaterial) GetComponent<MeshRenderer>().material = clothMaterial;

        // Set static topology
        UpdateMeshTopology();
    }

    private void ResolveSphereCollisions()
    {
        if (sphereColliders == null) return;

        foreach (var sphere in sphereColliders)
        {
            if (sphere == null) continue;

            // This allows to support scaled spheres by getting the global scale
            float radius = sphere.radius * Mathf.Max(sphere.transform.lossyScale.x, sphere.transform.lossyScale.y, sphere.transform.lossyScale.z);
            Vector3 center = sphere.transform.position + sphere.center;

            // Add a small buffer to prevent visual clipping
            float collisionMargin = 0.02f;
            float radiusSq = (radius + collisionMargin) * (radius + collisionMargin);

            foreach (var p in _particles)
            {
                if (p.isPinned) continue;

                Vector3 diff = p.position - center;

                if (diff.sqrMagnitude < radiusSq)
                {
                    // Push the particle out to the sphere surface
                    p.position = center + diff.normalized * (radius + collisionMargin);
                }
            }
        }
    }

    // Self collision logic
    private void ResolveSelfCollisions()
    {
        _spatialHash.Clear();

        // Populate the Hash Map
        foreach (var p in _particles)
        {
            Vector3Int key = GetSpatialKey(p.position);

            if (!_spatialHash.ContainsKey(key))
            {
                _spatialHash[key] = new List<ClothParticle>();
            }
            _spatialHash[key].Add(p);
        }

        // Check collisions only within relevant cells
        foreach (var kvp in _spatialHash)
        {
            List<ClothParticle> cellParticles = kvp.Value;

            // Check collisions between particles in the same cell
            for (int i = 0; i < cellParticles.Count; i++)
            {
                for (int j = i + 1; j < cellParticles.Count; j++)
                {
                    PushApart(cellParticles[i], cellParticles[j]);
                }
            }
        }
    }

    private Vector3Int GetSpatialKey(Vector3 position)
    {
        // Converts 3D position to a grid integer coordinate
        return new Vector3Int(
            Mathf.FloorToInt(position.x / _hashCellSize),
            Mathf.FloorToInt(position.y / _hashCellSize),
            Mathf.FloorToInt(position.z / _hashCellSize)
        );
    }

    private void PushApart(ClothParticle p1, ClothParticle p2)
    {
        Vector3 diff = p1.position - p2.position;
        float distSq = diff.sqrMagnitude;
        float minDist = selfCollisionRadius * 2f;

        // Check if particles are overlapping (and ignore if they are the exact same position to avoid NaN)
        if (distSq < minDist * minDist && distSq > 0.000001f)
        {
            float dist = Mathf.Sqrt(distSq);

            // Amount to push each particle
            float push = 0.5f * (minDist - dist);

            // Normalized direction * push amount
            Vector3 correction = diff / dist * push;

            if (!p1.isPinned) p1.position += correction;
            if (!p2.isPinned) p2.position -= correction;
        }
    }

    // Visual Updates
    private void UpdateMesh()
    {
        if (_meshVertices == null || _meshVertices.Length != _particles.Count)
            _meshVertices = new Vector3[_particles.Count];

        for (int i = 0; i < _particles.Count; i++)
        {
            _meshVertices[i] = _particles[i].position;
        }

        _mesh.vertices = _meshVertices;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    // Sets up triangles and UVs once at start
    private void UpdateMeshTopology()
    {
        Vector2[] uvs = new Vector2[_particles.Count];
        int[] triangles = new int[(segmentsX - 1) * (segmentsY - 1) * 6];

        int t = 0;
        for (int y = 0; y < segmentsY; y++)
        {
            for (int x = 0; x < segmentsX; x++)
            {
                int i = y * segmentsX + x;
                uvs[i] = new Vector2((float)x / segmentsX, (float)y / segmentsY);

                if (x < segmentsX - 1 && y < segmentsY - 1)
                {
                    triangles[t++] = i;
                    triangles[t++] = i + segmentsX + 1;
                    triangles[t++] = i + 1;

                    triangles[t++] = i;
                    triangles[t++] = i + segmentsX;
                    triangles[t++] = i + segmentsX + 1;
                }
            }
        }

        _mesh.vertices = new Vector3[_particles.Count];
        _mesh.uv = uvs;
        _mesh.triangles = triangles;
    }
}