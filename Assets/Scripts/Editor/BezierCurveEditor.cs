﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(BezierCurve))]
public class BezierCurveEditor : Editor
{
    private enum ManipulationMode
    {
        Free = 0,
        SelectAndTransform = 1
    }

    private class VisualSetting
    {
        public Color pathColor = Color.green;
        public Color inactivePathColor = Color.gray;
        public Color handleColor = Color.white;
    }

    #region Editor Variable

    private BezierCurve m_target;
    public BezierCurve Target
    {
        get
        {
            if (m_target == null)
                m_target = (BezierCurve)target;
            return m_target;
        }
    }
    private ManipulationMode m_manipulateMode;
    private VisualSetting m_visualSetting;
    private int m_selectId = -1;
    private int m_handleSelectId = -1;
    private bool m_drawPathInEditor = true;

    #endregion Editor Variable

    #region Editor GUIs

    private GUIContent addPointContent = new GUIContent("Add WayPoint", "Add a BezierPoint");
    private GUIContent deletePointContent = new GUIContent("X", "Deletes this BezierPoint");
    private GUIContent clearAllPointsContent = new GUIContent("Clear All", "Delete all BezierPoint");

    #endregion Editor GUIs

    #region Serialized Properties

    private SerializedObject serializedTarget;
    private SerializedProperty isAutoConnectProperty;

    #endregion Serialized Properties

    #region Inbuilt APIs
    void OnEnable()
    {
        EditorApplication.update += Update;

        if (Target == null) return;

        SetupEditorVariables();
        GetTargetProperties();
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    public override void OnInspectorGUI()
    {
        serializedTarget.Update();
        DrawButtons();
        DrawRawPointsValue();
        serializedTarget.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        //m_this.UpdateAllPointPoses();

        Handles.color = Color.white;
        if (Target.Points != null)
        {
            for (int i = 0; i < Target.Points.Count; ++i)
            {
                DrawWaypointSelectHandles(i);
                DrawWaypointTransformHandles(i);
            }
        }

        DrawBezierCurve();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Self update");
    }

    #endregion Inbuilt APIs
    #region Inspector Methods

    void SetupEditorVariables()
    {
        m_manipulateMode = (ManipulationMode)PlayerPrefs.GetInt("Editor_ManipulateMode", 0);
        m_drawPathInEditor = PlayerPrefs.GetInt("Editor_DrawPath", 1) == 1;
        m_visualSetting = new VisualSetting();
    }

    private void DrawButtons()
    {
        EditorGUI.BeginChangeCheck();
        m_manipulateMode = (ManipulationMode)EditorGUILayout.EnumPopup(
            "Mode to manipulate node", m_manipulateMode);
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetInt("Editor_ManipulateMode", (int)m_manipulateMode);
            SceneView.RepaintAll();
        }

        EditorGUI.BeginChangeCheck();
        m_drawPathInEditor = GUILayout.Toggle(m_drawPathInEditor, "Draw path in Editor", GUILayout.Width(Screen.width));
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetInt("Editor_DrawPath", m_drawPathInEditor ? 1 : 0);
        }

        isAutoConnectProperty.boolValue = GUILayout.Toggle(isAutoConnectProperty.boolValue, "Connect first and last nodes?", GUILayout.Width(Screen.width));

        if (GUILayout.Button(addPointContent))
        {
            BezierPoint point = new BezierPoint(true);
            Target.AddPoint(point);
        }

        if (GUILayout.Button(clearAllPointsContent))
        {
            //TODO: Use Target.RemoveAll() later
            Target.Points.Clear();
        }

