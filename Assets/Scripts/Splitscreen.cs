using System.Collections.Generic;
using UnityEngine;

// Each of the 4 gameplayer cameras renders into its render texture at full res to avoid warping etc.
// The shader does the clipping and renders into the trapezoidal quadrants 
// a dedicated UI camera is created to composite the quads and the deadzone/sidebars 

public class Splitscreen : MonoBehaviour
{
    [Header("Cams")]
    public Camera camTop;    // S1 Sub Pilot
    public Camera camLeft;   // S2 Sub Control
    public Camera camBottom; // S3 Diver Pilot
    public Camera camRight;  // S4 Diver Control

    [Header("Shader")]
    public Shader shader; 

    [Header("Dead Zone Settings")]
    [Range(0.05f, 0.45f)]
    public float deadZoneHalf = 0.27f;   // fraction of the square's side length
    public Color sidebarColor = Color.black;
    public float playAreaSize = 0f; //0 is full screen resolution 
    public int rtWidth  = 1920;
    public int rtHeight = 1080;

    private Camera uiCam;
    private GameObject uiCamGO;

    private GameObject deadZoneGO;
    private GameObject sidebarL;
    private GameObject sidebarR;

    private struct QuadData
    {
        public GameObject go;
        public RenderTexture rt;
        public Camera cam;
    }
    private readonly List<QuadData> quads = new List<QuadData>();

    private int   lastW, lastH;
    private float lastPlayArea, lastDeadZone;

    void OnEnable()
    {
        EnsureUICamera(); //build the splitscreen camera 
        Rebuild();
    }

    void OnDisable() => Teardown();

    void OnDestroy() => Teardown();

    void Update()
    {
        //check if the screen size changed  
        bool changed = Screen.width  != lastW || Screen.height != lastH || !Mathf.Approximately(playAreaSize,  lastPlayArea) || !Mathf.Approximately(deadZoneHalf,  lastDeadZone);
        if (changed) Rebuild();
    }

    public void Rebuild() { Teardown(); Build(); }

    void EnsureUICamera()
    {
        if (uiCam != null) return; //if it alr exists, you can safely delete it from heirarchy tho cause it causes scene to lag

        uiCamGO = new GameObject("SplitscreenUICamera") { hideFlags = HideFlags.DontSave };
        uiCam = uiCamGO.AddComponent<Camera>();

        //camera config
        uiCam.orthographic = true;
        uiCam.clearFlags = CameraClearFlags.Depth;
        uiCam.cullingMask = 1 << LayerMask.NameToLayer("UI");
        uiCam.depth = 100;
        uiCam.nearClipPlane = -1f;
        uiCam.farClipPlane =  1f;

        uiCamGO.transform.position = Vector3.zero;

        //disable audio listener to avoid conflicts with gameplay cams 
        var al = uiCamGO.GetComponent<AudioListener>();
        if (al) al.enabled = false;

        //make sure gameplay cameras dont render the UI layer
        foreach (var cam in new[]{ camTop, camBottom, camLeft, camRight })
        {
            if (cam == null) continue;
            cam.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
        }
    }

    void Build()
    {
        float screenW = Screen.width;
        float screenH = Screen.height;

        float square = (playAreaSize > 0f) ? Mathf.Min(playAreaSize, screenH) : screenH;
        float sidebarW = (screenW - square) * 0.5f;
        float half = square * 0.5f;
        float dz = square * deadZoneHalf;

        //uiCam.orthographicSize = screenH * 0.5f;

        // defining the quad corners with respect to the square center
        float pL = -half, pR = half, pT = half, pB = -half;

        //building the 4 quadrants, each quad covers the full play area but the shader clips the correct portion out of each. 
        CreateViewQuad(camBottom, pL, pR, pT, pB, 0, 0, "Bottom S3 Diver Pilot");
        CreateViewQuad(camTop, pL, pR, pT, pB, 1, 180f, "Top S1 Sub Pilot");
        CreateViewQuad(camLeft, pL, pR, pT, pB, 2, 90f, "Left S2 Sub Control");
        CreateViewQuad(camRight, pL, pR, pT, pB, 3, -90f, "Right S4 Diver Control");

        //build deadzone
        deadZoneGO = BuildColorRect("DeadZone", Color.black, -dz, dz, dz, -dz, localZ: 0.2f, sortOrder: 10);

        //build sidebars if needed 
        if (sidebarW > 0.5f)
        {
            float fL = -screenW * 0.5f, fR = screenW * 0.5f;
            sidebarL = BuildColorRect("SidebarL", sidebarColor, fL, pL,  pT, pB, 0.3f, 5);
            sidebarR = BuildColorRect("SidebarR", sidebarColor, pR, fR, pT, pB, 0.3f, 5);
        }

        lastW = Screen.width;
        lastH = Screen.height;
        lastPlayArea = playAreaSize;
        lastDeadZone = deadZoneHalf;
    }

