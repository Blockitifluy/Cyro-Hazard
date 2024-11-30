using UnityEngine;

[RequireComponent(typeof(MovementBasics))]
public abstract class CharacterControl : MonoBehaviour
{
  public MovementBasics movementBasics;

  // Update is called once per frame
  protected abstract void Update();
}