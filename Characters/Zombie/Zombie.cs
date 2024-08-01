using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class Zombie : BasicCharacter
{
  [ExportGroup("Wander")]
  [Export] public float WanderRange { get; set; }

  [ExportGroup("Search")]
  [Export] public float SearchRadius { get; set; }
  [Export] public double SearchCacheLength { get; set; }

  [ExportGroup("Attack")]
  [Export] public float AttackDistance { get; set; } = 2.5f;
  [Export] public float AttackRange { get; set; }
  [Export] public float AttackDamage { get; set; } = 45.0f;
  [Export] public double AttackRate { get; set; } = 45.0d;

  protected override void OnDeath()
  {
    QueueFree();
  }

  private Vector3 _ToMove = Vector3.Inf;
  private double _LastAttack = 0.0d;

  private bool IsSearchValid(BasicCharacter target)
  {
    // TODO - Add Raycasting
    if (target == null)
      return false;

    float distance = target.Position.DistanceSquaredTo(Position);
    bool inRange = distance < SearchRadius * SearchRadius;

    return inRange;
  }

  private void Wander()
  {
    float x = GD.Randf(),
    z = GD.Randf();

    Vector3 dir = new Vector3(x, 0, z).Normalized();
    float distance = GD.Randf() * WanderRange;

    _ToMove = Position + (dir * distance);

    CurrentState = State.Wandering;
  }

  private void Attack(BasicCharacter enemy)
  {
    double attackCooldown = 60.0d / AttackRate;

    if (_Timer - _LastAttack >= attackCooldown)
      return;

    enemy.Health -= AttackDamage;
    _LastAttack = _Timer;

    GD.Print("Zombie attacked Target!");
  }

  private double _LastSearchCache = double.MinValue;
  private double _SearchCacheDelay;
  private List<BasicCharacter> _SearchCache = new();

  private List<BasicCharacter> SearchForEnemies()
  {
    double searchDif = _Timer - (_LastSearchCache + _SearchCacheDelay);
    if (searchDif < SearchCacheLength)
      return _SearchCache;

    List<BasicCharacter> enemies = new();

    Player player = GetTree().Root.GetChildByType<Player>();

    if (!IsSearchValid(player))
      return new();

    enemies.Add(player);
    _SearchCache = enemies;
    _LastSearchCache = _Timer;
    return enemies;
  }

  protected override Vector3 GetMovementDirection()
  {
    if (_ToMove == Vector3.Inf)
      return Vector3.Zero;

    Vector3 dir = Position.DirectionTo(_ToMove);


    return dir;
  }

  private void Chase(BasicCharacter enemy, float distance)
  {
    Vector3 targetDir = Position.DirectionTo(enemy.Position);
    float spaceBetweenTarget = Mathf.Min(distance, AttackDistance);

    if (distance > AttackDistance)
      CurrentState = State.Chasing;

    _ToMove = enemy.Position - (targetDir * spaceBetweenTarget);
  }

  public override bool IsRunning()
  {
    return false; // TODO
  }

  public override void _Ready()
  {
    base._Ready();

    _SearchCacheDelay = GD.RandRange(0.0d, 15.0d);
  }

  public override void _Process(double delta)
  {
    base._Process(delta);

    List<BasicCharacter> enemySearch = SearchForEnemies();
    bool hasFoundEnemies = enemySearch.Count != 0;

    if (hasFoundEnemies)
    {
      var enemy = enemySearch[0];

      float distance = enemy.Position.DistanceTo(Position);

      Chase(enemy, distance);

      if (distance <= AttackRange)
        Attack(enemy);
    }
    else Wander();

    RunAction(delta);
  }
}