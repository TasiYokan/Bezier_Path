using UnityEngine;
using System.Collections;

public class BezierAnchor : BaseBezierControlPoint
{
    public override void OnMouseDown()
    {
        base.OnMouseDown();

        if (Input.GetKey(KeyCode.Space))
        {
            if(transform.parent.GetComponent<BezierPoint>().PrimaryHandle == this)
            {
                transform.parent.GetComponent<BezierPoint>().primaryHandle.gameObject.SetActive(true);
                //transform.parent.GetComponent<BezierPoint>().PrimaryHandle.OnMouseDown();
                //this.OnMouseUp();
            }
            else if(transform.parent.GetComponent<BezierPoint>().SecondaryHandle == this)
            {
                transform.parent.GetComponent<BezierPoint>().secondaryHandle.gameObject.SetActive(true);
                //transform.parent.GetComponent<BezierPoint>().SecondaryHandle.OnMouseDown();
                //this.OnMouseUp();
            }
        }
    }

    private void Update()
    {
        // Make it always at the center of BezierPoint
        // To make Hanlde.Button to easier track BezierPoint
        transform.parent.position = this.Position;
    }
}
