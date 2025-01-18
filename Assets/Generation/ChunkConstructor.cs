using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Generation
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
			public Vector3 LastPosition;
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

		/// <summary>
		/// A vertex used in a <see cref="ChunkTerrain"/>. Less 64 bits.
		/// </summary>
		public struct Vertex
		{
			/// <summary>
			/// The vertex's x position.
			/// </summary>
			public byte PositionX;
			/// <summary>
			/// The vertex's y position.
			/// </summary>
			public byte PositionY;

			/// <summary>
			/// The vertex's grid 2D position.
			/// </summary>
			public readonly Vector2Int GridPosition => new(PositionX, PositionY);
			/// <summary>
			/// The vertex's grid 2D position with the height.
			/// </summary>
			public readonly Vector3 Position => new(PositionX, Height, PositionY);
			/// <summary>
			/// The height of the vertex.
			/// </summary>
			public float Height;
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
		public int TilesPerAxis = 16;
		/// <summary>
		/// The physical actual size of the tiles.
		/// </summary>
		public float TileSize = .5f;
		/// <summary>
		/// The prefab used to add a chunk.
		/// </summary>
		public GameObject ChunkPrefab;
		public int ChunkRenderingRadius = 3;
		public int ChunkRenderingArea => ChunkRenderingDiameter * ChunkRenderingDiameter;
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

		private Dictionary<Vector2Int, ChunkTerrain> _ChunkDict = new();

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
		public abstract Vertex GetTileData(int x, int y, int chunkX, int chunkY);

		// Chunk Loading

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
			chunk.transform.position = worldSize;
			chunk.transform.parent = transform;

			_ChunkDict.Add(pos, terrain);

			return chunk;
		}

		public void UnloadChunk(Vector2Int pos)
		{
			ChunkTerrain terrain = _ChunkDict[pos];
			if (terrain == null)
				throw new NullReferenceException($"Chunk at {pos} wasn't found!");
			_ChunkDict.Remove(pos);
			Destroy(terrain.gameObject);
		}

		public void PlaceChunks(bool all = false)
		{
			if (!Focus.DidMove() || all) return;
			Focus.UpdatePosition();
			print(_ChunkDict.Count);

			foreach (var pos in new List<Vector2Int>(_ChunkDict.Keys))
			{ print(pos); UnloadChunk(pos); }

			for (int i = 0; i < ChunkRenderingArea; i++)
			{
				print(i);
				int x = i % ChunkRenderingDiameter - ChunkRenderingRadius,
				y = i / ChunkRenderingDiameter - ChunkRenderingRadius;

				Vector2Int pos = new(x, y);

				LoadChunk(pos + Focus.LastChunkPos);
			}
		}

		// Start is called before the first frame update
		public void Start()
		{
			//LoadChunk(Vector2Int.zero);
			//LoadChunk(Vector2Int.right);
			PlaceChunks(true);
		}

		// Update is called once per frame
		void Update()
		{
			PlaceChunks();
		}
	}
}