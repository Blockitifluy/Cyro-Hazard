using Godot;

[GlobalClass]
public partial class HitscanWeapon : WeaponTool
{
  public new virtual bool Fire()
  {
    base.Fire();

    var ray = ScreenPointToRay();

    Node3D hit = (Node3D)ray["collider"];
    if (hit == null) return false;

    Vector3 hitPos = (Vector3)ray["position"];

    var (inRange, _) = IsInRange(hitPos, Range);

    if (inRange)
    {
      GD.Print(hit);
      return true;
    };

    return false;
  }
}