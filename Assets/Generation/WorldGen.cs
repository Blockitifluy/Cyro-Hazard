using UnityEngine;

namespace CH.Generation
{
    public class WorldGen : ChunkConstructor
    {
        [Header("Generation")]
        public float HeightMultipler = 1.0f;
        public FastNoiseLite Noise;

        private float GenerateHeightNoise(Vector2 pos)
        {
            float rawNoise = Noise.GetNoise(pos.x, pos.y);
            float quad = Mathf.Pow(rawNoise, 4);

            return quad * HeightMultipler;
        }

        private Vector2 GetWorldPos(int x, int y, Vector2Int chunkPos)
        {
            return new(
                x + (chunkPos.x * TilesPerAxis),
                y + (chunkPos.y * TilesPerAxis)
            );
        }

        public override float GenerateVertexHeight(int x, int y, Vector2Int chunkPos)
        {
            Vector2 worldPos = GetWorldPos(x, y, chunkPos);

            float height = GenerateHeightNoise(worldPos);

            return height;
        }
    }
}
