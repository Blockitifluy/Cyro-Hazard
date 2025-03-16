using UnityEngine;

namespace CyroHazard.Generation
{
    [PreferBinarySerialization]
    /// <summary>
    /// A chunk's data storing it's vertices and triangles.  
    /// </summary>
    public class ChunkTerrain : MonoBehaviour
    {
        // Propetries

        /// <summary>
        /// The chunk's grid position.
        /// </summary>
        public Vector2Int ChunkPos;
        /// <summary>
        /// An array of vertices, with the length of <c>(X + 1) * (Y + 1)</c>,
        /// with X and Y being the chunk size.
        /// </summary>
        public Vector3[] Vertices;
        /// <summary>
        /// An array with all the triangle indies, with a length of all the vertices
        /// multiplied by 6.
        /// </summary>
        public int[] Triangles;

        // Private Propetries

        /// <inheritdoc cref="ChunkConstructor"/>
        private ChunkConstructor Constructor => ChunkConstructor.GetConstructor();
        /// <summary>
        /// The chunk's <c>MeshFilter</c>.
        /// </summary>
        private MeshFilter _MeshFilter;
        /// <summary>
        /// The chunk's <c>MeshCollider</c>.
        /// </summary>
        private MeshCollider _MeshCollider;
        private MeshRenderer _MeshRenderer;

        // Functions

        /// <summary>
        /// Generates the mesh for a chunk.
        /// </summary>
        /// <param name="chunkPos">The chunk's position.</param>
        /// <returns>A mesh for a chunk.</returns>
        public Mesh GenerateMesh(Vector2Int chunkPos)
        {
            Vector3[] vertices = Constructor.GenerateVertices(chunkPos);

            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = Constructor.Triangles,
                uv = Constructor.UVs,
                subMeshCount = Constructor.Materials.Length
            };

            mesh.RecalculateNormals();
            Vertices = vertices;
            Triangles = Constructor.Triangles;
            return mesh;
        }

        /// <summary>
        /// Loads in a mesh in a chunk. Setting the mesh filter and collider.
        /// </summary>
        /// <param name="chunkPos">The chunk's position</param>
        public void LoadMesh(Vector2Int chunkPos)
        {
            Mesh mesh = GenerateMesh(chunkPos);
            _MeshFilter.mesh = mesh;
            _MeshCollider.sharedMesh = mesh;
            ChunkPos = chunkPos;

            _MeshRenderer.materials = Constructor.Materials;
        }

        // Unity

        // Start is called before the first frame update
        void Awake()
        {
            _MeshFilter = GetComponent<MeshFilter>();
            _MeshCollider = GetComponent<MeshCollider>();
            _MeshRenderer = GetComponent<MeshRenderer>();
        }
    }
}
