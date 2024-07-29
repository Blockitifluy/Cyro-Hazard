using Godot;

[GlobalClass]
public partial class RangedWeapon : HitscanWeapon
{
  [Export]
  public int MaxAmmo { get; set; } = 20;

  private int _ammo = 20;
  [Export]
  public int Ammo
  {
    get { return _ammo; }
    set { if (_ammo >= 0) _ammo = value; }
  }

  protected override bool ShouldStopFiring()
  {
    return _ammo <= 0;
  }

  protected override void HitEffect()
  {
    _ammo -= 1;
  }
}