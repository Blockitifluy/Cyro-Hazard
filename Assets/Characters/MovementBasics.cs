using System;
using UnityEngine;
using CH.Character;

namespace CH.Character
{
	/// <summary>
	/// This class adds movement to an object; a secondary script is need to control the movement.
	/// The script currently only manages: moving and turning but more can be done.
	/// </summary>
	[RequireComponent(typeof(CharacterHealth))]
	public class MovementBasics : MonoBehaviour
	{
		private CharacterHealth CharacterHealth;

		/// <summary>
		/// The direction the object wants to move. 
		/// </summary>
		[HideInInspector]
		public Vector2 MovementDirection = Vector2.zero;
		/// <summary>
		/// The angle the object wants to turn.
		/// </summary>
		[HideInInspector]
		public float TurningAngle = 0;
		/// <summary>
		/// The current speed of the object
		/// </summary>
		[HideInInspector]
		private float CurrentMovementSpeed = 0;

		/// <summary>
		/// The speed of which the character turns.
		/// </summary>
		[Range(0.0f, 1.0f)]
		public float TurningSpeed = 35.0f;

		/// <summary>
		/// The top/max speed of object's movement.
		/// </summary>
		[Header("Movement")]
		public float MovementMaxSpeed = 4.5f;
		/// <summary>
		/// The acceleration of  object's movement.
		/// </summary>
		public float MovementAcceleration = 1.0f;

		/// <summary>
		/// Checks if the object is moving, based on <see cref="MovementDir"/>.
		/// </summary>
		/// <returns>If the object is moving</returns>
		public bool IsMoving() => MovementDirection != Vector2.zero;

		/// <summary>
		/// Gets the movement direction multipled by the speed (and the delta time).
		/// </summary>
		/// <returns>The movement direction with a speed magnitude.</returns>
		public Vector3 MovementOffset => CurrentMovementSpeed * Time.deltaTime * MovementDirection;

		/// <summary>
		/// Gets the speed added by the acceleration, which is capped by <see cref="MovementMaxSpeed"/>.
		/// </summary>
		/// <returns>Speed added by accelation (Also multipled the delta time).</returns>
		public float CalculateSpeed()
		{
			float uncapped = CurrentMovementSpeed + MovementAcceleration * Time.deltaTime,
			movementMult = CharacterHealth.GetOrganOperation(ECapability.Movement);
			return Mathf.Min(uncapped, MovementMaxSpeed) * movementMult;
		}

		/// <summary>
		/// Moves the object, run every frame.
		/// </summary>
		/// <param name="offset">The offset direction multipled by speed, usually from <see cref="GetMovementOffset"/>.</param>
		private void Move()
		{
			transform.position += transform.right * MovementOffset.x;
			transform.position += transform.forward * MovementOffset.y;
		}

		void Start()
		{
			CharacterHealth = GetComponent<CharacterHealth>();
		}

		// Update is called once per frame
		void Update()
		{
			if (!IsMoving())
			{
				CurrentMovementSpeed = 0;
				return;
			}

			CurrentMovementSpeed = CalculateSpeed();
			Move();
		}
	}
}