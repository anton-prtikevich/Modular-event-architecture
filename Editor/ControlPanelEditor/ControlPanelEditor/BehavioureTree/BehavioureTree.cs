using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//класс для сохранения данных о дереве в скриптовый объект
[CreateAssetMenu()]
public class BehavioureTree : ScriptableObject
{
    public NodeBase rootNode;
    public List<NodeBase> nodes = new List<NodeBase>();

#if UNITY_EDITOR
    public List<UnityEditor.Experimental.GraphView.Group> groups = new List<UnityEditor.Experimental.GraphView.Group>();
#endif

    public void SaveAssets()
    {
        // Undo.RegisterCreatedObjectUndo(node, "Behavior Tree (CreateNode)");
        #if UNITY_EDITOR
        // сохранить изменения
        AssetDatabase.SaveAssets();
        #endif
    }
    
    
    //записать данные о созданной ноде в скриптовый объект
    public NodeBase CreateNode(System.Type type)
    {
        //создать ноду
        NodeBase node = ScriptableObject.CreateInstance(type) as NodeBase;
        
        //присвоить ей имя
        node.name = type.Name;
        
        #if UNITY_EDITOR
        // присвоить ей уникальный идентификатор
        node.guid = GUID.Generate().ToString();
        // Undo.RecordObject(this, "Behavior Tree (CreateNode)");
        
        // добавить ее в список нод
        nodes.Add(node);

        // добавить ее в корень скриптового объекта
        AssetDatabase.AddObjectToAsset(node, this);
        #endif

        // Undo.RegisterCreatedObjectUndo(node, "Behavior Tree (CreateNode)");
        // AssetDatabase.SaveAssets();

        return node;
    }

    // удалить ноду из скриптового объекта
    public void DeleteNode(NodeBase node)
    {
        #if UNITY_EDITOR
        Undo.RecordObject(this, "Behavior Tree (DeleteNode)");

        // удалить ноду из списка нод
        nodes.Remove(node);

        // удалить ноду из корня скриптового объекта
        AssetDatabase.RemoveObjectFromAsset(node);

        Undo.DestroyObjectImmediate(node);
        
        // сохранить изменения
        AssetDatabase.SaveAssets();
        #endif
    }

    //добавить дочернюю ноду
    public void AddChild(NodeBase parent, NodeBase child)
    {
        #if UNITY_EDITOR
        Undo.RecordObject(parent, "Behavior Tree (AddChild)");
        #endif
        parent.childrens.Add(child);

        child.parents.Add(parent);
    }

    //добавить зависимость между нодами
    public void AddDependencie(NodeBase parent, NodeBase child)
    {
        #if UNITY_EDITOR
        Undo.RecordObject(parent, "Behavior Tree (AddChild)");
        #endif

        parent.childrenDependencies.Add(child);

        child.parentsDependencies.Add(parent);
    }

    public void RemoveChild(NodeBase parent, NodeBase child)
    {
        #if UNITY_EDITOR
        Undo.RecordObject(parent, "Behavior Tree (RemoveChild)");
        #endif

        parent.childrens.Remove(child);

        child.parents.Remove(parent);
    }
    public void RemoveDependencie(NodeBase parent, NodeBase child)
    {
        #if UNITY_EDITOR
        Undo.RecordObject(parent, "Behavior Tree (RemoveChild)");
        #endif
        parent.childrenDependencies.Remove(child);

        child.parentsDependencies.Remove(parent);
    }

    public List<NodeBase> GetChildren(NodeBase parent) => parent.childrenDependencies;
    public List<NodeBase> GetDerivatives(NodeBase parent) => parent.childrens;

}