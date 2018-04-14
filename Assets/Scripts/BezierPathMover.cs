using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class BezierPathMover : MonoBehaviour
{
    public BezierCurve bezierPath;
    public float speedInSecond;
    public float referenceSpeedInSecond;
    public AnimationCurve speedCurve;
    /// <summary>
    /// Have bugs sometime, still in alpha phase
    /// </summary>
    public bool keepSteadicamStable = false;
    /// <summary>
    /// Only update the frag it is on to save some computation
    /// </summary>
    public bool alwaysUpdateCurrentFrag;
    /// <summary>
    /// If greater than 0, will overwirte <see cref="speedInSecond"/>
    /// </summary>
    public float duration = -1;
    private int m_curFragId = 0;
    private int m_curSampleId = 0;
    private bool m_isStopped = false;
    // Offset from corresponding curve point 
    private Vector3 m_offset;
    private float m_elapsedTime = 0;

    public Vector3 rotationConstrain = new Vector3(180, 180, 180);

    public UnityEvent onEndCallback;
    public Action<int> onEveryNodeComplete;

    // Use this for initialization
    void Start()
    {
        // For test
        //Invoke("StartMove", 0.01f);
        StartMove();
    }

    // Update is called once per frame
    void Update()
    {
        // If the offset is too tiny, we could hardly notice it actually
        //Debug.DrawLine(path.CurvePoints[curId], transform.position, Color.green);

        if (alwaysUpdateCurrentFrag && m_isStopped == false)
            bezierPath.ForceUpdateOneFrag(m_curFragId);

        // For debug
        if (Input.GetKey(KeyCode.UpArrow))
        {
            speedInSecond *= 1.01f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            speedInSecond *= 0.99f;
        }
        //speed = Mathf.Max(0, speed);
    }

    public void StartMove()
    {
        StartCoroutine(MoveAlongPath());
    }

    private IEnumerator MoveAlongPath()
    {
        m_isStopped = false;
        m_curFragId = 0;
        m_curSampleId = 0;
        m_offset = Vector3.zero;

        //print("Start time: " + Time.time);
        if (duration > 0)
        {
            speedInSecond = bezierPath.totalLength / duration;
        }

        bezierPath.UpdateAnchorTransforms();
        bezierPath.ForceUpdateAllFrags();

        while (true)
        {
            if (duration > 0)
            {
                speedInSecond = 
                    referenceSpeedInSecond * speedCurve.Evaluate(m_elapsedTime / duration);
            }

            // To make mover Steadicam stable
            if (keepSteadicamStable)
                m_curSampleId = bezierPath.Fragments[m_curFragId].FindNearestSampleOnFrag(transform.position, ref m_offset);

            int prevId = m_curFragId;
            bezierPath.GetCurvePos(ref m_curFragId, ref m_curSampleId, speedInSecond * Time.deltaTime, ref m_offset);

            if (m_curFragId != prevId)
            {
                if (onEveryNodeComplete != null)
                    onEveryNodeComplete(m_curFragId);
            }

            if (m_curFragId < 0 || m_curFragId >= bezierPath.Fragments.Count
                || bezierPath.Fragments[m_curFragId].SampleIdWithinFragment(m_curSampleId) == false)
            {
                print("Has reached end");
                //print("Finish time: " + Time.time);
                if (onEndCallback != null)
                    onEndCallback.Invoke();

                m_isStopped = true;
                yield break;
            }

            transform.position =
                bezierPath.Fragments[m_curFragId].SamplePos[m_curSampleId] + m_offset;

            //transform.forward = bezierPath.GetSampleVectorAmongAllFrags(m_curFragId, m_curSampleId, speedInSecond.Sgn());
            //transform.LookAt(bezierPath.GetNextSamplePosAmongAllFrags(m_curFragId, m_curSampleId, speed.Sgn()));
            transform.forward = Vector3.Lerp(transform.forward, bezierPath.GetSampleVectorAmongAllFrags(m_curFragId, m_curSampleId, speedInSecond.Sgn()), speedInSecond * 10 * Time.deltaTime);

            Vector3 rotInEuler = transform.rotation.eulerAngles;
            rotInEuler = new Vector3(
                rotInEuler.x > 180 ? rotInEuler.x - 360 : rotInEuler.x,
                rotInEuler.y > 180 ? rotInEuler.y - 360 : rotInEuler.y,
                rotInEuler.z > 180 ? rotInEuler.z - 360 : rotInEuler.z);

            rotInEuler = new Vector3(
                Mathf.Clamp(rotInEuler.x, -rotationConstrain.x, rotationConstrain.x),
                Mathf.Clamp(rotInEuler.y, -rotationConstrain.y, rotationConstrain.y),
                Mathf.Clamp(rotInEuler.z, -rotationConstrain.z, rotationConstrain.z));

            transform.rotation = Quaternion.Euler(rotInEuler);

            m_elapsedTime += Time.deltaTime;

            yield return null;
        }
    }
}
