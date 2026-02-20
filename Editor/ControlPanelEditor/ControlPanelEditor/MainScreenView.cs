using System;
using System.Collections.Generic;
using MyEditor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


// namespace MyScripts.Architecture.Editor.ControlPanel.ControlPanelEditor
// {
public class MainScreenView : GraphView
    {
        //событие выбора ноды
        public Action<WindowNodeView> OnNodeSelected;
        public new class UxmlFactory : UxmlFactory<MainScreenView, GraphView.UxmlTraits> { }
        public BehavioureTree treeModel;

        public MainScreenView()
        {
            // добавить бэкграунд
            Insert(0, new GridBackground());

            // Add a minimap
            // Add(new MiniMap { anchored = true });

            // добавить группы нод
            // AddElement(new Group { title = "Group", autoUpdateGeometry = true }); 

            var zoomer = new ContentZoomer();
            zoomer.minScale = 0.1f;  // Минимальный масштаб 10%
            zoomer.maxScale = 2.0f;  // Максимальный масштаб 300%
            zoomer.scaleStep = 0.1f; // Шаг масштабирования

            // добавить манипуляторы
            this.AddManipulator(zoomer);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());
            
            //подключить файл стилей
            var config = ControlPanelEditorConfig.Instance;
            if (config != null && config.behaviourTreeEditorStyle != null)
            {
                styleSheets.Add(config.behaviourTreeEditorStyle);
            }
            else
            {
                Debug.LogWarning("ControlPanelEditorConfig: behaviourTreeEditorStyle не назначен!");
            }

            Undo.undoRedoPerformed += OnUndoRedo;

            // Отписаться при удалении элемента
            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                Undo.undoRedoPerformed -= OnUndoRedo;
            });
        }

        // метод для проверки отмены действия
        private void OnUndoRedo()
        {
            PopulateView(treeModel);

            AssetDatabase.SaveAssets();
        }

        // Количество нод, создаваемых за один кадр
        private const int BATCH_SIZE = 10;
        private IVisualElementScheduledItem _populateTask;

        //перезаполнить дерево каждый раз когда мы его выделяем или открываем
        internal void PopulateView(BehavioureTree tree)
        {
            this.treeModel = tree;

            // Отменить предыдущую загрузку, если она ещё идёт
            _populateTask?.Pause();
            _populateTask = null;
            
            //отписаться от события изменения графа для того чтобы не вызывалось при удалении нод
            graphViewChanged -= OnGraphViewChanged;
        
            //удалить все элементы
            DeleteElements(graphElements);
            
            graphViewChanged += OnGraphViewChanged;

            if (tree.nodes.Count == 0) return;

            // Создаём ноды батчами, чтобы не блокировать UI
            int currentIndex = 0;
            _populateTask = schedule.Execute(() =>
            {
                int end = Math.Min(currentIndex + BATCH_SIZE, tree.nodes.Count);
                for (int i = currentIndex; i < end; i++)
                {
                    CreateNodeView(tree.nodes[i]);
                }
                currentIndex = end;

                // Когда все ноды созданы — остановить и центрировать камеру
                if (currentIndex >= tree.nodes.Count)
                {
                    _populateTask?.Pause();
                    _populateTask = null;

                    //создать связи между нодами(ребра)
                    // tree.nodes.ForEach(node => СreateСonnections(tree, node));

                    // Центрировать камеру на всех нодах
                    schedule.Execute(() => FrameAll());
                }
            }).Every(16); // ~60fps, каждый кадр создаём батч
        }

        // метод сохранить дерево
        private void SaveTree(BehavioureTree tree)
        {
            // сохранить изменения иначе данные удалятся после перезагрузки
            EditorUtility.SetDirty(tree);
                
            // сохранить изменения скриптбл обджектов
            AssetDatabase.SaveAssets();
        }
        
        private void СreateСonnections(BehavioureTree tree, MyEditor.NodeBase node)
        {
            //получить детей ноды
            var children = tree.GetChildren(node);
            
            //пройтись по всем детям
            children.ForEach(child =>
            {
                //получить ноды для соединения ребра
                NodeView parentView = FindNodeView(node);

                NodeView childView = FindNodeView(child);


                Edge edge = parentView.outputPort.ConnectTo(childView.inputPort);

                AddElement(edge);
            });
            
            //получить детей ноды
            var derivatives = tree.GetDerivatives(node);
            
            //пройтись по всем детям
            derivatives.ForEach(child =>
            {
                //получить ноды для соединения ребра
                NodeView parentView = FindNodeView(node);

                NodeView childView = FindNodeView(child);

                Edge edge = parentView.childPort.ConnectTo(childView.ParentPort);

                AddElement(edge);
            });
        }


        //получить порт ноды по имени порта 
        private NodeView FindNodeView(MyEditor.NodeBase node) => GetNodeByGuid(node.guid) as NodeView;


        //метод изменения графа для того чтобы удалять ноды из модели дерева
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            // Debug.Log("OnGraphViewChanged");
            //если есть элементы для удаления
            if(graphViewChange.elementsToRemove != null)
            {
                //пройтись по всем элементам
                graphViewChange.elementsToRemove.ForEach(element =>
                {
                    //получить ноду
                    WindowNodeView nodeView = element as WindowNodeView;
                    
                    //если нода не пустая
                    if(nodeView != null)
                    {
                        //удалить ноду из модели дерева 
                        treeModel.DeleteNode(nodeView.node);
                    }

                    // Edge edge = element as Edge;

                    // if(edge != null)
                    // {
                    //     //получить ноды
                        // NodeView childView = edge.input.node as NodeView;
                    //     NodeView parentView = edge.output.node as NodeView;

                    //     //если ноды не пустые
                    //     if(childView != null && parentView != null)
                    //     {
                    //         if(edge.input.portName == "предки" && edge.output.portName == "дочерние")
                    //         {
                    //             treeModel.RemoveChild(parentView.node, childView.node);
                    //         }
                    //         else
                    //         {
                    //             //удалить связь между нодами
                    //             treeModel.RemoveDependencie(parentView.node, childView.node);
                    //         }
                    //     }
                    // }

                });
            }

            // if(graphViewChange.edgesToCreate != null)
            // {
            //     graphViewChange.edgesToCreate.ForEach(edge =>
            //     {
            //         //получить ноды
            //         NodeView childView = edge.input.node as NodeView;
            //         NodeView parentView = edge.output.node as NodeView;
                    
            //         //если ноды не пустые
            //         if(childView != null && parentView != null)
            //         {
            //             if(edge.input.portName == "предки" && edge.output.portName == "дочерние")
            //             {
            //                 treeModel.AddChild(parentView.node, childView.node);
            //             }
            //             else
            //             {
            //                 //добавить связь между нодами
            //                 treeModel.AddDependencie(parentView.node, childView.node);
            //             }
            //         }
            //     });
            // }

            //вернуть изменения
            return graphViewChange;
        }

        //переопределить метод создания контекстного меню
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            // Пересчитать позицию мыши в координаты графа (с учётом зума и панорамирования)
            Vector2 graphMousePos = contentViewContainer.WorldToLocal(
                this.LocalToWorld(evt.localMousePosition));

            //получить все типы наследуемые от NodeBase
            var types = TypeCache.GetTypesDerivedFrom<MyEditor.NodeBase>();

            foreach (var type in types)
            {
                // Добавление действия в контекстное меню
                evt.menu.AppendAction($"[{type.BaseType.Name}] {type.Name}", 
                    (a) => CreateNode(type, graphMousePos)); 
            }

            evt.menu.AppendAction($"Создать группу", (a) => CreteGroup("новая группа"));
        }

        public Group CreteGroup(string title)
        {
            Group group = new Group();

            treeModel.groups.Add(group);

            group.title = title;

            return group;
        }

        //создать ноду
        public MyEditor.NodeBase CreateNode(System.Type type, Vector2 position = default)
        {
            MyEditor.NodeBase node = treeModel.CreateNode(type);
            node.Position = position;

            CreateNodeView(node);
            return node;
        }
        

        //переопределить метод создания ноды
        public void CreateNodeView(MyEditor.NodeBase node)
        {
            // NodeView nodeView = new NodeView(node);
            WindowNodeView nodeView = new WindowNodeView(node, this);

            nodeView.OnNodeSelected = OnNodeSelected;

            AddElement(nodeView);
            
            // if(node.group != null)
            // {
            //     node.group.AddElement(nodeView);

            //     AddElement(node.group);
            // }
        }
    }
// }