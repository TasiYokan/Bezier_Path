using System.Collections;
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
                DrawWaypointHandles(i);
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

    private void DrawBezierCurve()
    {
        if (m_drawPathInEditor == false)
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

    private Vector3 CreateFreeMoveHandle(int _id)
    {
        float size = HandleUtility.GetHandleSize(Target.Points[_id].Position) * 0.3f;

        return Handles.FreeMoveHandle(
            Target.Points[_id].Position,
            (Tools.pivotRotation == PivotRotation.Local)
                ? Target.Points[_id].Rotation : Quaternion.identity,
            size,
            Vector3.zero,
            Handles.CubeHandleCap);
    }

    private Vector3 CreatePositionHandle(int _id)
    {
        return Handles.PositionHandle(
            Target.Points[_id].Position,
            (Tools.pivotRotation == PivotRotation.Local)
                ? Target.Points[_id].Rotation : Quaternion.identity);
    }

    private Quaternion CreateFreeRotateHandle(int _id)
    {
        float size = HandleUtility.GetHandleSize(Target.Points[_id].Position) * 0.4f;

        return Handles.FreeRotateHandle(
            (Tools.pivotRotation == PivotRotation.Local)
                ? Target.Points[_id].Rotation : Quaternion.identity,
            Target.Points[_id].Position,
            size);
    }

    private Quaternion CreateRotationHandle(int _id)
    {
        return Handles.RotationHandle(
            (Tools.pivotRotation == PivotRotation.Local)
                ? Target.Points[_id].Rotation : Quaternion.identity,
            Target.Points[_id].Position);
    }

    private void DrawWaypointHandles(int i)
    {
        if (m_drawPathInEditor == false)
            return;

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
                pos = CreateFreeMoveHandle(i);
            }
            else if (m_manipulateMode == ManipulationMode.SelectAndTransform)
            {
                if (i == m_selectId)
                    pos = CreatePositionHandle(i);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Moved waypoint");
                Target.SetAnchorPosition(i, pos);
                Repaint();
            }
        }
        else if (Tools.current == Tool.Rotate)
        {
            EditorGUI.BeginChangeCheck();
            Quaternion rot = Quaternion.identity;

            if (m_manipulateMode == ManipulationMode.Free)
            {
                rot = CreateFreeRotateHandle(i);
            }
            else if (m_manipulateMode == ManipulationMode.SelectAndTransform)
            {
                if (i == m_selectId)
                    rot = CreateRotationHandle(i);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Target, "Rotated waypoint");
                Target.SetAnchorRotation(i, rot);
                Repaint();
            }
        }
        else if (Tools.current == Tool.Transform || Tools.current == Tool.Rect)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            if (m_manipulateMode == ManipulationMode.Free)
            {
                pos = CreateFreeMoveHandle(i);
                rot = CreateFreeRotateHandle(i);
            }
            else if (m_manipulateMode == ManipulationMode.SelectAndTransform)
            {
                if (i == m_selectId)
                {
                    pos = CreatePositionHandle(i);
                    rot = CreateRotationHandle(i);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, "Transformed waypoint");
                Target.SetAnchorPosition(i, pos);
                Target.SetAnchorRotation(i, rot);
                Repaint();
            }
        }
    }

    #endregion Scene Methods
    void GetTargetProperties()
    {
        serializedTarget = new SerializedObject(Target);
        isAutoConnectProperty = serializedTarget.FindProperty("isAutoConnect");
    }
}
