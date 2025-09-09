using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class WaypointEditorWindow : EditorWindow
{
    private int waypointCount = 1;
    private float spacing = 5f;
    private Vector3 offsetDirection = Vector3.forward;

    [MenuItem("Tools/Waypoint Editor")]
    public static void ShowWindow()
    {
        GetWindow<WaypointEditorWindow>("Waypoint Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Waypoint Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Waypoint Creation Section
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Create Waypoints", EditorStyles.boldLabel);

        waypointCount = EditorGUILayout.IntField("Number of Waypoints", waypointCount);
        waypointCount = Mathf.Max(1, waypointCount);

        spacing = EditorGUILayout.FloatField("Spacing", spacing);
        offsetDirection = EditorGUILayout.Vector3Field("Direction", offsetDirection).normalized;

        EditorGUILayout.Space();

        if (GUILayout.Button("CREATE Next to Selected", GUILayout.Height(30)))
        {
            CreateNextToSelected();
        }

        if (GUILayout.Button("CREATE Chain from Selected", GUILayout.Height(30)))
        {
            CreateChainFromSelected();
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Waypoint Linking Section
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Waypoint Linking", EditorStyles.boldLabel);

        if (Selection.gameObjects.Length < 2)
        {
            EditorGUILayout.HelpBox("Select at least 2 Waypoints", MessageType.Info);
        }
        else
        {
            List<Waypoint> selectedWaypoints = GetSelectedWaypoints();

            if (selectedWaypoints.Count >= 2)
            {
                for (int i = 0; i < selectedWaypoints.Count; i++)
                {
                    EditorGUILayout.LabelField($"{i + 1}. {selectedWaypoints[i].name}");
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("LINK ALL TO FIRST", GUILayout.Height(30)))
                {
                    LinkAllToFirst(selectedWaypoints);
                }

                if (GUILayout.Button("LINK SEQUENTIAL (A→B, B→C)", GUILayout.Height(30)))
                {
                    LinkSequential(selectedWaypoints);
                }

                if (GUILayout.Button("CLEAR Links from First"))
                {
                    selectedWaypoints[0].ClearAllNextWaypoints();
                    EditorUtility.SetDirty(selectedWaypoints[0]);
                }

                if (GUILayout.Button("CLEAR Links from All"))
                {
                    ClearAllLinks(selectedWaypoints);
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void CreateNextToSelected()
    {
        Waypoint selectedWaypoint = GetSelectedWaypoint();
        if (selectedWaypoint == null)
        {
            Debug.LogWarning("Please select a waypoint first!");
            return;
        }

        

        Vector3 spawnPosition = selectedWaypoint.transform.position + offsetDirection * spacing;
       GameObject newWaypoint = CreateWaypoint(spawnPosition, selectedWaypoint.transform.rotation);
        selectedWaypoint.AddNextWaypoint(newWaypoint.gameObject.GetComponent<Waypoint>());
    }

    private void CreateChainFromSelected()
    {
        Waypoint selectedWaypoint = GetSelectedWaypoint();
        if (selectedWaypoint == null)
        {
            Debug.LogWarning("Please select a waypoint first!");
            return;
        }

        List<GameObject> createdWaypoints = new List<GameObject>();
        Waypoint currentWaypoint = selectedWaypoint;

        for (int i = 0; i < waypointCount; i++)
        {
            Vector3 spawnPosition = currentWaypoint.transform.position + offsetDirection * spacing;
            GameObject newWaypoint = CreateWaypoint(spawnPosition, currentWaypoint.transform.rotation);
            createdWaypoints.Add(newWaypoint);

            // Auto-link the current waypoint to the new one
            currentWaypoint.AddNextWaypoint(newWaypoint.GetComponent<Waypoint>());
            EditorUtility.SetDirty(currentWaypoint);

            currentWaypoint = newWaypoint.GetComponent<Waypoint>();
        }

        // Select all created waypoints
        Selection.objects = createdWaypoints.ToArray();
        Debug.Log($"Created chain of {waypointCount} waypoints from {selectedWaypoint.name}");
    }

    private GameObject CreateWaypoint(Vector3 position, Quaternion rotation)
    {
        GameObject waypointObject = new GameObject("Waypoint");
        waypointObject.transform.position = position;
        waypointObject.transform.rotation = rotation;
        waypointObject.AddComponent<Waypoint>();

        Undo.RegisterCreatedObjectUndo(waypointObject, "Create Waypoint");
        return waypointObject;
    }

    private Waypoint GetSelectedWaypoint()
    {
        if (Selection.activeGameObject != null)
        {
            return Selection.activeGameObject.GetComponent<Waypoint>();
        }
        return null;
    }

    private List<Waypoint> GetSelectedWaypoints()
    {
        List<Waypoint> waypoints = new List<Waypoint>();
        foreach (GameObject obj in Selection.gameObjects)
        {
            Waypoint wp = obj.GetComponent<Waypoint>();
            if (wp != null) waypoints.Add(wp);
        }
        return waypoints;
    }

    private void LinkAllToFirst(List<Waypoint> waypoints)
    {
        Waypoint first = waypoints[0];
        int linkedCount = 0;

        for (int i = 1; i < waypoints.Count; i++)
        {
            first.AddNextWaypoint(waypoints[i]);
            linkedCount++;
        }

        EditorUtility.SetDirty(first);
        Debug.Log($"Linked {linkedCount} waypoints to {first.name}");
    }

    private void LinkSequential(List<Waypoint> waypoints)
    {
        int linkedCount = 0;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            waypoints[i].AddNextWaypoint(waypoints[i + 1]);
            EditorUtility.SetDirty(waypoints[i]);
            linkedCount++;
        }

        Debug.Log($"Created sequential links: {linkedCount} connections");
    }

    private void ClearAllLinks(List<Waypoint> waypoints)
    {
        foreach (var wp in waypoints)
        {
            wp.ClearAllNextWaypoints();
            EditorUtility.SetDirty(wp);
        }
        Debug.Log($"Cleared all links from {waypoints.Count} waypoints");
    }
}