using UnityEngine;

namespace Generation
{
    public class WorldGen : ChunkConstructor
    {
        [Header("Generation")]
        public float Scale = 0.2f;
        public float HeightMultipler = 1.0f;

        public override Vertex GetTileData(int x, int y, int chunkX, int chunkY)
        {
            Vector2 realPos = new(x + (chunkX * TilesPerAxis), y + (chunkY * TilesPerAxis));

            float height = (Mathf.PerlinNoise(realPos.x * Scale, realPos.y * Scale) - 0.5f) * HeightMultipler * 2.0f;

            Vertex tile = new()
            {
                PositionX = (byte)x,
                PositionY = (byte)y,
                Height = height
            };

            return tile;
        }
    }
}
