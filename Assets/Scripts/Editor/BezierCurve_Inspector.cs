using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurve_Inspector : Editor
{
    private BezierCurve m_target;

    void OnEnable()
    {
        EditorApplication.update += Update;

        m_target = (BezierCurve)target;
        if (m_target == null) return;
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
        if (m_target.Points.Count > 2)
        {
            for(int i = 0; i< m_target.Points.Count;++i)
            {
                Handles.color = Color.green;
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
            pos = Handles.FreeMoveHandle(m_target.Points[i].Position, (Tools.pivotRotation == PivotRotation.Local) ? m_target.Points[i].Rotation : Quaternion.identity, HandleUtility.GetHandleSize(m_target.Points[i].Position) * 0.2f, Vector3.zero, Handles.RectangleHandleCap);
        }
    }
}
