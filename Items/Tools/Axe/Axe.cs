using Godot;

[GlobalClass]
public partial class Axe : WeaponTool
{
  public override float Damage => 15.0f;
  public override float Range => 5;

  public override double FireRate => 60.0d;
  public override FireTypes FiringMode => FireTypes.Single;

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