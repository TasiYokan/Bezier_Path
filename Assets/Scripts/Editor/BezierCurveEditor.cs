using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveEditor : Editor
{
    private enum ManipulationMode
    {
        Free,
        SelectAndTransform
    }

    private ManipulationMode m_manipulateMode;
    private BezierCurve m_target;
    private int m_selectId = -1;

    public BezierCurve Target
    {
        get
        {
            if (m_target == null)
                m_target = (BezierCurve)target;
            return m_target;
        }
    }

    private GUIContent addPointContent = new GUIContent("Add WayPoint", "Add a BezierPoint");

    void OnEnable()
    {
        EditorApplication.update += Update;

        if (Target == null) return;

        SetupEditorVariables();
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    void SelectIndex(int _id)
    {
        m_selectId = _id;
        Repaint();
    }

    void SetupEditorVariables()
    {
        m_manipulateMode = ManipulationMode.SelectAndTransform;
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
            for (int i = 0; i < Target.Points.Count; ++i)
            {
                DrawWaypointHandles(i);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawButtons();
    }

    private void DrawButtons()
    {
        if (GUILayout.Button(addPointContent))
        {
            GameObject obj = new GameObject("BezierPoint ("+Target.Points.Count+")");
            obj.transform.parent = Target.transform;
            BezierPoint point = obj.AddComponent<BezierPoint>();
            point.Init();
            Target.Points.Add(point);
        }
    }

    private void DrawWaypointHandles(int i)
    {
        float size = HandleUtility.GetHandleSize(Target.Points[i].Position) * 0.2f;
        if (m_selectId != i && Event.current.button != 1)
        {
            if (Handles.Button(Target.Points[i].Position, Quaternion.identity, size, size, Handles.CubeHandleCap))
            {
                SelectIndex(i);
            }
        }

        if (Tools.current == Tool.Move)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 pos = Vector3.zero;

            if (m_manipulateMode == ManipulationMode.Free)
            {
                pos = Handles.FreeMoveHandle(
                Target.Points[i].Position,
                (Tools.pivotRotation == PivotRotation.Local) ? Target.Points[i].Rotation : Quaternion.identity,
                size,
                Vector3.zero,
                Handles.CubeHandleCap);
            }
            else if (m_manipulateMode == ManipulationMode.SelectAndTransform)
            {
                if (i == m_selectId)
                    pos = Handles.PositionHandle(Target.Points[i].Position, (Tools.pivotRotation == PivotRotation.Local) ? Target.Points[i].Rotation : Quaternion.identity);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Target.Points[i].transform, "Moved Waypoint");
                Target.Points[i].Position = pos;
            }
        }
        else if (Tools.current == Tool.Rotate)
        {
            EditorGUI.BeginChangeCheck();
            Quaternion rot = Quaternion.identity;

            if (m_manipulateMode == ManipulationMode.Free)
            {
                rot = Handles.FreeRotateHandle(
                    Target.Points[i].Rotation,
                    Target.Points[i].Position,
                    HandleUtility.GetHandleSize(Target.Points[i].Position) * 0.2f);
            }
            else if (m_manipulateMode == ManipulationMode.SelectAndTransform)
            {
                if (i == m_selectId)
                    rot = Handles.RotationHandle(Target.Points[i].Rotation, Target.Points[i].Position);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Target.Points[i].transform, "Rotated Waypoint");
                Target.Points[i].Rotation = rot;
            }
        }
    }
}
