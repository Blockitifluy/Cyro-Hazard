using Godot;

public static class GDExtensions
{
  /// <summary>
  /// Gets a node child by type.
  /// </summary>
  /// <typeparam name="T">Has to be a Node. he type to be searched</typeparam>
  /// <param name="node"></param>
  /// <param name="recursive">Look into the children's children, ect.</param>
  /// <returns>The child node</returns>
  public static T GetChildByType<T>(this Node node, bool recursive = true)
      where T : Node
  {
    int childCount = node.GetChildCount();

    for (int i = 0; i < childCount; i++)
    {
      Node child = node.GetChild(i);
      if (child is T childT)
        return childT;

      if (recursive && child.GetChildCount() > 0)
      {
        T recursiveResult = child.GetChildByType<T>(true);
        if (recursiveResult != null)
          return recursiveResult;
      }
    }

    return null;
  }

  /// <summary>
  /// Gets a node's parent by type.
  /// </summary>
  /// <typeparam name="T">Has to be a Node. he type to be searched</typeparam>
  /// <param name="node"></param>
  /// <returns>The child node</returns>
  public static T GetParentByType<T>(this Node node)
      where T : Node
  {
    Node parent = node.GetParent();
    if (parent != null)
    {
      if (parent is T parentT)
      {
        return parentT;
      }
      else
      {
        return parent.GetParentByType<T>();
      }
    }

    return null;
  }
}