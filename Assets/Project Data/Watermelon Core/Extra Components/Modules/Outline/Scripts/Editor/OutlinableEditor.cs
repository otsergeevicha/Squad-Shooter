using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Watermelon.Outline
{
    [CustomEditor(typeof(Outlinable))]
    public class OutlinableEditor : WatermelonEditor
    {
        //default fields
        private const string OUTLINE_LAYER_PROPERTY_PATH = "outlineLayer";
        private const string COMPLEX_MASKING_MODE_PROPERTY_PATH =  "complexMaskingMode";
        private const string DRAWING_MODE_PROPERTY_PATH = "drawingMode";
        private const string RENDER_STYLE_PROPERTY_PATH = "renderStyle";
        SerializedProperty outlineLayerProperty;
        SerializedProperty complexMaskingModeProperty;
        SerializedProperty drawingModeProperty;
        SerializedProperty renderStyleProperty;

        //OutlineProperty
        private const string OUTLINE_PROPERTIES_PROPERTY_PATH = "defaultOutlineProperty";
        private const string BACK_PROPERTIES_PROPERTY_PATH = "backOutlineProperty";
        private const string FRONT_PROPERTIES_PROPERTY_PATH = "frontOutlineProperty";
        private const string ENABLED_PROPERTY_PATH = "enabled";
        private const string COLOR_PROPERTY_PATH = "color";
        private const string DILATE_SHIFT_PROPERTY_PATH = "dilateShift";
        private const string BLUR_SHIFT_PROPERTY_PATH = "blurShift";
        private const string FILL_PASS_PROPERTY_PATH = "fillPass";
        private const string DEFAULT_LABEL = "Default properties :";
        private const string FRONT_LABEL = "Front properties :";
        private const string BACK_LABEL = "Back properties :";
        private const string OUTLINE_PROPERTY_WARNING = "Changing dilate and blur settings will enable info buffer which will increase draw calls and will have some performance impact. Use Outliner settings if you don't need per object settings.";
        private const string FILL_PASS_LABEL = "Fill parameters";
        SerializedProperty defaultOutlineProperty;
        SerializedProperty backOutlineProperty;
        SerializedProperty frontOutlineProperty;
        SerializedProperty enabledProperty;
        SerializedProperty colorProperty;
        SerializedProperty dilateShiftProperty;
        SerializedProperty blurShiftProperty;
        SerializedProperty fillPassProperty;

        //OutlineTargets
        private const string OUTLINE_TARGETS_PROPERTY_PATH = "outlineTargets";
        private const string RENDERER_PROPERTY_PATH = "renderer";
        private const string CUTOUT_TEXTURE_NAME_PROPERTY_PATH = "cutoutTextureName";
        private const string CUTOUT_TEXTURE_INDEX_PROPERTY_PATH = "cutoutTextureIndex";
        private const string BOUNDS_MODE_PROPERTY_PATH = "boundsMode";
        private const string BOUNDS_PROPERTY_PATH = "bounds";
        private const string CULL_MODE_PROPERTY_PATH = "cullMode";
        private const string SUBMESH_INDEX_PROPERTY_PATH = "submeshIndex";
        private const string RENDERERS_INSTRUCTION = "All renderers that will be included to outline rendering should be in the list below.";
        private const string RENDERERS_LABEL = "Renderers :";
        private const string NONE = "none";
        private const string BRAKET_RIGHT = "\"";
        private const string BRAKET_LEFT = " \"";
        private const string CUTOUT_SOURCE_LABEL = "Cutout source";
        private const string DILATE_RENDERING_MODE_PROPERTY_PATH = "dilateRenderingMode";
        private const string EDGE_DILATE_AMOUNT_PROPERTY_PATH = "edgeDilateAmount";
        private const string FRONT_EDGE_DILATE_AMOUNT_PROPERTY_PATH = "frontEdgeDilateAmount";
        private const string BACK_EDGE_DILATE_AMOUNT_PROPERTY_PATH = "backEdgeDilateAmount";
        private const string CUTOUT_MASK_PROPERTY_PATH = "CutoutMask";
        private const string CUTOUT_THRESHOLD_PROPERTY_PATH = "CutoutThreshold";
        private const string DIALOG_TITLE = "Warning";
        private const string DIALOG_CONTENT = "\"Optimize mesh data\" option is enabled in build settings. In order the feature to work it should be disabled. It might seems to work in editor but will not work in build if the setting is enabled.\nDisable \"optimize mesh data\" option?";
        private const string DIALOG_YES = "Yes";
        private const string DIALOG_CANCEL = "Cancel";
        private const string DEFAULT_EDGE_DILATE_LABEL = "Edge Dilate";
        private const string FRONT_EDGE_DILATE_LABEL = "Front Edge Dilate";
        private const string BACK_EDGE_DILATE_LABEL = "Back Edge Dilate";
        private const string ADD_ALL_LABEL = "Add all renderers";
        private const string ADD_BASIC_LABEL = "Add basic renderers";
        private const string ADD_SPECIFIC_LABEL = "Add specific renderer";
        private const string MOVE_UP_LABEL = "Move renderer up";
        private const string MOVE_DOWN_LABEL = "Move renderer down";
        private const string REMOVE_LABEL = "Remove renderer";
        SerializedProperty outlineTargetsProperty;
        SerializedProperty boundsModeProperty;
        SerializedProperty dilateRenderingModeProperty;
        private Rect allocatedRect;
        private Rect fieldRect;
        private Rect labelRect;
        private bool cutoutIsUsed;
        private Renderer renderer;
        private bool isRendererExist;
        private Renderer[] childRenderers;

        //SerializedPass
        private const string SHADER_PROPERTY_PATH = "shader";
        private const string SHADER_FILTER = "t:Shader";
        private const string SERIALIZED_PROPERTIES_PROPERTY_PATH = "serializedProperties";
        private const string PROPERTY_NAME_PROPERTY_PATH = "PropertyName";
        private const string PROPERTY_PROPERTY_PATH = "Property";
        private const string COLOR_VALUE_PROPERTY_PATH = "ColorValue";
        private const string VECTOR_VALUE_PROPERTY_PATH = "VectorValue";
        private const string FLOAT_VALUE_PROPERTY_PATH = "FloatValue";
        private const string PROPERTY_TYPE_PROPERTY_PATH = "PropertyType";
        private string fieldValue;
        private bool shaderExist;
        private string assetPath;
        private SerializedProperty shaderProperty;
        private UnityEngine.Shader shader;
        private Dictionary<string, SerializedProperty> propertiesDictionary;
        private SerializedProperty serializedPropertiesProperty;
        private SerializedProperty elementProperty;
        private GUIContent content;
        private SerializedProperty menuFillPassProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            outlineLayerProperty = serializedObject.FindProperty(OUTLINE_LAYER_PROPERTY_PATH);
            complexMaskingModeProperty = serializedObject.FindProperty(COMPLEX_MASKING_MODE_PROPERTY_PATH);
            drawingModeProperty = serializedObject.FindProperty(DRAWING_MODE_PROPERTY_PATH);
            renderStyleProperty = serializedObject.FindProperty(RENDER_STYLE_PROPERTY_PATH);

            defaultOutlineProperty = serializedObject.FindProperty(OUTLINE_PROPERTIES_PROPERTY_PATH);
            backOutlineProperty = serializedObject.FindProperty(BACK_PROPERTIES_PROPERTY_PATH);
            frontOutlineProperty = serializedObject.FindProperty(FRONT_PROPERTIES_PROPERTY_PATH);

            outlineTargetsProperty = serializedObject.FindProperty(OUTLINE_TARGETS_PROPERTY_PATH);

            propertiesDictionary = new Dictionary<string, SerializedProperty>();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(outlineLayerProperty);
            EditorGUILayout.PropertyField(complexMaskingModeProperty);
            EditorGUILayout.PropertyField(drawingModeProperty);
            EditorGUILayout.PropertyField(renderStyleProperty);

            if (((OutlinableDrawingMode)drawingModeProperty.intValue).HasFlag(OutlinableDrawingMode.Normal))
            {
                if(renderStyleProperty.enumValueIndex == 0)
                {
                    DrawOutlineProperties(defaultOutlineProperty, DEFAULT_LABEL);
                }
                else
                {
                    DrawOutlineProperties(frontOutlineProperty, FRONT_LABEL);
                    DrawOutlineProperties(backOutlineProperty, BACK_LABEL);
                }
            }

            EditorGUILayout.HelpBox(RENDERERS_INSTRUCTION, MessageType.Info);
            EditorGUILayout.LabelField(RENDERERS_LABEL, EditorStylesExtended.label_medium);

            for (int i = 0; i < outlineTargetsProperty.arraySize; i++)
            {
                if(outlineTargetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(RENDERER_PROPERTY_PATH).objectReferenceValue == null)
                {
                    outlineTargetsProperty.DeleteArrayElementAtIndex(i);
                    i--;
                    continue;
                }

                DrawOutlineTarget(outlineTargetsProperty.GetArrayElementAtIndex(i), i);
            }

            if (GUILayout.Button(ADD_ALL_LABEL))
            {
                Renderer[] childRenderers = ((Outlinable)target).GetComponentsInChildren<Renderer>();

                for (int i = 0; i < childRenderers.Length; i++)
                {
                    for (int j = 0; j < GetSubmeshCount(childRenderers[i]); j++)
                    {
                        AddOutlineTarget(childRenderers[i], j);
                    }
                }

            }

            if (GUILayout.Button(ADD_BASIC_LABEL))
            {
                Renderer[] childRenderers = ((Outlinable)target).GetComponentsInChildren<Renderer>();

                for (int i = 0; i < childRenderers.Length; i++)
                {
                    if ((childRenderers[i] is MeshRenderer) || (childRenderers[i] is SkinnedMeshRenderer))
                    {
                        for (int j = 0; j < GetSubmeshCount(childRenderers[i]); j++)
                        {
                            AddOutlineTarget(childRenderers[i], j);
                        }
                    }
                }
            }

            if (GUILayout.Button(ADD_SPECIFIC_LABEL))
            {
                childRenderers = ((Outlinable)target).GetComponentsInChildren<Renderer>();
                GenericMenu menu = new GenericMenu();
                string path;
                Transform targetTransform = ((Outlinable)target).transform;
                Transform parentTransform;

                for (int i = 0; i < childRenderers.Length; i++)
                {
                    path = string.Empty;

                    if (childRenderers[i].transform != targetTransform)
                    {
                        parentTransform = childRenderers[i].transform;
                        do
                        {
                            path = string.Format("{0}/{1}", parentTransform.gameObject.name, path);
                            parentTransform = parentTransform.transform.parent;
                        }
                        while (parentTransform != targetTransform);

                        path = string.Format("{0}/{1}", parentTransform.gameObject.name, path);
                        path = path.Substring(0, path.Length - 1);
                    }
                    else
                    {
                        path = childRenderers[i].GetType().ToString().Substring(12);
                    }

                    for (int j = 0; j < GetSubmeshCount(childRenderers[i]); j++)
                    {
                        menu.AddItem(new GUIContent($"{path} {childRenderers[i].GetType().ToString().Substring(12)} submeshIndex: {j}"), IsUsed(childRenderers[i]), AddFunction, new Vector2Int(i,j));
                    }
                }

                menu.ShowAsContext();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AddFunction(object userdata)
        {
            Vector2Int data = (Vector2Int)userdata;
            AddOutlineTarget(childRenderers[data.x], data.y);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOutlineProperties(SerializedProperty outlineParametersProperty, string label)
        {
            enabledProperty = outlineParametersProperty.FindPropertyRelative(ENABLED_PROPERTY_PATH);
            colorProperty = outlineParametersProperty.FindPropertyRelative(COLOR_PROPERTY_PATH);
            dilateShiftProperty = outlineParametersProperty.FindPropertyRelative(DILATE_SHIFT_PROPERTY_PATH);
            blurShiftProperty = outlineParametersProperty.FindPropertyRelative(BLUR_SHIFT_PROPERTY_PATH);
            fillPassProperty = outlineParametersProperty.FindPropertyRelative(FILL_PASS_PROPERTY_PATH);

            EditorGUILayout.LabelField(label, EditorStylesExtended.label_medium);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(enabledProperty);
            EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);

            EditorGUILayout.PropertyField(colorProperty);
            EditorGUILayout.PropertyField(dilateShiftProperty);
            EditorGUILayout.PropertyField(blurShiftProperty);

            //we go throught this all because fillPassProperty isn`t displayed properly othervise
            allocatedRect = GUILayoutUtility.GetRect(Screen.width, EditorGUIUtility.singleLineHeight, GUI.skin.textField);
            labelRect = new Rect(allocatedRect);
            fieldRect = new Rect(allocatedRect);
            labelRect.xMax += EditorGUIUtility.labelWidth;
            fieldRect.xMin += EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(labelRect, FILL_PASS_LABEL);
            HandleFillPassProperty(fieldRect, fillPassProperty);

            if (!(Mathf.Approximately(dilateShiftProperty.floatValue, 1.0f) && Mathf.Approximately(blurShiftProperty.floatValue, 1.0f)))
            {
                EditorGUILayout.HelpBox(OUTLINE_PROPERTY_WARNING, MessageType.Warning);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }

        private void HandleFillPassProperty(Rect fieldRect, SerializedProperty fillPassProperty)
        {
            shaderProperty = fillPassProperty.FindPropertyRelative(SHADER_PROPERTY_PATH);
            shader = shaderProperty.objectReferenceValue as UnityEngine.Shader;
            fieldValue = NONE;
            shaderExist = false;

            if(shader != null)
            {
                fieldValue = shader.name;
                shaderExist = true;
                serializedPropertiesProperty = fillPassProperty.FindPropertyRelative(SERIALIZED_PROPERTIES_PROPERTY_PATH);
                EditorGUI.indentLevel++;

                for (int i = 0; i < serializedPropertiesProperty.arraySize; i++)
                {
                    elementProperty = serializedPropertiesProperty.GetArrayElementAtIndex(i);
                    name = elementProperty.FindPropertyRelative(PROPERTY_NAME_PROPERTY_PATH).stringValue;
                    elementProperty = elementProperty.FindPropertyRelative(PROPERTY_PROPERTY_PATH);

                    switch ((ShaderUtil.ShaderPropertyType)elementProperty.FindPropertyRelative(PROPERTY_TYPE_PROPERTY_PATH).intValue)
                    {
                        case ShaderUtil.ShaderPropertyType.Color:
                            EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative(COLOR_VALUE_PROPERTY_PATH), new GUIContent(name));
                            break;
                        case ShaderUtil.ShaderPropertyType.Vector:
                            EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative(VECTOR_VALUE_PROPERTY_PATH), new GUIContent(name));
                            break;
                        case ShaderUtil.ShaderPropertyType.Float:
                        case ShaderUtil.ShaderPropertyType.Range:
                            EditorGUILayout.PropertyField(elementProperty.FindPropertyRelative(FLOAT_VALUE_PROPERTY_PATH), new GUIContent(name));
                            break;
                        default:
                            break;
                    }

                }

                EditorGUI.indentLevel--;
            }

            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(fieldValue), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();

                menuFillPassProperty = fillPassProperty.Copy();

                menu.AddItem(new GUIContent(NONE), shader == null , ResetShader);
                string[] shadersUID = AssetDatabase.FindAssets(SHADER_FILTER);
                PathCombiner pathCombiner = new PathCombiner();

                for (int i = 0; i < shadersUID.Length; i++)
                {
                    assetPath = AssetDatabase.GUIDToAssetPath(shadersUID[i]);
                    pathCombiner.AddElement(assetPath);
                }

                KeyValuePair<string,string>[] mofiedAssetPaths = pathCombiner.GetResult();

                for (int i = 0; i < mofiedAssetPaths.Length; i++)
                {
                    menu.AddItem(new GUIContent(mofiedAssetPaths[i].Value), shaderExist && mofiedAssetPaths[i].Key.Equals(AssetDatabase.GetAssetPath(shader)), SetShaderInMenu, mofiedAssetPaths[i].Key);
                }

                menu.ShowAsContext();
            }
        }

        private void ResetShader()
        {
            menuFillPassProperty.FindPropertyRelative(SHADER_PROPERTY_PATH).objectReferenceValue = null;
            menuFillPassProperty.FindPropertyRelative(SERIALIZED_PROPERTIES_PROPERTY_PATH).arraySize = 0;
            serializedObject.ApplyModifiedProperties();
        }

        private void SetShaderInMenu(object userdata)
        {
            UnityEngine.Shader shader = AssetDatabase.LoadAssetAtPath((string)userdata, typeof(UnityEngine.Shader)) as UnityEngine.Shader;
            menuFillPassProperty.FindPropertyRelative(SHADER_PROPERTY_PATH).objectReferenceValue = shader;
            serializedPropertiesProperty = menuFillPassProperty.FindPropertyRelative(SERIALIZED_PROPERTIES_PROPERTY_PATH);
            serializedPropertiesProperty.arraySize = 0;

            for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
            {
                name = ShaderUtil.GetPropertyName(shader, i);

                //Maybe add some name filter here

                serializedPropertiesProperty.arraySize++;
                elementProperty = serializedPropertiesProperty.GetArrayElementAtIndex(serializedPropertiesProperty.arraySize - 1);
                elementProperty.FindPropertyRelative(PROPERTY_NAME_PROPERTY_PATH).stringValue = name;
                elementProperty = elementProperty.FindPropertyRelative(PROPERTY_PROPERTY_PATH);
                elementProperty.FindPropertyRelative(PROPERTY_TYPE_PROPERTY_PATH).intValue = (int)ShaderUtil.GetPropertyType(shader, i);
                Material material = new Material(shader);

                switch (ShaderUtil.GetPropertyType(shader, i))
                {
                    case ShaderUtil.ShaderPropertyType.Color:
                        elementProperty.FindPropertyRelative(COLOR_VALUE_PROPERTY_PATH).colorValue = material.GetColor(name);
                        break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                        elementProperty.FindPropertyRelative(VECTOR_VALUE_PROPERTY_PATH).vector4Value = material.GetVector(name);
                        break;
                    case ShaderUtil.ShaderPropertyType.Float:
                        elementProperty.FindPropertyRelative(FLOAT_VALUE_PROPERTY_PATH).floatValue = material.GetFloat(name);
                        break;
                    case ShaderUtil.ShaderPropertyType.Range:
                        elementProperty.FindPropertyRelative(FLOAT_VALUE_PROPERTY_PATH).floatValue = material.GetFloat(name);
                        break;
                    default:
                        break;
                }

                GameObject.DestroyImmediate(material);
                propertiesDictionary.Add(name, elementProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOutlineTarget(SerializedProperty outlineTargetProperty, int index)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(RENDERER_PROPERTY_PATH));
            DrawCutoutSource(outlineTargetProperty);
            DrawBoundsModeProperty(outlineTargetProperty);
            EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(CUTOUT_TEXTURE_INDEX_PROPERTY_PATH));
            DrawDilateRenderingMode(outlineTargetProperty);
            EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(CULL_MODE_PROPERTY_PATH));
            EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(SUBMESH_INDEX_PROPERTY_PATH));
            DrawMenu(outlineTargetProperty, index);
            EditorGUILayout.EndVertical();
        }



        private void DrawCutoutSource(SerializedProperty outlineTargetProperty)
        {
            GenericMenu menu = new GenericMenu();
            SerializedProperty cutoutTextureNameProperty = outlineTargetProperty.FindPropertyRelative(CUTOUT_TEXTURE_NAME_PROPERTY_PATH);

            cutoutIsUsed = (cutoutTextureNameProperty.stringValue.Length != 0);

            menu.AddItem(new GUIContent(NONE), !cutoutIsUsed, () =>
            {
                cutoutTextureNameProperty.stringValue = string.Empty;
                serializedObject.ApplyModifiedProperties();
            });

            string reference = NONE;
            renderer = outlineTargetProperty.FindPropertyRelative(RENDERER_PROPERTY_PATH).objectReferenceValue as Renderer;

            isRendererExist = (renderer != null);

            if (isRendererExist)
            {
                Material material = renderer.sharedMaterial;

                if (material != null)
                {
                    for (int i = 0; i < ShaderUtil.GetPropertyCount(material.shader); i++)
                    {
                        if (ShaderUtil.GetPropertyType(material.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                        {
                            string shaderPropertyName = ShaderUtil.GetPropertyName(material.shader, i);

                            if (shaderPropertyName.Equals(cutoutTextureNameProperty.stringValue))
                            {
                                reference = ShaderUtil.GetPropertyDescription(material.shader, i) + BRAKET_LEFT + shaderPropertyName + BRAKET_RIGHT;
                                menu.AddItem(new GUIContent(reference), true, delegate { });
                            }
                            else
                            {
                                menu.AddItem(new GUIContent(ShaderUtil.GetPropertyDescription(material.shader, i) + BRAKET_LEFT + shaderPropertyName + BRAKET_RIGHT), false, () =>
                                {
                                    cutoutTextureNameProperty.stringValue = shaderPropertyName;
                                    serializedObject.ApplyModifiedProperties();
                                });
                            }
                        }
                    }
                }
            }

            allocatedRect = GUILayoutUtility.GetRect(Screen.width, EditorGUIUtility.singleLineHeight, GUI.skin.textField);
            labelRect = new Rect(allocatedRect);
            fieldRect = new Rect(allocatedRect);
            labelRect.xMax += EditorGUIUtility.labelWidth;
            fieldRect.xMin += EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(labelRect, CUTOUT_SOURCE_LABEL);

            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(reference), FocusType.Passive))
            {
                menu.ShowAsContext();
            }
        }

        private void DrawBoundsModeProperty(SerializedProperty outlineTargetProperty)
        {
            boundsModeProperty = outlineTargetProperty.FindPropertyRelative(BOUNDS_MODE_PROPERTY_PATH);
            EditorGUILayout.PropertyField(boundsModeProperty);

            if ((BoundsMode)boundsModeProperty.intValue == BoundsMode.Manual)
            {
                EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(BOUNDS_PROPERTY_PATH));
            }
        }

        private void DrawDilateRenderingMode(SerializedProperty outlineTargetProperty)
        {
            if ((cutoutIsUsed && isRendererExist) || (renderer is SpriteRenderer))
            {
                EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(CUTOUT_MASK_PROPERTY_PATH));
                EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(CUTOUT_THRESHOLD_PROPERTY_PATH));
            }
            else
            {
                if (renderer.gameObject.isStatic)
                {
                    return;
                }

                dilateRenderingModeProperty = outlineTargetProperty.FindPropertyRelative(DILATE_RENDERING_MODE_PROPERTY_PATH);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(dilateRenderingModeProperty);
                if (EditorGUI.EndChangeCheck() && ((DilateRenderMode)dilateRenderingModeProperty.intValue == DilateRenderMode.EdgeShift) && PlayerSettings.stripUnusedMeshComponents)
                {
                    if (EditorUtility.DisplayDialog(DIALOG_TITLE, DIALOG_CONTENT, DIALOG_YES, DIALOG_CANCEL))
                    {
                        PlayerSettings.stripUnusedMeshComponents = false;
                    }
                    else
                    {
                        dilateRenderingModeProperty.intValue = 0;
                    }
                }


                if ((DilateRenderMode)dilateRenderingModeProperty.intValue == DilateRenderMode.EdgeShift)
                {
                    EditorGUI.indentLevel++;

                    if (renderStyleProperty.enumValueIndex == 0)
                    {
                        EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(EDGE_DILATE_AMOUNT_PROPERTY_PATH), new GUIContent(DEFAULT_EDGE_DILATE_LABEL));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(FRONT_EDGE_DILATE_AMOUNT_PROPERTY_PATH), new GUIContent(FRONT_EDGE_DILATE_LABEL));
                        EditorGUILayout.PropertyField(outlineTargetProperty.FindPropertyRelative(BACK_EDGE_DILATE_AMOUNT_PROPERTY_PATH), new GUIContent(BACK_EDGE_DILATE_LABEL));
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawMenu(SerializedProperty outlineTargetProperty, int index)
        {
            if(GUILayout.Button(MOVE_UP_LABEL))
            {
                outlineTargetsProperty.MoveArrayElement(index, index - 1);
            }

            if(GUILayout.Button(MOVE_DOWN_LABEL))
            {
                outlineTargetsProperty.MoveArrayElement(index, index + 1);
            }

            if (GUILayout.Button(REMOVE_LABEL))
            {
                outlineTargetsProperty.DeleteArrayElementAtIndex(index);
            }
        }

        private void AddOutlineTarget(Renderer renderer, int submeshIndex)
        {
            outlineTargetsProperty.arraySize++;
            SerializedProperty newElement = outlineTargetsProperty.GetArrayElementAtIndex(outlineTargetsProperty.arraySize - 1);
            newElement.ClearProperty();
            newElement.FindPropertyRelative(RENDERER_PROPERTY_PATH).objectReferenceValue = renderer;
            newElement.FindPropertyRelative(SUBMESH_INDEX_PROPERTY_PATH).intValue = submeshIndex;
            newElement.FindPropertyRelative(CUTOUT_THRESHOLD_PROPERTY_PATH).floatValue = 0.5f;
            newElement.FindPropertyRelative(DILATE_RENDERING_MODE_PROPERTY_PATH).enumValueIndex = 0;
            newElement.FindPropertyRelative(EDGE_DILATE_AMOUNT_PROPERTY_PATH).floatValue = 5.0f;
            newElement.FindPropertyRelative(FRONT_EDGE_DILATE_AMOUNT_PROPERTY_PATH).floatValue = 5.0f;
            newElement.FindPropertyRelative(BACK_EDGE_DILATE_AMOUNT_PROPERTY_PATH).floatValue = 5.0f;

            if(renderer is SpriteRenderer)
            {
                newElement.FindPropertyRelative(CULL_MODE_PROPERTY_PATH).intValue = (int)UnityEngine.Rendering.CullMode.Off;
            }
            else
            {
                newElement.FindPropertyRelative(CULL_MODE_PROPERTY_PATH).intValue = (int)UnityEngine.Rendering.CullMode.Back;
            }

        }

        private bool IsUsed(Renderer renderer)
        {
            for (int i = 0; i < outlineTargetsProperty.arraySize; i++)
            {
                if(outlineTargetsProperty.GetArrayElementAtIndex(i).FindPropertyRelative(RENDERER_PROPERTY_PATH).objectReferenceValue == renderer)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetSubmeshCount(Renderer renderer)
        {
            if (renderer is MeshRenderer)
                return renderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount;
            else if (renderer is SkinnedMeshRenderer)
                return (renderer as SkinnedMeshRenderer).sharedMesh.subMeshCount;
            else
                return 1;
        }
    }
}
