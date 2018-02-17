using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurve_Inspector : Editor
{
    private BezierCurve m_target;

    public BezierCurve Target
    {
        get
        {
            if(m_target == null)
                m_target = (BezierCurve)target;
            return m_target;
        }
    }

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

    void OnSceneGUI()
    {
        //m_this.UpdateAllPointPoses();

        Handles.color = Color.white;
        if (Target.Points != null && Target.Points.Count > 2)
        {
            for(int i = 0; i< Target.Points.Count;++i)
            {
                DrawWaypointHandles(i);
            }
        }
    }

    private void DrawWaypointHandles(int i)
    {
        if(Tools.current == Tool.Move)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 pos = Vector3.zero;
            pos = Handles.FreeMoveHandle(
                Target.Points[i].Position,
                (Tools.pivotRotation == PivotRotation.Local) ? Target.Points[i].Rotation : Quaternion.identity,
                HandleUtility.GetHandleSize(Target.Points[i].Position) * 0.2f,
                Vector3.zero,
                Handles.CubeHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Target.Points[i].transform, "Moved Anchor");
                Target.Points[i].Position = pos;
            }
        }
    }
}
