using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class Player : BasicCharacter
{
  /// <summary>
  /// The time it takes to pickup an item
  /// </summary>
  [Export] public double PickupTime = 0.5d;
  /// <summary>
  /// The max distance to pickup the pickup item
  /// </summary>
  [Export] public float MaxPickDistance = 10.0f;

  private Marker3D Grip;
  private Marker3D CameraPivot;

  public override bool IsRunning()
  {
    return Input.IsActionPressed("running");
  }

  public Inventory inventory = new(new Vector2I(10, 10), 3);
  public Inventory.Hotbar Hotbar { get { return inventory.hotbar; } }

  /// <summary>
  /// The action of the player running
  /// </summary>
  /// <param name="delta">The time distance between the previous 2 frames</param>
  /// <returns>The stamina taken when running</returns>
  public float RunAction(double delta)
  {
    Vector3 cameraRot = CameraPivot.Rotation;

    Vector2 unit2D = Input.GetVector("move-left", "move-right", "move-forward", "move-backward");
    Vector3 unit = new Vector3(unit2D.X, 0, unit2D.Y)
    .Rotated(cameraRot.Normalized(), cameraRot.Length())
    .Normalized();

    if (Stamina == -StaminaLower) return 0.0f;

    var (lookAt, velo) = Run(delta, unit);

    targetVelo = velo;

    if (velo != Vector3.Zero) Pivot.Basis = lookAt;

    return SpeedToStamina(velo.Length());
  }

  /// <summary>
  /// Check if the user requested a slot change
  /// </summary>
  /// <returns>The slot index. Returns <c>-1</c> if no request has been made during the frame.</returns>
  public int SlotInput()
  {
    int slotAmount = Hotbar.Slots.Capacity;

    for (int i = 0; i < slotAmount; i++)
    {
      bool isPressed = Input.IsActionJustPressed($"equip-{i + 1}");
      if (isPressed) return i;
    }

    return -1; // -1 means it has not been changed
  }

  public void EquipAction()
  {
    int slotIndex = SlotInput();
    if (slotIndex == -1) return;

    Hotbar.UnequipCurrent();

    try
    {
      BaseTool tool = Hotbar.GetToolFromHotbar(slotIndex);
      Grip.AddChild(tool);
    }
    catch (NullReferenceException) { }
    catch (ArgumentOutOfRangeException) { }
  }

  /// <summary>
  /// Gets the closest and in-range pickup (Can return null)
  /// </summary>
  /// <returns>The Closest pickup</returns>
  private Pickup GetClosestPickup()
  {
    var Pickups = GetTree().GetNodesInGroup("Pickups");

    Vector3 MousePos3D = ScreenPointToRay();

    Pickup ClosestPickup = null;
    float closestDistance = float.MaxValue;
    for (int index = 0; index < Pickups.Count; index++)
    {
      Pickup pck;
      try
      {
        pck = (Pickup)Pickups[index];
      }
      catch (InvalidCastException err)
      {
        GD.PrintErr(err);
        continue;
      }

      float currentDist = MousePos3D.DistanceSquaredTo(pck.Position),
      distFromPlayer = Position.DistanceSquaredTo(pck.Position);

      bool inRange = distFromPlayer <= MaxPickDistance * MaxPickDistance,
      isClosest = closestDistance > currentDist;

      if (inRange && isClosest)
      {
        closestDistance = currentDist;
        ClosestPickup = pck;
      }
    }

    return ClosestPickup;
  }

  private double PickupTimer = 0.0d;
  private bool PickupStarted = false;

  private bool AddPickupToInventory(Pickup pickup)
  {
    Items.ItemData baseItem = Items.CodeToItem(pickup.ItemCode);

    try
    {
      Vector2I at = inventory.FindSpaceFor(baseItem.Size);
      if (at == Vector2I.MinValue) return false;

      inventory.AddItem(pickup.ItemCode, at, pickup.Amount);
      return true;
    }
    catch (ArgumentOutOfRangeException exception)
    {
      GD.PrintErr($"{exception.ParamName} was out of range");
    }
    catch (Inventory.DoesNotFitException)
    {
      GD.Print($"[b][color=PURPLE]Item[/color][/b] No space found for {baseItem}");
    }

    return false;
  }

  /// <summary>
  /// Uses press and hold as it's action. Gets closest pickup, then destroys it.
  /// </summary>
  /// <param name="delta">The time between the previous 2 frames</param>
  private void PickupAction(double delta)
  {
    if (Input.IsActionJustPressed("pickup"))
      PickupStarted = true;

    if (Input.IsActionPressed("pickup") && PickupStarted)
      PickupTimer += delta;


    if (PickupTimer >= PickupTime && PickupStarted)
    {
      PickupStarted = false;
      PickupTimer = 0;

      // Actual Pickup Logic
      var closestPickup = GetClosestPickup();
      if (closestPickup != null)
      {
        GD.PrintRich($"[b][color=PURPLE]Item[/color][/b] Picked up {closestPickup.Item}");

        bool successful = AddPickupToInventory(closestPickup);

        if (successful)
          closestPickup?.QueueFree();
      }
    }

    if (Input.IsActionJustReleased("pickup"))
    {
      if (PickupTimer < PickupTime && PickupStarted)
        GD.PrintRich("[b][color=GREY]Input[/color][/b] Released Pickup Action");

      PickupStarted = false;
      PickupTimer = 0;
    }
  }

  /// <summary>
  /// Calculates the 3D mouse position 
  /// </summary>
  /// <returns>The mouse 3D position</returns>
  public Vector3 ScreenPointToRay()
  {
    var spaceState = GetWorld3D().DirectSpaceState;

    Vector2 MousePos = GetViewport().GetMousePosition();
    Camera3D camera = GetTree().Root.GetCamera3D();

    Vector3 rayOrigin = camera.ProjectRayOrigin(MousePos),
    rayEnd = camera.ProjectRayNormal(MousePos) * 2000;

    var Params = new PhysicsRayQueryParameters3D
    {
      From = rayOrigin,
      To = rayEnd,
      Exclude = new Godot.Collections.Array<Rid>() { GetRid() },
      HitFromInside = false
    };

    var rayArray = spaceState.IntersectRay(Params);

    if (rayArray.ContainsKey("position"))
      return (Vector3)rayArray["position"];

    return new Vector3();
  }

  public override void _Ready()
  {
    base._Ready();

    Grip = GetNode<Marker3D>("Pivot/Grip");
    CameraPivot = (Marker3D)GetTree().GetFirstNodeInGroup("CameraPivot");

    Inventory.InventoryItem axe = inventory.AddItem(Items.ItemCode.Axe, new(0, 0), 1);
    Hotbar.AddToFromHotbar(axe, 0);

    Inventory.InventoryItem shovel = inventory.AddItem(Items.ItemCode.Shovel, new(2, 0), 1);
    Hotbar.AddToFromHotbar(shovel, 1);
  }

  public override void _PhysicsProcess(double delta)
  {
    base._PhysicsProcess(delta);

    PickupAction(delta);

    // Running
    float runStamina = RunAction(delta),
    staminaGen = StaminaRegen * (float)delta;

    Stamina -= runStamina;
    Stamina += staminaGen;

    Velocity = targetVelo - Vector3.Down * FallAcceleration;
    MoveAndSlide();

    PickupTimer += delta;

    EquipAction();

    if (Input.IsActionJustPressed("unequip"))
      Hotbar.UnequipCurrent();
  }
}
