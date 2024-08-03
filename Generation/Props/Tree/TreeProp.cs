using Godot;

[GlobalClass]
public partial class TreeProp : GenProp
{
  [Export]
  public int WoodDropAmount { get; set; } = 5;
  protected override void OnHit(float newHealth)
  {
    GD.Print(newHealth);
  }

  protected override void OnDeath()
  {
    Pickup.DropItem(GlobalPosition, Items.ItemCode.Wood, WoodDropAmount, GetTree());

    base.OnDeath();
  }
}