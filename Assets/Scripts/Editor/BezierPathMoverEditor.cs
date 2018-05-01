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

    #region Serialized Properties

    private SerializedObject serializedTarget;
    private SerializedProperty modeProperty;

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
            if (Target.speedCurve.keys.Length < keyFrameNum)
            {
                for (int i = 0; i < Target.speedCurve.keys.Length; ++i)
                {
                    Keyframe keyframe = Target.speedCurve[i];
                    keyframe.time = i * interval;
                    Target.speedCurve.MoveKey(i, keyframe);
                }

                for (int i = Target.speedCurve.keys.Length; i < keyFrameNum; ++i)
                {
                    Target.speedCurve.AddKey(i * interval, 1);
                }
            }
            else if (Target.speedCurve.keys.Length > keyFrameNum)
            {
                for (int i = Target.speedCurve.keys.Length - 1; i > keyFrameNum - 1; --i)
                {
                    Target.speedCurve.RemoveKey(i);
                }
            }

            // Align the last keyframe with the first one since they are the same point
            if (Target.bezierPath.isAutoConnect)
            {
                Keyframe keyframe = Target.speedCurve[keyFrameNum - 1];
                keyframe.value = Target.speedCurve[0].value;
                Target.speedCurve.MoveKey(keyFrameNum - 1, keyframe);
            }
        }
        else if (modeProperty.enumValueIndex == (int)BezierPathMover.MoveMode.DurationBased)
        {
            if (Target.speedCurve[Target.speedCurve.length - 1].time != 1)
            {
                Keyframe keyframe = Target.speedCurve[Target.speedCurve.length - 1];
                keyframe.time = 1;
                Target.speedCurve.MoveKey(Target.speedCurve.length - 1, keyframe);
            }
        }
    }

    void InitKeyframes()
    {
        if (Target.speedCurve == null)
            Target.speedCurve = new AnimationCurve();
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
    }
}