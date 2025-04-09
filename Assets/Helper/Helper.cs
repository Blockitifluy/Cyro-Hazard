using System;
using System.Collections.Generic;
using System.Reflection;
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