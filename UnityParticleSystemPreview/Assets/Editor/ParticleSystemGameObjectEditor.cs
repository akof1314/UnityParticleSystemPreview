using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameObject)), CanEditMultipleObjects]
public class ParticleSystemGameObjectEditor : OverrideEditor
{
    private ParticleSystemPreview m_Preview;

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
        Editor editor = null;
        var baseType = Types.GetType("UnityEditor.GameObjectInspector", "UnityEditor.dll");
        CreateCachedEditor(targets, baseType, ref editor);
        return editor;
    }
    
    void OnEnable()
    {
    }

    void OnDisable()
    {
        preview.OnDestroy();
        DestroyImmediate(baseEditor);
    }

    private bool HasParticleSystemPreview()
    {
        return preview.HasPreviewGUI();
    }

    public override bool HasPreviewGUI()
    {
        return HasParticleSystemPreview() || baseEditor.HasPreviewGUI();
    }

    public override GUIContent GetPreviewTitle()
    {
        return HasParticleSystemPreview() ? preview.GetPreviewTitle() : baseEditor.GetPreviewTitle();
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (HasParticleSystemPreview())
        {
            preview.OnPreviewGUI(r, background);
        }
        else
        {
            baseEditor.OnPreviewGUI(r, background);
        }
    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {
        if (HasParticleSystemPreview())
        {
            preview.OnInteractivePreviewGUI(r, background);
        }
        else
        {
            baseEditor.OnInteractivePreviewGUI(r, background);
        }
    }

    public override void OnPreviewSettings()
    {
        if (HasParticleSystemPreview())
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
        return HasParticleSystemPreview() ? preview.GetInfoString() : baseEditor.GetInfoString();
    }

    public override void ReloadPreviewInstances()
    {
        if (HasParticleSystemPreview())
        {
            preview.ReloadPreviewInstances();
        }
        else
        {
            baseEditor.ReloadPreviewInstances();
        }
    }
}