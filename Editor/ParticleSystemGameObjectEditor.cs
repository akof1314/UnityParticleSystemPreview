using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace WuHuan
{
    [CustomEditor(typeof(GameObject)), CanEditMultipleObjects]
    public class ParticleSystemGameObjectEditor : OverrideEditor
    {
        private static Type s_GameObjectType;
        private static MethodInfo s_OnSceneDragMethodInfo;
        private static FieldInfo s_PreviewCacheFieldInfo;
        private static PropertyInfo s_TargetIndexPropertyInfo;

        private class Styles
        {
            public GUIContent ps = new GUIContent("PS", "Show particle system preview");
#if UNITY_2019_3_OR_NEWER
            public GUIStyle preButton = EditorStyles.toolbarButton;
#else
            public GUIStyle preButton = "preButton";
#endif
        }

        private bool m_ShowParticlePreview;

        private int m_DefaultHasPreview;

        private ParticleSystemPreview m_Preview;

        private static Styles s_Styles;

        private ParticleSystemPreview preview
        {
            get
            {
                if (m_Preview == null)
                {
                    m_Preview = new ParticleSystemPreview();
                    m_Preview.SetEditor(this);
                    m_Preview.Initialize(targets);
                }

                return m_Preview;
            }
        }

        protected override Editor GetBaseEditor()
        {
            if (s_GameObjectType == null)
            {
                var assembly = typeof(Editor).Assembly;
                s_GameObjectType = assembly.GetType("UnityEditor.GameObjectInspector");
                s_OnSceneDragMethodInfo = s_GameObjectType.GetMethod("OnSceneDrag",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                s_PreviewCacheFieldInfo = s_GameObjectType.GetField("m_PreviewCache",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Type realTypeEditor = assembly.GetType("UnityEditor.Editor");
                s_TargetIndexPropertyInfo = realTypeEditor.GetProperty("referenceTargetIndex",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            }

            Editor editor = null;
            var baseType = s_GameObjectType;
            CreateCachedEditor(targets, baseType, ref editor);
            return editor;
        }

        void OnEnable()
        {
            m_ShowParticlePreview = true;
        }

        void OnDisable()
        {
            if (m_Preview != null)
            {
                m_Preview.OnDestroy();
#if UNITY_2021_1_OR_NEWER
                m_Preview.Cleanup();
#endif
                m_Preview = null;
            }

            if (HasBaseEditor() && IsPreviewCacheNotNull)
            {
                DestroyImmediate(baseEditor);
                baseEditor = null;
            }
        }

        private bool HasParticleSystemPreview()
        {
            return preview.HasPreviewGUI();
        }

        private bool HasBasePreview()
        {
            if (m_DefaultHasPreview == 0)
            {
                m_DefaultHasPreview = baseEditor.HasPreviewGUI() ? 1 : -1;
            }

            return m_DefaultHasPreview == 1;
        }

        private bool IsShowParticleSystemPreview()
        {
            return HasParticleSystemPreview() && m_ShowParticlePreview;
        }

        public override bool HasPreviewGUI()
        {
            return HasParticleSystemPreview() || HasBasePreview();
        }

        public override GUIContent GetPreviewTitle()
        {
            return IsShowParticleSystemPreview() ? preview.GetPreviewTitle() : baseEditor.GetPreviewTitle();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (IsShowParticleSystemPreview())
            {
                preview.OnPreviewGUI(r, background);
            }
            else
            {
                SetEditorTargetIndex();
                baseEditor.OnPreviewGUI(r, background);
            }
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {
            if (IsShowParticleSystemPreview())
            {
                preview.OnInteractivePreviewGUI(r, background);
            }
            else
            {
                SetEditorTargetIndex();
                baseEditor.OnInteractivePreviewGUI(r, background);
            }
        }

        public override void OnPreviewSettings()
        {
            if (s_Styles == null)
            {
                s_Styles = new Styles();
            }

            if (HasBasePreview() && HasParticleSystemPreview())
            {
                m_ShowParticlePreview = GUILayout.Toggle(m_ShowParticlePreview, s_Styles.ps, s_Styles.preButton);
            }

            if (IsShowParticleSystemPreview())
            {
                preview.OnPreviewSettings();
            }
            else
            {
                baseEditor.OnPreviewSettings();
            }
        }

        public override string GetInfoString()
        {
            return IsShowParticleSystemPreview() ? preview.GetInfoString() : baseEditor.GetInfoString();
        }

        public override void ReloadPreviewInstances()
        {
            if (IsShowParticleSystemPreview())
            {
                preview.ReloadPreviewInstances();
            }
            else
            {
                baseEditor.ReloadPreviewInstances();
            }
        }

        /// <summary>
        /// 需要调用 GameObjectInspector 的场景拖曳，否则无法拖动物体到 Scene 视图
        /// </summary>
#if UNITY_2020_2_OR_NEWER
        public void OnSceneDrag(SceneView sceneView, int index)
        {
            if (s_OnSceneDragMethodInfo != null)
            {
                s_OnSceneDragMethodInfo.Invoke(baseEditor, new object[] { sceneView, index });
            }
        }
#else
        public void OnSceneDrag(SceneView sceneView)
        {
            if (s_OnSceneDragMethodInfo != null)
            {
                s_OnSceneDragMethodInfo.Invoke(baseEditor, new object[] { sceneView });
            }
        }
#endif

        private bool IsPreviewCacheNotNull
        {
            get
            {
                if (null != s_PreviewCacheFieldInfo)
                {
                    var value = s_PreviewCacheFieldInfo.GetValue(baseEditor);
                    return null != value;
                }
                return false;
            }
        }

        private void SetEditorTargetIndex()
        {
            var newVal = s_TargetIndexPropertyInfo.GetValue(this, null);
            s_TargetIndexPropertyInfo.SetValue(baseEditor, newVal, null);
        }
    }
}