using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Watermelon.Shader
{
    public class ToonUnlitGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material targetMat = materialEditor.target as Material;

            var _Color = FindProperty("_Color", properties);
            var _MainTex = FindProperty("_MainTex", properties);

            var _AlbedoTex = FindProperty("_AlbedoTex", properties);
            var _Albedo = FindProperty("_Albedo", properties);
            var _AlbedoColor = FindProperty("_Albedo_Color", properties);

            var _Use_Ramp_Texture = FindProperty("_Use_Ramp_Texture", properties);
            var _Ramp = FindProperty("_Ramp", properties);
            var _RampMin = FindProperty("_RampMin", properties);
            var _RampMax = FindProperty("_RampMax", properties);

            var headerStyle = new GUIStyle
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState
                {
                    textColor = new Color32(194, 194, 194, 255),
                }
            };

            bool useAlbedo = _Albedo.floatValue == 1;
            bool useRampTexture = _Use_Ramp_Texture.floatValue == 1;


            GUILayout.Label("Main", headerStyle);

            EditorGUILayout.BeginHorizontal();
            {
                var texture = (Texture)EditorGUILayout.ObjectField(_MainTex.textureValue, typeof(Texture), false, GUILayout.Width(70), GUILayout.Height(70));

                _MainTex.textureValue = texture;

                GUILayout.Space(20);

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        _Color.colorValue = EditorGUILayout.ColorField(_Color.colorValue, GUILayout.Width(70));
                        GUILayout.Label("Color");
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            GUILayout.Label("Albedo", headerStyle);

            if (useAlbedo)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var texture = (Texture)EditorGUILayout.ObjectField(_AlbedoTex.textureValue, typeof(Texture), false, GUILayout.Width(70), GUILayout.Height(70));

                    _AlbedoTex.textureValue = texture;

                    GUILayout.Space(20);

                    EditorGUILayout.BeginVertical();
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            _AlbedoColor.colorValue = EditorGUILayout.ColorField(_AlbedoColor.colorValue, GUILayout.Width(70));
                            GUILayout.Label("Albedo Color");
                        }
                        EditorGUILayout.EndHorizontal();

                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            {
                _Albedo.floatValue = EditorGUILayout.Toggle(_Albedo.floatValue == 1, GUILayout.ExpandWidth(false)) ? 1 : 0;
                GUILayout.Label("Use Albedo", GUILayout.ExpandWidth(false));

                GUILayout.FlexibleSpace();

            }
            EditorGUILayout.EndHorizontal();


            GUILayout.Space(20);

            GUILayout.Label("Ramp", headerStyle);

            if (useRampTexture)
            {
                var texture = (Texture)EditorGUILayout.ObjectField(_Ramp.textureValue, typeof(Texture), false, GUILayout.Width(70), GUILayout.Height(70));

                _Ramp.textureValue = texture;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                {
                    _RampMin.colorValue = EditorGUILayout.ColorField(_RampMin.colorValue, GUILayout.Width(70));
                    GUILayout.Label("Ramp Min");
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    _RampMax.colorValue = EditorGUILayout.ColorField(_RampMax.colorValue, GUILayout.Width(70));
                    GUILayout.Label("Ramp Max");
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal(GUILayout.Width(120));
            {
                _Use_Ramp_Texture.floatValue = EditorGUILayout.Toggle(_Use_Ramp_Texture.floatValue == 1, GUILayout.ExpandWidth(false)) ? 1 : 0;
                GUILayout.Label("Use Ramp Texture", GUILayout.ExpandWidth(false));

                GUILayout.FlexibleSpace();

            }
            EditorGUILayout.EndHorizontal();
        }
    }
}