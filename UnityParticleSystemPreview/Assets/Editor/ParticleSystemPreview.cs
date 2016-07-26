using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPreview(typeof(GameObject))]
public class ParticleSystemPreview : ObjectPreview
{
    private PreviewRenderUtility m_PreviewUtility;
    private GameObject m_PreviewInstance;
    private Vector2 m_PreviewDir;
    private Mesh m_FloorPlane;
    private Texture2D m_FloorTexture;
    private Material m_FloorMaterial;
    private Material m_FloorMaterialSmall;

    private bool m_Playing;
    private float m_RunningTime;
    private double m_PreviousTime;
    private const float kDuration = 30f;
    private static GUIContent[] s_PlayIcons = new GUIContent[2];
    private static int PreviewCullingLayer = 31;

    public override void Initialize(Object[] targets)
    {
        base.Initialize(targets);
        if (m_PreviewUtility == null)
        {
            m_PreviewUtility = new PreviewRenderUtility(true);
            m_PreviewUtility.m_CameraFieldOfView = 30f;
			m_PreviewUtility.m_Camera.cullingMask = -2147483648;
            CreatePreviewInstances();

            s_PlayIcons[0] = EditorGUIUtility.IconContent("preAudioPlayOff", "Play");
            s_PlayIcons[1] = EditorGUIUtility.IconContent("preAudioPlayOn", "Stop");
        }
        if (this.m_FloorPlane == null)
        {
            this.m_FloorPlane = (Resources.GetBuiltinResource(typeof(Mesh), "New-Plane.fbx") as Mesh);
        }
        if (this.m_FloorTexture == null)
        {
            this.m_FloorTexture = (Texture2D)EditorGUIUtility.Load("Avatar/Textures/AvatarFloor.png");
        }
        if (this.m_FloorMaterial == null)
        {
            Shader shader = EditorGUIUtility.LoadRequired("Previews/PreviewPlaneWithShadow.shader") as Shader;
            this.m_FloorMaterial = new Material(shader);
            this.m_FloorMaterial.mainTexture = this.m_FloorTexture;
            this.m_FloorMaterial.mainTextureScale = Vector2.one * 5f * 4f;
            this.m_FloorMaterial.SetVector("_Alphas", new Vector4(0.5f, 0.3f, 0f, 0f));
            this.m_FloorMaterial.hideFlags = HideFlags.HideAndDontSave;
            this.m_FloorMaterialSmall = new Material(this.m_FloorMaterial);
            this.m_FloorMaterialSmall.mainTextureScale = Vector2.one * 0.2f * 4f;
            this.m_FloorMaterialSmall.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        m_PreviewDir = Drag2D(m_PreviewDir, r);
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }
        m_PreviewUtility.BeginPreview(r, background);
        DoRenderPreview();
        m_PreviewUtility.EndAndDrawPreview(r);

        EditorGUI.DropShadowLabel(new Rect(r.x, r.yMax - 20f, r.width, 20f), "Playback Time:" + m_RunningTime);
    }

    public override GUIContent GetPreviewTitle()
    {
        GUIContent content = base.GetPreviewTitle();
        content.text += " super";
        return content;
    }

    public override void OnPreviewSettings()
    {
        bool flag = CycleButton(!m_Playing ? 0 : 1, s_PlayIcons, "preButton") != 0;
        if (flag != m_Playing)
        {
            if (flag)
            {
                SimulateEnable();
            }
            else
            {
                SimulateDisable();
            }
        }
    }

    public override void ReloadPreviewInstances()
    {
        CreatePreviewInstances();
    }

