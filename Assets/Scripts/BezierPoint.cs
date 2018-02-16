using UnityEngine;
using System.Collections;

public class BezierPoint : MonoBehaviour
{
    private BaseBezierControlPoint m_anchor;
    public BaseBezierControlPoint primaryHandle;
    public BaseBezierControlPoint secondaryHandle;
    private BaseBezierControlPoint m_activeHandle;
    private Vector3 m_position;
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

    public BaseBezierControlPoint Anchor
    {
        get
        {
            if (m_anchor == null)
                m_anchor = GetComponentInChildren<BezierAnchor>();
            return m_anchor;
        }

        set
        {
            m_anchor = value;
        }
    }

    public BaseBezierControlPoint PrimaryHandle
    {
        get
        {
            if (primaryHandle.gameObject.activeInHierarchy)
                return primaryHandle;
            else
                return Anchor;
        }

        set
        {
            primaryHandle = value;
        }
    }

    public BaseBezierControlPoint SecondaryHandle
    {
        get
        {
            if (secondaryHandle.gameObject.activeInHierarchy)
                return secondaryHandle;
            else
                return Anchor;
        }

        set
        {
            secondaryHandle = value;
        }
    }

    public BaseBezierControlPoint ActiveHandle
    {
        get
        {
            return m_activeHandle;
        }

        set
        {
            m_activeHandle = value;
        }
    }

    public Vector3 Position
    {
        get
        {
            m_position = transform.position;
            return m_position;
        }

        set
        {
            m_position = value;
            transform.position = m_position;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            m_rotation = transform.rotation;
            return m_rotation;
        }

        set
        {
            m_rotation = value;
            transform.rotation = m_rotation;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        UpdatePosition();
    }

    /// <summary>
    /// Adjsut secondary handle to the opposite postion of primary one.
    /// </summary>
    private void SmoothHandle(bool _basedOnPrimary = true)
    {
        if (PrimaryHandle == Anchor || SecondaryHandle == Anchor)
            return;

        if (_basedOnPrimary)
        {
            SecondaryHandle.Position = 2 * Anchor.Position - PrimaryHandle.Position;
        }
        else
        {
            PrimaryHandle.Position = 2 * Anchor.Position - SecondaryHandle.Position;
        }
    }

    public void UpdatePosition()
    {
        if (IsAutoSmooth)
        {
            SmoothHandle(m_activeHandle != secondaryHandle);
        }
    }
}
