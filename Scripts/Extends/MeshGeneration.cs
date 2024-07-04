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
  public struct Follower
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

    public Follower(Node3D node, Vector3 lastPosition)
    {
      Node = node;
      LastPosition = lastPosition;
    }
  }

  /// <summary>
  /// A tile's type, position and height 
  /// </summary>
  public struct Tile
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
    public TileType Type;

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

    public Tile(Vector2I pos, TileType type, Vector4 height)
    {
      Position = pos;
      Type = type;
      TileHeight = height;
    }
  }

  public readonly struct Corner
  {
    public readonly float Height;
    public readonly Vector2I Position2D;

    public readonly Vector3 ChunkPosition
    {
      get { return new(Position2D.X, Height, Position2D.Y); }
    }

    public Corner(float height, Vector2I pos2D)
    {
      Height = height;
      Position2D = pos2D;
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
  [Export] public int RenderRadius = 5;
  /// <summary>
  /// How mesh's faces displayed
  /// </summary>
  [Export] public Mesh.PrimitiveType primitiveType = Mesh.PrimitiveType.Triangles;

  [ExportGroup("Materials")]
  [Export] public StandardMaterial3D SnowMaterial;
  [Export] public StandardMaterial3D StoneMaterial;

  public int ChunkArea { get { return ChunkSize * ChunkSize; } }
  public int ChunkLoaded { get { return RenderRadius * RenderRadius; } }

  private readonly Dictionary<Tile.TileType, StandardMaterial3D> MaterialMap = new();
  private Follower NodeFollower;

  /// <summary>
  /// Gets a tile's data based on position
  /// </summary>
  /// <param name="tilePos">Tile's relative position</param>
  /// <param name="chunkPos">Chunk's non scaled position</param>
  /// <returns>The tile's infomation</returns>
  /// <exception cref="ArgumentOutOfRangeException">TilePos is bigger than the chunk size</exception>
  protected abstract Tile GetTile(Vector2I tilePos, Vector2I chunkPos);

  /// <summary>
  /// Generates a vertex
  /// </summary>
  /// <param name="st">The surface tool</param>
  /// <param name="height">The height of the vertex</param>
  /// <param name="pos2D">The plane position of the vertex</param>
  /// <param name="i">The index</param>
  /// <param name="normal">The face normal</param>
  private void AddVertex(SurfaceTool st, Corner corner)
  {
    st.SetUV((Vector2)corner.Position2D / ChunkSize);
    st.AddVertex(corner.ChunkPosition);
  }

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

  public Vector2I[] ChunkMapping()
  {
    Vector2I[] chunkMapping = new Vector2I[ChunkLoaded];

    Parallel.For(0, ChunkLoaded, i =>
    {
      int X = i % RenderRadius,
      Y = Mathf.FloorToInt(i / RenderRadius);

      chunkMapping[i] = new(X - RenderRadius / 2, Y - RenderRadius / 2);
    });

    return chunkMapping;
  }

  public Vector2I[] ChunkMapping(Vector2I pos)
  {
    Vector2I[] chunkMapping = new Vector2I[ChunkLoaded];

    int i = 0;
    do
    {
      int X = i % ChunkSize,
      Y = Mathf.FloorToInt(i / ChunkSize);

      Vector2I chunkPos = new(X - RenderRadius / 2, Y - RenderRadius / 2);
      chunkMapping[i] = chunkPos + pos;
      i++;
    } while (i < ChunkLoaded);

    return chunkMapping;
  }

  private void LoadTile(int tileIndex, Vector2I chunkPos, SurfaceTool st)
  {
    int X = tileIndex % ChunkSize,
      Y = Mathf.FloorToInt(tileIndex / ChunkSize);

    Vector2I tilePos = new(X, Y);

    Tile tile = Debug ? new(tilePos, Tile.TileType.Snow, new(0, 0, 0, 0)) : GetTile(tilePos, chunkPos);
    Vector4 height = tile.TileHeight;

    Vector2I[] corners = Tile.TileCorners(tilePos);

    // Tri 0
    AddVertex(st, new(height.X, corners[0]));
    AddVertex(st, new(height.Y, corners[1]));
    AddVertex(st, new(height.Z, corners[2]));

    // Tri 1
    AddVertex(st, new(height.Y, corners[1]));
    AddVertex(st, new(height.W, corners[3]));
    AddVertex(st, new(height.Z, corners[2]));
  }

  /// <summary>
  /// Generates a chunks mesh
  /// </summary>
  /// <param name="chunkPos">Chunk's non-scaled position</param>
  /// <returns>A mesh</returns>
  private ArrayMesh GenerateChunk(Vector2I chunkPos)
  {
    SurfaceTool st = new();
    st.Begin(primitiveType);
    st.SetMaterial(SnowMaterial);

    int i = 0;
    do
    {
      LoadTile(i, chunkPos, st);
      i++;
    } while (i < ChunkArea);

    st.Index();
    if (primitiveType == Mesh.PrimitiveType.Triangles)
      st.GenerateNormals();

    return st.Commit();
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

    /*
    ConvexPolygonShape3D collisionShape = mesh.CreateConvexShape(clean: true, simplify: false);
    collisionShape.Margin = 0.1f;
   */

    MeshInstance3D meshInstance = new()
    {
      Mesh = mesh,
      CastShadow = GeometryInstance3D.ShadowCastingSetting.On
    };
    chunk.AddChild(meshInstance);

    CollisionShape3D collision = new()
    {
      Shape = mesh.CreateConvexShape(),
    };

    chunk.AddChild(collision);

    return chunk;
  }

  /// <summary>
  /// Clears all pre-existing chunks, and generates chunks surrounding and under the follower
  /// </summary>
  public StaticBody3D[] GenerateChunks()
  {
    Vector2I chunkPos = PositionToChunk(ChunkSize, NodeFollower.Node.GlobalPosition);
    GD.Print($"Follower moved {chunkPos}");

    CleanChunks();

    StaticBody3D[] chunks = new StaticBody3D[ChunkLoaded];
    Vector2I[] chunkMapping = ChunkMapping();

    ulong timeStarted = Time.GetTicksMsec();

    int i = 0;
    do
    {
      chunks[i] = MakeChunk(chunkMapping[i] + chunkPos);
      i++;
    } while (i < ChunkLoaded);

    ulong timeTaken = Time.GetTicksMsec() - timeStarted,
    timePerChunk = timeTaken / (ulong)ChunkLoaded;
    GD.Print($"Generated {ChunkLoaded} Chunks in {timeTaken}ms ({timePerChunk} ms per Chunk)");

    return chunks;
  }

  public override void _Ready()
  {
    base._Ready();

    Node3D player = GetNode<Node3D>("/root/Main/Player");
    NodeFollower = new(player, player.GlobalPosition);

    MaterialMap.Add(Tile.TileType.Snow, SnowMaterial);
    MaterialMap.Add(Tile.TileType.Stone, StoneMaterial);
    MaterialMap.Add(Tile.TileType.Dirt, new());

    foreach (var chk in GenerateChunks())
      AddChild(chk);
  }

  public override void _Process(double delta)
  {
    base._Process(delta);

    if (!NodeFollower.IfMoved(ChunkSize))
      return;

    foreach (var chk in GenerateChunks())
    {
      AddChild(chk);
    }
  }
}