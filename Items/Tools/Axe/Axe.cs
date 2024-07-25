using Godot;

[GlobalClass]
public partial class Axe : TerrainEditorTool
{
  private void HitTree(TreeProp hit)
  {
    hit.Health -= Damage;

    GD.Print(hit);
  }

  public override void Fire()
  {
    base.Fire();

    var ray = ScreenPointToRay();

    Node3D hit = (Node3D)ray["collider"];
    if (hit == null) return;

    Vector3 hitPos = (Vector3)ray["position"];

    var (inRange, _) = IsInRange(hitPos, Range);
    if (!inRange) return;

    if (hit is TreeProp tree)
    {
      HitTree(tree);
    }
  }
}