    private void DoRenderPreview()
    {
        GameObject gameObject = m_PreviewInstance;
        Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
        GetRenderableBoundsRecurse(ref bounds, gameObject);
        float num = Mathf.Max(bounds.extents.magnitude, 0.0001f);
        float num2 = num * 3.8f;
        Quaternion quaternion = Quaternion.Euler(-this.m_PreviewDir.y, -this.m_PreviewDir.x, 0f);
        Vector3 position = bounds.center - quaternion * (Vector3.forward * num2);
        this.m_PreviewUtility.m_Camera.transform.position = position;
        this.m_PreviewUtility.m_Camera.transform.rotation = quaternion;
        this.m_PreviewUtility.m_Camera.nearClipPlane = num2 - num * 1.1f;
        this.m_PreviewUtility.m_Camera.farClipPlane = num2 + num * 1.1f;
        this.m_PreviewUtility.m_Light[0].intensity = 0.7f;
        this.m_PreviewUtility.m_Light[0].transform.rotation = quaternion * Quaternion.Euler(40f, 40f, 0f);
        this.m_PreviewUtility.m_Light[1].intensity = 0.7f;
        this.m_PreviewUtility.m_Light[1].transform.rotation = quaternion * Quaternion.Euler(340f, 218f, 177f);
        Color ambient = new Color(0.1f, 0.1f, 0.1f, 0f);
        InternalEditorUtility.SetCustomLighting(this.m_PreviewUtility.m_Light, ambient);
        bool fog = RenderSettings.fog;
        Unsupported.SetRenderSettingsUseFogNoDirty(false);

        this.m_PreviewUtility.m_Camera.nearClipPlane = 0.5f * 1;
        this.m_PreviewUtility.m_Camera.farClipPlane = 100f * 1;
        Quaternion rotation = Quaternion.Euler(-this.m_PreviewDir.y, -this.m_PreviewDir.x, 0f);
        Vector3 position2 = rotation * (Vector3.forward * -5.5f * 1) ;
        this.m_PreviewUtility.m_Camera.transform.position = position2;
        this.m_PreviewUtility.m_Camera.transform.rotation = rotation;
        Quaternion identity = Quaternion.identity;
        Material floorMaterial = this.m_FloorMaterial;
        Matrix4x4 matrix2 = Matrix4x4.TRS(position, identity, Vector3.one * 5f * 1f);
        //floorMaterial.mainTextureOffset = -new Vector2(position.x, position.z) * 5f * 0.08f * (1f / this.m_AvatarScale);
        //floorMaterial.SetTexture("_ShadowTexture", renderTexture);
        //floorMaterial.SetMatrix("_ShadowTextureMatrix", matrix);
        //floorMaterial.SetVector("_Alphas", new Vector4(0.5f * num3, 0.3f * num3, 0f, 0f));
        Graphics.DrawMesh(this.m_FloorPlane, matrix2, floorMaterial, PreviewCullingLayer, this.m_PreviewUtility.m_Camera, 0);

        SetEnabledRecursive(gameObject, true);
        this.m_PreviewUtility.m_Camera.Render();
        SetEnabledRecursive(gameObject, false);
        Unsupported.SetRenderSettingsUseFogNoDirty(fog);
        InternalEditorUtility.RemoveCustomLighting();
    }

