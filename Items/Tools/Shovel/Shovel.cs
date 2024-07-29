using Godot;

[GlobalClass]
public partial class Shovel : TerrainEditorTool
{
  [Export]
  public float DigDepth { get; set; } = 1.0f;

  private void EditChunk(Vector3 rayHit, ChunkComponents components)
  {
    MeshDataTool meshDataTool = new();
    meshDataTool.CreateFromSurface(components.MeshInstance.Mesh as ArrayMesh, 0);

    var (_, vertex) = GetNearestVertex(
      meshDataTool,
      components.chunk.GlobalPosition,
      rayHit
    );

    meshDataTool.Clear();

    var (inRange, _) = IsInRange(vertex, Range);
    if (!inRange) return;

    Vector2I vrtPos = new((int)vertex.X, (int)vertex.Z);
    float newHeight = vertex.Y - DigDepth;

    TerrainGenerator.EditVertexHeight(components.chunk, vrtPos, newHeight);
  }

  public override void Fire()
  {
    base.Fire();

    var (rayHit, components) = GetChunkFromRay();
    if (components == null) return;

    EditChunk(rayHit, components);
  }
}