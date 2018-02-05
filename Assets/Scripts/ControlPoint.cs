using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPoint : MonoBehaviour
{
    public Transform coHandle;
    public Transform anchor;
    public bool isUnderControl;
    public bool isForce; // Control only this point

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(isUnderControl && coHandle != null && isForce == false
            && coHandle.GetComponent<ControlPoint>().isUnderControl == false)
        {
            coHandle.position = 2 * anchor.position - transform.position;
        }
    }
}
