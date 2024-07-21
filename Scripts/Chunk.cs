using Godot;

[GlobalClass]
public partial class Chunk : StaticBody3D
{
  public MeshGeneration.Tile[] Tiles { get; set; }
  [Export]
  public Vector2I GridPosition { get; set; }
}