        GUILayout.Space(10);
    }

    private void DrawRawPointsValue()
    {
        //foreach (BezierPoint point in Target.Points)
        for (int i = 0; i < Target.Points.Count; ++i)
        {
            DrawRawPointValue(i);
        }
    }

    private void DrawRawPointValue(int _pointId)
    {
        GUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginVertical("Box");
        Vector3 pos = EditorGUILayout.Vector3Field("Anchor Pos",
            Target.Points[_pointId].LocalPosition);
        Vector3 rotInEuler = EditorGUILayout.Vector3Field("Anchor Rot",
            Target.Points[_pointId].LocalRotation.eulerAngles);
        Vector3 pos_0 = EditorGUILayout.Vector3Field("Handle 1th",
            Target.Points[_pointId].GetHandle(0).LocalPosition);
        Vector3 pos_1 = EditorGUILayout.Vector3Field("Handle 2rd",
            Target.Points[_pointId].GetHandle(1).LocalPosition);
        GUILayout.EndVertical();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(Target, "Changed handle transform");
            Target.SetAnchorLocalRotation(_pointId, Quaternion.Euler(rotInEuler));
            Target.SetAnchorLocalPosition(_pointId, pos);
            Target.Points[_pointId].SetHandleLocalPosition(0, pos_0);
            Target.Points[_pointId].SetHandleLocalPosition(1, pos_1);
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

    #endregion Inspector Methods
    #region Scene Methods

    void SelectIndex(int _id)
    {
        m_selectId = _id;
        Repaint();
    }

    void SelectHandleIndex(int _id)
    {
        m_handleSelectId = _id;
        Repaint();
    }

    private void DrawBezierCurve()
    {
        if (m_drawPathInEditor == false || Target.Points == null)
            return;

        Target.UpdateAnchorsTransform();

        if (Target.Points.Count >= 2)
        {
            for (int i = 0; i < Target.Points.Count; i++)
            {
                Target.Points[i].UpdateHandlesPosition();

                if (i < Target.Points.Count - 1)
                {
                    var index = Target.Points[i];
                    var indexNext = Target.Points[i + 1];
                    Handles.DrawBezier(index.Position, indexNext.Position, index.GetHandle(0).Position,
                        indexNext.GetHandle(1).Position, ((Selection.activeGameObject == Target.gameObject) ? m_visualSetting.pathColor : m_visualSetting.inactivePathColor), null, 5);


                }
                else
                {
                    if (Target.isAutoConnect)
                    {
                        var index = Target.Points[i];
                        var indexNext = Target.Points[0];
                        UnityEditor.Handles.DrawBezier(index.Position, indexNext.Position, index.GetHandle(0).Position,
                            indexNext.GetHandle(1).Position, ((Selection.activeGameObject == Target.gameObject) ? m_visualSetting.pathColor : m_visualSetting.inactivePathColor), null, 5);
                    }
                }

                Handles.DrawLine(Target.Points[i].Position, Target.Points[i].GetHandle(0).Position);
                Handles.DrawLine(Target.Points[i].Position, Target.Points[i].GetHandle(1).Position);
            }
        }
    }

    private void CreateMoveHandle(int _id, ref Vector3 _pos, ref Vector3 _pos_0, ref Vector3 _pos_1)
    {
        Vector3 pos = Target.Points[_id].Position;
        float size = Mathf.Max(0.1f, HandleUtility.GetHandleSize(Target.Points[_id].Position) * 0.05f);
        Quaternion viewRot = (Tools.pivotRotation == PivotRotation.Local)
            ? Target.Points[_id].Rotation : Quaternion.identity;

        if (m_manipulateMode == ManipulationMode.Free)
        {
            if (m_selectId == _id && m_handleSelectId == -1)
            {
                _pos = Handles.FreeMoveHandle(
                    pos, viewRot, size * 3, Vector3.zero, Handles.CubeHandleCap);
            }

            if (m_selectId == _id && m_handleSelectId == 0)
            {
                _pos_0 = Handles.FreeMoveHandle(
                    _pos_0, viewRot, size, Vector3.zero, Handles.SphereHandleCap);
            }
            if (m_selectId == _id && m_handleSelectId == 1)
            {
                _pos_1 = Handles.FreeMoveHandle(
                    _pos_1, viewRot, size, Vector3.zero, Handles.SphereHandleCap);
            }
        }
        else if (m_manipulateMode == ManipulationMode.SelectAndTransform
            && _id == m_selectId)
        {
            if (m_handleSelectId == -1)
            {
                _pos = Handles.PositionHandle(pos, viewRot);
            }

            if (m_handleSelectId == 0)
            {
                _pos_0 = Handles.PositionHandle(_pos_0, viewRot);
            }
            if (m_handleSelectId == 1)
            {
                _pos_1 = Handles.PositionHandle(_pos_1, viewRot);
            }
        }
    }

    private void CreateRotateHandle(int _id, ref Quaternion _rot)
    {
        Vector3 pos = Target.Points[_id].Position;
        float size = Mathf.Max(0.1f, HandleUtility.GetHandleSize(Target.Points[_id].Position) * 0.05f);
        Quaternion viewRot = (Tools.pivotRotation == PivotRotation.Local)
            ? Target.Points[_id].Rotation : Quaternion.identity;

        if (m_manipulateMode == ManipulationMode.Free)
        {
            _rot = Handles.FreeRotateHandle(viewRot, pos, size * 4);
        }
        else if (m_manipulateMode == ManipulationMode.SelectAndTransform
            && _id == m_selectId)
        {
            if (m_handleSelectId == -1)
            {
                _rot = Handles.RotationHandle(viewRot, pos);
            }
        }
    }

    private void DrawWaypointSelectHandles(int i)
    {
        float size = Mathf.Max(0.1f, HandleUtility.GetHandleSize(Target.Points[i].Position) * 0.05f);
        if (Event.current.button != 1)
        {
            Handles.color = m_visualSetting.inactivePathColor;

            if (m_selectId != i || m_handleSelectId != 0)
            {
                if (Handles.Button(Target.Points[i].GetHandle(0).Position, Quaternion.identity, size * 2, size * 2, Handles.SphereHandleCap))
                {
                    SelectIndex(i);
                    SelectHandleIndex(0);
                    Debug.Log("select " + m_selectId + " " + m_handleSelectId);
                }
            }

            if (m_selectId != i || m_handleSelectId != 1)
            {
                if (Handles.Button(Target.Points[i].GetHandle(1).Position, Quaternion.identity, size * 2, size * 2, Handles.SphereHandleCap))
                {
                    SelectIndex(i);
                    SelectHandleIndex(1);
                    Debug.Log("select " + m_selectId + " " + m_handleSelectId);
                }
            }

            if (m_selectId != i || m_handleSelectId != -1)
            {
                if (Handles.Button(Target.Points[i].Position, Quaternion.identity, size * 5, size * 5, Handles.CubeHandleCap))
                {
                    SelectIndex(i);
                    SelectHandleIndex(-1);
                    Debug.Log("select " + m_selectId + " " + m_handleSelectId);
                }
            }

        }
    }

    private void DrawWaypointTransformHandles(int i)
    {
        if (m_drawPathInEditor == false)
            return;

        Handles.color = m_visualSetting.handleColor;

        EditorGUI.BeginChangeCheck();
        Vector3 pos = Target.Points[i].Position;
        Quaternion rot = Target.Points[i].Rotation;
        Vector3 pos_0 = Target.Points[i].GetHandle(0).Position;
        Vector3 pos_1 = Target.Points[i].GetHandle(1).Position;

        if (Tools.current == Tool.Move
               || Tools.current == Tool.Rect
               || Tools.current == Tool.Transform)
        {
            CreateMoveHandle(i, ref pos, ref pos_0, ref pos_1);
        }
        if (Tools.current == Tool.Rotate
            || Tools.current == Tool.Rect
            || Tools.current == Tool.Transform)
        {
            CreateRotateHandle(i, ref rot);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterCompleteObjectUndo(Target, "Transformed waypoint");
            if (m_selectId == i && m_handleSelectId == -1)
            {
                Target.SetAnchorRotation(i, rot);
                Target.SetAnchorPosition(i, pos);
            }
            if (m_selectId == i && m_handleSelectId == 0)
            {
                Target.Points[i].SetHandlePosition(0, pos_0);
            }
            if (m_selectId == i && m_handleSelectId == 1)
            {
                Target.Points[i].SetHandlePosition(1, pos_1);
            }
            Repaint();
        }
    }

    #endregion Scene Methods
    void GetTargetProperties()
    {
        serializedTarget = new SerializedObject(Target);
        isAutoConnectProperty = serializedTarget.FindProperty("isAutoConnect");
    }
}
