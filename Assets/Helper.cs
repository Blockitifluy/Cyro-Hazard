using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO - Split Helper into multiple classes

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

	public static IEnumerable<Type> GetTypesWithAttribute<TAttribute>(Assembly assembly) where TAttribute : Attribute
	{
		foreach (Type type in assembly.GetTypes())
		{
			if (type.GetCustomAttributes(typeof(TAttribute), true).Length == 0)
				continue;
			yield return type;
		}
	}

	public static TAttribute GetCustomAttribute<TAttribute>(this MemberInfo memberInfo) where TAttribute : Attribute
	{
		return (TAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(TAttribute));
	}

	public static Texture2D ToTexture2D(this RenderTexture rTex)
	{
		Texture2D tex = new(rTex.width, rTex.height, TextureFormat.RGB24, false);
		var old_rt = RenderTexture.active;
		RenderTexture.active = rTex;

		tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
		tex.Apply();

		RenderTexture.active = old_rt;
		return tex;
	}

	public static float Mean(params float[] x)
	{
		float sum = 0;

		foreach (float a in x)
			sum += a;

		return sum / x.Length;
	}
}