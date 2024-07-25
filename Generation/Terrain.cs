using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class Terrain : MeshGeneration
{
  [ExportGroup("Trees")]
  [Export(PropertyHint.Range, "0,1")] public float TreeDensity = 0.25f;
  [Export] public float TreeSpawnScale = 2;
  [Export] public FastNoiseLite TreeNoise = new();
  [Export] public PackedScene TreeScene;

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

  private Node3D LoadTree(Vector2 tilePos, Vector4 tileHeight)
  {
    GD.Seed((ulong)tilePos.GetHashCode());

    float centerX = tilePos.X + GD.Randf(),
    centerZ = tilePos.Y + GD.Randf();

    float centerY = (tileHeight.Y + tileHeight.W) / 2;

    Vector3 pos = new(centerX, centerY, centerZ);
    float rotation = (float)GD.RandRange(0.0f, 180.0f);

    TreeProp tree = TreeScene.Instantiate<TreeProp>();
    tree.Position = pos;
    tree.Rotate(Vector3.Up, rotation);
    tree.AddToGroup("Trees");

    GD.Randomize();

    return tree;
  }

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
  public Vector4 GetHeightForTile(Vector2I[] crns)
  {
    return new Vector4(
      GetHeightForVertex(crns[0]),
      GetHeightForVertex(crns[1]),
      GetHeightForVertex(crns[2]),
      GetHeightForVertex(crns[3])
    );
  }

  public override HashSet<Node> GetProps(Vector2I chunkPos, Tile[] tiles)
  {
    HashSet<Node> props = new();

    for (int i = 0; i < ChunkArea; i++)
    {
      int X = i % ChunkSize,
      Y = i / ChunkSize;

      Vector2I tilePos = new(X, Y);
      Vector2I globalPos = chunkPos * ChunkSize + tilePos;

      if (ShouldLoadTree(globalPos))
        props.Add(LoadTree(tilePos, tiles[i].TileHeight));
    }

    return props;
  }

  public override Tile GetTile(Vector2I tilePos, Vector2I chunkPos)
  {
    if (tilePos.X >= ChunkSize || tilePos.Y >= ChunkSize)
      throw new ArgumentOutOfRangeException(nameof(tilePos));

    Vector2I globalPos = chunkPos * ChunkSize + tilePos;
    Vector2I[] crns = Tile.TileCorners(globalPos);
    Tile.TileType tileType = Tile.TileType.Snow;
    Vector4 TileHeight = GetHeightForTile(crns);

    Tile tile = new(tilePos, tileType, TileHeight);

    return tile;
  }

  public override void _Ready()
  {
    int seed = (int)GD.Randi();

    GD.PrintRich($"[b][color=GREEN]Chunk Generation[/color][/b] Using seed {seed}");

    TerrainNoise.Seed = seed;
    TreeNoise.Seed = seed;

    base._Ready();
  }
}