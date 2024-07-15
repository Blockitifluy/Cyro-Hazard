using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

		public readonly Vector2I GetGridPosition(int chunkSize)
		{
			return PositionToChunk(chunkSize, LastPosition);
		}

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

		public Vector4 TileHeight;

		public Vector3[] TileVertices
		{
			get
			{
				return new Vector3[4] {
				new(Position.X + 0, TileHeight.X, Position.Y + 0),
				new(Position.X + 1, TileHeight.Y, Position.Y + 0),
				new(Position.X + 0, TileHeight.Z, Position.Y + 1),
				new(Position.X + 1, TileHeight.W, Position.Y + 1),
			};
			}
		}

		public (Vector3, Vector3) GetNormals()
		{
			Vector3 normal0 = ComputeNormal(TileVertices[0], TileVertices[1], TileVertices[2]),
			normal1 = ComputeNormal(TileVertices[1], TileVertices[3], TileVertices[2]);

			return (normal0, normal1);
		}

		private static Vector3 ComputeNormal(Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 edge0 = b - a,
			edge1 = c - a;

			Vector3 cross = edge0.Cross(edge1).Normalized();

			return cross;
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

		public Tile(Vector2I pos, TileType type, Vector4 height)
		{
			Position = pos;
			Type = type;
			TileHeight = height;
		}
	}

	public struct Corner
	{
		public float Height;
		public Vector2I Position2D;

		public Vector3 ChunkPosition
		{
			get { return new(Position2D.X, Height, Position2D.Y); }
		}

		public Corner(float height, Vector2I pos2D)
		{
			Height = height;
			Position2D = pos2D;
		}
	}

	[GlobalClass]
	public partial class ChunkFactory : ArrayMesh
	{
		private List<Vector3> Vertices = new();
		private List<Vector2> UVs = new();
		private List<Vector3> Normals = new();
		private Godot.Collections.Array SurfaceArray = new();

		private PrimitiveType Primitive;

		public void Begin(PrimitiveType primitive)
		{
			ClearSurfaces();
			Vertices.Clear();
			UVs.Clear();
			Normals.Clear();

			Primitive = primitive;
		}

		public void End(Material material)
		{
			SurfaceArray.Resize((int)ArrayType.Max);
			SurfaceArray[(int)ArrayType.Vertex] = Vertices.ToArray();
			SurfaceArray[(int)ArrayType.TexUV] = UVs.ToArray();
			SurfaceArray[(int)ArrayType.Normal] = Normals.ToArray();

			AddSurfaceFromArrays(Primitive, SurfaceArray);

			SurfaceSetMaterial(0, material);
			SurfaceSetName(0, "Snow");
		}

		private void DrawPoint(Vector3 position, Vector3 direction, Vector2 uv)
		{
			Normals.Add(direction);
			UVs.Add(uv);
			Vertices.Add(position);
		}

		public void DrawTile(Tile tile, int i)
		{
			Vector3[] verts = tile.TileVertices;

			var (normal0, normal1) = tile.GetNormals();

			DrawPoint(verts[0], -normal0, Vector2.Zero);
			DrawPoint(verts[1], -normal0, Vector2.Right);
			DrawPoint(verts[2], -normal0, Vector2.Down);

			DrawPoint(verts[1], -normal1, Vector2.Right);
			DrawPoint(verts[3], -normal1, Vector2.One);
			DrawPoint(verts[2], -normal1, Vector2.Down);
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

	protected abstract HashSet<Node> GetProps(Vector2I chunkPos, Tile[] tiles);

	/// <summary>
	/// Clears and deletes all chunks
	/// </summary>
	private void CleanChunks()
	{
		foreach (var chk in GetTree().GetNodesInGroup("chunks").Cast<StaticBody3D>())
			chk.QueueFree();
	}

	public Vector2I[] ChunkMapping()
	{
		Vector2I[] chunkMapping = new Vector2I[ChunkLoaded];

		Parallel.For(0, ChunkLoaded, i =>
		{
			int X = i % RenderRadius,
			Y = i / RenderRadius;

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
			Y = i / ChunkSize;

			Vector2I chunkPos = new(X - RenderRadius / 2, Y - RenderRadius / 2);
			chunkMapping[i] = chunkPos + pos;
			i++;
		} while (i < ChunkLoaded);

		return chunkMapping;
	}

	private Tile[] PreloadTiles(Vector2I chunkPos)
	{
		Tile[] tiles = new Tile[ChunkArea];

		Parallel.For(0, ChunkArea, i =>
		{
			int X = i % ChunkSize,
			Y = i / ChunkSize;

			Vector2I tilePos = new(X, Y);

			if (Debug)
			{
				tiles[i] = new(tilePos, Tile.TileType.Snow, new(0, 0, 0, 0));
				return;
			}

			Tile tile = GetTile(tilePos, chunkPos);

			tiles[i] = tile;
		});

		return tiles;
	}

	/// <summary>
	/// Generates a chunks mesh
	/// </summary>
	/// <param name="chunkPos">Chunk's non-scaled position</param>
	/// <returns>A mesh</returns>
	private ArrayMesh GenerateChunk(Vector2I chunkPos, Node3D chunk)
	{
		ChunkFactory cf = new();
		cf.Begin(primitiveType);

		Tile[] tiles = PreloadTiles(chunkPos);
		HashSet<Node> props = GetProps(chunkPos, tiles);

		for (int i = 0; i < ChunkArea; i++)
			cf.DrawTile(tiles[i], i);

		foreach (Node prp in props)
			chunk.AddChild(prp);

		cf.End(SnowMaterial);
		return cf;
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

		ArrayMesh mesh = GenerateChunk(chunkPos, chunk);

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
	public List<StaticBody3D> GenerateChunks()

	{
		Vector2I chunkPos = PositionToChunk(ChunkSize, NodeFollower.Node.GlobalPosition);

		CleanChunks();

		List<StaticBody3D> chunks = new();
		Vector2I[] chunkMapping = ChunkMapping();

		Stopwatch stopwatch = new();
		stopwatch.Start();

		for (int i = 0; i < ChunkLoaded; i++)
		{
			var map = chunkMapping[i];
			chunks.Add(MakeChunk(map + chunkPos));
		}

		stopwatch.Stop();
		float timePerChunk = (float)stopwatch.ElapsedMilliseconds / ChunkLoaded;
		GD.PrintRich($"[b][color=GREEN]Chunk Generation ({ChunkLoaded})[/color][/b] {stopwatch.ElapsedMilliseconds}ms ({timePerChunk}ms per Chunk)");

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