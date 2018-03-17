using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierPoint))]
public class BezierPointEditor : Editor
{
    private BezierPoint m_target;

    public BezierPoint Target
    {
        get
        {
            if (m_target == null)
                m_target = (BezierPoint)target;
            return m_target;
        }
    }

    private GUIContent addPointContent = new GUIContent("Reset Point", "Reset point position based on its parent");

    void OnEnable()
    {
        EditorApplication.update += Update;

        if (Target == null) return;

    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Self update");
    }

    public override void OnInspectorGUI()
    {
        DrawRawHandleValue();
        DrawButtons();
    }

    private void DrawButtons()
    {
        if (GUILayout.Button(addPointContent))
        {
            Target.Handles[0] = new BezierHandle(Target);
            Target.Handles[1] = new BezierHandle(Target);
        }
    }

    private void DrawRawHandleValue()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginVertical("Box");
        Vector3 pos_0 = Vector3.zero;
        Vector3 pos_1 = Vector3.zero;
        if (Target.Handles[0] != null)
        {
            pos_0 = EditorGUILayout.Vector3Field("Handle Position",
                Target.Handles[0].LocalPosition);
        }
        if (Target.Handles[1] != null)
        {
            pos_1 = EditorGUILayout.Vector3Field("Handle Position",
                Target.GetHandle(1).LocalPosition);
        }
        GUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(Target, "Changed handle transform");
            if (Target.Handles[0] != null)
            {
                Target.GetHandle(0).LocalPosition = pos_0;
            }
            if (Target.Handles[1] != null)
            {
                Target.GetHandle(1).LocalPosition = pos_1;
            }
            SceneView.RepaintAll();
        }
    }
}