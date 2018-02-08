using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierHandle))]
public class BezierHandle_Inspector : Editor
{
    private BezierHandle m_this;

    void OnEnable()
    {
        EditorApplication.update += Update;

        m_this = (BezierHandle)target;
        if (m_this == null) return;
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnSceneGUI()
    {
        m_this.transform.parent.GetComponent<BezierPoint>().UpdatePosition();
    }
}
