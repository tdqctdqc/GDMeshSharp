using Godot;
using System;

public class QuadTreeNode<T>
{
    public int Level { get; set; }
    public bool HasElement { get; set; }
    public bool IsLeaf { get; set; }
    public Rectangle Bounds { get; set; }
    public int ID { get; set; }
    public bool HasParent { get; set; }
    public QuadTreeNode<T> Parent { get; set; }
    public int ChildCount { get; set; }
    public int FirstChildID { get; set; }
}
