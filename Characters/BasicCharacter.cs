using System;
using System.Text;
using Godot;

[GlobalClass]
public abstract partial class BasicCharacter : CharacterBody3D
{
  /// <summary>
  /// The fall acceleration of character
  /// </summary>
  [Export] public int FallAcceleration { get; set; } = -10;
  /// <summary>
  /// The character's look lerp weight
  /// </summary>
  [Export(PropertyHint.Range, "0,1,")] public float LookWeight { get; set; } = 0.25f;

  /// <summary>
  /// The base walk speed
  /// </summary>
  [ExportGroup("Speed")]
  [Export] public int BaseSpeed { get; set; } = 150;
  /// <summary>
  /// The speed penality, if <see cref="Stamina"/> is under 0
  /// </summary>
  [Export(PropertyHint.Range, "0,1,")] public float SpeedPenality { get; set; } = 0.5f;
  /// <summary>
  /// The stamina walk cost
  /// </summary>
  [Export] public float RunCost { get; set; } = 3.5f;
  /// <summary>
  /// The run speed (also cost more stamina)
  /// </summary>
  [Export] public int RunSpeed { get; set; } = 200;

  /// <summary>
  /// The <see cref="Stamina"=> cap
  /// </summary>
  [ExportGroup("Stamina")]
  [Export] public float StaminaMax { get; set; } = 100;
  /// <summary>
  /// The character's stamina. If stamina is under 0, the character is exhasted
  /// </summary>
  [Export]
  public float Stamina
  {
    get => _stamina;
    set { _stamina = Mathf.Clamp(value, -StaminaLower, StaminaMax); }
  }
  /// <summary>
  /// The lower bound of the stamina. If stamina is under 0, the character is exhasted
  /// </summary>
  [Export] public float StaminaLower { get; set; } = 50;
  /// <summary>
  /// The stamina regeneration per second
  /// </summary>
  [Export] public float StaminaRegen { get; set; } = 4.0f;

  [ExportGroup("Health")]
  [Export] public float MaxHealth { get; set; } = 100.0f;
  [Export]
  public float Health
  {
    get { return _health; }
    set
    {
      if (value <= 0)
      {
        // TODO - On Death Behaviour
        GD.Print("Died!");
        OnDeath();
        return;
      }

      if (value > MaxHealth) return;

      _health = value;
    }
  }

  private float _health = 100.0f;
  private float _stamina = 0.0f;
  /// <summary>
  /// The target velocity
  /// </summary>
  protected Vector3 TargetVelocity = Vector3.Zero;
  /// <summary>
  /// The charater pivot
  /// </summary>
  protected Node3D Pivot;
  protected double _Timer = 0.0f;

  protected abstract Vector3 GetMovementDirection();

  protected abstract void OnDeath();

  /// <summary>
  /// Gets the falling speed of the character
  /// </summary>
  /// <param name="delta">The frame's delta</param>
  /// <returns>The falling speed</returns>
  protected float GetFallingSpeed(double delta)
  {
    if (IsOnFloor()) return 0.0f;

    return FallAcceleration * (float)delta;
  }

  /// <summary>
  /// Is the character running
  /// </summary>
  /// <returns>If running</returns>
  public abstract bool IsRunning();

  /// <summary>
  /// Get the character's speed
  /// </summary>
  /// <returns>The current speed</returns>
  public float GetSpeed()
  {
    float currentSpeed = IsRunning() ? RunSpeed : BaseSpeed,
    multipler = 1.0f;

    if (Stamina <= 0)
    {
      return BaseSpeed * SpeedPenality;
    }

    return currentSpeed * multipler;
  }

  /// <summary>
  /// Get the character's look direction and velocity, if the character would run.
  /// </summary>
  /// <param name="delta">The frame's delta</param>
  /// <param name="unitDir">The walk direction</param>
  /// <returns>The look and velocity</returns>
  protected (Basis, Vector3) GetRunData(double delta, Vector3 unitDir)
  {
    float currentSpeed = GetSpeed() * (float)delta;

    Vector3 walkDisplace = unitDir.Normalized() * currentSpeed;

    float staminaGen = StaminaRegen * (float)delta;
    Stamina += staminaGen;

    if (walkDisplace.X != 0 || walkDisplace.Z != 0)
    {
      Basis LastBasis = Pivot.Basis,
      LookingAt = Basis.LookingAt(new(walkDisplace.X, 0, walkDisplace.Z)),
      LookingLerp = LastBasis.Slerp(LookingAt, LookWeight);

      return (LookingLerp, walkDisplace);
    }

    return (new Basis(), new Vector3());
  }

  /// <summary>
  /// The action of the player running
  /// </summary>
  /// <param name="delta">The time distance between the previous 2 frames</param>
  /// <returns>The stamina taken when running</returns>
  public float RunAction(double delta)
  {
    Vector3 dir = GetMovementDirection();

    if (Stamina == -StaminaLower) return 0.0f;

    var (lookAt, velo) = GetRunData(delta, dir);

    TargetVelocity = velo;

    if (velo != Vector3.Zero) Pivot.Basis = lookAt;

    float runStamina = SpeedToStamina(velo.Length());

    Stamina -= runStamina;

    return runStamina;
  }

  /// <summary>
  /// Converts the speed to stamina
  /// </summary>
  /// <param name="speed">The speed in m/s</param>
  /// <returns>The stamina cost</returns>
  protected float SpeedToStamina(float speed)
  {
    if (speed == 0) return 0.0f;

    return RunCost * (speed / BaseSpeed);
  }

  public override void _Ready()
  {
    base._Ready();

    Stamina = StaminaMax;
    Pivot = GetNode<Node3D>("Pivot");
  }

  public override void _PhysicsProcess(double delta)
  {
    base._PhysicsProcess(delta);

    Velocity = TargetVelocity - Vector3.Down * FallAcceleration;
    MoveAndSlide();
  }

  public override void _Process(double delta)
  {
    base._Process(delta);

    float staminaGen = StaminaRegen * (float)delta;
    Stamina += staminaGen;

    _Timer += delta;
  }
}