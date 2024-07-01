using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public abstract partial class MeshGeneration : Node
{
  /// <summary>
  /// A node with it last position stored, commonly used if the node moved to another chunk. 
  /// </summary>
  public struct FollowerData
  {
    /// <summary>
    /// The node that is being Tracked
    /// </summary>
    public readonly Node3D Node;
    /// <summary>
    /// The last recorded position of the Node
    /// </summary>
    public Vector3 LastPosition;

    /// <summary>
    /// Did the Node move into another Chunk
    /// </summary>
    /// <param name="ChunkSize">The chunk size</param>
    /// <returns>If the Node moved</returns>
    public bool IfMoved(int ChunkSize)
    {
      Vector3 currentPos = Node.GlobalPosition;

      Vector2I lastChunk = PositionToChunk(ChunkSize, LastPosition),
      newChunk = PositionToChunk(ChunkSize, currentPos);

      LastPosition = currentPos;

      return lastChunk != newChunk;
    }

    public FollowerData(Node3D node, Vector3 lastPosition)
    {
      Node = node;
      LastPosition = lastPosition;
    }
  }

  /// <summary>
  /// A tile's type, position and height 
  /// </summary>
  public struct TileData
  {
    /// <summary>
    /// The tile's type, e.g. Snow,
    /// </summary>
    public enum TileType : uint
    {
      Snow,
      Dirt,
      Stone,
    }

    /// <summary>
    /// The global position of the tile
    /// </summary>
    readonly public Vector2I Position;

    /// <summary>
    /// The tile's type e.g. Snow.
    /// </summary>
    public TileType Tile;

    /// <summary>
    /// 1. Top Left.
    /// 2. Top Right.
    /// 3. Bottom Left.
    /// 4. Bottom Right.
    /// </summary>
    public Vector4 TileHeight;

    /// <summary>
    /// The tile's height in a 4 float array
    /// </summary>
    public float[] HeightArray
    {
      readonly get
      {
        return new float[4]
        {
          TileHeight.X,
          TileHeight.Y,
          TileHeight.Z,
          TileHeight.W
        };
      }
      set { TileHeight = new(value[0], value[1], value[2], value[4]); }
    }

    /// <summary>
    /// Every element of the 4 vector3 array that X and Z in the 2D coorinates and Y is the height
    /// </summary>
    public readonly Vector3[] HeightVector
    {
      get
      {
        return new Vector3[4]
        {
          new(0, TileHeight.X, 0),
          new(1, TileHeight.Y, 0),
          new(0, TileHeight.Z, 1),
          new(1, TileHeight.W, 1),
        };
      }
    }

    /// <summary>
    /// Get the corner's coornidates offseted by the tilePos
    /// </summary>
    /// <param name="tilePos">The offset</param>
    /// <returns>corner + offset</returns>
    public static Vector2[] TileCorners(Vector2 tilePos)
    {
      Vector2 tl = tilePos,
        tr = tilePos + new Vector2(1, 0),
        bl = tilePos + new Vector2(0, 1),
        br = tilePos + new Vector2(1, 1);

      return new Vector2[4] { tl, tr, bl, br };
    }

    /// <summary>
    /// Get the corner's coornidates offseted by the tilePos
    /// </summary>
    /// <param name="tilePos">The offset</param>
    /// <returns>corner + offset</returns>
    public static Vector2I[] TileCorners(Vector2I tilePos)
    {
      Vector2I tl = tilePos,
        tr = tilePos + new Vector2I(1, 0),
        bl = tilePos + new Vector2I(0, 1),
        br = tilePos + new Vector2I(1, 1);

      return new Vector2I[4] { tl, tr, bl, br };
    }

    public TileData(Vector2I pos, TileType tile, Vector4 height)
    {
      Position = pos;
      Tile = tile;
      TileHeight = height;
    }
  }

  /// <summary>
  /// Transforms a position to a non-scaled chunk position
  /// </summary>
  /// <param name="chunkSize">The chunk size</param>
  /// <param name="pos">The position wanted to be transformed</param>
  /// <returns>A non-scaled chunk position</returns>
  public static Vector2I PositionToChunk(int chunkSize, Vector3 pos)
  {
    Vector2I flaten = new((int)pos.X, (int)pos.Z);
    Vector2I chunkPos = flaten / chunkSize;

    Vector2I rounded = new(chunkPos.X, chunkPos.Y);

    return rounded;
  }

  /// <summary>
  /// Generate the noise as flat
  /// </summary>
  [Export] public bool Debug = false;
  /// <summary>
  /// The Chunk's size
  /// </summary>
  [Export] public int ChunkSize = 16;
  /// <summary>
  /// The chunk rendering radius
  /// </summary>
  [ExportGroup("Rendering")]
  [Export] public int RenderRadius = 4;
  /// <summary>
  /// How mesh's faces displayed
  /// </summary>
  [Export] public Mesh.PrimitiveType primitiveType = Mesh.PrimitiveType.Triangles;

  private FollowerData Follower;
  private StandardMaterial3D SnowMaterial;

  /// <summary>
  /// Gets a tile's data based on position
  /// </summary>
  /// <param name="tilePos">Tile's relative position</param>
  /// <param name="chunkPos">Chunk's non scaled position</param>
  /// <returns>The tile's infomation</returns>
  /// <exception cref="ArgumentOutOfRangeException">TilePos is bigger than the chunk size</exception>
  public abstract TileData GetTile(Vector2I tilePos, Vector2I chunkPos);

  /// <summary>
  /// Clears and deletes all chunks
  /// </summary>
  private List<Vector2I> CleanChunks()
  {
    List<Vector2I> keptPos = new();

    // TODO - Don't clear chunks that are already are loaded

    foreach (var chk in GetTree().GetNodesInGroup("chunks").Cast<StaticBody3D>())
    {
      chk.QueueFree();
    }

    return keptPos;
  }

  /// <summary>
  /// Generates a vertex
  /// </summary>
  /// <param name="st">The surface tool</param>
  /// <param name="height">The height of the vertex</param>
  /// <param name="pos2D">The plane position of the vertex</param>
  /// <param name="i">The index</param>
  /// <param name="normal">The face normal</param>
  private Vector3 AddVertex(SurfaceTool st, float height, Vector2I pos2D)
  {
    st.SetUV((Vector2)pos2D / ChunkSize);
    st.SetMaterial(SnowMaterial);

    Vector3 pos = new(pos2D.X, height, pos2D.Y);

    st.AddVertex(pos);

    return pos;
  }

  public List<Vector2I> ChunkMapping()
  {
    List<Vector2I> chunkMapping = new();
    for (int i = 0; i < RenderRadius * RenderRadius; i++)
    {
      int X = i % RenderRadius,
      Y = (int)Mathf.Floor(i / RenderRadius);

      chunkMapping.Add(new(X - RenderRadius / 2, Y - RenderRadius / 2));
    }

    return chunkMapping;
  }

  public List<Vector2I> ChunkMapping(Vector2I pos)
  {
    List<Vector2I> chunkMapping = new();
    for (int i = 0; i < RenderRadius * RenderRadius; i++)
    {
      int X = i % ChunkSize,
      Y = (int)Mathf.Floor(i / ChunkSize);

      Vector2I chunkPos = new(X - RenderRadius / 2, Y - RenderRadius / 2);
      chunkMapping.Add(chunkPos + pos);
    }

    return chunkMapping;
  }

  private void LoadTile(int tileIndex, Vector2I chunkPos, SurfaceTool surfaceTool)
  {
    int X = tileIndex % ChunkSize,
      Y = (int)Mathf.Floor(tileIndex / ChunkSize);

    Vector2I tilePos = new(X, Y);

    TileData tile = Debug ? new(tilePos, TileData.TileType.Snow, new(0, 0, 0, 0)) : GetTile(tilePos, chunkPos);
    Vector4 height = tile.TileHeight;

    Vector2I[] corners = TileData.TileCorners(tilePos);

    // Tri 0
    AddVertex(surfaceTool, height.X, corners[0]);
    AddVertex(surfaceTool, height.Y, corners[1]);
    AddVertex(surfaceTool, height.Z, corners[2]);

    // Tri 1
    AddVertex(surfaceTool, height.Y, corners[1]);
    AddVertex(surfaceTool, height.W, corners[3]);
    AddVertex(surfaceTool, height.Z, corners[2]);
  }

  /// <summary>
  /// Generates a chunks mesh
  /// </summary>
  /// <param name="chunkPos">Chunk's non-scaled position</param>
  /// <returns>A mesh</returns>
  private ArrayMesh GenerateChunk(Vector2I chunkPos)
  {
    SurfaceTool surfaceTool = new();
    surfaceTool.Begin(primitiveType);

    for (int i = 0; i < ChunkSize * ChunkSize; i++)
      LoadTile(i, chunkPos, surfaceTool);

    surfaceTool.Index();
    if (primitiveType == Mesh.PrimitiveType.Triangles)
      surfaceTool.GenerateNormals();

    return surfaceTool.Commit(flags: 256 | 1);
  }

  /// <summary>
  /// Creates a chunk based on it's position and generated with it's mesh
  /// </summary>
  /// <param name="chunkPos">The chunk's non-scaled position</param>
  private StaticBody3D MakeChunk(Vector2I chunkPos)
  {
    string chunkName = $"Chunk {chunkPos}";

    Vector3 scaledPos = new(chunkPos.X * ChunkSize, 0, chunkPos.Y * ChunkSize);

    StaticBody3D chunk = new()
    {
      Position = scaledPos,
      Name = chunkName
    };
    chunk.AddToGroup("chunks");

    ArrayMesh mesh = GenerateChunk(chunkPos);

    MeshInstance3D meshInstance = new()
    {
      Mesh = mesh
    };
    chunk.AddChild(meshInstance);

    CollisionShape3D collision = new()
    {
      Shape = mesh.CreateConvexShape(clean: false, simplify: false),
      ProcessPriority = -2
    };

    chunk.AddChild(collision);

    return chunk;
  }

  /// <summary>
  /// Clears all pre-existing chunks, and generates chunks surrounding and under the follower
  /// </summary>
  public List<StaticBody3D> GenerateChunks()
  {
    Node3D Node = Follower.Node;
    Vector2I chunkPos = PositionToChunk(ChunkSize, Node.GlobalPosition);
    GD.Print($"Follower moved {chunkPos}");

    CleanChunks();

    List<StaticBody3D> loaded = new();
    List<Vector2I> chunkMapping = ChunkMapping();

    ulong timeStarted = Time.GetTicksMsec();
    foreach (var Map in chunkMapping)
    {
      StaticBody3D chk = MakeChunk(Map + chunkPos);
      loaded.Add(chk);
    };

    ulong timeTaken = Time.GetTicksMsec() - timeStarted;
    int chunksGenerated = chunkMapping.Count;
    GD.Print($"Generated {chunksGenerated} Chunks in {timeTaken}ms ({timeTaken / (ulong)chunksGenerated} ms per Chunk)");

    return loaded;
  }

  public override void _Ready()
  {
    base._Ready();

    Node3D player = GetNode<Node3D>("/root/Main/Player");
    Follower = new(player, player.GlobalPosition);

    SnowMaterial = GD.Load<StandardMaterial3D>("res://Textures/Materials/Snow.tres");

    var chunks = GenerateChunks();
    foreach (var chk in chunks)
      AddChild(chk);
  }

  public override void _Process(double delta)
  {
    base._Process(delta);

    bool didMove = Follower.IfMoved(ChunkSize);

    if (!didMove)
      return;

    GetTree().Paused = true;

    var chunks = GenerateChunks();
    foreach (var chk in chunks)
    {
      AddChild(chk);
    }
    GetTree().Paused = false;
  }
}