using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace CyroHazard.Generation
{
	public class ChunkPool
	{
		public GameObject Prefab;
		public ObjectPool<ChunkTerrain> Pool;

		private ChunkConstructor Constructor => ChunkConstructor.GetConstructor();

		private const int DefaultSize = 81;
		private const int MaxSize = 32 * 32;

		public ChunkTerrain CreatePooledObject()
		{
			GameObject chunk = UnityEngine.Object.Instantiate(Constructor.ChunkPrefab);

			ChunkTerrain terrain = chunk.GetComponent<ChunkTerrain>();

			return terrain;
		}

		private void OnGetFromPool(ChunkTerrain pooledObject)
		{
			pooledObject.gameObject.SetActive(true);
		}

		private void OnDestroyPooledObject(ChunkTerrain pooledObject)
		{
			UnityEngine.Object.Destroy(pooledObject);
		}

		private void OnReturnToPool(ChunkTerrain pooledObject)
		{
			pooledObject.gameObject.SetActive(false);
		}

		public ChunkTerrain GetObject(Vector2Int pos)
		{
			var constructor = ChunkConstructor.GetConstructor();

			ChunkTerrain terrain = Pool.Get();
			GameObject chunk = terrain.gameObject;

			Vector3 worldSize = new(
				pos.x * constructor.TilePhysicalSize * constructor.TilesPerAxis,
				0,
				pos.y * constructor.TilePhysicalSize * constructor.TilesPerAxis
			);

			terrain.LoadMesh(pos);
			chunk.name = $"Chunk ({pos.x}, {pos.y})";
			chunk.transform.position = worldSize;
			chunk.transform.parent = constructor.transform;
			chunk.tag = ChunkConstructor.ChunkTag;
			return terrain;
		}

		public void ReleaseObject(ChunkTerrain obj)
		{
			Pool.Release(obj);
		}

		public ChunkPool(GameObject prefab)
		{
			Prefab = prefab;
			Pool = new ObjectPool<ChunkTerrain>(
				CreatePooledObject,
				OnGetFromPool,
				OnReturnToPool,
				OnDestroyPooledObject,
				true, DefaultSize, MaxSize
			);
		}
	}

	/// <summary>
	/// Constructs chunk based on the focuses position.
	/// </summary>
	public abstract class ChunkConstructor : MonoBehaviour
	{
		// Classes and Structures

		/// <summary>
		/// An object that the <see cref="ChunkConstructor"/> uses to
		/// generate chunks at the <see cref="FocusObject"/>'s position.
		/// </summary>
		[Serializable]
		public class FocusObject
		{
			/// <summary>
			/// The GameObject that being tracked.
			/// </summary> 
			public GameObject GameObj;
			/// <summary>
			/// The last's FocusObject recorded position.
			/// </summary>
			[HideInInspector]
			public Vector3 LastPosition = Vector3.positiveInfinity;
			/// <summary>
			/// The last's FocusObject recorded grid position.
			/// </summary>
			public Vector2Int LastChunkPos => Constructor.WorldPosToChunkPos(LastPosition);

			public ChunkConstructor Constructor => GetConstructor();

			public bool DidMove()
			{
				var current = Constructor.WorldPosToChunkPos(GameObj.transform.position);
				return current != LastChunkPos;
			}

			/// <summary>
			/// Updates the current position. See <see cref="LastPosition"/>.
			/// </summary>
			public void UpdatePosition()
			{
				LastPosition = GameObj.transform.position;
			}
		}

		// Constants

		/// <summary>
		/// The tag of the <see cref="ChunkConstructor"/>.
		/// </summary>
		/// <remarks>
		/// Used in <see cref="GetConstructor()"/>. 
		/// </remarks>
		/// <seealso cref="GetConstructor()"/>.
		public const string ChunkConstructorTag = "ChunkConstructor";
		public const string ChunkTag = "Chunk";

		// Public Propetries and Fields

		/// <summary>
		/// When the objects listed here, move outside a chunks boundary. New chunk will load at the new chunk position.
		/// </summary>
		public FocusObject Focus;

		/// <summary>
		/// How many tiles are in the X and Y each.
		/// </summary>
		/// <remarks>
		/// Not to be confused for <see cref="TilesPerChunk"/>.
		/// </remarks>
		[Header("Chunks")]
		/// <summary>
		/// The prefab used to add a chunk.
		/// </summary>
		public GameObject ChunkPrefab;
		/// <summary>
		/// How far the chunk will load.
		/// </summary>
		public int ChunkRenderingRadius = 3;

		/// <summary>
		/// How many tiles are in a chunk's axis (X and Y axes).
		/// </summary>
		[Header("Tiles")]
		public int TilesPerAxis = 16;
		/// <summary>
		/// The physical actual size of the tiles.
		/// </summary>
		public float TilePhysicalSize = 0.5f;

		[Header("Materials")]
		public Material[] Materials;
		public float UVScale;

		/// <summary>
		/// The rendering area of the chunk. 
		/// </summary>
		public int ChunkRenderingArea => ChunkRenderingDiameter * ChunkRenderingDiameter;

		/// <summary>
		/// The rendering diameter of the chunk. 
		/// </summary>
		public int ChunkRenderingDiameter => ChunkRenderingRadius * 2 + 1;

		/// <summary>
		/// How many tiles are in a chunk.
		/// </summary>
		public int TilesPerChunk => TilesPerAxis * TilesPerAxis;

		/// <summary>
		/// How many vertices are in a chunk
		/// </summary>
		public int VerticesPerChunk => VerticesPerAxis * VerticesPerAxis;

		/// <summary>
		/// How many vertices are in an axis
		/// </summary>
		public int VerticesPerAxis => TilesPerAxis + 1;

		[HideInInspector]
		public int[] Triangles;

		[HideInInspector]
		public Vector2[] UVs;

		// Private Propetries and Fields

		/// <summary>
		/// The chunk dictionary. Contains all chunks loaded by the <see cref="ChunkConstructor"/>
		/// </summary>
		private ChunkPool _ChunkPool;
		private readonly Dictionary<Vector2Int, ChunkTerrain> _ChunkDict = new();

		// Generator Getter

		/// <summary>
		/// The cached generator from <see cref="GetConstructor()"/>
		/// </summary>
		private static ChunkConstructor _CachedGenerator;

		/// <summary>
		/// Gets the ChunkConstructor.
		/// </summary>
		/// <returns><see cref="ChunkConstructor"/></returns>
		/// <exception cref="NullReferenceException">If the ChunkConstructor couldn't be found.</exception>
		public static ChunkConstructor GetConstructor()
		{
			if (_CachedGenerator != null)
				return _CachedGenerator;

			var gen = GameObject.FindGameObjectWithTag(ChunkConstructorTag);
			if (gen == null)
				throw new NullReferenceException("Generator not found!");
			if (!gen.TryGetComponent<ChunkConstructor>(out var comp))
				throw new NullReferenceException("Generator script not found!");
			_CachedGenerator = comp;
			return comp;
		}

		/// <summary>
		/// Translates a 3D world position to a 2D chunk grid position. 
		/// </summary>
		/// <param name="pos">The world position.</param>
		/// <returns>2D chunk grid position</returns>
		public Vector2Int WorldPosToChunkPos(Vector3 pos)
		{
			var length = TilesPerAxis * TilePhysicalSize;
			Vector3 div = pos / length;
			Vector2Int final = new(
				(int)div.x,
				(int)div.z
			);
			return final;
		}

		public int MaterialCount => Materials.Length;

		// Chunk Generation

		public abstract float GenerateVertexHeight(Vector2Int tilePos, int i, Vector2Int chunkPos);
		public abstract byte GetTriangleMaterial(int triangleIndex, Vector2Int chunkPos);

		public virtual void PrepareGeneration(Vector2Int chunkPos) { }

		/// <summary>
		/// Generates the vertices for a chunk. See <seealso cref="Vertices"/>.
		/// </summary>
		/// <param name="chunkPos">The chunk's position</param>
		/// <returns>An array of vertices.</returns>
		public Vector3[] GenerateVertices(Vector2Int chunkPos)
		{
			PrepareGeneration(chunkPos);

			Vector3[] vertices = new Vector3[VerticesPerChunk];

			for (int i = 0; i < VerticesPerChunk; i++)
			{
				int x = i % VerticesPerAxis,
				y = i / VerticesPerAxis;

				Vector2Int pos = new(x, y);
				float height = (GenerateVertexHeight(pos, i, chunkPos) - 0.5f) * 2.0f;
				vertices[i] = new(x * TilePhysicalSize, height, y * TilePhysicalSize);
			}

			return vertices;
		}

		[ContextMenu("Load Triangles")]
		public void LoadTriangles()
		{
			int[] triangles = new int[TilesPerChunk * 6];

			int vert = 0,
			tris = 0;
			for (int y = 0; y < TilesPerAxis; y++)
			{
				for (int x = 0; x < TilesPerAxis; x++)
				{
					triangles[tris] = vert;
					triangles[tris + 1] = vert + TilesPerAxis + 1;
					triangles[tris + 2] = vert + 1;
					triangles[tris + 3] = vert + 1;
					triangles[tris + 4] = vert + TilesPerAxis + 1;
					triangles[tris + 5] = vert + TilesPerAxis + 2;

					vert++;
					tris += 6;
				}
				vert++;
			}

			Triangles = triangles;
		}

		[ContextMenu("Load UVs")]
		public void LoadUVs()
		{
			Vector2[] uvs = new Vector2[VerticesPerChunk];

			for (int i = 0, y = 0; y <= TilesPerAxis; y++)
			{
				for (int x = 0; x <= TilesPerAxis; x++)
				{
					uvs[i] = new(
						x / UVScale,
						y / UVScale
					);
					i++;
				}
			}

			UVs = uvs;
		}

		// Chunk Loading

		/// <summary>
		/// Loads a chunk at <paramref name="pos"/>.
		/// </summary>
		/// <param name="pos">The chunk's position.</param>
		/// <returns>The chunk object.</returns>
		public ChunkTerrain LoadChunk(Vector2Int pos)
		{
			var terrain = _ChunkPool.GetObject(pos);

			_ChunkDict.Add(pos, terrain);

			return terrain;
		}

		/// <summary>
		/// Unloads a chunk at the grid position. 
		/// </summary>
		/// <param name="pos">The chunk's position.</param>
		/// <exception cref="NullReferenceException">The chunk wasn't found.</exception>
		public void UnloadChunk(Vector2Int pos)
		{
			ChunkTerrain terrain = _ChunkDict[pos];

			if (terrain == null)
				throw new NullReferenceException($"Couldn't unload chunk at {pos}, because it doesn't exist.");

			_ChunkPool.ReleaseObject(terrain);
			_ChunkDict.Remove(pos);
		}

		/// <summary>
		/// Gets where all chunk's position are loaded.
		/// </summary>
		/// <returns>An array of chunk position</returns>
		/// <param name="pos">A chunk's grid position that offsets the mapping.</param>
		public Vector2Int[] GetChunkMappings(Vector2Int pos)
		{
			Vector2Int[] mappings = new Vector2Int[ChunkRenderingArea];

			for (int i = 0; i < ChunkRenderingArea; i++)
			{
				int x = i % ChunkRenderingDiameter - ChunkRenderingRadius,
				y = i / ChunkRenderingDiameter - ChunkRenderingRadius;

				mappings[i] = new Vector2Int(x + pos.x, y + pos.y);
			}

			return mappings;
		}
		/// <summary>
		/// Clears all the chunks that will out of range of the <see cref="Focus"/>.
		/// </summary>
		/// <param name="mappings">The chunk mappings.</param>
		/// <returns></returns>
		private IEnumerable<Vector2Int> CleanOutOfRangeChunks(Vector2Int[] mappings)
		{
			var copyList = new List<Vector2Int>(_ChunkDict.Keys);
			foreach (var pos in copyList)
			{
				if (mappings.Contains(pos))
				{
					yield return pos;
					continue;
				}

				UnloadChunk(pos);
			}
		}

		public void ClearChunks()
		{
			List<Vector2Int> positions = new(_ChunkDict.Keys);
			foreach (var pos in positions)
			{
				UnloadChunk(pos);
			}
		}

		// Other Functions

		public (int, int) GetXYByIndex(int i)
		{
			return (i % TilesPerAxis, i / TilesPerAxis);
		}

		public Vector2Int GetPosByIndex(int i)
		{
			return new(i % TilesPerAxis, i / TilesPerAxis);
		}

#if UNITY_EDITOR
		[ContextMenu("Refresh Chunks")]
		public void ContextUnloadAllChunks()
		{
			Vector2Int[] mappings = GetChunkMappings(Focus.LastChunkPos);

			ClearChunks();

			foreach (Vector2Int pos in mappings)
				LoadChunk(pos);
		}
#endif

		/// <summary>
		/// Refreshs the chunks at the <see cref="Focus"/>'s position.
		/// </summary>
		public void RefreshChunks()
		{
			System.Diagnostics.Stopwatch timer = new();
			timer.Start();

			Vector2Int[] mappings = GetChunkMappings(Focus.LastChunkPos);
			var kept = CleanOutOfRangeChunks(mappings);

			int chunkLoaded = 0;
			foreach (Vector2Int pos in mappings)
			{
				if (kept.Contains(pos))
					continue;

				LoadChunk(pos);
				chunkLoaded++;
			}

			timer.Stop();
			Debug.Log($"Loaded {chunkLoaded} chunk(s) in {timer.ElapsedMilliseconds}ms ({(double)timer.ElapsedMilliseconds / chunkLoaded}ms per chunk)");
		}

		// Start is called before the first frame update
		public virtual void Start()
		{
			_ChunkPool = new(ChunkPrefab);
			LoadTriangles();
			LoadUVs();

			RefreshChunks();
		}

		// Update is called once per frame
		private void Update()
		{
			if (Focus.DidMove())
			{
				Focus.UpdatePosition();
				RefreshChunks();
			}
		}
	}
}