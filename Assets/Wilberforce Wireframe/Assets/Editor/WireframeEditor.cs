// Copyright (c) 2017 Jakub Boksansky, Adam Pospisil - All Rights Reserved
// Wilberforce Wireframe Unity Shader 0.9.1beta

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//#if UNITY_EDITOR
using UnityEditor;
//#endif
using System;

namespace Wilberforce.Wireframe
{
    public class WilberforceWireframeShaderGUI : ShaderGUI
    {

        private static readonly GUIContent wireThicknessContent = new GUIContent("Thickness:", "Controls thickness of the wire");
        private static readonly GUIContent transitionModeContent = new GUIContent("Thickness Units:", "Units in which is thickness of the transition between wire and fill color specified");
        private static readonly GUIContent transitionRelativeContent = new GUIContent("Thickness:", "Thickness of the transition relative to wire thickness");
        private static readonly GUIContent transitionPixelsContent = new GUIContent("Thickness:", "Thickness of the transition in pixels (wire anti-aliasing)");

        private static readonly GUIContent wireColorContent = new GUIContent("Color:", "Color of the wire (can be transparent)");
        private static readonly GUIContent fillColorContent = new GUIContent("Color:", "Color of the fill (can be transparent)");
        private static readonly GUIContent removeDiagonalContent = new GUIContent("Enable:", "Removes wire on longest edge of each triangle");
        private static readonly GUIContent ignoreModelTransformDiagonalRemovalContent = new GUIContent("Ignore Model Transformations:", "This will not take transformations (position, rotation, scale) of model into account when removing diagonals. Recommended for dynamic meshes.");
        private static readonly GUIContent flatShadingContent = new GUIContent("Flat Shading:", "Uses flat shading (cosine of normal with view vector) on fill color");
        private static readonly GUIContent flatShadingWireContent = new GUIContent("Flat Shading:", "Uses flat shading (cosine of normal with view vector) on wire color");
        private static readonly GUIContent objectSpaceContent = new GUIContent("Calculate in object space (Perspective):", "Will calculate wire with perspective projection, making it thinner further from camera");
        private static readonly GUIContent capsContent = new GUIContent("Enable:", "Will show line caps");
        private static readonly GUIContent capSizeContent = new GUIContent("Line Caps Size:", "Controls size of line caps");
        private static readonly GUIContent depthBiasContent = new GUIContent("Depth Bias:", "Offsets depth of wire by given amount - use this to combat Z-fighting or for interesting effect");
        private static readonly GUIContent cullingSettingContent = new GUIContent("Apply To Faces:", "Culling option - wire can be drawn on either front, back or both faces");

        private static readonly GUIContent wireControlsLabelContent = new GUIContent("Wire Settings:", "Controls for adjusting appearance of the wire");
        private static readonly GUIContent fillControlsLabelContent = new GUIContent("Fill Color Settings:", "Controls for adjusting fill color");
        private static readonly GUIContent diagonalRemovalLabelContent = new GUIContent("Diagonal Removal:", "Controls for diagonals removal method");
        private static readonly GUIContent transitionControlsLabelContent = new GUIContent("Wire/Fill Color Transition Settings:", "Controls for appearance of transition between wire and fill color (anti-aliasing)");
        private static readonly GUIContent lineCapsLabelContent = new GUIContent("Line Caps (Beta):", "Controls for appearance of line caps");
        private static readonly GUIContent renderingLabelContent = new GUIContent("Rendering Settings:", "Controls for integration within rendering pipeline");

        private bool wireControlsFoldout = true;
        private bool fillControlsFoldout = true;
        private bool diagonalRemovalFoldout = true;
        private bool transitionFoldout = true;
        private bool lineCapsFoldout = true;
        private bool renderingFoldout = true;

