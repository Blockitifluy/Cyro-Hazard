using System;
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

	public static bool IsBitSet(ulong b, int pos)
	{
		return (b & (1UL << pos)) != 0UL;
	}

	public static void SetValue<T>(this T sender, string propertyName, object value)
	{
		var propertyInfo = sender.GetType().GetProperty(propertyName);

		if (propertyInfo is null) return;

		var type = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

		if (propertyInfo.PropertyType.IsEnum)
		{
			propertyInfo.SetValue(sender, Enum.Parse(propertyInfo.PropertyType, value.ToString()!));
		}
		else
		{
			var safeValue = (value == null) ? null : Convert.ChangeType(value, type);
			propertyInfo.SetValue(sender, safeValue, null);
		}
	}

}