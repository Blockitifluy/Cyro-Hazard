using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementBasics))]
public abstract class CharacterControl : MonoBehaviour
{
  public MovementBasics MovementBasics;

  public List<Backpack> DetectBackpacks()
  {
    var allBackpacks = GameObject.FindGameObjectsWithTag("Backpack");
    List<Backpack> actualPacks = new();

    foreach (GameObject obj in allBackpacks)
    {
      if (gameObject.transform.IsChildOf(obj.transform)) continue;

      if (!obj.TryGetComponent<Backpack>(out var backpack))
      {
        Debug.LogWarning($"Even though {obj.name} has tag backpack, it's doesn't have the Backpack component");
        continue;
      }

      actualPacks.Add(backpack);
    }

    return actualPacks;
  }

  // Update is called once per frame
  protected abstract void Update();
}