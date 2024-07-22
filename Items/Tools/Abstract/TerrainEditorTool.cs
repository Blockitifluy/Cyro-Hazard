using System;
using System.Linq;
using Godot;

[GlobalClass]
public partial class TerrianEditorTool : WeaponTool
{
  protected class ChunkComponents
  {
    public MeshInstance3D MeshInstance;
    public Chunk chunk;

    internal ChunkComponents(Chunk chunk, MeshInstance3D meshInstance)
    {
      MeshInstance = meshInstance;
      this.chunk = chunk;
    }
  }

  protected MeshGeneration TerrainGenerator
  {
    get
    {
      MeshGeneration meshGeneration = GetTree().Root
      .GetChildByType<MeshGeneration>();

      if (meshGeneration == null) throw new NullReferenceException("MeshGeneration doesn't Exist");

      return meshGeneration;
    }
  }

  protected static (int, Vector3) GetNearestVertex(MeshDataTool meshTool, Vector3 globalPos, Vector3 at)
  {
    Vector3 localAt = at - globalPos;

    Vector3 nearestVertex = Vector3.Inf;
    int nearestIndex = int.MaxValue;
    float nearestDistance = float.MaxValue;

    int vertexCount = meshTool.GetVertexCount();
    for (int i = 0; i < vertexCount; i++)
    {
      Vector3 vertex = meshTool.GetVertex(i);

      float dist = vertex.DistanceSquaredTo(localAt);

      if (dist >= nearestDistance) continue;

      nearestDistance = dist;
      nearestVertex = vertex;
      nearestIndex = i;
    }

    return (nearestIndex, nearestVertex);
  }

  protected (Vector3, ChunkComponents) GetChunkFromRay()
  {
    (Vector3, ChunkComponents) nullReturn = (Vector3.Inf, null);

    var ray = ScreenPointToRay();

    if (!ray.ContainsKey("position")) return nullReturn;

    Vector3 position = (Vector3)ray["position"];
    Node chunk = (Node)ray["collider"];

    if (chunk is not Chunk) return nullReturn;

    MeshInstance3D chunkMesh = chunk.GetChildByType<MeshInstance3D>();
    if (chunkMesh == null)
      return nullReturn;

    return (position, new(chunk as Chunk, chunkMesh));
  }
}