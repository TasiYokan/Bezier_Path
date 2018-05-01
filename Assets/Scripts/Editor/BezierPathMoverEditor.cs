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
    #endregion Editor Variable

    #region Serialized Properties

    private SerializedObject serializedTarget;
    private SerializedProperty onEndCallbackProperty;

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
        DrawProperties();
        serializedTarget.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if (Target.mode == BezierPathMover.MoveMode.NodeBased)
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
        else if (Target.mode == BezierPathMover.MoveMode.DurationBased)
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
        onEndCallbackProperty = serializedTarget.FindProperty("onEndCallback");
    }

    void DrawProperties()
    {
        Target.bezierPath = (BezierCurve)EditorGUILayout.ObjectField(
            "Path", Target.bezierPath, typeof(BezierCurve), true);
        Target.mode = (BezierPathMover.MoveMode)EditorGUILayout.EnumPopup(
            "Mode to manipulate node", Target.mode);
        Target.duration = EditorGUILayout.FloatField(
            "Duration: ", Target.duration);
        Target.velocityCurve = EditorGUILayout.CurveField(
            "Velocity Curve", Target.velocityCurve);
        Target.referenceVelocity = EditorGUILayout.FloatField(
            "Reference Velocity: ", Target.referenceVelocity);
        EditorGUILayout.FloatField(
            "Actual Velocity: ", Target.actualVelocity);
        Target.alwaysForward = EditorGUILayout.Toggle(
            "Always forward", Target.alwaysForward);
        Target.alwaysUpdateCurrentFrag = EditorGUILayout.Toggle(
            "Always update current frag", Target.alwaysUpdateCurrentFrag);
        Target.rotationConstrain = EditorGUILayout.Vector3Field(
            "Rotation constrain", Target.rotationConstrain);
        Target.keepSteadicamStable = EditorGUILayout.Toggle(
            "Keep Steadicam Stable", Target.keepSteadicamStable);
        EditorGUILayout.PropertyField(onEndCallbackProperty, new GUIContent("List Callbacks", ""));
    }
}