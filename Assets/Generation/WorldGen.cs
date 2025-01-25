using UnityEngine;

namespace Generation
{
    public class WorldGen : ChunkConstructor
    {
        [Header("Generation")]
        public float Scale = 0.2f;
        public float HeightMultipler = 1.0f;
        [Range(-1.0f, 1.0f)]
        public float HeightFloor = -0.5f;
        [Header("Hills")]
        public float HillScale = 0.35f;
        public float HillMultipler = 5;

        private float GenerateHeightNoise(Vector2 scaled)
        {
            float rawNoise = Mathf.PerlinNoise(scaled.x, scaled.y);
            float adjusted = (rawNoise - 0.5f) * 2.0f;

            return Mathf.Max(adjusted * HeightMultipler, HeightFloor);
        }

        private float GenerateHillNoise(Vector2 worldPos)
        {
            float rawNoise = Mathf.PerlinNoise(worldPos.x * HillScale, worldPos.y * HillScale);
            return Mathf.Clamp01(rawNoise);
        }

        public override float GenerateVertexHeight(int x, int y, Vector2Int chunkPos)
        {
            Vector2 worldPos = new(x + (chunkPos.x * TilesPerAxis), y + (chunkPos.y * TilesPerAxis)),
            scaled = worldPos * Scale;

            float hillDist = GenerateHillNoise(worldPos),
            height = GenerateHeightNoise(scaled) * hillDist * HillMultipler;

            return height;
        }
    }
}