    void Teardown()
    {
        foreach (var q in quads)
        {
            if (q.cam != null) q.cam.targetTexture = null;
            if (q.rt  != null) { q.rt.Release(); SafeDestroy(q.rt); }
            SafeDestroy(q.go);
        }
        quads.Clear();

        SafeDestroy(ref deadZoneGO);
        SafeDestroy(ref sidebarL);
        SafeDestroy(ref sidebarR);
    }

    //builds a quad for a camera to render into, the clipping is handled by the PeppersGhostSplit shader itself 
    void CreateViewQuad(Camera cam, float xMin, float xMax, float yTop, float yBot, int sortOrder, float rotationZ, string label)
    {
        //create a new render texture and assign it to the camera + name it 
        var rt = new RenderTexture(rtWidth, rtHeight, 24, RenderTextureFormat.DefaultHDR)
        {
            name = $"RT_{label}"
        };
        rt.Create();
        cam.targetTexture = rt;

        //create a new game object for the quads and configure it with the prescribed values 
        var go = new GameObject($"Quad_{label}") { hideFlags = HideFlags.DontSave };
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(uiCamGO.transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, 0.5f); 
        go.transform.localRotation = Quaternion.Euler(0, 0, rotationZ); 

        //build rectangular meshes, shader will do clipping 
        var mesh = new Mesh { name = $"Mesh_{label}" };

        mesh.vertices = new[]
        {
            new Vector3(xMin, yTop, 0f),  // TL
            new Vector3(xMax, yTop, 0f),  // TR
            new Vector3(xMax, yBot, 0f),  // BR
            new Vector3(xMin, yBot, 0f),  // BL
        };

        //UVs to map the full rt to the quad 
        mesh.uv = new[]
        {
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f),
            new Vector2(0f, 0f),
        };
        
        //triangle coordinates 
        mesh.triangles = new[] 
        { 
            0, 1, 2,  
            0, 2, 3 
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.mesh = mesh;

        //find triangle clip custom shader 
        //var shader = Shader.Find("Custom/PeppersGhostSplit");
        //var shader = Resources.Load<Shader>("Custom/PeppersGhostSplit");
        //if (shader == null) .... 

        //create new material using shader for the quad and assign rt to it, config other stuff
        var mat = new Material(shader) { name = $"Mat_{label}" };
        mat.SetTexture("_MainTex", rt);
        mr.material = mat;
        mr.sortingOrder = sortOrder;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        quads.Add(new QuadData { go = go, rt = rt, cam = cam });
    }

    //builds a solid coloured rect for use in the deadzones and side bars
    GameObject BuildColorRect(string label, Color color, float xMin, float xMax, float yTop, float yBot, float localZ, int sortOrder)
    {
        //build gameobject and configure it with prescribed vals.
        var go = new GameObject(label) { hideFlags = HideFlags.DontSave };
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(uiCamGO.transform, false);
        go.transform.localPosition = new Vector3(0f, 0f, localZ);

        //build rectangular mesh
        var mesh = new Mesh { name = $"Mesh_{label}" };

        mesh.vertices = new[]
        {
            new Vector3(xMin, yTop, 0f),
            new Vector3(xMax, yTop, 0f),
            new Vector3(xMax, yBot, 0f),
            new Vector3(xMin, yBot, 0f),
        };

        //uv coordinates 
        mesh.uv= new[]
        { 
            new Vector2(0,1), 
            new Vector2(1,1), 
            new Vector2(1,0), 
            new Vector2(0,0) 
        };

        //triangle coordinates 
        mesh.triangles = new[] 
        { 
            0, 1, 2,  
            0, 2, 3 
        };

        mesh.RecalculateNormals();

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.mesh = mesh;

        //find simple unlit shader for solid colour, create material and build the rect, config other stuff
        var shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("UI/Unlit/Transparent");
        var mat = new Material(shader) { name = $"Mat_{label}", color = color };
        mr.material = mat;
        mr.sortingOrder = sortOrder;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        return go;
    }

    //helpers
    static void SafeDestroy(Object obj)
    {
        if (obj == null) return;
    #if UNITY_EDITOR
        if (!Application.isPlaying) Object.DestroyImmediate(obj);
        else
    #endif
        Object.Destroy(obj);
    }

    static void SafeDestroy(ref GameObject go)
    {
        if (go == null) return;
        SafeDestroy(go);
        go = null;
    }
}