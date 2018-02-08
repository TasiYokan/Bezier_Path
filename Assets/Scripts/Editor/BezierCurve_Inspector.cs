using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurve_Inspector : Editor
{
    private BezierCurve m_this;

    void OnEnable()
    {
        EditorApplication.update += Update;

        m_this = (BezierCurve)target;
        if (m_this == null) return;
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Self update");
    }

    void OnSceneGUI()
    {
        //m_this.UpdateAllPointPoses();
    }
}
