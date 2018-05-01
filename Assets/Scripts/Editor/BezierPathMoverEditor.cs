using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TasiYokan.Utilities.Serialization;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(BezierPathMover))]
public class BezierPathMoverEditor : Editor
{
    #region Editor Variable
    private BezierPathMover m_target;

    public BezierPathMover Target
    {
        get
        {
            if (m_target == null)
                m_target = (BezierPathMover)target;
            return m_target;
        }
    }

    AnimationCurve speedCurve;
    #endregion Editor Variable

    #region Serialized Properties

    private SerializedObject serializedTarget;
    private SerializedProperty modeProperty;
    private SerializedProperty durationProperty;
    private SerializedProperty referenceVelocityProperty;
    private SerializedProperty bezierPathProperty;
    private SerializedProperty alwaysForwardProperty;
    private SerializedProperty alwaysUpdateCurrentFragProperty;
    private SerializedProperty rotationConstrainProperty;

    #endregion Serialized Properties

    #region Inbuilt APIs
    void OnEnable()
    {
        EditorApplication.update += Update;

        if (Target == null) return;

        GetTargetProperties();

        InitKeyframes();
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    public override void OnInspectorGUI()
    {
        serializedTarget.Update();

        //Target.speedCurve = EditorGUILayout.CurveField(speedCurve, GUILayout.Width(Screen.width / 3f));
        DrawProperties();
        serializedTarget.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }

    void OnSceneGUI()
    {
        if (modeProperty.enumValueIndex == (int)BezierPathMover.MoveMode.NodeBased)
        {
            int keyFrameNum = Target.bezierPath.Points.Count + (Target.bezierPath.isAutoConnect ? 1 : 0);
            //Debug.Log("points " + keyFrameNum);

            float interval = 1f / (keyFrameNum - 1);
            if (Target.velocityCurve.keys.Length < keyFrameNum)
            {
                for (int i = 0; i < Target.velocityCurve.keys.Length; ++i)
                {
                    Keyframe keyframe = Target.velocityCurve[i];
                    keyframe.time = i * interval;
                    Target.velocityCurve.MoveKey(i, keyframe);
                }

                for (int i = Target.velocityCurve.keys.Length; i < keyFrameNum; ++i)
                {
                    Target.velocityCurve.AddKey(i * interval, 1);
                }
            }
            else if (Target.velocityCurve.keys.Length > keyFrameNum)
            {
                for (int i = Target.velocityCurve.keys.Length - 1; i > keyFrameNum - 1; --i)
                {
                    Target.velocityCurve.RemoveKey(i);
                }
            }

            // Align the last keyframe with the first one since they are the same point
            if (Target.bezierPath.isAutoConnect)
            {
                Keyframe keyframe = Target.velocityCurve[keyFrameNum - 1];
                keyframe.value = Target.velocityCurve[0].value;
                Target.velocityCurve.MoveKey(keyFrameNum - 1, keyframe);
            }
        }
        else if (modeProperty.enumValueIndex == (int)BezierPathMover.MoveMode.DurationBased)
        {
            if (Target.velocityCurve[Target.velocityCurve.length - 1].time != 1)
            {
                Keyframe keyframe = Target.velocityCurve[Target.velocityCurve.length - 1];
                keyframe.time = 1;
                Target.velocityCurve.MoveKey(Target.velocityCurve.length - 1, keyframe);
            }
        }
    }

    void InitKeyframes()
    {
        if (Target.velocityCurve == null)
            Target.velocityCurve = new AnimationCurve();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Self update");
    }

    #endregion Inbuilt APIs

    void GetTargetProperties()
    {
        serializedTarget = new SerializedObject(Target);
        modeProperty = serializedTarget.FindProperty("mode");
        durationProperty = serializedTarget.FindProperty("duration");
        referenceVelocityProperty = serializedObject.FindProperty("referenceVelocity");
        alwaysForwardProperty = serializedObject.FindProperty("alwaysForward");
        alwaysUpdateCurrentFragProperty = serializedObject.FindProperty("alwaysUpdateCurrentFrag");
        rotationConstrainProperty = serializedObject.FindProperty("rotationConstrain");
    }

    void DrawProperties()
    {
        modeProperty.enumValueIndex = Convert.ToInt32(EditorGUILayout.EnumPopup(
            "Mode to manipulate node", (BezierPathMover.MoveMode)modeProperty.enumValueIndex));

        durationProperty.floatValue = EditorGUILayout.FloatField("Duration: ", durationProperty.floatValue);
        referenceVelocityProperty.floatValue = EditorGUILayout.FloatField("Reference Velocity: ", referenceVelocityProperty.floatValue);
        EditorGUILayout.FloatField("Actual Velocity: ", Target.actualVelocity);
        alwaysForwardProperty.boolValue = EditorGUILayout.Toggle("Always forward", alwaysForwardProperty.boolValue);
        alwaysUpdateCurrentFragProperty.boolValue = EditorGUILayout.Toggle("Always update current frag", alwaysUpdateCurrentFragProperty.boolValue);
        rotationConstrainProperty.vector3Value = EditorGUILayout.Vector3Field("Rotation constrain", rotationConstrainProperty.vector3Value);
    }
}