        private void CreateShaderPropertyControl(MaterialEditor materialEditor, MaterialProperty prop, GUIContent label)
        {
#if UNITY_5_4_OR_NEWER
            materialEditor.ShaderProperty(prop, label);
#else
            materialEditor.ShaderProperty(prop, label.text);
#endif
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material targetMat = materialEditor.target as Material;

            // Read current values
            float wireThickness = targetMat.GetFloat("_WireThickness");
            float capSize = targetMat.GetFloat("_CapSize");
            int transitionWidthMode = targetMat.GetInt("_TransitionSetting");

            // Enforce parameters limits
            targetMat.SetFloat("_WireThickness", Mathf.Max(0.0f, wireThickness));
            targetMat.SetFloat("_CapSize", Mathf.Max(0.0f, capSize));

            // Find properties
            MaterialProperty _WireThickness = ShaderGUI.FindProperty("_WireThickness", properties);
            MaterialProperty _TransitionSetting = ShaderGUI.FindProperty("_TransitionSetting", properties);
            MaterialProperty _TransitionPixels = ShaderGUI.FindProperty("_TransitionPixels", properties);
            MaterialProperty _TransitionThickness = ShaderGUI.FindProperty("_TransitionThickness", properties);
            MaterialProperty _WireColor = ShaderGUI.FindProperty("_WireColor", properties);
            MaterialProperty _FillColor = ShaderGUI.FindProperty("_FillColor", properties);
            MaterialProperty _RemoveDiagonal = ShaderGUI.FindProperty("_RemoveDiagonal", properties);
            MaterialProperty _IgnoreModelTransformDiagonalRemoval = ShaderGUI.FindProperty("_IgnoreModelTransformDiagonalRemoval", properties);
            MaterialProperty _FlatShading = ShaderGUI.FindProperty("_FlatShading", properties);
            MaterialProperty _FlatShadingWire = ShaderGUI.FindProperty("_FlatShadingWire", properties);
            MaterialProperty _ObjectSpace = ShaderGUI.FindProperty("_ObjectSpace", properties);
            MaterialProperty _Caps = ShaderGUI.FindProperty("_Caps", properties);
            MaterialProperty _CapSize = ShaderGUI.FindProperty("_CapSize", properties);
            MaterialProperty _DepthBias = ShaderGUI.FindProperty("_DepthBias", properties);
            MaterialProperty _CullingSetting = ShaderGUI.FindProperty("_CullingSetting", properties);

            // Set default UI style
            materialEditor.SetDefaultGUIWidths();

            // Draw GUI controls for shader properties
            EditorGUI.indentLevel++;

            // Wire controls
            wireControlsFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), wireControlsFoldout, wireControlsLabelContent, true, EditorStyles.foldout);

            if (wireControlsFoldout)
            {
                EditorGUI.indentLevel++;

                CreateShaderPropertyControl(materialEditor, _WireThickness, wireThicknessContent);
                CreateShaderPropertyControl(materialEditor, _WireColor, wireColorContent);
                CreateShaderPropertyControl(materialEditor, _FlatShadingWire, flatShadingWireContent);

                EditorGUILayout.Space();
                CreateShaderPropertyControl(materialEditor, _ObjectSpace, objectSpaceContent);

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            // Fill color controls
            fillControlsFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), fillControlsFoldout, fillControlsLabelContent, true, EditorStyles.foldout);

            if (fillControlsFoldout)
            {
                EditorGUI.indentLevel++;

                CreateShaderPropertyControl(materialEditor, _FillColor, fillColorContent);
                CreateShaderPropertyControl(materialEditor, _FlatShading, flatShadingContent);

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            // Transition color controls
            transitionFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), transitionFoldout, transitionControlsLabelContent, true, EditorStyles.foldout);

            if (transitionFoldout)
            {
                EditorGUI.indentLevel++;

                CreateShaderPropertyControl(materialEditor, _TransitionSetting, transitionModeContent);

                if (transitionWidthMode == 2)
                {
                    CreateShaderPropertyControl(materialEditor, _TransitionThickness, transitionRelativeContent);
                }
                else
                {
                    CreateShaderPropertyControl(materialEditor, _TransitionPixels, transitionPixelsContent);
                }

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            // Diagonal removal controls
            diagonalRemovalFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), diagonalRemovalFoldout, diagonalRemovalLabelContent, true, EditorStyles.foldout);

            if (diagonalRemovalFoldout)
            {
                EditorGUI.indentLevel++;

                CreateShaderPropertyControl(materialEditor, _RemoveDiagonal, removeDiagonalContent);
                CreateShaderPropertyControl(materialEditor, _IgnoreModelTransformDiagonalRemoval, ignoreModelTransformDiagonalRemovalContent);

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            // Line caps controls
            lineCapsFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), lineCapsFoldout, lineCapsLabelContent, true, EditorStyles.foldout);

            if (lineCapsFoldout)
            {
                EditorGUI.indentLevel++;

                CreateShaderPropertyControl(materialEditor, _Caps, capsContent);
                CreateShaderPropertyControl(materialEditor, _CapSize, capSizeContent);

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            // Rendering controls
            renderingFoldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), renderingFoldout, renderingLabelContent, true, EditorStyles.foldout);

            if (renderingFoldout)
            {
                EditorGUI.indentLevel++;

                CreateShaderPropertyControl(materialEditor, _DepthBias, depthBiasContent);
                CreateShaderPropertyControl(materialEditor, _CullingSetting, cullingSettingContent);

                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

#if UNITY_5_5_OR_NEWER
            materialEditor.RenderQueueField();
#endif
        }
    }
}