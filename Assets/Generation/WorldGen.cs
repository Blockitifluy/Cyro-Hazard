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

        private Texture2D _ChunkTexture;

        public override float GenerateVertexHeight(Vector2Int tilePos, int i, Vector2Int chunkPos)
        {
            Color pixelColour = _ChunkTexture.GetPixel(tilePos.x, tilePos.y);
            float height = pixelColour.grayscale;

            return height * HeightMultipler;
        }

        public override byte GetTriangleMaterial(int triangleIndex, Vector2Int chunkPos)
        {
            // TODO: TEST REMOVE LATER
            byte materialIndex = (byte)(triangleIndex % 2);

            return materialIndex;
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

            _ChunkTexture = renderTexture.ToTexture2D();

            renderTexture.Release();
        }
    }
}
