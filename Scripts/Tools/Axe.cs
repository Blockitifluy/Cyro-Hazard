using Godot;

[GlobalClass]
public partial class Axe : BaseTool
{
  // TODO

  protected override void Equip()
  {
    base.Equip();

    GD.Print("Equiped Axe");
  }

  public override void Unequip()
  {
    base.Unequip();

    GD.Print("Unequiped Axe");
  }
}