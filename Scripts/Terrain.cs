using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Godot;

public partial class Terrain : MeshGeneration
{
  [ExportGroup("Trees")]
  [Export(PropertyHint.Range, "0,1")] public float TreeDensity = 0.25f;
  [Export] public float TreeSpawnScale = 2;
  [Export] public FastNoiseLite TreeNoise = new();

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
  [Export] public FastNoiseLite TerrainNoise = new();


  public bool ShouldLoadTree(Vector2I pos)
  {
    Vector2 scaledMap = (Vector2)pos * TreeSpawnScale;
    float noise = (Mathf.Clamp(TreeNoise.GetNoise2Dv(scaledMap), -1, 1) + 1) / 2;

    return noise <= TreeDensity;
  }

  /// <summary>
  /// Gets the corner's height
  /// </summary>
  /// <param name="vert">The global corner's position</param>
  /// <returns>The height</returns>
  public float GetHeightForVertex(Vector2 vert)
  {
    var noise2D = TerrainNoise.GetNoise2Dv(vert * TerrainScale);
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
      GetHeightForVertex(crns[0]),
      GetHeightForVertex(crns[1]),
      GetHeightForVertex(crns[2]),
      GetHeightForVertex(crns[3])
    );
  }

  protected override Tile GetTile(Vector2I tilePos, Vector2I chunkPos)
  {
    if (tilePos.X >= ChunkSize || tilePos.Y >= ChunkSize)
      throw new ArgumentOutOfRangeException(nameof(tilePos));

    Vector2I globalPos = chunkPos * ChunkSize + tilePos;

    Vector2[] crns = MeshGeneration.Tile.TileCorners((Vector2)globalPos);

    Tile.TileType tileType = MeshGeneration.Tile.TileType.Snow;

    Tile Tile = new(tilePos, tileType, GetHeightForTile(crns));
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