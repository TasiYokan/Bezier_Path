using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

[System.Serializable]
public class BezierHandle : IBezierPos
{
    [SerializeField]
    private BezierPoint m_parent;
    [SerializeField]
    private Vector3 m_localPosition;
    [SerializeField]
    private Vector3 m_position;

    public Vector3 Position
    {
        get
        {
            return m_position;
        }

        set
        {
            m_position = value;
            m_localPosition = Quaternion.Inverse(m_parent.Rotation) * (m_position - m_parent.Position);
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
            m_position = m_parent.Rotation * m_localPosition + m_parent.Position;
        }
    }

    public BezierHandle(BezierPoint _parent)
    {
        m_parent = _parent;
        LocalPosition = Vector3.zero;
    }

    public void UpdatePositionBasedOnParent()
    {
        Assert.IsNotNull(m_parent, "Parent should be set before applying local position.");

        LocalPosition = m_localPosition;
    }
}
