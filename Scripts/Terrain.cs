using System;
using Godot;

public partial class Terrain : MeshGeneration
{
  [ExportGroup("Trees")]
  [Export] public float TreeDensity = 0.25f;
  [Export] public float TreeSpawnScale = 2;

  private FastNoiseLite TreeNoise = new();

  /// <summary>
  /// The noise's height
  /// </summary>
  [ExportGroup("Terrain")]
  [Export] public float TerrainHeight = 3.5f;
  /// <summary>
  /// The noise's size
  /// </summary>
  [Export] public float TerrainScale = 2.0f;
  /// <summary>
  /// If the noise is lower than MinFloor, then MinFloor is picked
  /// </summary>
  [Export] public float MinFloor = -0.75f;

  private FastNoiseLite TerrainNoise = new();

  /// <summary>
  /// Gets the corner's height
  /// </summary>
  /// <param name="crn">The global corner's position</param>
  /// <returns>The height</returns>
  public float GetHeightForCorner(Vector2 crn)
  {
    var noise2D = TerrainNoise.GetNoise2Dv(crn * TerrainScale);
    return Math.Max(noise2D, MinFloor) * TerrainHeight;
  }

  /// <summary>
  /// Every four corners' height for a tile
  /// </summary>
  /// <param name="crns">The 4 corner's global position</param>
  /// <returns>A vector4 height map</returns>
  public Vector4 GetHeightForTile(Vector2[] crns)
  {
    return new Vector4(
      GetHeightForCorner(crns[0]),
      GetHeightForCorner(crns[1]),
      GetHeightForCorner(crns[2]),
      GetHeightForCorner(crns[3])
    );
  }

  public override TileData GetTile(Vector2I tilePos, Vector2I chunkPos)
  {
    if (tilePos.X >= ChunkSize || tilePos.Y >= ChunkSize)
      throw new ArgumentOutOfRangeException(nameof(tilePos));

    Vector2I globalPos = chunkPos * ChunkSize + tilePos;

    Vector2[] crns = TileData.TileCorners((Vector2)globalPos);

    Vector4 tileHeight = GetHeightForTile(crns);

    TileData.TileType tileType = TileData.TileType.Snow;

    TileData Tile = new(tilePos, tileType, tileHeight);
    return Tile;
  }

  public override void _Ready()
  {
    int seed = (int)GD.Randi();

    GD.Print($"Using seed: {seed}");

    TerrainNoise.Seed = seed;
    TreeNoise.Seed = seed;

    base._Ready();
  }
}