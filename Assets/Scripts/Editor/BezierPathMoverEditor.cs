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

    #region Inbuilt APIs
    void OnEnable()
    {
        EditorApplication.update += Update;

        if (Target == null) return;

        InitKeyframes();
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    void OnSceneGUI()
    {
    }

    void InitKeyframes()
    {
        if (Target.speedCurve.keys.Length < Target.bezierPath.Points.Count)
        {
            float interval = 1f / (Target.bezierPath.Points.Count - 1);
            for (int ei = 0; ei < Target.speedCurve.keys.Length; ++ei)
            {
                Keyframe keyframe = Target.speedCurve[ei];
                keyframe.time = ei * interval;
                Target.speedCurve.MoveKey(ei, keyframe);
            }

            for (int i = Target.speedCurve.keys.Length; i < Target.bezierPath.Points.Count; ++i)
            {
                Target.speedCurve.AddKey(i * interval, 1);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Self update");
    }

    #endregion Inbuilt APIs
}