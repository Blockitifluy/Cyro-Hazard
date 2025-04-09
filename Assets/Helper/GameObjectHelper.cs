using UnityEngine;

public static class GameObjectHelper
{
	/// <summary>
	/// Gets the rect transform of game object.
	/// </summary>
	/// <param name="gameObject">The game object a rect transform is attached to.</param>
	/// <returns>RectTransform</returns>
	public static RectTransform GetRectTransform(this GameObject gameObject)
	{
		return gameObject.GetComponent<RectTransform>();
	}

	/// <inheritdoc cref="GetRectTransform(GameObject)"/>
	/// <param name="component"> The component a rect transform is attached to.</param>
	public static RectTransform GetRectTransform(this Component component)
	{
		return component.gameObject.GetRectTransform();
	}
}