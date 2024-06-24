using Godot;

public class InputPlus
{
  /// <summary>
  /// Makes a value (-1, 0 or 1) based on the <c>negInput</c> and <c>posInput</c>
  /// </summary>
  /// <param name="negInput">The input that equates to -1</param>
  /// <param name="posInput">The input that equates to 1</param>
  /// <returns>A value that is -1, 0 or 1</returns>
  public static float GetMoveAxis(string negInput, string posInput)
  {
    float final = 0;

    if (Input.IsActionPressed(negInput)) final -= 1;
    if (Input.IsActionPressed(posInput)) final += 1;

    return final;
  }

  /// <summary>
  /// Get the 3D movement direction
  /// </summary>
  /// <see cref="GetMoveAxis"/>
  /// <returns>A unitised direction vector</returns>
  public static Vector3 GetMoveDir()
  {
    float dirY = GetMoveAxis("move-forward", "move-backward"),
    dirX = GetMoveAxis("move-left", "move-right"); 

    Vector3 dir = new(dirX, 0, dirY);

    if (dir == Vector3.Zero) return dir;

    return dir.Normalized();
  }
}