using Godot;

[GlobalClass]
public abstract partial class GenProp : StaticBody3D
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

      if (_health <= 0) OnDeath();

      if (_health > value) OnHit(value);

      _health = value;
    }
  }

  protected abstract void OnHit(float newHealth);

  protected virtual void OnDeath()
  {
    if (_health <= 0)
      QueueFree();
  }
}