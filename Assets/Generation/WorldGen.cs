using UnityEngine;

namespace CH.Generation
{
    public class WorldGen : ChunkConstructor
    {
        [Header("Generation")]
        public ComputeShader Compute;
        public int Seed = 1337;
        public float HeightMultipler = 5.0f;
        public float NoiseScale = 50.0f;

        [SerializeField]
        private Texture2D _ChunkTexture;

        public override float GenerateVertexHeight(Vector2Int tilePos, int i, Vector2Int chunkPos)
        {
            Color pixelColour = _ChunkTexture.GetPixel(tilePos.x, tilePos.y);
            float height = pixelColour.grayscale;

            return (height - 0.5f) * HeightMultipler;
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
            Compute.SetInt("ChunkSize", TilesPerAxis);
            Compute.SetFloat("Seed", Seed);
            Compute.SetFloat("NoiseScale", NoiseScale);
            Compute.SetInts("ChunkPos", chunkPos.x, chunkPos.y);

            int numThreadGroups = VerticesPerAxis / 32 + 1;
            Compute.Dispatch(0, numThreadGroups, numThreadGroups, 1);

            _ChunkTexture = renderTexture.ToTexture2D();

            renderTexture.Release();
        }
    }
}
