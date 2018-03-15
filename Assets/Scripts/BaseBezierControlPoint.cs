using UnityEngine;
using System.Collections;

public class BaseBezierControlPoint : MonoBehaviour
{
    private Vector3 m_position;
    private Vector3 m_dragOffset;
    protected bool m_isMouseDown;

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

    Vector3 DragOffset
    {
        get
        {
            return m_dragOffset;
        }

        set
        {
            m_dragOffset = value;
        }
    }

    public virtual void OnMouseDown()
    {
        m_isMouseDown = true;
        m_dragOffset = transform.position - GetMouseWorldPos();
    }

    protected virtual void OnMouseDrag()
    {
        transform.position = GetMouseWorldPos() + m_dragOffset;
    }

    protected virtual void OnMouseUp()
    {
        m_isMouseDown = false;
    }

    private Vector3 GetMouseWorldPos()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    }
}