    public static void SetEnabledRecursive(GameObject go, bool enabled)
    {
        Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            Renderer renderer = componentsInChildren[i];
            renderer.enabled = enabled;
        }
    }

    public static void GetRenderableBoundsRecurse(ref Bounds bounds, GameObject go)
    {
        MeshRenderer meshRenderer = go.GetComponent(typeof(MeshRenderer)) as MeshRenderer;
        MeshFilter meshFilter = go.GetComponent(typeof(MeshFilter)) as MeshFilter;
        if (meshRenderer && meshFilter && meshFilter.sharedMesh)
        {
            if (bounds.extents == Vector3.zero)
            {
                bounds = meshRenderer.bounds;
            }
            else
            {
                bounds.Encapsulate(meshRenderer.bounds);
            }
        }
        SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
        if (skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
        {
            if (bounds.extents == Vector3.zero)
            {
                bounds = skinnedMeshRenderer.bounds;
            }
            else
            {
                bounds.Encapsulate(skinnedMeshRenderer.bounds);
            }
        }
        SpriteRenderer spriteRenderer = go.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        if (spriteRenderer && spriteRenderer.sprite)
        {
            if (bounds.extents == Vector3.zero)
            {
                bounds = spriteRenderer.bounds;
            }
            else
            {
                bounds.Encapsulate(spriteRenderer.bounds);
            }
        }
        foreach (Transform transform in go.transform)
        {
            GetRenderableBoundsRecurse(ref bounds, transform.gameObject);
        }
    }

    private void CreatePreviewInstances()
    {
        Debug.Log("ParticleSystemPreview CreatePreviewInstances()");
        DestroyPreviewInstances();
        GameObject gameObject = Object.Instantiate(target) as GameObject;
        InitInstantiatedPreviewRecursive(gameObject);
        Animator component = gameObject.GetComponent<Animator>();
        if (component)
        {
            component.enabled = false;
            component.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            component.logWarnings = false;
            component.fireEvents = false;
        }
        SetEnabledRecursive(gameObject, false);
        m_PreviewInstance = gameObject;
    }
    private void DestroyPreviewInstances()
    {
        if (m_PreviewInstance == null)
        {
            return;
        }
        UnityEngine.Object.DestroyImmediate(m_PreviewInstance);
    }

    private static void InitInstantiatedPreviewRecursive(GameObject go)
    {
        //go.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        go.hideFlags = HideFlags.HideAndDontSave;
        go.layer = PreviewCullingLayer;
        foreach (Transform transform in go.transform)
        {
            InitInstantiatedPreviewRecursive(transform.gameObject);
        }
    }

    public void OnDestroy()
    {
        Debug.Log("ParticleSystemPreview OnDestroy()");
        DestroyPreviewInstances();
        if (m_PreviewUtility != null)
        {
            m_PreviewUtility.Cleanup();
            m_PreviewUtility = null;
        }
    }

    private void SimulateEnable()
    {
        m_PreviousTime = EditorApplication.timeSinceStartup;

        EditorApplication.update -= InspectorUpdate;
        EditorApplication.update += InspectorUpdate;
        m_RunningTime = 0f;
        m_Playing = true;
    }

    private void SimulateDisable()
    {
        EditorApplication.update -= InspectorUpdate;
        m_RunningTime = 0f;
        m_Playing = false;
    }

    private void SimulateUpdate()
    {
        GameObject gameObject = m_PreviewInstance;
        ParticleSystem particleSystem = gameObject.GetComponentInChildren<ParticleSystem>(true);
        if (particleSystem)
        {
            particleSystem.Simulate(m_RunningTime, true);
            InspectorWindowUtil.Init();
            InspectorWindowUtil.repaintAllInspectors();
        }
    }

    private void InspectorUpdate()
    {
        var delta = EditorApplication.timeSinceStartup - m_PreviousTime;
        m_PreviousTime = EditorApplication.timeSinceStartup;

        if (m_Playing)
        {
            m_RunningTime = Mathf.Clamp(m_RunningTime + (float)delta, 0f, kDuration);
            SimulateUpdate();
        }
    }

    public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
    {
        int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
        Event current = Event.current;
        switch (current.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (position.Contains(current.mousePosition) && position.width > 50f)
                {
                    GUIUtility.hotControl = controlID;
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                }
                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;
                }
                EditorGUIUtility.SetWantsMouseJumping(0);
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
                    scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                    current.Use();
                    GUI.changed = true;
                }
                break;
        }
        return scrollPosition;
    }

    private static int CycleButton(int selected, GUIContent[] contents, GUIStyle style)
    {
        bool flag = GUILayout.Button(contents[selected], style);
        if (flag)
        {
            int num = selected;
            selected = num + 1;
            bool flag2 = selected >= contents.Length;
            if (flag2)
            {
                selected = 0;
            }
        }
        return selected;
    }
}
