using Godot;

[GlobalClass]
public abstract partial class WeaponTool : BaseTool
{
  /// <summary>
  /// Lists out all the available firing types.
  /// </summary>
  public enum FireTypes : uint
  {
    /// <summary>
    /// Single firing tools like pistols, rifles and most melees.
    /// </summary>
    Single = 0,
    /// <summary>
    /// Fires repeated until command to stop, used in chainsaws, machine guns, ARs.
    /// </summary>
    Auto = 1,
    /// <summary>
    /// Burst weapons fire at intervals and stopping on command or a counter has been reached.
    /// <br />
    /// This firing mode has two unqiue properties: <see cref="BurstAmount"/> and <see cref="BurstPeriod"/>
    /// </summary>
    Burst = 2,
  }

  protected const string Fire1 = "fire-1";

  [Export]
  /// <summary>
  /// The damage that the hitscan / projectile deals, can be multipled based on factors like headshots.
  /// </summary>
  public float Damage { get; set; } = 16;
  [Export]
  /// <summary>
  /// The range of the weapon, on most ranged weapons it is unlimited.
  /// </summary>
  public float Range { get; set; } = float.MaxValue;

  [ExportGroup("Fire Rate")]
  [Export]
  /// <summary>
  /// The firing type of weapon, such as:
  /// <list type="ol"><item>Single</item> <item>Auto</item> <item>Burst</item></list>
  /// </summary>
  public FireTypes FiringMode { get; set; } = FireTypes.Single;
  [Export]
  /// <summary>
  /// The fire rate of the weapon, rounds per minute.  
  /// </summary>
  public double FireRate { get; set; } = 32.0d;

  [ExportGroup("Burst")]
  [Export]
  /// <summary>
  /// The maximum of burst count of an <see cref="FireTypes.Burst"/> weapon.
  /// </summary>
  public int BurstAmount { get; set; } = 3;
  [Export]
  /// <summary>
  /// The burst cooldown of an <see cref="FireTypes.Burst"/> weapon.
  /// </summary>
  public double BurstPeriod { get; set; } = .25d;

  private double _Timer;
  private double _LastFire;

  /// <summary>
  /// Is the weapon actively shotting.
  /// </summary>
  private bool _IsActive = false;

  /// <summary>
  /// Calculates the 3D mouse position 
  /// </summary>
  /// <returns>The mouse 3D position</returns>
  protected Godot.Collections.Dictionary ScreenPointToRay()
  {
    var spaceState = GetWorld3D().DirectSpaceState;

    Vector2 MousePos = GetViewport().GetMousePosition();
    Camera3D camera = GetTree().Root.GetCamera3D();

    Vector3 rayOrigin = camera.ProjectRayOrigin(MousePos),
    rayEnd = camera.ProjectRayNormal(MousePos) * 2000;

    BasicCharacter user = GetUser();

    Godot.Collections.Array<Rid> exclude = new() { user.GetRid() };

    var Params = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd, exclude: exclude);

    var rayArray = spaceState.IntersectRay(Params);

    return rayArray;
  }

  /// <summary>
  /// If an position is in range of the user.
  /// </summary>
  /// <param name="to">The position.</param>
  /// <param name="range">The tool's range</param>
  /// <returns>(bool) Is it in range, (float) Distance -- As a tuple</returns>
  protected (bool, float) IsInRange(Vector3 to, float range)
  {
    BasicCharacter user = GetUser();

    float dist = user.Position.DistanceTo(to);
    bool isInRange = dist < range;

    return (isInRange, dist);
  }

  /// <summary>
  /// Handles automatic firing.
  /// </summary>
  private void AutoHandler()
  {
    if (Input.IsActionJustReleased(Fire1) || ShouldStopFiring())
    {
      _IsActive = false;
      return;
    }

    if (CanFire())
      Fire();
  }

  /// <summary>
  /// The current burst count.
  /// </summary>
  private int _BurstCount = 0
  ;
  /// <summary>
  /// Handles burst firing.
  /// </summary>
  private void BurstHandler()
  {
    if (Input.IsActionJustReleased(Fire1))
    {
      _IsActive = false;
      return;
    }

    if (BurstAmount <= _BurstCount)
    {
      _IsActive = false;
      _BurstCount = 0;
      return;
    }

    if (_Timer - _LastFire > BurstPeriod)
      Fire();
  }

  protected virtual bool ShouldStopFiring()
  {
    return false;
  }

  protected virtual bool CanFire()
  {
    return _Timer - _LastFire > 60.0d / FireRate;
  }

  /// <summary>
  /// Picks the method on which on how to fire based on the <see cref="FiringMode"/>.
  /// </summary>
  private void FireHandle()
  {
    switch (FiringMode)
    {
      case FireTypes.Single:
        Fire();
        _IsActive = false;
        break;
      case FireTypes.Burst:
        BurstHandler();
        break;
      case FireTypes.Auto:
        AutoHandler();
        break;
    }
  }

  public override void _Process(double delta)
  {
    base._Process(delta);

    if (Input.IsActionJustPressed(Fire1) && CanFire())
      _IsActive = true;

    if (_IsActive)
      FireHandle();

    _Timer += delta;
  }

  public virtual void Fire()
  {
    GD.Print($"Fired Weapon {Name}");
    _LastFire = _Timer;
    _BurstCount++;
  }
}