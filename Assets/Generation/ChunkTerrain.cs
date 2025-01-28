using UnityEngine;

namespace CH.Generation
{
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
            Vector3[] vertices = new Vector3[Constructor.VerticesPerChunk];

            for (int i = 0, y = 0; y <= Constructor.TilesPerAxis; y++)
            {
                for (int x = 0; x <= Constructor.TilesPerAxis; x++)
                {
                    float height = Constructor.GenerateVertexHeight(x, y, chunkPos);
                    vertices[i] = new(x * Constructor.TileSize, height, y * Constructor.TileSize);
                    i++;
                }
            }

            return vertices;
        }

        // Functions

        /// <summary>
        /// Generates the triangle indices for a chunk. See <seealso cref="Triangles"/>.
        /// </summary>
        /// <returns>An array of triangle.</returns>
        private int[] GenerateTriangle()
        {
            int[] triangles = new int[Constructor.TilesPerChunk * 6];

            int vert = 0,
            tris = 0;
            for (int y = 0; y < Constructor.TilesPerAxis; y++)
            {
                for (int x = 0; x < Constructor.TilesPerAxis; x++)
                {
                    triangles[tris] = vert;
                    triangles[tris + 1] = vert + Constructor.TilesPerAxis + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + Constructor.TilesPerAxis + 1;
                    triangles[tris + 5] = vert + Constructor.TilesPerAxis + 2;

                    vert++;
                    tris += 6;
                }
                vert++;
            }

            return triangles;
        }

        private Vector2[] GenerateUVs()
        {
            Vector2[] uvs = new Vector2[Constructor.VerticesPerChunk];

            for (int i = 0, y = 0; y <= Constructor.TilesPerAxis; y++)
            {
                for (int x = 0; x <= Constructor.TilesPerAxis; x++)
                {
                    uvs[i] = new(
                        x / Constructor.UVScale,
                        y / Constructor.UVScale
                    );
                    i++;
                }
            }

            return uvs;
        }

        /// <summary>
        /// Generates the mesh for a chunk.
        /// </summary>
        /// <param name="chunkPos">The chunk's position.</param>
        /// <returns>A mesh for a chunk.</returns>
        public Mesh GenerateMesh(Vector2Int chunkPos)
        {
            Vector3[] vertices = GenerateVertices(chunkPos);
            int[] triangles = GenerateTriangle();
            Vector2[] uvs = GenerateUVs();

            Mesh mesh = new()
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };

            mesh.RecalculateNormals();
            Vertices = vertices;
            Triangles = triangles;
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
