using System;
using System.Collections.Generic;
using UnityEngine;

//класс для отображения ноды в скриптовом объекте
public abstract class NodeBase : ScriptableObject
{
    public string NodeName;
    public Type nodeType;
    public string guid;
    public Vector2 Position;
    public Color ColorNode = new Color(0.2627451f , 0.2627451f, 0.2627451f);

#if UNITY_EDITOR
    public UnityEditor.Experimental.GraphView.Group group;
#endif
    public List<NodeBase> parentsDependencies = new List<NodeBase>();
    public List<NodeBase> childrenDependencies = new List<NodeBase>();
    public List<NodeBase> parents = new List<NodeBase>();
    public List<NodeBase> childrens = new List<NodeBase>();

}