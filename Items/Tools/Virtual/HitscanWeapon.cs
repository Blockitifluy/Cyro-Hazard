using Godot;

[GlobalClass]
public partial class HitscanWeapon : WeaponTool
{
  protected virtual void HitEffect()
  {

  }

  public override void Fire()
  {
    base.Fire();

    var ray = ScreenPointToRay();

    Node3D hit = (Node3D)ray["collider"];
    if (hit == null) return;

    Vector3 hitPos = (Vector3)ray["position"];

    var (inRange, _) = IsInRange(hitPos, Range);

    if (inRange && hit is Zombie target)
      Attack(target);

    HitEffect();
  }
}