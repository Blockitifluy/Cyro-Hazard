using System;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace CyroHazard.Generation
{
    /// <summary>
    /// The default world generation system used by Cyro Hazard.
    /// <br/>
    /// The vert noise is controlled by a compute shader.
    /// </summary>
    [AddComponentMenu("Generation/World Generation")]
    public class WorldGen : ChunkConstructor
    {
        /// <summary>
        /// Multiples the <see cref="_Vertices"/> array by this.
        /// </summary>
        [Header("Terrain Parameters")]
        public float HeightMultipler = 5.0f;
        /// <summary>
        /// How much the noise scales, the lower the more compact the space.
        /// </summary>
        [Tooltip("How much the noise scales, the lower the more compact the space.")]
        public float NoiseScale = 50.0f;
        /// <summary>
        /// The angle a triangle is set to a stone material.
        /// </summary>
        [Tooltip("The angle a triangle is set to a stone material.")]
        public float MountainAngle = 0.005f;

        [Header("Generation")]
        public ComputeShader Compute;
        /// <summary>
        /// The DNA of the terrain.
        /// </summary>
        [Tooltip("The DNA of the terrain.")]
        public int Seed = 1337;

        private float[,] _Vertices;

        public Vector3 GetVertCoord(Vector2Int pos)
        {
            return new(pos.x, _Vertices[pos.x, pos.y], pos.y);
        }

        public override float GenerateVertexHeight(Vector2Int tilePos, int i, Vector2Int chunkPos)
        {
            return _Vertices[tilePos.x, tilePos.y] * HeightMultipler;
        }

        const byte SnowMaterial = 0;
        const byte StoneMaterial = 1;

        public override byte GetTriangleMaterial(int triangleIndex, Vector2Int chunkPos)
        {
            int tileIndex = triangleIndex / 2;
            bool isFirst = triangleIndex % 2 == 0;
            Vector2Int pos = GetPosByIndex(tileIndex);

            Vector3 a = GetVertCoord(pos + (isFirst ? Vector2Int.zero : Vector2Int.one)),
            b = GetVertCoord(pos + Vector2Int.right),
            c = GetVertCoord(pos + Vector2Int.up);

            // float dir0 = Vector3.Dot(b - a, c - a),
            // dir1 = Vector3.Dot(b - d, c - d),
            float dir = Vector3.Dot((b - a).Abs(), (c - a).Abs()),
            factor = Mathf.Abs(dir * Mathf.Rad2Deg);

            byte material = factor <= MountainAngle ? SnowMaterial : StoneMaterial;

            return material;
        }

        public override void PrepareGeneration(Vector2Int chunkPos)
        {
            base.PrepareGeneration(chunkPos);

            RenderTexture renderTexture = new(VerticesPerAxis, VerticesPerAxis, 32)
            {
                enableRandomWrite = true,
                filterMode = FilterMode.Point
            };

            renderTexture.Create();

            Compute.SetTexture(0, "Result", renderTexture);
            Compute.SetInt("ChunkSize", TilesPerAxis);
            Compute.SetFloat("Seed", Seed);
            Compute.SetFloat("NoiseScale", NoiseScale);
            Compute.SetInts("ChunkPos", chunkPos.x, chunkPos.y);

            int numThreadGroups = VerticesPerAxis / 32 + 1;
            Compute.Dispatch(0, numThreadGroups, numThreadGroups, 1);

            Texture2D texture = renderTexture.ToTexture2D();
            float[,] verts = new float[VerticesPerAxis, VerticesPerAxis];

            for (int i = 0; i < VerticesPerChunk; i++)
            {
                int x = i % VerticesPerAxis,
                y = i / VerticesPerAxis;

                verts[x, y] = texture.GetPixel(x, y).grayscale;
            }

            _Vertices = verts;

            renderTexture.Release();
        }
    }
}
