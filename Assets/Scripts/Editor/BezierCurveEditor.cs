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
    private GUIContent deletePointContent = new GUIContent("X", "Deletes this BezierPoint");
    private GUIContent clearAllPointsContent = new GUIContent("Clear All", "Delete all BezierPoint");

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
        if (Target.Points != null)
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
        DrawRawPointsValue();
    }

    private void DrawButtons()
    {
        if (GUILayout.Button(addPointContent))
        {
            BezierPoint point = new BezierPoint();
            Target.AddPoint(point);
        }

        if(GUILayout.Button(clearAllPointsContent))
        {
            //TODO: Use Target.RemoveAll() later
            Target.Points.Clear();
        }

        GUILayout.Space(10);
    }

    private void DrawRawPointsValue()
    {
        //foreach (BezierPoint point in Target.Points)
        for(int i = 0; i< Target.Points.Count; ++i)
        {
            DrawRawPointValue(i);
        }
    }

    private void DrawRawPointValue(int _pointId)
    {
        GUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginVertical("Box");
        Vector3 pos = EditorGUILayout.Vector3Field("Anchor",
            Target.Points[_pointId].Position);
        Vector3 pos_0 = EditorGUILayout.Vector3Field("Handle 1th",
            Target.Points[_pointId].Handles[0].LocalPosition);
        Vector3 pos_1 = EditorGUILayout.Vector3Field("Handle 2rd",
            Target.Points[_pointId].GetHandle(1).LocalPosition);
        GUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(Target, "Changed handle transform");
            Target.Points[_pointId].Position = pos;
            Target.Points[_pointId].GetHandle(0).LocalPosition = pos_0;
            Target.Points[_pointId].GetHandle(1).LocalPosition = pos_1;
            SceneView.RepaintAll();
        }
        
        if (GUILayout.Button(deletePointContent))
        {
            Undo.RecordObject(Target, "Deleted a waypoint");
            Target.RemovePoint(Target.Points[_pointId]);
            SceneView.RepaintAll();
        }

        GUILayout.EndHorizontal();

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
                Undo.RegisterCompleteObjectUndo(Target, "Moved waypoint");
                Target.Points[i].Position = pos;
                Repaint();
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
                Undo.RecordObject(Target, "Rotated waypoint");
                Target.Points[i].Rotation = rot;
                Repaint();
            }
        }
        else if(Tools.current == Tool.Transform)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            if (m_manipulateMode == ManipulationMode.Free)
            {
                pos = Handles.FreeMoveHandle(
                    Target.Points[i].Position,
                    (Tools.pivotRotation == PivotRotation.Local) ? Target.Points[i].Rotation : Quaternion.identity,
                    size,
                    Vector3.zero,
                    Handles.CubeHandleCap);
                rot = Handles.FreeRotateHandle(
                    Target.Points[i].Rotation,
                    Target.Points[i].Position,
                    HandleUtility.GetHandleSize(Target.Points[i].Position) * 0.2f);
            }
            else if (m_manipulateMode == ManipulationMode.SelectAndTransform)
            {
                if (i == m_selectId)
                {
                    pos = Handles.PositionHandle(Target.Points[i].Position, (Tools.pivotRotation == PivotRotation.Local) ? Target.Points[i].Rotation : Quaternion.identity);
                    rot = Handles.RotationHandle(Target.Points[i].Rotation, Target.Points[i].Position);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Transformed waypoint");
                Target.Points[i].Position = pos;
                Target.Points[i].Rotation = rot;
                Repaint();
            }
        }
    }
}
