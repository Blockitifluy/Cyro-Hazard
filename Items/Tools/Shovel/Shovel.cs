using Godot;

[GlobalClass]
public partial class Shovel : TerrianEditorTool
{
  public override float Damage => 5.0f;
  public override float Range => 5.0f;

  public override double FireRate => 120.0d;
  public override FireTypes FiringMode => FireTypes.Single;

  public virtual float DigDepth { get; } = 1.0f;

  protected override void Equip()
  {
    base.Equip();

    GD.Print("Equiped Shovel");
  }

  public override void Fire()
  {
    base.Fire();

    var (rayHit, components) = GetChunkFromRay();
    if (components == null) return;

    MeshDataTool meshDataTool = new();
    meshDataTool.CreateFromSurface(components.MeshInstance.Mesh as ArrayMesh, 0);

    var (_, vertex) = GetNearestVertex(
      meshDataTool,
      components.chunk.GlobalPosition,
      rayHit
    );

    meshDataTool.Clear();

    Vector2I vrtPos = new((int)vertex.X, (int)vertex.Z);
    float newHeight = vertex.Y - DigDepth;

    GD.PrintRich($"[b][color=PURPLE]Item[/color][/b] Shovel dug {vrtPos} from {vertex.Y} to {newHeight}");
    TerrainGenerator.EditVertexHeight(components.chunk, vrtPos, vertex.Y - DigDepth);
  }
}