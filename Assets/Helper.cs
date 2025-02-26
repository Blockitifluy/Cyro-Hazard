using System;
using System.Xml;
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

	public static string GetNodeText(this XmlElement element, string path)
	{
		return element.SelectSingleNode(path).InnerText;
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

	public static RectTransform GetRectTransform(this GameObject gameObject)
	{
		return gameObject.GetComponent<RectTransform>();
	}

	public static RectTransform GetRectTransform(this Component component)
	{
		return component.gameObject.GetRectTransform();
	}

	public static Vector2Int Round(this Vector2 vector)
	{
		return new(
			Mathf.RoundToInt(vector.x),
			Mathf.RoundToInt(vector.y)
		);
	}

	public static Vector2Int Floor(this Vector2 vector)
	{
		return new(
			Mathf.FloorToInt(vector.x),
			Mathf.FloorToInt(vector.y)
		);
	}

	public static Vector2 Min(this Vector2 vector, Vector2 other)
	{
		return new(
			Mathf.Min(vector.x, other.x),
			Mathf.Min(vector.y, other.y)
		);
	}

	public static Vector2 Max(this Vector2 vector, Vector2 other)
	{
		return new(
			Mathf.Max(vector.x, other.x),
			Mathf.Max(vector.y, other.y)
		);
	}

	public static Vector2Int Min(this Vector2Int vector, Vector2Int other)
	{
		return new(
			Mathf.Min(vector.x, other.x),
			Mathf.Min(vector.y, other.y)
		);
	}

	public static Vector2Int Max(this Vector2Int vector, Vector2Int other)
	{
		return new(
			Mathf.Max(vector.x, other.x),
			Mathf.Max(vector.y, other.y)
		);
	}
}