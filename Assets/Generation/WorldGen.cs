using System.Threading.Tasks;
using UnityEngine;

namespace CyroHazard.Generation
{
    public class WorldGen : ChunkConstructor
    {
        [Header("Generation")]
        public ComputeShader Compute;
        public int Seed = 1337;
        public float HeightMultipler = 5.0f;
        public float NoiseScale = 50.0f;
        public float MountainGradient = 2f;

        private float[,] _Vertices;

        public override float GenerateVertexHeight(Vector2Int tilePos, int i, Vector2Int chunkPos)
        {
            return _Vertices[tilePos.x, tilePos.y] * HeightMultipler;
        }

        const byte SnowMaterial = 0;
        const byte StoneMaterial = 1;

        const int SurroundingsLength = 2;

        public float[] GetSurroundingGradients(int[] surroundingSource, int tileIndex, Vector2Int pos, float center, bool isOnXAxis)
        {
            float[] gradients = new float[SurroundingsLength];

            for (int i = 0; i < SurroundingsLength; i++)
            {
                int nextIndex = surroundingSource[i];
                int local = tileIndex + nextIndex;

                Vector2Int adj = GetPosByIndex(local);
                bool inside = 0 < local && local <= (TilesPerAxis + 1) * (TilesPerAxis + 1);

                float z = inside ? _Vertices[adj.x, adj.y] : center;

                int delta = isOnXAxis ? (adj.x - pos.x) : (adj.y - pos.y);
                float gradient = delta / (z - center);

                if (float.IsNaN(gradient) || Mathf.Abs(gradient) == float.PositiveInfinity)
                    continue;

                gradients[i] = gradient;
            }

            return gradients;
        }

        public override byte GetTriangleMaterial(int triangleIndex, Vector2Int chunkPos)
        {
            int[] surroundingsX = new int[SurroundingsLength] { -1, 1 };
            int[] surroundingsY = new int[SurroundingsLength] { -TilesPerAxis, TilesPerAxis };

            int tileIndex = triangleIndex / 2;
            Vector2Int pos = GetPosByIndex(tileIndex);

            float center = _Vertices[pos.x, pos.y];
            byte material = SnowMaterial;

            float[] gradientX = GetSurroundingGradients(surroundingsX, tileIndex, pos, center, isOnXAxis: true),
            gradientY = GetSurroundingGradients(surroundingsY, tileIndex, pos, center, isOnXAxis: false);

            int i = 0;

            float[] gradients = new float[SurroundingsLength * 2];
            foreach (float grad in gradientX)
            {
                gradients[i] = grad;
                i++;
            }
            foreach (float grad in gradientY)
            {
                gradients[i] = grad;
                i++;
            }

            float gradMean = Helper.Mean(gradients);
            if (Mathf.Abs(gradMean) > MountainGradient || Mathf.Abs(gradMean) >= 400)
                material = StoneMaterial;

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
