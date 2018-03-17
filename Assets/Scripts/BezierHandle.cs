using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

[System.Serializable]
public class BezierHandle : IBezierPos
{
    [SerializeField]
    private Vector3 m_position;
    [SerializeField]
    private Vector3 m_localPosition;
    

    public Vector3 Position
    {
        get
        {
            return m_position;
        }

        set
        {
            m_position = value;
        }
    }

    public Vector3 LocalPosition
    {
        get
        {
            return m_localPosition;
        }

        set
        {
            m_localPosition = value;
        }
    }
}
