using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierHandle))]
public class BezierHandle_Inspector : Editor
{
    private BezierHandle m_target;

    void OnEnable()
    {
        EditorApplication.update += Update;

        m_target = (BezierHandle)target;
        if (m_target == null) return;
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
        m_target.transform.parent.GetComponent<BezierPoint>().UpdatePosition();

        if (Event.current.type == EventType.MouseDown)
        {
            m_target.transform.parent.GetComponent<BezierPoint>().ActiveHandle = m_target;
        }
    }
}
