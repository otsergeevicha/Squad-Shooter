using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    [CustomEditor(typeof(IAPSettings))]
    public class IAPSettingsEditor : WatermelonEditor
    {
        private readonly Vector2 ENUM_WINDOW_SIZE = new Vector2(300, 320);
        private const string ENUM_WINDOW_TITLE = "[IAP Manager]: Product Type";
        private const string TYPES_FILE_NAME = "ProductKeyType";
        private const string FILE_PATH_SUFFIX = "\\ProductKeyType.cs";
        

        private SerializedProperty storeItemsProperty;
        private SerializedProperty selectedProperty;

        private GUIContent addButton;
        private GUIContent arrowDownContent;
        private GUIContent arrowUpContent;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            //obtain store items
            storeItemsProperty = serializedObject.FindProperty("storeItems");

            ForceInitStyles();
        }

        protected override void Styles()
        {
            addButton = new GUIContent("", EditorStylesExtended.GetTexture("icon_add", EditorStylesExtended.IconColor));

            arrowDownContent = new GUIContent(EditorStylesExtended.GetTexture("icon_arrow_down", new Color(0.2f, 0.2f, 0.2f)));
            arrowUpContent = new GUIContent(EditorStylesExtended.GetTexture("icon_arrow_up", new Color(0.2f, 0.2f, 0.2f)));
        }

        public override void OnInspectorGUI()
        {
            InitStyles();

            Rect panelRect = EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

            EditorGUILayoutCustom.Header("PRODUCTS");

            if (storeItemsProperty.arraySize > 0)
            {
#if !MODULE_IAP
                EditorGUILayoutCustom.HelpBox("IAP define isn't enabled!", "Enable", delegate
                {
                    DefineManager.EnableDefine("MODULE_IAP");
                });
#endif

                SerializedProperty tempSerializedProperty;
                string tempTitle = "";
                Rect itemRect;
                for (int i = 0; i < storeItemsProperty.arraySize; i++)
                {
                    itemRect = EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box, GUILayout.Height(16));

                    tempSerializedProperty = storeItemsProperty.GetArrayElementAtIndex(i);
                    tempTitle = tempSerializedProperty.FindPropertyRelative("id").stringValue;

                    Rect rect = EditorGUILayout.BeginHorizontal();

                    GUILayout.Space(5);
                    EditorGUILayout.LabelField(!string.IsNullOrEmpty(tempTitle) ? tempTitle : "draft", GUILayout.ExpandWidth(true));
                    EditorGUILayout.LabelField(tempSerializedProperty.isExpanded ? arrowUpContent : arrowDownContent, GUILayout.Width(16), GUILayout.Height(16));

                    GUILayout.Space(8);

                    EditorGUILayout.EndHorizontal();

                    if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                    {
                        tempSerializedProperty.isExpanded = !tempSerializedProperty.isExpanded;
                    }

                    if (tempSerializedProperty.isExpanded)
                    {
                        EditorGUILayout.PropertyField(tempSerializedProperty.FindPropertyRelative("id"), new GUIContent("ID"));
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(tempSerializedProperty.FindPropertyRelative("productKeyType"));
                        if (GUILayout.Button(addButton, GUILayout.Width(18), GUILayout.Height(18)))
                        {
                            OpenTypesWindow();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(tempSerializedProperty.FindPropertyRelative("productType"));

                        serializedObject.ApplyModifiedProperties();

                        GUILayout.Space(2);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Delete", GUILayout.Width(80)))
                        {
                            if (EditorUtility.DisplayDialog("Remove", "Are you sure you want to remove this item?", "Remove", "Cancel"))
                            {
                                DeleteItem(i);

                                GUIUtility.ExitGUI();

                                Event.current.Use();

                                return;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Products list is empty!", MessageType.Info);
            }

            EditorGUILayout.EndVertical();

            // Buttons panel
            GUILayout.Space(20);

            Rect buttonsPanelRect = new Rect(panelRect.x + panelRect.width - 40, panelRect.y + panelRect.height, 30, 20);
            Rect addButtonRect = new Rect(buttonsPanelRect.x + 5, buttonsPanelRect.y, 20, 20);

            GUI.Box(buttonsPanelRect, "", EditorStylesExtended.panelBottom);
            GUI.Label(addButtonRect, addButton, EditorStylesExtended.labelCentered);

            if (GUI.Button(buttonsPanelRect, GUIContent.none, GUIStyle.none))
            {
                AddNewItem();

                GUIUtility.ExitGUI();

                return;
            }

            GUILayout.FlexibleSpace();
        }

        private void OpenTypesWindow()
        {
            IAPTypesWindow window = (IAPTypesWindow)EditorWindow.GetWindow(typeof(IAPTypesWindow), true);
            window.titleContent = new GUIContent(ENUM_WINDOW_TITLE);
            window.minSize = ENUM_WINDOW_SIZE;
            window.maxSize = ENUM_WINDOW_SIZE;
            window.ShowUtility();
        }

        private void DeleteItem(int index)
        {
            storeItemsProperty.RemoveFromVariableArrayAt(index);

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateItemSelection(int i)
        {
            selectedProperty = storeItemsProperty.GetArrayElementAtIndex(i);
            selectedProperty.isExpanded = true;
        }

        private void AddNewItem()
        {
            int newItemIndex = storeItemsProperty.arraySize;

            storeItemsProperty.arraySize++;

            SerializedProperty newSerializedProperty = storeItemsProperty.GetArrayElementAtIndex(newItemIndex);
            newSerializedProperty.FindPropertyRelative("id").stringValue = "";
            newSerializedProperty.FindPropertyRelative("productType").enumValueIndex = 0;

            serializedObject.ApplyModifiedProperties();

            UpdateItemSelection(storeItemsProperty.arraySize - 1);
        }

        private class IAPTypesWindow : WatermelonWindow
        {
            private List<EnumData> enumDataList;

            private string newTypeName;
            private Rect itemTypeRect;
            private string itemTypeNewName;
            private int selectedItemType;

            private string filePath;

            private Vector2 scrollView = Vector2.zero;

            protected override void OnEnable()
            {
                base.OnEnable();

                //get enumData
                enumDataList = new List<EnumData>();
                int[] values = (int[])System.Enum.GetValues(typeof(ProductKeyType));
                string[] names = System.Enum.GetNames(typeof(ProductKeyType));

                for (int i = 0; i < values.Length; i++)
                {
                    enumDataList.Add(new EnumData(values[i], names[i]));
                }

                // finding path for enum generation         
                MonoScript iapTypesScript = EditorUtils.GetAssetByName<MonoScript>(TYPES_FILE_NAME);
                filePath = EditorUtils.projectFolderPath + AssetDatabase.GetAssetPath(iapTypesScript);

                selectedItemType = -1;
                newTypeName = "";
                itemTypeNewName = "";
            }

            private void OnGUI()
            {
                EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);
                EditorGUILayout.BeginHorizontal();

                newTypeName = EditorGUILayout.TextField(newTypeName);

                bool uniqueName = enumDataList.FindIndex(x => x.name == newTypeName) == -1;

                if (GUILayout.Button("Add") && newTypeName != "" && uniqueName)
                {
                    enumDataList.Add(new EnumData(enumDataList.Count, newTypeName));

                    RegenerateEnum();

                    newTypeName = "";
                    GUI.FocusControl(null);
                }

                EditorGUILayout.EndHorizontal();

                if (!uniqueName)
                {
                    EditorGUILayout.HelpBox("Product type name should be unique", MessageType.Warning);
                }
                EditorGUILayout.EndVertical();

                scrollView = EditorGUILayoutCustom.BeginScrollView(scrollView);
                EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                //Draw existing items
                if(enumDataList.Count > 0)
                {
                    for (int i = 0; i < enumDataList.Count; i++)
                    {
                        itemTypeRect = EditorGUILayout.BeginVertical(EditorStylesExtended.editorSkin.box);

                        if (i != selectedItemType)
                        {
                            EditorGUILayout.LabelField(enumDataList[i].name);
                        }
                        else
                        {
                            itemTypeNewName = EditorGUILayout.TextField(itemTypeNewName);

                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("Update name", GUILayout.Width(100)))
                            {
                                enumDataList[i].name = itemTypeNewName;
                                RegenerateEnum();
                            }

                            if (GUILayout.Button("Delete", GUILayout.Width(70)))
                            {
                                if (EditorUtility.DisplayDialog("Remove this item?", "Are you sure you want to remove " + enumDataList[i].name + " item?", "Remove", "Cancel"))
                                {
                                    enumDataList.RemoveAt(i);
                                    RegenerateEnum();

                                    GUI.FocusControl(null);
                                    selectedItemType = -1;
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        if (GUI.Button(itemTypeRect, GUIContent.none, GUIStyle.none))
                        {
                            if (i == selectedItemType)
                            {
                                selectedItemType = -1;
                            }
                            else
                            {
                                itemTypeNewName = enumDataList[i].name;
                                selectedItemType = i;
                            }
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Enum is empty!", MessageType.Info);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }

            private void RegenerateEnum()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("namespace Watermelon");
                sb.AppendLine("{");
                sb.AppendLine("    public enum ProductKeyType");
                sb.AppendLine("    {");
                for (int i = 0; i < enumDataList.Count; i++)
                {
                    sb.AppendLine("        " + enumDataList[i].name + " = " + enumDataList[i].value + ",");
                }
                sb.AppendLine("    }");
                sb.AppendLine("}");

                File.WriteAllText(filePath, sb.ToString(), System.Text.Encoding.UTF8);

                AssetDatabase.Refresh();
            }

            private class EnumData
            {
                public int value;
                public string name;

                public EnumData(int value, string name)
                {
                    this.value = value;
                    this.name = name;
                }
            }
        }
    }
}

// -----------------
// IAP Manager v 1.1
// -----------------