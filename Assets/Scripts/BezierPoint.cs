using UnityEngine;
using System.Collections;

[System.Serializable]
public class BezierPoint
{
    public int activeHandleId = 1;
    
    [SerializeField]
    private BezierHandle[] m_handles = new BezierHandle[2];
    [SerializeField]
    private Vector3 m_position;
    [SerializeField]
    private Quaternion m_rotation;

    [SerializeField]
    private bool m_isAutoSmooth;

    public bool IsAutoSmooth
    {
        get
        {
            return m_isAutoSmooth;
        }

        set
        {
            m_isAutoSmooth = value;
        }
    }

    public Vector3 Position
    {
        get
        {
            return m_position;
        }

        set
        {
            m_position = value;
            UpdateHandlesPosition();
        }
    }

    public Quaternion Rotation
    {
        get
        {
            return m_rotation;
        }

        set
        {
            m_rotation = value;
            UpdateHandlesPosition();
        }
    }

    public BezierHandle[] Handles
    {
        get
        {
            return m_handles;
        }
    }

    public BezierPoint()
    {
        m_position = Vector3.zero;
        m_rotation = Quaternion.identity;

        if (m_handles[0] == null)
            m_handles[0] = new BezierHandle(this);
        if (m_handles[1] == null)
            m_handles[1] = new BezierHandle(this);
    }

    public void UpdateHandlesPosition()
    {
        m_handles[0].UpdatePositionBasedOnParent();
        m_handles[1].UpdatePositionBasedOnParent();
    }

    public BezierHandle GetHandle(int _id)
    {
        return m_handles[_id];
    }

    /// <summary>
    /// Adjsut secondary handle to the opposite postion of primary one.
    /// </summary>
    private void SmoothHandle(bool _basedOnPrimary = true)
    {
        int refId = _basedOnPrimary ? 0 : 1;

        GetHandle(1 - refId).Position = 2 * Position - GetHandle(refId).Position;
    }

    public void UpdatePosition()
    {
        if (IsAutoSmooth)
        {
            SmoothHandle(activeHandleId == 0);
        }
    }
}
