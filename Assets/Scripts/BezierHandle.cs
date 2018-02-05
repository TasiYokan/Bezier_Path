using UnityEngine;
using System.Collections;

public class BezierHandle : BaseBezierControlPoint
{
    public override void OnMouseDown()
    {
        base.OnMouseDown();

        transform.parent.GetComponent<BezierPoint>().ActiveHandle = this;

        if(Input.GetKey(KeyCode.LeftAlt)||Input.GetKey(KeyCode.RightAlt))
        {
            transform.parent.GetComponent<BezierPoint>().IsAutoSmooth = false;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            transform.parent.GetComponent<BezierPoint>().IsAutoSmooth = true;
        }
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        //transform.parent.GetComponent<BezierPoint>().ActiveHandle = this;
    }
}
