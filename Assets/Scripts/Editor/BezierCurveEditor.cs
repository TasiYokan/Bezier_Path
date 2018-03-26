using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
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
        public Color activeHandleColor = Color.yellow;
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
    private int m_selectedId = -1;
    private int m_selectedHandleId = -1;
    private bool m_drawPathInEditor = true;
    private ReorderableList m_reorderablePointsList;

    #endregion Editor Variable

    #region Editor GUIs

    private GUIContent addPointContent = new GUIContent("Add WayPoint", "Add a BezierPoint");
    private GUIContent deletePointContent = new GUIContent("Delete", "Deletes this BezierPoint");
    private GUIContent resetPointContent = new GUIContent("Reset", "Select this BezierPoint");
    private GUIContent gotoPointContent = new GUIContent("Goto", "Select this BezierPoint");
    private GUIContent clearAllPointsContent = new GUIContent("Clear All", "Delete all BezierPoint");

    #endregion Editor GUIs

    #region Serialized Properties

    private SerializedObject serializedTarget;
    private SerializedProperty isAutoConnectProperty;
    private SerializedProperty totalSampleCountProperty;
    private SerializedProperty drawDebugPathProperty;

    #endregion Serialized Properties

    #region Inbuilt APIs
    void OnEnable()
    {
        EditorApplication.update += Update;

        if (Target == null) return;

        SetupEditorVariables();
        GetTargetProperties();
        InitReorderableList();
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    public override void OnInspectorGUI()
    {
        serializedTarget.Update();
        DrawButtons();
        //DrawRawPointsValue();
        float restoreLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 80;
        EditorGUIUtility.wideMode = true;
        m_reorderablePointsList.DoLayoutList();
        EditorGUIUtility.labelWidth = restoreLabelWidth;
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
        GUILayout.BeginHorizontal();
        GUILayout.Label("Total Sample Count: ", GUILayout.Width(Screen.width / 2f));
        totalSampleCountProperty.intValue = EditorGUILayout.IntField(totalSampleCountProperty.intValue);
        GUILayout.EndHorizontal();
        drawDebugPathProperty.boolValue = GUILayout.Toggle(drawDebugPathProperty.boolValue,
            "Use LineRenderer to draw path in game?", GUILayout.Width(Screen.width));

        if (GUILayout.Button(addPointContent))
        {
            BezierPoint point = new BezierPoint(
                SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 3f,
                Quaternion.identity,
                true);
            Target.AddPoint(point);
        }

        if (GUILayout.Button(clearAllPointsContent))
        {
            //TODO: Use Target.RemoveAll() later
            Target.Points.Clear();
        }

        GUILayout.Space(10);
    }

    void InitReorderableList()
    {
        Debug.Log("InitReorderableList");
        //m_reorderablePointsList = CreateList(serializedObject, serializedObject.FindProperty("Points"));
        SerializedProperty prop = serializedObject.FindProperty("Points");
        m_reorderablePointsList = new ReorderableList(serializedObject, prop, true, true, false, false);

        List<float> heights = new List<float>(prop.arraySize);

        float singleLine = EditorGUIUtility.singleLineHeight;
        m_reorderablePointsList.elementHeight *= 5;// singleLine * 10f;


        bool showTransform = true;

        m_reorderablePointsList.drawElementCallback = (rect, index, active, focused) =>
        {
            if (index > Target.Points.Count - 1)
                return;
            float startWidth = rect.width;
            float startX = rect.x;


            rect.height = singleLine;
            rect.width = startWidth * 0.2f;
            rect.x += startWidth * 0.05f;
            GUI.Label(rect, "# " + index);
            rect.x += startWidth * 0.25f;
            if (GUI.Button(rect, deletePointContent))
            {
                Undo.RecordObject(Target, "Deleted a waypoint");
                Target.RemovePoint(Target.Points[index]);
                SceneView.RepaintAll();
            }
            rect.x += startWidth * 0.25f;
            if (GUI.Button(rect, resetPointContent))
            {
                Undo.RecordObject(Target, "Reset a waypoint");
                Target.SetAnchorLocalPosition(index, Vector3.zero);
                Target.SetAnchorLocalRotation(index, Quaternion.identity);
                Target.Points[index].SetHandleLocalPosition(0, Vector3.zero);
                Target.Points[index].SetHandleLocalPosition(1, Vector3.zero);
            }
            rect.x += startWidth * 0.25f;
            if (GUI.Button(rect, gotoPointContent))
            {
                Debug.Log("Goto " + index);
                m_selectedId = index;
                m_selectedHandleId = -1;
                SceneView.lastActiveSceneView.pivot = Target.Points[index].Position;
                SceneView.lastActiveSceneView.size = 3;
                SceneView.lastActiveSceneView.Repaint();
            }
            rect.y += singleLine + 10;

            rect.width = startWidth;
            rect.x = startX;


            float height = EditorGUIUtility.singleLineHeight * 2.5f;
            if (active)
            {
                showTransform = true;

                rect.y -= EditorGUIUtility.singleLineHeight * 0.25f;
                showTransform = EditorGUI.Foldout(rect, showTransform, "transform", true, EditorStyles.foldout);
                if (showTransform)
                {
                    rect.y += EditorGUIUtility.singleLineHeight;
                    height = EditorGUIUtility.singleLineHeight * 7;

                    EditorGUI.BeginChangeCheck();
                    //GUILayout.BeginVertical("Box");
                    Vector3 pos = EditorGUI.Vector3Field(rect, "Anchor Pos",
                        Target.Points[index].LocalPosition);
                    rect.y += singleLine * 1;

                    Vector3 rotInEuler = EditorGUI.Vector3Field(rect, "Anchor Rot",
                        Target.Points[index].LocalRotation.eulerAngles);
                    rect.y += singleLine * 1;

                    Vector3 pos_0 = EditorGUI.Vector3Field(rect, "Handle 1th",
                        Target.Points[index].GetHandle(0).LocalPosition);
                    rect.y += singleLine * 1;

                    Vector3 pos_1 = EditorGUI.Vector3Field(rect, "Handle 2rd",
                        Target.Points[index].GetHandle(1).LocalPosition);
                    rect.y += singleLine * 1;

                    //GUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(Target, "Changed handle transform");
                        Target.SetAnchorLocalRotation(index, Quaternion.Euler(rotInEuler));
                        Target.SetAnchorLocalPosition(index, pos);
                        Target.Points[index].SetHandleLocalPosition(0, pos_0);
                        Target.Points[index].SetHandleLocalPosition(1, pos_1);
                        SceneView.RepaintAll();
                    }
                }
            }
            else
            {
                rect.y -= EditorGUIUtility.singleLineHeight * 0.25f;

                showTransform = EditorGUI.Foldout(rect, showTransform, "transform", true, EditorStyles.foldout);
                if (showTransform)
                {
                    rect.y += EditorGUIUtility.singleLineHeight;
                    height = EditorGUIUtility.singleLineHeight * 7;

                    Rect area = new Rect(rect);
                    area.y -= EditorGUIUtility.singleLineHeight * 0.2f;
                    area.x += 2f;
                    area.height *= 4.3f;
                    EditorGUI.DrawRect(area, Color.white);

                    EditorGUI.BeginChangeCheck();
                    //GUILayout.BeginVertical("Box");
                    Vector3 pos = EditorGUI.Vector3Field(rect, "Anchor Pos",
                        Target.Points[index].LocalPosition);
                    rect.y += singleLine * 1;

                    Vector3 rotInEuler = EditorGUI.Vector3Field(rect, "Anchor Rot",
                        Target.Points[index].LocalRotation.eulerAngles);
                    rect.y += singleLine * 1;

                    Vector3 pos_0 = EditorGUI.Vector3Field(rect, "Handle 1th",
                        Target.Points[index].GetHandle(0).LocalPosition);
                    rect.y += singleLine * 1;

                    Vector3 pos_1 = EditorGUI.Vector3Field(rect, "Handle 2rd",
                        Target.Points[index].GetHandle(1).LocalPosition);
                    rect.y += singleLine * 1;

                    //GUILayout.EndVertical();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(Target, "Changed handle transform");
                        Target.SetAnchorLocalRotation(index, Quaternion.Euler(rotInEuler));
                        Target.SetAnchorLocalPosition(index, pos);
                        Target.Points[index].SetHandleLocalPosition(0, pos_0);
                        Target.Points[index].SetHandleLocalPosition(1, pos_1);
                        SceneView.RepaintAll();
                    }
                }
            }

            if (heights.Count > 0)
                heights[index] = height;

            float[] floats = heights.ToArray();
            Array.Resize(ref floats, prop.arraySize);
            heights = floats.ToList();
        };

        m_reorderablePointsList.elementHeightCallback = (index) =>
        {
            Repaint();
            float height = 0;

            if (heights.Count > 0)
                height = heights[index];

            float[] floats = heights.ToArray();
            Array.Resize(ref floats, prop.arraySize);
            heights = floats.ToList();

            return height;
        };

        m_reorderablePointsList.onSelectCallback = list =>
        {
            showTransform = true;
            //Debug.Log("Select " + list.index);
            m_selectedId = list.index;
            m_selectedHandleId = -1;
            SceneView.lastActiveSceneView.pivot = Target.Points[list.index].Position;
            SceneView.lastActiveSceneView.size = 3;
            SceneView.lastActiveSceneView.Repaint();
            SceneView.RepaintAll();
        };
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
            Target.Points[_pointId].LocalPosition, GUILayout.MinWidth(300));
        Vector3 rotInEuler = EditorGUILayout.Vector3Field("Anchor Rot",
            Target.Points[_pointId].LocalRotation.eulerAngles, GUILayout.MinWidth(300));
        Vector3 pos_0 = EditorGUILayout.Vector3Field("Handle 1th",
            Target.Points[_pointId].GetHandle(0).LocalPosition, GUILayout.MinWidth(300));
        Vector3 pos_1 = EditorGUILayout.Vector3Field("Handle 2rd",
            Target.Points[_pointId].GetHandle(1).LocalPosition, GUILayout.MinWidth(300));
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

        //GUILayout.BeginVertical();
        //if (GUILayout.Button(deletePointContent))
        //{
        //    Undo.RecordObject(Target, "Deleted a waypoint");
        //    Target.RemovePoint(Target.Points[_pointId]);
        //    SceneView.RepaintAll();
        //}
        //if (GUILayout.Button(resetPointContent))
        //{
        //    Undo.RecordObject(Target, "Reset a waypoint");
        //    Target.SetAnchorLocalPosition(_pointId, Vector3.zero);
        //    Target.SetAnchorLocalRotation(_pointId, Quaternion.identity);
        //    Target.Points[_pointId].SetHandleLocalPosition(0, Vector3.zero);
        //    Target.Points[_pointId].SetHandleLocalPosition(1, Vector3.zero);
        //}
        //if (GUILayout.Button(gotoPointContent))
        //{
        //    Debug.Log("Goto " + _pointId);
        //    m_selectedId = _pointId;
        //    m_selectedHandleId = -1;
        //    SceneView.lastActiveSceneView.pivot = Target.Points[_pointId].Position;
        //    SceneView.lastActiveSceneView.size = 3;
        //    SceneView.lastActiveSceneView.Repaint();
        //}
        //GUILayout.EndVertical();

        GUILayout.EndHorizontal();

    }

    #endregion Inspector Methods
    #region Scene Methods

    void SelectIndex(int _id)
    {
        m_selectedId = _id;
        Repaint();
    }

    void SelectHandleIndex(int _id)
    {
        m_selectedHandleId = _id;
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
            EditorGUI.BeginChangeCheck();
            _pos = Handles.FreeMoveHandle(
                pos, viewRot, size * 3, Vector3.zero, Handles.CubeHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                m_selectedHandleId = _id;
                SelectHandleIndex(-1);
            }

            EditorGUI.BeginChangeCheck();
            _pos_0 = Handles.FreeMoveHandle(
                _pos_0, viewRot, size, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                m_selectedHandleId = _id;
                SelectHandleIndex(0);
            }

            EditorGUI.BeginChangeCheck();
            _pos_1 = Handles.FreeMoveHandle(
                _pos_1, viewRot, size, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                m_selectedHandleId = _id;
                SelectHandleIndex(1);
            }
        }
        else if (m_manipulateMode == ManipulationMode.SelectAndTransform
            && _id == m_selectedId)
        {
            if (m_selectedHandleId == -1)
            {
                _pos = Handles.PositionHandle(pos, viewRot);
            }

            if (m_selectedHandleId == 0)
            {
                _pos_0 = Handles.PositionHandle(_pos_0, viewRot);
            }
            if (m_selectedHandleId == 1)
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
            && _id == m_selectedId)
        {
            if (m_selectedHandleId == -1)
            {
                _rot = Handles.RotationHandle(viewRot, pos);
            }
        }
    }

    private void DrawWaypointSelectHandles(int i)
    {
        if (m_manipulateMode == ManipulationMode.Free)
            return;

        float size = HandleUtility.GetHandleSize(Target.Points[i].Position) * 0.2f;
        if (Event.current.button != 1)
        {
            if (m_selectedId != i || m_selectedHandleId != -1)
            {
                Handles.color = m_visualSetting.handleColor;

                if (Handles.Button(Target.Points[i].Position, Target.Points[i].Rotation, size * 1.5f, size * 1.5f, Handles.CubeHandleCap))
                {
                    SelectIndex(i);
                    SelectHandleIndex(-1);

                    m_reorderablePointsList.index = i;
                }
            }
            else
            {
                Handles.color = m_visualSetting.activeHandleColor;
                //Handles.DrawWireCube( Target.Points[i].Position, Vector3.one * size * 5);
                Handles.CubeHandleCap(0, Target.Points[i].Position, Target.Points[i].Rotation, size * 1.5f, Event.current.type);
            }

            // Force all its handles to be highlighted
            if (m_selectedHandleId == -1)
                Handles.color = m_visualSetting.activeHandleColor;

            if (m_selectedId != i || m_selectedHandleId != 0)
            {
                Handles.color = m_visualSetting.handleColor;

                if (Handles.Button(Target.Points[i].GetHandle(0).Position, Quaternion.identity, size, size, Handles.SphereHandleCap))
                {
                    SelectIndex(i);
                    SelectHandleIndex(0);

                    m_reorderablePointsList.index = i;
                }
            }
            else
            {
                Handles.color = m_visualSetting.activeHandleColor;
                Handles.SphereHandleCap(0, Target.Points[i].GetHandle(0).Position, Quaternion.identity, size, Event.current.type);
            }

            if (m_selectedId != i || m_selectedHandleId != 1)
            {
                Handles.color = m_visualSetting.handleColor;

                if (Handles.Button(Target.Points[i].GetHandle(1).Position, Quaternion.identity, size, size, Handles.SphereHandleCap))
                {
                    SelectIndex(i);
                    SelectHandleIndex(1);

                    m_reorderablePointsList.index = i;
                }
            }
            else
            {
                Handles.color = m_visualSetting.activeHandleColor;
                Handles.SphereHandleCap(0, Target.Points[i].GetHandle(1).Position, Quaternion.identity, size, Event.current.type);
            }

        }
    }

    private void DrawWaypointTransformHandles(int i)
    {
        if (m_drawPathInEditor == false)
            return;

        if (m_manipulateMode == ManipulationMode.SelectAndTransform && i != m_selectedId)
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
            if (m_selectedHandleId == -1)
            {
                Target.SetAnchorRotation(i, rot);
                Target.SetAnchorPosition(i, pos);
            }
            else if (m_selectedHandleId == 0)
            {
                Target.Points[i].SetHandlePosition(0, pos_0);
            }
            else if (m_selectedHandleId == 1)
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
        totalSampleCountProperty = serializedTarget.FindProperty("totalSampleCount");
        drawDebugPathProperty = serializedTarget.FindProperty("drawDebugPath");
    }
}
