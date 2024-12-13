using UnityEditor;
using UnityEngine;

public class WaypointManagerWindow : EditorWindow
{
    
    [MenuItem("Tool/Waypoint Editor")]
    public static void Open()
    {
        GetWindow<WaypointManagerWindow>();


    }
    

    public Transform waypointRoot;

    private void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        EditorGUILayout.PropertyField(obj.FindProperty("waypointRoot"));

        if(waypointRoot == null)
        {
            EditorGUILayout.HelpBox("Root transform must be eselected please assign a root transform", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.BeginVertical("box");
            DrawButtons();
            EditorGUILayout.EndVertical();
        }

        obj.ApplyModifiedProperties();
    }

    void DrawButtons()
    {
        if ( GUILayout.Button("Create Waypoint"))
        {
            CreateWaypoint();
        }
        if(Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Waypoint>())
        {
            if(GUILayout.Button("Add Branch Waypoint"))
            {
                CreateBranch();
            }
            if(GUILayout.Button("Create Waypoint Before"))
            {
                CreateWaypointBefore();
            }
            if(GUILayout.Button("Create Waypoint After"))
            {
                CreateWaypointAfter();
            }
            if(GUILayout.Button("Remove Waypoint"))
            {
                RemoveWaypoint();
            }
        }
    }

    void CreateWaypoint(){

        GameObject waypointObject = new GameObject("Waypoint "+ waypointRoot.childCount, typeof(Waypoint));
        waypointObject.transform.SetParent(waypointRoot, false);

        Waypoint waypoint = waypointObject.GetComponent<Waypoint>();
        if(waypointRoot.childCount>1)
        {
            waypoint.previousWaypoint = waypointRoot.GetChild(waypointRoot.childCount - 2).GetComponent<Waypoint>();
            waypoint.previousWaypoint.nextWaypoint = waypoint;

            //place the waypoint in the next pos
            waypoint.transform.position = waypoint.previousWaypoint.transform.position;
            waypoint.transform.forward = waypoint.previousWaypoint.transform.forward;
        }

        Selection.activeGameObject = waypoint.gameObject;
    }

    void CreateWaypointAfter(){
        
        GameObject waypointObject = new GameObject("Waypoint "+waypointRoot.childCount, typeof(Waypoint));
        waypointObject.transform.SetParent(waypointRoot, false);

        Waypoint newWaypoint = waypointObject.GetComponent<Waypoint>();
        Waypoint selectedWaypoint = Selection.activeGameObject.GetComponent<Waypoint>();

        waypointObject.transform.position = selectedWaypoint.transform.position;
        waypointObject.transform.forward = selectedWaypoint.transform.forward;

        newWaypoint.previousWaypoint = selectedWaypoint;

        if(selectedWaypoint.nextWaypoint != null)
        {
            selectedWaypoint.nextWaypoint.previousWaypoint = newWaypoint;
            newWaypoint.nextWaypoint = selectedWaypoint.nextWaypoint;
        }

        selectedWaypoint.nextWaypoint = newWaypoint;

        newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());

        Selection.activeGameObject = newWaypoint.gameObject;

    }

    void CreateWaypointBefore(){

        GameObject waypointObject = new GameObject("Waypoint "+waypointRoot.childCount, typeof(Waypoint));
        waypointObject.transform.SetParent(waypointRoot, false);

        Waypoint newWaypoint = waypointObject.GetComponent<Waypoint>();
        Waypoint selectedWaypoint = Selection.activeGameObject.GetComponent<Waypoint>();

        waypointObject.transform.position = selectedWaypoint.transform.position;
        waypointObject.transform.forward = selectedWaypoint.transform.forward;

        if(selectedWaypoint.previousWaypoint != null)
        {
            newWaypoint.previousWaypoint = selectedWaypoint.previousWaypoint;
            selectedWaypoint.previousWaypoint.nextWaypoint = newWaypoint;
        }

        newWaypoint.nextWaypoint = selectedWaypoint;

        selectedWaypoint.previousWaypoint = newWaypoint;

        newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());

        Selection.activeGameObject = newWaypoint.gameObject;

    }
    void RemoveWaypoint(){

        Waypoint selectedWaypoint = Selection.activeGameObject.GetComponent<Waypoint>();

        if(selectedWaypoint.nextWaypoint != null)
        {
            selectedWaypoint.nextWaypoint.previousWaypoint = selectedWaypoint.previousWaypoint;
        }
        if(selectedWaypoint.previousWaypoint != null)
        {
            selectedWaypoint.previousWaypoint.nextWaypoint = selectedWaypoint.nextWaypoint;
            Selection.activeGameObject = selectedWaypoint.previousWaypoint.gameObject;
        }

        DestroyImmediate(selectedWaypoint.gameObject);

    }

    void CreateBranch()
{
    GameObject selectedObject = Selection.activeGameObject;

    // Verifica che esista un oggetto selezionato
    if (selectedObject == null)
    {
        Debug.LogError("Nessun oggetto selezionato! Seleziona un waypoint prima di creare un ramo.");
        return;
    }

    // Verifica che l'oggetto selezionato sia un Waypoint
    Waypoint branchedFrom = selectedObject.GetComponent<Waypoint>();
    if (branchedFrom == null)
    {
        Debug.LogError("L'oggetto selezionato non ha un componente Waypoint!");
        return;
    }

    // Crea un nuovo waypoint
    GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(Waypoint));
    waypointObject.transform.SetParent(waypointRoot, false);

    Waypoint waypoint = waypointObject.GetComponent<Waypoint>();
    branchedFrom.branches.Add(waypoint);

    waypoint.transform.position = branchedFrom.transform.position;
    waypoint.transform.forward = branchedFrom.transform.forward;

    // Seleziona il nuovo waypoint
    Selection.activeGameObject = waypoint.gameObject;
}

}
