#if UNITY_EDITOR
namespace Gaming.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Avatar;

    internal class ElementBuilder : EditorWindow
    {
        private Camera camera;
        private Element current;
        private string[] mountPoints;
        private Vector2 point = Vector2.zero;

        public void OnGUI()
        {
            if (mountPoints == null)
            {
                mountPoints = new[] { "None" };
            }

            if (camera == null)
            {
                camera = GameObject.Find("Main Camera").GetComponent<Camera>();
                if (camera == null)
                {
                    return;
                }

                camera.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                camera.transform.localScale = Vector3.one;
            }

            if (ElementBuilderConfig.actived != null)
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                ElementBuilderConfig.actived.skeleton = this.DrawingObject<GameObject>("Skeleton:", ElementBuilderConfig.actived.skeleton, 100, 200, 18);
                GUILayout.FlexibleSpace();
                current = (Element)this.DrawingEnumPopup("", current, 0, 200, EditorStyles.toolbarPopup);
                if (GUILayout.Button("Clear", EditorStyles.toolbarDropDown))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("All"), false, () => ElementBuilderConfig.actived.Clear(true));
                    menu.AddItem(new GUIContent("Select"), false, () => ElementBuilderConfig.actived.Clear(false));
                    menu.ShowAsContext();
                }

                if (GUILayout.Button("Buiding", EditorStyles.toolbarDropDown))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("All"), false, () => ElementBuilderConfig.actived.Builder(camera, true));
                    menu.AddItem(new GUIContent("Select"), false, () => ElementBuilderConfig.actived.Builder(camera, false));
                    menu.ShowAsContext();
                }

                GUILayout.EndHorizontal();
                if (ElementBuilderConfig.actived.skeleton == null)
                {
                    GUI.Label(new Rect(position.width / 2 - 100, position.height / 2 - 10, 200, 20), "config missing skeleton");
                    return;
                }

                OnDrawItems();
            }
        }

        private void OnDrawItems()
        {
            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(position.height - 25));
            point = EditorGUILayout.BeginScrollView(point);
            ElementItemData[] elements = ElementBuilderConfig.actived.GetElementDatas(current);
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                ElementItemData elementItemData = elements[i];
                if (elementItemData == null)
                {
                    continue;
                }

                DrawingElementData(elementItemData);
                GUILayout.Space(5);
            }

            if (Event.current.type == EventType.DragUpdated && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }

            if (Event.current.type == EventType.DragPerform && rect.Contains(Event.current.mousePosition))
            {
                GetDragElementList();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawingElementData(ElementItemData elementItemData)
        {
            Rect clickRange = EditorGUILayout.BeginHorizontal(elementItemData.isOn ? EditorStyles.selectionRect : EditorStyles.helpBox);
            {
                elementItemData.icon = this.DrawingObject<Texture2D>("", elementItemData.icon, 0);
                GUILayout.BeginVertical();
                {
                    elementItemData.fbx = this.DrawingObject<GameObject>("Fbx", elementItemData.fbx, 100, 200, 20);
                    elementItemData.element = (Element)this.DrawingEnumPopup("Element Type:", elementItemData.element);
                    this.DrawingLabel("Asset Path:", elementItemData.fbx == null ? "" : AssetDatabase.GetAssetPath(elementItemData.fbx));
                    elementItemData.isNormal = this.DrawingToggle("Is Normal", elementItemData.isNormal);
                    elementItemData.material = this.DrawingObject<Material>("material", elementItemData.material, 100, 200, 20);
                    GUILayout.EndVertical();
                }
                GUILayout.FlexibleSpace();
                if (clickRange.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    elementItemData.isOn = !elementItemData.isOn;
                    Repaint();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        void GetDragElementList()
        {
            for (int i = 0; i < DragAndDrop.paths.Length; i++)
            {
                if (!string.IsNullOrEmpty(Path.GetExtension(DragAndDrop.paths[i])))
                {
                    ElementBuilderConfig.actived.AddElementData(camera, DragAndDrop.paths[i]);
                }
                else
                {
                    string[] files = Directory.GetFiles(DragAndDrop.paths[i], "*.*", SearchOption.AllDirectories);
                    for (int j = 0; j < files.Length; j++)
                    {
                        ElementBuilderConfig.actived.AddElementData(camera, files[j]);
                    }
                }
            }

            EditorUtility.SetDirty(ElementBuilderConfig.actived);
        }
    }
}
#endif