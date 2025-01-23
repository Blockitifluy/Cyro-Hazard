using UnityEngine;

namespace Generation
{
    public class WorldGen : ChunkConstructor
    {
        [Header("Generation")]
        public float Scale = 0.2f;
        public float HeightMultipler = 1.0f;

        public override float GenerateVertexHeight(int x, int y, Vector2Int chunkPos)
        {
            Vector2 realPos = new(x + (chunkPos.x * TilesPerAxis), y + (chunkPos.y * TilesPerAxis));

            float height = (Mathf.PerlinNoise(realPos.x * Scale, realPos.y * Scale) - 0.5f) * HeightMultipler * 2.0f;

            return height;
        }
    }
}
