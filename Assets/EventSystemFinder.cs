using UnityEngine;
using UnityEngine.EventSystems;

public class DirectEventSystemFix : MonoBehaviour
{
    void Start()
    {
        FixEventSystems();
    }

    [ContextMenu("Fix Event Systems Now")]
    public void FixEventSystems()
    {
        Debug.Log("🔍 SCANNING FOR ALL EVENT SYSTEMS...");

        // Find ALL EventSystem components, including inactive ones
        EventSystem[] allEventSystems = Resources.FindObjectsOfTypeAll<EventSystem>();

        // Filter only scene objects (not prefabs)
        var sceneEventSystems = new System.Collections.Generic.List<EventSystem>();

        foreach (var eventSystem in allEventSystems)
        {
            // Check if it's a scene object (not a prefab)
            if (eventSystem.gameObject.scene.IsValid())
            {
                sceneEventSystems.Add(eventSystem);
            }
        }

        Debug.Log($"📊 Found {sceneEventSystems.Count} EventSystems in scene:");

        // Log all found EventSystems
        for (int i = 0; i < sceneEventSystems.Count; i++)
        {
            var es = sceneEventSystems[i];
            string path = GetFullPath(es.gameObject);
            Debug.Log($"   [{i}] {es.name} at {path} (Enabled: {es.enabled})");
        }

        if (sceneEventSystems.Count > 1)
        {
            Debug.Log("⚠️ MULTIPLE EVENT SYSTEMS DETECTED - FIXING...");

            // Strategy: Keep the one on MRTK camera, disable others
            EventSystem keepThis = null;

            // Prefer EventSystem on CenterEyeAnchor or MRTK-related object
            foreach (var es in sceneEventSystems)
            {
                string path = GetFullPath(es.gameObject).ToLower();
                if (path.Contains("centereyeanchor") || path.Contains("mrtk") || path.Contains("mixedreality"))
                {
                    keepThis = es;
                    break;
                }
            }

            // If no MRTK one found, keep the first enabled one
            if (keepThis == null)
            {
                foreach (var es in sceneEventSystems)
                {
                    if (es.enabled)
                    {
                        keepThis = es;
                        break;
                    }
                }
            }

            // If still none, keep the first one
            if (keepThis == null && sceneEventSystems.Count > 0)
            {
                keepThis = sceneEventSystems[0];
            }

            // Disable all others
            int disabledCount = 0;
            foreach (var es in sceneEventSystems)
            {
                if (es != keepThis)
                {
                    es.enabled = false;
                    Debug.Log($"   ❌ Disabled EventSystem on: {es.name}");
                    disabledCount++;
                }
            }

            Debug.Log($"   ✅ Kept EventSystem on: {keepThis.name}");
            Debug.Log($"🎉 FIXED! Disabled {disabledCount} extra EventSystems");
        }
        else if (sceneEventSystems.Count == 1)
        {
            Debug.Log("✅ Only one EventSystem found - no conflict!");
        }
        else
        {
            Debug.LogError("❌ No EventSystems found! This will cause problems.");
        }

        // Double-check by counting active ones
        var activeEventSystems = FindObjectsOfType<EventSystem>();
        Debug.Log($"🔍 Final check: {activeEventSystems.Length} active EventSystems remaining");

        if (activeEventSystems.Length <= 1)
        {
            Debug.Log("🎯 SUCCESS! Event System conflict resolved!");
        }
        else
        {
            Debug.LogError("⚠️ Still multiple active EventSystems detected!");
        }
    }

    string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    void Update()
    {
        // Press X to run the fix
        if (Input.GetKeyDown(KeyCode.X))
        {
            FixEventSystems();
        }
    }
}