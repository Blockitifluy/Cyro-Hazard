using UnityEngine;

namespace CH.Generation
{
    public class WorldGen : ChunkConstructor
    {
        [Header("Generation")]
        public ComputeShader Compute;
        public float HeightMultipler = 1.0f;
        public FastNoiseLite Noise;

        [SerializeField]
        private Texture2D _ChunkTexture;

        private float GenerateHeightNoise(Vector2 pos)
        {
            float rawNoise = Noise.GetNoise(pos.x, pos.y);
            float quad = Mathf.Pow(rawNoise, 4);

            return quad * HeightMultipler;
        }

        private Vector2Int GetWorldPos(int x, int y, Vector2Int chunkPos)
        {
            return new(
                x + (chunkPos.x * TilesPerAxis),
                y + (chunkPos.y * TilesPerAxis)
            );
        }

        public override float GenerateVertexHeight(Vector2Int tilePos, int i, Vector2Int chunkPos)
        {
            return _ChunkTexture.GetPixel(tilePos.x, tilePos.y).grayscale;
        }

        public override void OnPreGenerate(Vector2Int chunkPos)
        {
            base.OnPreGenerate(chunkPos);

            RenderTexture renderTexture = new(VerticesPerAxis, VerticesPerAxis, 32)
            {
                enableRandomWrite = true,

            };

            renderTexture.Create();

            Compute.SetTexture(0, "Result", renderTexture);
            Compute.SetInt("ChunkSize", TilesPerChunk);
            Compute.SetInts("ChunkPos", chunkPos.x, chunkPos.y);

            int numThreadGroups = VerticesPerAxis / 8;
            Compute.Dispatch(0, numThreadGroups, numThreadGroups, 1);

            _ChunkTexture = renderTexture.ToTexture2D();

            renderTexture.Release();
        }
    }
}
