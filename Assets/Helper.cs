using UnityEngine;
using UnityEngine.InputSystem;

public static class Helper
{
  static public RaycastHit? GetMouseRayHitInfo(Camera camera)
  {
    Vector2 mousePos = Mouse.current.position.ReadValue();
    Ray ray = camera.ScreenPointToRay(mousePos);

    if (Physics.Raycast(ray, out RaycastHit hit))
    {
      return hit;
    }

    return null;
  }

  static public bool IsBitSet(ulong b, int pos)
  {
    return (b & (1UL << pos)) != 0UL;
  }
}