using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GTF_Shaders
{
    public class GTFUnlitGUI : ShaderGUI
    {
        enum BlendMode
        {
            Opaque,
            Cutout,
            Transparent,
        }

        static bool IsShowAdvancedSettings = false;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            DrawBlendMode(materialEditor, properties);

            var mainTex = FindProperty("_MainTex", properties);
            materialEditor.ShaderProperty(mainTex, mainTex.displayName);

            var color = FindProperty("_Color", properties);
            materialEditor.ShaderProperty(color, color.displayName);


            IsShowAdvancedSettings = EditorGUILayout.Foldout(IsShowAdvancedSettings, "高度な設定");
            if (IsShowAdvancedSettings)
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.HelpBox("ここの項目は、ほとんどの場合触る必要がありません。 また、BlendModeを変更すると初期値に戻ります。", MessageType.Info);

                var cullMode = FindProperty("_CullMode", properties);
                materialEditor.ShaderProperty(cullMode, cullMode.displayName);

                var zTest = FindProperty("_ZTest", properties);
                materialEditor.ShaderProperty(zTest, zTest.displayName);

                var zWrite = FindProperty("_ZWrite", properties);
                materialEditor.ShaderProperty(zWrite, zWrite.displayName);

                EditorGUILayout.HelpBox("設定例 (SrcFactor Dstfactor : 結果)\nSrcAlpha OneMinusSrcAlpha : 基本的な合成\nBlend One OneMinusSrcAlpha : Premultiplied\nBlend One One : Additive\nBlend OneMinusDstColor One : Soft Additive\nBlend DstColor Zero : Multiply\nBlend DstColor SrcColor : Multiply 2x", MessageType.None);
                
                var srcFactor = FindProperty("_SrcFactor", properties);
                materialEditor.ShaderProperty(srcFactor, srcFactor.displayName);

                var dstFactor = FindProperty("_DstFactor", properties);
                materialEditor.ShaderProperty(dstFactor, dstFactor.displayName);

                materialEditor.RenderQueueField();
                materialEditor.DoubleSidedGIField();
            }
        }

        void DrawBlendMode(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            var blendMode = FindProperty("_BlendMode", properties);
            var cutoff = FindProperty("_Cutoff", properties);
            var mode = (BlendMode) blendMode.floatValue;

            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                mode = (BlendMode) EditorGUILayout.Popup("Blend Mode", (int) mode, Enum.GetNames(typeof(BlendMode)));

                if (scope.changed) 
                {
                    blendMode.floatValue = (int) mode;
                    foreach (UnityEngine.Object obj in blendMode.targets)
                    {
                        ApplyBlendMode(obj as Material, mode);
                    }
                }
            }

            if (mode == BlendMode.Cutout)
            {
                materialEditor.ShaderProperty(cutoff, cutoff.displayName);
            }
        }

        static void ApplyBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_Cutoff", 0);
                material.SetInt("_SrcFactor", (int) UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstFactor", (int) UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZTest", 4);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.renderQueue = -1;
                break;

            case BlendMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetFloat("_Cutoff", (float) 0.5);
                material.SetInt("_SrcFactor", (int) UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstFactor", (int) UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZTest", 4);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;

            case BlendMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_Cutoff", 0);
                material.SetInt("_SrcFactor", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstFactor", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZTest", 4);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
                break;

            default:
                throw new ArgumentOutOfRangeException("blendMode", blendMode, null);
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            ApplyBlendMode(material, (BlendMode) material.GetFloat("_BlendMode"));
        }
    }
}

/******************************************************************************
参考資料
https://xjine.booth.pm/items/931290 のVol.01とVol.02
https://qiita.com/gam0022/items/c26a73e244dbbde9b034
https://tsubakit1.hateblo.jp/entry/2016/01/08/221404
******************************************************************************/