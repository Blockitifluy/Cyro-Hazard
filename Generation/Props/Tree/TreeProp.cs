using Godot;

[GlobalClass]
public partial class TreeProp : StaticBody3D
{
  [Export]
  public float MaxHealth = 35;

  private float _health = 35;
  [Export]
  public float Health
  {
    get { return _health; }
    set
    {
      if (value > MaxHealth) return;

      if (_health > value) OnTreeHit(value);

      _health = value;
    }
  }

  private void OnTreeHit(float health)
  {
    if (health <= 0)
    {
      QueueFree();
    }
  }
}