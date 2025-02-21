using UnityEngine;
using UnityEngine.EventSystems;
using System;

[DisallowMultipleComponent]
[AddComponentMenu("UI/ClickableObject")]
public class ClickableObject : MonoBehaviour, IPointerClickHandler
{
    public event EventHandler<PointerEventData> OnClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(this, eventData);
    }
}
