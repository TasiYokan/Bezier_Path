using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TasiYokan.Utilities.Serialization;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TasiYokan.Curve
{
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
            EditorGUI.BeginChangeCheck();
            Target.bezierPath = (BezierCurve)EditorGUILayout.ObjectField(
                "Path", Target.bezierPath, typeof(BezierCurve), true);
            Target.mode = (BezierPathMover.MoveMode)EditorGUILayout.EnumPopup(
                "Mode to manipulate node", Target.mode);
            Target.velocityCurve = EditorGUILayout.CurveField(
                "Velocity Curve", Target.velocityCurve);
            if (Target.mode == BezierPathMover.MoveMode.DurationBased)
            {
                Target.duration = EditorGUILayout.FloatField(
                    "Duration: ", Target.duration);
                EditorGUILayout.FloatField(
                    "(Readonly) Ref Vel: ", Target.referenceVelocity);
            }
            else if (Target.mode == BezierPathMover.MoveMode.NodeBased)
            {
                Target.referenceVelocity = EditorGUILayout.FloatField(
                    "Reference Velocity: ", Target.referenceVelocity);
            }
            if (EditorGUI.EndChangeCheck())
            {
                //Debug.Log("Has changed something!");
                if (Target.mode == BezierPathMover.MoveMode.DurationBased)
                {
                    Target.bezierPath.InitArcsFromPoints();
                    Target.bezierPath.ForceUpdateAllArcs();
                    float sum_dist = IntegrateCurve(Target.velocityCurve, 0, 1, 100);
                    if (sum_dist < 0)
                        Debug.Log("The destination couldn't be reached since the integral of speed is less than 0!");
                    else
                        Target.referenceVelocity = Target.bezierPath.totalLength / (sum_dist * Target.duration);
                    Debug.Log("sum dist " + sum_dist + " total length: " + Target.bezierPath.totalLength);
                }
            }
            EditorGUILayout.FloatField(
                "Actual Velocity: ", Target.actualVelocity);
            Target.alwaysForward = EditorGUILayout.Toggle(
                "Always forward", Target.alwaysForward);
            Target.alwaysUpdateCurrentArc = EditorGUILayout.Toggle(
                "Always update current arc", Target.alwaysUpdateCurrentArc);
            Target.rotationConstrain = EditorGUILayout.Vector3Field(
                "Rotation constrain", Target.rotationConstrain);
            Target.keepSteadicamStable = EditorGUILayout.Toggle(
                "Keep Steadicam Stable", Target.keepSteadicamStable);
            EditorGUILayout.PropertyField(onEndCallbackProperty, new GUIContent("List Callbacks", ""));
        }

        // Integrate area under AnimationCurve between start and end time
        public static float IntegrateCurve(AnimationCurve _curve, float _startTime, float _endTime, int _steps)
        {
            return Integrate(_curve.Evaluate, _startTime, _endTime, _steps);
        }

        // Integrate function f(x) using the trapezoidal rule between x=x_low..x_high
        public static float Integrate(Func<float, float> _evaFunc, float _xMin, float _xMax, int _steps)
        {
            float h = (_xMax - _xMin) / _steps;
            float res = (_evaFunc(_xMin) + _evaFunc(_xMax)) / 2;
            for (int i = 1; i < _steps; i++)
            {
                res += _evaFunc(_xMin + i * h);
            }
            return h * res;
        }
    }
}