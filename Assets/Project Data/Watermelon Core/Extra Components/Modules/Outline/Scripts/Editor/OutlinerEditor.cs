using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Watermelon.Outline
{
    [CustomEditor(typeof(Outliner))]
    public class OutlinerEditor : WatermelonEditor
    {
        //rendering settings
        private const string RENDERING_STRATEGY_PROPERTY_PATH = "renderingStrategy";
        private const string RENDERING_MODE_PROPERTY_PATH = "renderingMode";
        private const string STAGE_PROPERTY_PATH = "stage";
        private const string OUTLINE_LAYER_MASK_PROPERTY_PATH = "outlineLayerMask";

        private const string RENDERING_SETTINGS_LABEL = "Rendering settings:";
        private const string OUTLINE_LAYER_MASK = "Outline Layer Mask";
        SerializedProperty renderingStrategyProperty;
        SerializedProperty renderingModeProperty;
        SerializedProperty stageProperty;
        SerializedProperty outlineLayerMaskProperty;



        //primary settings
        private const string PRIMARY_RENDERER_SCALE_PROPERTY_PATH = "primaryRendererScale";
        private const string PRIMARY_BUFFER_SIZE_PROPERTY_PATH = "primaryBufferSizeMode";
        private const string PRIMARY_SIZE_REFERENCE_PROPERTY_PATH = "primarySizeReference";
        private const string PRIMARY_SETTINGS_LABEL = "Primary settings:";
        private const string PRIMARY_RENDERER_SCALE_LABEL = "Renderer Scale";
        private const string PRIMARY_BUFFER_SIZE_MODE_LABEL = "Buffer Size Mode";
        private const string PRIMARY_SIZE_REFERENCE_LABEL = "Size Reference";
        SerializedProperty primaryRendererScaleProperty;
        SerializedProperty primaryBufferSizeModeProperty;
        SerializedProperty primarySizeReferenceProperty;

        //blur settings
        private const string BLUR_SHIFT_PROPERTY_PATH = "blurShift";
        private const string BLUR_ITERATIONS_PROPERTY_PATH = "blurIterations";
        private const string BLUR_TYPE_PROPERTY_PATH = "blurType";
        private const string BLUR_SETTINGS_LABEL = "Blur settings:";
        SerializedProperty blurShiftProperty;
        SerializedProperty blurIterationsProperty;
        SerializedProperty blurTypeProperty;

        //dilate settings
        private const string DILATE_SHIFT_PROPERTY_PATH = "dilateShift";
        private const string DILATE_ITERATIONS_PROPERTY_PATH = "dilateIterations";
        private const string DILATE_QUALITY_PROPERTY_PATH = "dilateQuality";
        private const string DILATE_SETTINGS_LABEL = "Dilate settings:";
        SerializedProperty dilateShiftProperty;
        SerializedProperty dilateIterationsProperty;
        SerializedProperty dilateQualityProperty;

        private const string NOTHING = "Nothing";
        private const string EVERYTHING = "Everything";
        private const string MIXED = "Mixed...";
        private bool isHDPRUsed;
        private Rect allocatedRect;
        private Rect labelRect;
        private Rect fieldRect;
        private string fieldValue;
        private int lowerNumber =  15;
        private int highterNumber =  30;

        protected override void OnEnable()
        {
            base.OnEnable();
            renderingStrategyProperty = serializedObject.FindProperty(RENDERING_STRATEGY_PROPERTY_PATH);
            renderingModeProperty = serializedObject.FindProperty(RENDERING_MODE_PROPERTY_PATH);
            stageProperty = serializedObject.FindProperty(STAGE_PROPERTY_PATH);
            outlineLayerMaskProperty = serializedObject.FindProperty(OUTLINE_LAYER_MASK_PROPERTY_PATH);

            primaryRendererScaleProperty = serializedObject.FindProperty(PRIMARY_RENDERER_SCALE_PROPERTY_PATH);
            primaryBufferSizeModeProperty = serializedObject.FindProperty(PRIMARY_BUFFER_SIZE_PROPERTY_PATH);
            primarySizeReferenceProperty = serializedObject.FindProperty(PRIMARY_SIZE_REFERENCE_PROPERTY_PATH);

            blurShiftProperty = serializedObject.FindProperty(BLUR_SHIFT_PROPERTY_PATH);
            blurIterationsProperty = serializedObject.FindProperty(BLUR_ITERATIONS_PROPERTY_PATH);
            blurTypeProperty = serializedObject.FindProperty(BLUR_TYPE_PROPERTY_PATH);

            dilateShiftProperty = serializedObject.FindProperty(DILATE_SHIFT_PROPERTY_PATH);
            dilateIterationsProperty = serializedObject.FindProperty(DILATE_ITERATIONS_PROPERTY_PATH);
            dilateQualityProperty = serializedObject.FindProperty(DILATE_QUALITY_PROPERTY_PATH);

#if (URP_OUTLINE || HDRP_OUTLINE) && UNITY_EDITOR && UNITY_2019_1_OR_NEWER
            isHDPRUsed = PipelineAssetUtility.IsHDRP(PipelineAssetUtility.CurrentAsset);
#else
            isHDPRUsed = false;
#endif

            if(outlineLayerMaskProperty.intValue < 0)
            {
                outlineLayerMaskProperty.intValue = int.MaxValue;
            }

            UpdateLayerMaskFieldValue();
            serializedObject.ApplyModifiedProperties();
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(RENDERING_SETTINGS_LABEL, EditorStylesExtended.label_medium);
            EditorGUILayout.PropertyField(renderingStrategyProperty);
            EditorGUILayout.PropertyField(renderingModeProperty);
            EditorGUILayout.PropertyField(stageProperty);
            DisplayMask();

            EditorGUILayout.LabelField(PRIMARY_SETTINGS_LABEL, EditorStylesExtended.label_medium);

            if (!isHDPRUsed)
            {
                EditorGUILayout.PropertyField(primaryRendererScaleProperty, new GUIContent(PRIMARY_RENDERER_SCALE_LABEL));
            }
            
            EditorGUILayout.PropertyField(primaryBufferSizeModeProperty, new GUIContent(PRIMARY_BUFFER_SIZE_MODE_LABEL));
            EditorGUILayout.PropertyField(primarySizeReferenceProperty, new GUIContent(PRIMARY_SIZE_REFERENCE_LABEL));

            EditorGUILayout.LabelField(BLUR_SETTINGS_LABEL, EditorStylesExtended.label_medium);
            EditorGUILayout.PropertyField(blurShiftProperty);
            EditorGUILayout.PropertyField(blurIterationsProperty);
            EditorGUILayout.PropertyField(blurTypeProperty);

            EditorGUILayout.LabelField(DILATE_SETTINGS_LABEL, EditorStylesExtended.label_medium);
            EditorGUILayout.PropertyField(dilateShiftProperty);
            EditorGUILayout.PropertyField(dilateIterationsProperty);
            EditorGUILayout.PropertyField(dilateQualityProperty);

            serializedObject.ApplyModifiedProperties();
        }

        private void DisplayMask()
        {
            allocatedRect = GUILayoutUtility.GetRect(Screen.width, EditorGUIUtility.singleLineHeight, GUI.skin.textField);
            labelRect = new Rect(allocatedRect);
            fieldRect = new Rect(allocatedRect);
            labelRect.xMax += EditorGUIUtility.labelWidth;
            fieldRect.xMin += EditorGUIUtility.labelWidth;
            EditorGUI.LabelField(labelRect, OUTLINE_LAYER_MASK);


            if (EditorGUI.DropdownButton(fieldRect, new GUIContent(fieldValue), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent(NOTHING), (outlineLayerMaskProperty.intValue == 0), () =>
                {
                    outlineLayerMaskProperty.intValue = 0;
                    serializedObject.ApplyModifiedProperties();
                    UpdateLayerMaskFieldValue();
                });

                menu.AddItem(new GUIContent(EVERYTHING), (outlineLayerMaskProperty.intValue == int.MaxValue), () =>
                {
                    outlineLayerMaskProperty.intValue = int.MaxValue;
                    serializedObject.ApplyModifiedProperties();
                    UpdateLayerMaskFieldValue();
                });


                for (int index = 0; index <= highterNumber; index++)
                {
                    if ((index < lowerNumber))
                    {
                        menu.AddItem(new GUIContent(index.ToString()), (outlineLayerMaskProperty.intValue & 1 << index) != 0, ChangeFlag, index);
                    }
                    else
                    {
                        menu.AddItem(new GUIContent($"{lowerNumber}-{highterNumber}/{index}"), (outlineLayerMaskProperty.intValue & 1 << index) != 0, ChangeFlag, index);
                    }
                }
                menu.ShowAsContext();
            }
        }

        private void ChangeFlag(object userdata)
        {
            outlineLayerMaskProperty.intValue ^= (1 << ((int)userdata));
            serializedObject.ApplyModifiedProperties();
            UpdateLayerMaskFieldValue();
        }

        private void UpdateLayerMaskFieldValue()
        {
            if (outlineLayerMaskProperty.intValue == 0)
            {
                fieldValue = NOTHING;
            }
            else if ((outlineLayerMaskProperty.intValue == int.MaxValue))
            {
                fieldValue = EVERYTHING;
            }
            else
            {
                fieldValue = MIXED;
            }
        }
    }
}