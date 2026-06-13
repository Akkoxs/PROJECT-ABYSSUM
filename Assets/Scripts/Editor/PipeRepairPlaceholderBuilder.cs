#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-click placeholder so the pipe-burst repair mechanic can be tested before the real art exists.
// Menu: Tools > Pipe Repair > Build Placeholder Test Setup
//
// It builds a screen-space Canvas with a PipeRepairUI panel laid out like the ADM modulation panel
// (four channel containers: knob / h-slider / knob / v-slider, each with a marker + a leak overlay),
// creates a PipeRepairSystem, and auto-wires PipeRepairUI, the submarine Health, and the submarine's
// pipeRepair field. Everything uses flat-colour UI Images as stand-ins for the final assets.
//
// Re-running the menu item rebuilds the placeholder from scratch (the old one is removed first).
public static class PipeRepairPlaceholderBuilder
{
    private const string CanvasName = "PipeRepair Placeholder Canvas";

    private static readonly int[] Modes = { 0, 1, 0, 2 }; // 0 knob, 1 h-slider, 2 knob, 3 v-slider
    private static readonly float[] ChannelX = { -495f, -165f, 165f, 495f };

    private static readonly Color PanelColor = new Color(0.05f, 0.07f, 0.10f, 0.85f);
    private static readonly Color TrackColor = new Color(0.20f, 0.22f, 0.26f, 1f);
    private static readonly Color KnobColor = new Color(1f, 0.85f, 0.2f, 1f);
    private static readonly Color SliderColor = new Color(0.3f, 0.8f, 1f, 1f);
    private static readonly Color LeakColor = new Color(1f, 0.25f, 0.1f, 0.55f);

    [MenuItem("Tools/Pipe Repair/Build Placeholder Test Setup")]
    public static void Build()
    {
        int uiLayer = LayerMask.NameToLayer("UI");

        // remove any previous placeholder canvas so re-runs are idempotent
        var existing = GameObject.Find(CanvasName);
        if (existing != null) Object.DestroyImmediate(existing);

        // ---- Canvas (always active) holds the PipeRepairUI controller ----
        var canvasGO = new GameObject(CanvasName, typeof(Canvas), typeof(CanvasScaler), typeof(PipeRepairUI));
        if (uiLayer >= 0) canvasGO.layer = uiLayer;
        Undo.RegisterCreatedObjectUndo(canvasGO, "Build Pipe Repair Placeholder");

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000; // sit on top so it's visible over the splitscreen composite
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // ---- Panel (the toggled root, == GlobalModulationUI.minigameRoot) ----
        var panel = NewImage("PipeRepairPanel", canvasGO.transform, uiLayer, new Vector2(1320, 360),
                             Vector2.zero, new Vector2(0.5f, 0.5f), PanelColor);

        var markers = new RectTransform[4];
        var overlays = new GameObject[4];

        for (int i = 0; i < 4; i++)
        {
            int mode = Modes[i];
            string modeName = mode == 0 ? "Knob" : (mode == 1 ? "HSlider" : "VSlider");

            var container = new GameObject($"Channel{i}_{modeName}", typeof(RectTransform));
            if (uiLayer >= 0) container.layer = uiLayer;
            var crt = (RectTransform)container.transform;
            crt.SetParent(panel.transform, false);
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.pivot = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(300, 300);
            crt.anchoredPosition = new Vector2(ChannelX[i], 0f);

            if (mode == 0) // rotary knob: a needle that swings around its base
            {
                NewImage("Disc", crt, uiLayer, new Vector2(120, 120), Vector2.zero, new Vector2(0.5f, 0.5f), TrackColor);
                overlays[i] = NewImage("Leak", crt, uiLayer, new Vector2(150, 150), Vector2.zero, new Vector2(0.5f, 0.5f), LeakColor).gameObject;
                // pivot at bottom-centre so localEulerAngles.z rotates it like a gauge needle
                markers[i] = NewImage("Needle", crt, uiLayer, new Vector2(12, 64), Vector2.zero, new Vector2(0.5f, 0f), KnobColor).rectTransform;
            }
            else if (mode == 1) // horizontal slider
            {
                NewImage("Track", crt, uiLayer, new Vector2(300, 18), Vector2.zero, new Vector2(0.5f, 0.5f), TrackColor);
                overlays[i] = NewImage("Leak", crt, uiLayer, new Vector2(320, 70), Vector2.zero, new Vector2(0.5f, 0.5f), LeakColor).gameObject;
                markers[i] = NewImage("Handle", crt, uiLayer, new Vector2(26, 48), Vector2.zero, new Vector2(0.5f, 0.5f), SliderColor).rectTransform;
            }
            else // vertical slider
            {
                NewImage("Track", crt, uiLayer, new Vector2(18, 300), Vector2.zero, new Vector2(0.5f, 0.5f), TrackColor);
                overlays[i] = NewImage("Leak", crt, uiLayer, new Vector2(70, 320), Vector2.zero, new Vector2(0.5f, 0.5f), LeakColor).gameObject;
                markers[i] = NewImage("Handle", crt, uiLayer, new Vector2(48, 26), Vector2.zero, new Vector2(0.5f, 0.5f), SliderColor).rectTransform;
            }
        }

        // ---- wire PipeRepairUI (public fields, set directly) ----
        var pipeUI = canvasGO.GetComponent<PipeRepairUI>();
        pipeUI.repairPanelRoot = panel.gameObject;
        pipeUI.channelMarkers = markers;
        pipeUI.leakOverlays = overlays;
        EditorUtility.SetDirty(pipeUI);

        // ---- PipeRepairSystem + auto-wiring ----
        var system = Object.FindFirstObjectByType<PipeRepairSystem>();
        if (system == null)
        {
            var sysGO = new GameObject("PipeRepairSystem", typeof(PipeRepairSystem));
            Undo.RegisterCreatedObjectUndo(sysGO, "Build Pipe Repair Placeholder");
            system = sysGO.GetComponent<PipeRepairSystem>();
        }

        var sub = Object.FindFirstObjectByType<Submarine>();
        Health subHealth = sub != null ? sub.GetComponent<Health>() : null;

        var sysSO = new SerializedObject(system);
        SetObjectRef(sysSO, "ui", pipeUI);
        if (subHealth != null) SetObjectRef(sysSO, "subHealth", subHealth);
        sysSO.ApplyModifiedProperties();

        if (sub != null)
        {
            var subSO = new SerializedObject(sub);
            SetObjectRef(subSO, "pipeRepair", system);
            subSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(sub);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;

        string subMsg = sub != null
            ? (subHealth != null ? "wired to submarine Health + pipeRepair field." : "submarine found but it has no Health component — set PipeRepairSystem.subHealth manually.")
            : "no Submarine found in scene — set PipeRepairSystem.subHealth and Submarine.pipeRepair manually.";
        Debug.Log($"[PipeRepair] Placeholder built. PipeRepairUI on '{CanvasName}', PipeRepairSystem {subMsg} " +
                  "Enter Play mode, ram terrain to burst a pipe (red overlay), then move the matching control to repair.");
    }

    // creates a flat-colour UI Image stand-in and returns it
    private static Image NewImage(string name, Transform parent, int uiLayer, Vector2 size, Vector2 pos, Vector2 pivot, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (uiLayer >= 0) go.layer = uiLayer;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = pivot;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    private static void SetObjectRef(SerializedObject so, string propName, Object value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.objectReferenceValue = value;
        else Debug.LogWarning($"[PipeRepair] Could not find serialized property '{propName}' on {so.targetObject.GetType().Name}.");
    }
}
#endif
