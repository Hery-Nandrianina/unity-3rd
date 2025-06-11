using UnityEngine;
using UnityEditor;

public class LadderBaker : EditorWindow
{
    int stepNumber;
    GameObject prefab;
    float zSize = 0.5f;
    float topYSize = 1;
    int layer;

    [MenuItem("Window/Ladder Baker")]
    static void Init()
    {
        LadderBaker window = (LadderBaker)EditorWindow.GetWindow(typeof(LadderBaker));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Ladder", EditorStyles.boldLabel);
        stepNumber = EditorGUILayout.IntField("Number of steps", stepNumber);
        zSize = EditorGUILayout.FloatField("Empty space", zSize);
        topYSize = EditorGUILayout.FloatField("Top height", topYSize);
        prefab = (GameObject) EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        layer = EditorGUILayout.LayerField("Layer:", layer);

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create"))
        {
            if (prefab == null || stepNumber==0)
                ShowNotification(new GUIContent("Please fill the settings"));
            else {
                Renderer render = prefab.GetComponent<Renderer>();
                GameObject root = new GameObject("Ladder");
                root.layer = layer;
                root.tag = "Ladder";
                root.transform.position = GetViewCenterWorldPos();
                GameObject parent = Selection.activeGameObject;
                if(parent != null)
                    root.transform.parent = parent.transform;

                for (int i = 0; i < stepNumber; i++)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.name = string.Format("step_{0}", i);
                    obj.transform.parent = root.transform;
                    obj.transform.localPosition = new Vector3(0, i*render.bounds.size.y, 0);
                    obj.layer = root.layer;
                }
                
                BoxCollider box = root.AddComponent<BoxCollider>();
                Vector3 size = render.bounds.size;
                size.y *= stepNumber;
                size.z += zSize;
                box.size = size;
                box.center = new Vector3(0, (float)stepNumber/2 * render.bounds.size.y, zSize/2);
                box.isTrigger = true;

                GameObject top = new GameObject("Top");
                top.layer = root.layer;
                top.transform.parent = root.transform;
                top.transform.localPosition = Vector3.up * render.bounds.size.y * (stepNumber);

                BoxCollider box2 = top.AddComponent<BoxCollider>();
                size.y = topYSize;
                box2.size = size;
                box2.center += new Vector3(0, 0, -zSize/2);
                box2.isTrigger = true;
            }
        }
    }

    private static Vector3 GetViewCenterWorldPos() {
        Ray worldRay = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
        // Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        // float distanceToGround;
        // groundPlane.Raycast(worldRay, out distanceToGround);
        // Vector3 worldPos = worldRay.GetPoint(distanceToGround);

        RaycastHit hit;
        if(Physics.Raycast(worldRay, out hit, 4))
            return hit.point;
        else
            return worldRay.GetPoint(4);

        // return worldPos;
    }
}
