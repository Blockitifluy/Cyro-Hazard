using UnityEngine;

namespace CH.Generation
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

        /// <summary>
        /// Generates the vertices for a chunk. See <seealso cref="Vertices"/>.
        /// </summary>
        /// <param name="chunkPos">The chunk's position</param>
        /// <returns>An array of vertices.</returns>
        private Vector3[] GenerateVertices(Vector2Int chunkPos)
        {
            Constructor.OnPreGenerate(chunkPos);

            Vector3[] vertices = new Vector3[Constructor.VerticesPerChunk];

            for (int i = 0, y = 0; y <= Constructor.TilesPerAxis; y++)
            {
                for (int x = 0; x <= Constructor.TilesPerAxis; x++)
                {
                    Vector2Int pos = new(x, y);
                    float height = (Constructor.GenerateVertexHeight(pos, i, chunkPos) - 0.5f) * 2.0f;
                    vertices[i] = new(x * Constructor.TileSize, height, y * Constructor.TileSize);
                    i++;
                }
            }

            return vertices;
        }

        // Functions

        /// <summary>
        /// Generates the mesh for a chunk.
        /// </summary>
        /// <param name="chunkPos">The chunk's position.</param>
        /// <returns>A mesh for a chunk.</returns>
        public Mesh GenerateMesh(Vector2Int chunkPos)
        {
            Vector3[] vertices = GenerateVertices(chunkPos);

            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = Constructor.Triangles,
                uv = Constructor.UVs
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
