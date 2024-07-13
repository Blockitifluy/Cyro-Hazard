using Godot;

[GlobalClass]
public partial class Axe : Tool
{
  // TODO

  protected override void Equip()
  {
    base.Equip();

    GD.Print("Equiped Axe");
  }

  public override void Unequip()
  {
    GD.Print("Unequiped Axe");

    base.Unequip();
  }
}