using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CH.Generation
{
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
		public float TileSize = 0.5f;


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
		public int VerticesPerChunk => (TilesPerAxis + 1) * (TilesPerAxis + 1);

		// Private Propetries and Fields

		/// <summary>
		/// The chunk dictionary. Contains all chunks loaded by the <see cref="ChunkConstructor"/>
		/// </summary>
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
			var length = TilesPerAxis * TileSize;
			Vector3 div = pos / length;
			Vector2Int final = new(
				(int)div.x,
				(int)div.z
			);
			return final;
		}

		// Chunk Generation

		public abstract float GenerateVertexHeight(int x, int y, Vector2Int chunkPos);

		// Chunk Loading

		/// <summary>
		/// Loads a chunk at <paramref name="pos"/>.
		/// </summary>
		/// <param name="pos">The chunk's position.</param>
		/// <returns>The chunk object.</returns>
		public GameObject LoadChunk(Vector2Int pos)
		{
			GameObject chunk = Instantiate(ChunkPrefab);

			Vector3 worldSize = new(
				pos.x * TileSize * TilesPerAxis,
				0,
				pos.y * TileSize * TilesPerAxis
			);

			ChunkTerrain terrain = chunk.GetComponent<ChunkTerrain>();
			terrain.LoadMesh(pos);
			chunk.name = $"Chunk ({pos.x}, {pos.y})";
			chunk.transform.position = worldSize;
			chunk.transform.parent = transform;
			chunk.tag = ChunkTag;

			_ChunkDict.Add(pos, terrain);

			return chunk;
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
				throw new NullReferenceException($"Chunk at {pos} wasn't found!");
			_ChunkDict.Remove(pos);
			Destroy(terrain.gameObject);
		}
		/// <summary>
		/// Gets where all chunk's position are loaded.
		/// </summary>
		/// <returns>An array of chunk position</returns>
		public Vector2Int[] GetChunkMappings()
		{
			return GetChunkMappings(Vector2Int.zero);
		}
		/// <inheritdoc cref="GetChunkMappings()"/>
		/// <param name="pos">A chunk's grid position that offsets the mapping.</param>
		public Vector2Int[] GetChunkMappings(Vector2Int pos)
		{
			Vector2Int[] mappings = new Vector2Int[ChunkRenderingArea];
			for (int i = 0; i < ChunkRenderingArea; i++)
			{
				int x = i % ChunkRenderingDiameter - ChunkRenderingRadius,
				y = i / ChunkRenderingDiameter - ChunkRenderingRadius;

				mappings[i] = new Vector2Int(x, y) + pos;
			}
			return mappings;
		}
		/// <summary>
		/// Clears all the chunks that will out of range of the <see cref="Focus"/>.
		/// </summary>
		/// <param name="mappings">The chunk mappings.</param>
		/// <returns></returns>
		private List<Vector2Int> CleanOutOfRangeChunks(Vector2Int[] mappings)
		{
			var copyList = new List<Vector2Int>(_ChunkDict.Keys);

			List<Vector2Int> kept = new();

			foreach (var pos in copyList)
			{
				if (mappings.Contains(pos))
				{
					kept.Add(pos);
					continue;
				}

				UnloadChunk(pos);
			}

			return kept;
		}

		public void ClearChunks()
		{
			foreach (var pos in new List<Vector2Int>(_ChunkDict.Keys))
				UnloadChunk(pos);
		}

		[ContextMenu("Refresh Chunks")]
		public void ContextUnloadAllChunks()
		{
			Vector2Int[] mappings = GetChunkMappings(Focus.LastChunkPos);

			ClearChunks();

			foreach (Vector2Int pos in mappings)
				LoadChunk(pos);
		}

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
		public void Start()
		{
			RefreshChunks();
		}

		// Update is called once per frame
		void Update()
		{
			if (Focus.DidMove())
			{
				Focus.UpdatePosition();
				RefreshChunks();
			}
		}
	}
}