using System;
using UnityEngine;

namespace CH.Character
{
	/// <summary>
	/// This class adds movement to an object; a secondary script is need to control the movement.
	/// The script currently only manages: moving and turning but more can be done.
	/// </summary>
	public class MovementBasics : MonoBehaviour
	{
		/// <summary>
		/// The direction the object wants to move. 
		/// </summary>
		protected Vector3 _MovementDirection = Vector3.forward;
		/// <summary>
		/// The angle the object wants to turn.
		/// </summary>
		protected float _TurningAngle = 0;
		/// <summary>
		/// The current speed of the object
		/// </summary>
		protected float _CurrentMovementSpeed = 0;

		/// <summary>
		/// <inheritdoc cref="_MovementDirection"/>
		/// </summary>
		public Vector3 MovementDir
		{
			get { return _MovementDirection; }
		}

		/// <summary>
		/// <inheritdoc cref="_CurrentMovementSpeed"/>
		/// </summary>
		public float CurrentMovementSpeed
		{
			get { return _CurrentMovementSpeed; }
		}

		/// <summary>
		/// <inheritdoc cref="_TurningAngle"/>
		/// </summary>
		public float TurningAngle
		{
			get { return _TurningAngle; }
		}

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
		public bool IsMoving()
		{
			return _MovementDirection != Vector3.zero;
		}

		/// <summary>
		/// Updates the forward direction of the object.
		/// </summary>
		/// <param name="z">A value between -1 and 1; -1 being backwards and 1 being forwards.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="z"/> isn't between -1 and 1.</exception>
		public void UpdateDir(Vector2 dir2D)
		{
			_MovementDirection = new(dir2D.x, 0, dir2D.y);
		}

		/// <summary>
		/// Gets the movement direction multipled by the speed (and the delta time).
		/// </summary>
		/// <returns>The movement direction with a speed magnitude.</returns>
		public Vector3 GetMovementOffset()
		{
			if (_MovementDirection == Vector3.zero)
				return Vector3.zero;
			return _CurrentMovementSpeed * Time.deltaTime * _MovementDirection;
		}

		/// <summary>
		/// Gets the speed added by the acceleration, which is capped by <see cref="MovementMaxSpeed"/>.
		/// </summary>
		/// <returns>Speed added by accelation (Also multipled the delta time).</returns>
		public float CalculateSpeed()
		{
			var uncapped = _CurrentMovementSpeed + MovementAcceleration * Time.deltaTime;
			return Mathf.Min(uncapped, MovementMaxSpeed);
		}

		/// <summary>
		/// Moves the object, run every frame.
		/// </summary>
		/// <param name="offset">The offset direction multipled by speed, usually from <see cref="GetMovementOffset"/>.</param>
		private void Move()
		{
			_CurrentMovementSpeed = CalculateSpeed();

			Vector3 offset = GetMovementOffset();
			transform.position += transform.right * offset.x;
			transform.position += transform.forward * offset.z;
		}

		// Update is called once per frame
		public void Update()
		{
			if (!IsMoving())
			{
				_CurrentMovementSpeed = 0;
				return;
			}

			Move();
		}
	}
}