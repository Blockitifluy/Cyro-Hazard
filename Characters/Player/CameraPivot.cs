using Godot;

public partial class CameraPivot : Marker3D
{
	/// <summary>
	/// The subject the camera follows
	/// </summary>
	[Export]
	public Node3D Follower;

	[Export]
	public float Angle = 180;
	[Export]
	public float TurnPower = 10.0f;

	/// <summary>
	/// The <c>a</c> in the quadatic equation: <c>ax^2+ bx</c>
	/// </summary>
	[Export]
	[ExportCategory("Quadatrics")]
	public float QuadA = 1.0f;

	/// <summary>
	/// The <c>b</c> in the quadatic equation: <c>ax^2+ bx</c>
	/// </summary>
	[Export] public float QuadB = 1.0f;

	private Camera3D Camera;

	public override void _Ready()
	{
		base._Ready();

		Camera = GetNode<Camera3D>("Camera");
	}

	public Vector3 LerpPosition(double delta)
	{
		Vector3 followPos = Follower.Position;
		Vector3 currentPos = Position;

		float dist = followPos.DistanceTo(currentPos);

		float distSquared = dist * dist;
		// ax^2 + bx
		float cameraLag = (QuadA * distSquared + QuadB * dist) * (float)delta;

		Vector3 posLerp = currentPos + (followPos - currentPos) * cameraLag;

		return posLerp;
	}

	private void TurnPivotFromUser()
	{
		float turnPower = Input.GetAxis("camera-left", "camera-right");

		float toRad = Mathf.DegToRad(TurnPower);

		RotateY(turnPower * toRad);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Follower == null)
		{
			GD.PrintErr("Follower doesn't Exist");
			return;
		};

		TurnPivotFromUser();

		var posLerp = LerpPosition(delta);

		Position = posLerp;
	}
}
