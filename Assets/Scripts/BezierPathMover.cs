using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

public class BezierPathMover : MonoBehaviour
{
    public enum MoveMode
    {
        NodeBased = 0,
        DurationBased = 1
    }

    public MoveMode mode;
    public BezierCurve bezierPath;
    public float actualVelocity;
    public float referenceVelocity;
    public bool alwaysForward = false;
    public AnimationCurve velocityCurve;
    /// <summary>
    /// Have bugs sometime, still in alpha phase
    /// </summary>
    public bool keepSteadicamStable = false;
    /// <summary>
    /// Only update the arc it is on to save some computation
    /// </summary>
    public bool alwaysUpdateCurrentArc;
    /// <summary>
    /// If greater than 0, will overwirte <see cref="actualVelocity"/>
    /// </summary>
    public float duration = -1;
    private int m_curArcId = 0;
    private int m_curSampleId = 0;
    // Denote the mover is moving along the order of list or not
    private int m_dirSgn = 0;
    private bool m_isStopped = false;
    // Offset from corresponding curve point 
    private Vector3 m_offset;
    private float m_offsetLength;
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

        if (alwaysUpdateCurrentArc && m_isStopped == false)
            bezierPath.ForceUpdateOneArc(m_curArcId);

        // For debug
        if (Input.GetKey(KeyCode.UpArrow))
        {
            actualVelocity *= 1.01f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            actualVelocity *= 0.99f;
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
        m_curArcId = 0;
        m_curSampleId = 0;
        m_dirSgn = 0;
        m_offset = Vector3.zero;
        m_elapsedTime = 0;

        bezierPath.UpdateAnchorTransforms();
        //bezierPath.ForceUpdateAllArcs();
        bezierPath.InitArcsFromPoints();

        //print("Start time: " + Time.time);
        //if (duration > 0)
        //{
        //    actualVelocity = bezierPath.totalLength / duration;
        //    referenceVelocity = actualVelocity;
        //}

        actualVelocity = referenceVelocity;
        m_dirSgn = 1;
        m_offsetLength = 0;

        #region old method
        //while (true)
        //{

        //    // To make mover Steadicam stable
        //    //if (keepSteadicamStable)
        //    //    m_curSampleId = bezierPath.Arcs[m_curArcId].FindNearestSampleOnArc(transform.position, ref m_offset);

        //    // Should switch the back and front sample point of this mover
        //    if (m_dirSgn * actualVelocity.Sgn() < 0)
        //    {
        //        // These are next ids
        //        int oldArcId = m_curArcId;
        //        int oldSampleId = m_curSampleId;
        //        bezierPath.GetNextId(ref m_curArcId, ref m_curSampleId, m_dirSgn);

        //        Vector3 oldToNew = bezierPath.GetSamplePos(m_curArcId, m_curSampleId)
        //            - bezierPath.GetSamplePos(oldArcId, oldSampleId);
        //        // Update offset after switching base
        //        m_offset = m_offset - oldToNew;
        //        m_dirSgn = actualVelocity.Sgn();
        //    }

        //    int prevId = m_curArcId;
        //    bezierPath.GetCurvePos(ref m_curArcId, ref m_curSampleId, actualVelocity * Time.deltaTime, ref m_offset);

        //    if (m_curArcId != prevId)
        //    {
        //        if (onEveryNodeComplete != null)
        //            onEveryNodeComplete(m_curArcId);
        //    }

        //    if (m_curArcId < 0 || m_curArcId >= bezierPath.Arcs.Count
        //        || bezierPath.Arcs[m_curArcId].SampleIdWithinArc(m_curSampleId) == false)
        //    {
        //        print("Has reached end with time: " + m_elapsedTime);
        //        //print("Finish time: " + Time.time);
        //        if (onEndCallback != null)
        //            onEndCallback.Invoke();

        //        m_isStopped = true;
        //        yield break;
        //    }

        //    transform.position =
        //        bezierPath.Arcs[m_curArcId].SamplePos[m_curSampleId] + m_offset;

        //    //transform.forward = bezierPath.GetSampleVectorAmongAllArcs(m_curArcId, m_curSampleId, speedInSecond.Sgn());
        //    //transform.LookAt(bezierPath.GetNextSamplePosAmongAllArcs(m_curArcId, m_curSampleId, speed.Sgn()));
        //    int curArcId = m_curArcId;
        //    int curSampleId = m_curSampleId;
        //    if (alwaysForward == true && actualVelocity.Sgn() < 0)
        //    {
        //        bezierPath.GetNextId(ref curArcId, ref curSampleId, -1);
        //    }
        //    int nextArcId = curArcId;
        //    if (bezierPath.isAutoConnect)
        //    {
        //        // Find connected arc
        //        nextArcId = (nextArcId + 1 + bezierPath.Arcs.Count) % bezierPath.Arcs.Count;
        //    }
        //    else
        //    {
        //        nextArcId = Mathf.Clamp(nextArcId + 1, 0, bezierPath.Points.Count - 1);
        //    }

        //    //transform.forward = Vector3.Lerp(transform.forward,
        //    //    bezierPath.GetSampleVectorAmongAllArcs(curArcId, curSampleId, alwaysForward ? 1 : speedInSecond.Sgn()),
        //    //    Mathf.Abs(speedInSecond) * 1 * Time.deltaTime);
        //    Vector3 curArcVector = bezierPath.Points[nextArcId].Position - bezierPath.Points[curArcId].Position;
        //    Vector3 transVector = this.transform.position - bezierPath.Points[curArcId].Position;
        //    float offsetLength = Vector3.Dot(curArcVector, transVector) / curArcVector.sqrMagnitude;
        //    transform.forward = bezierPath.Arcs[m_curArcId].CalculateCubicBezierVelocity(offsetLength)
        //        * (alwaysForward ? 1 : actualVelocity.Sgn());

        //    Vector3 rotInEuler = transform.rotation.eulerAngles;
        //    rotInEuler = new Vector3(
        //        rotInEuler.x > 180 ? rotInEuler.x - 360 : rotInEuler.x,
        //        rotInEuler.y > 180 ? rotInEuler.y - 360 : rotInEuler.y,
        //        rotInEuler.z > 180 ? rotInEuler.z - 360 : rotInEuler.z);

        //    rotInEuler = new Vector3(
        //        Mathf.Clamp(rotInEuler.x, -rotationConstrain.x, rotationConstrain.x),
        //        Mathf.Clamp(rotInEuler.y, -rotationConstrain.y, rotationConstrain.y),
        //        Mathf.Clamp(rotInEuler.z, -rotationConstrain.z, rotationConstrain.z));

        //    transform.rotation = Quaternion.Euler(rotInEuler);

        //    m_elapsedTime += Time.deltaTime;

        //    // Update speed based on curve
        //    if (mode == MoveMode.NodeBased)
        //    {
        //        if (referenceVelocity > 0)
        //        {
        //            float interval = 1f / bezierPath.Arcs.Count;

        //            actualVelocity =
        //                referenceVelocity
        //                * velocityCurve.Evaluate(interval * (curArcId + offsetLength));
        //        }
        //    }
        //    else if (mode == MoveMode.DurationBased)
        //    {
        //        if (referenceVelocity > 0)
        //        {
        //            float interval = 1f / bezierPath.Arcs.Count;

        //            actualVelocity =
        //                referenceVelocity
        //                * velocityCurve.Evaluate((m_elapsedTime % duration) / duration);
        //        }
        //    }

        //    yield return null;
        //}
        #endregion old method

        print("Curve Length " + bezierPath.totalLength);

        while (true)
        {
            if (m_dirSgn != actualVelocity.Sgn())
            {
                m_offsetLength = bezierPath.Arcs[m_curArcId].Length - m_offsetLength;
                m_dirSgn = actualVelocity.Sgn();
            }
            float lengthInArc = m_offsetLength
                + m_dirSgn * actualVelocity * Time.deltaTime;

            // nextArcId: For this frame, which arc we should move to
            int nextArcId = m_curArcId;

            while (nextArcId >= 0 && nextArcId <= bezierPath.Arcs.Count - 1)
            {
                if (bezierPath.Arcs[nextArcId].Length > lengthInArc)
                    break;

                lengthInArc -= bezierPath.Arcs[nextArcId].Length;

                nextArcId = m_curArcId + m_dirSgn;
                if (bezierPath.isAutoConnect)
                    nextArcId = (nextArcId + bezierPath.Arcs.Count) % bezierPath.Arcs.Count;
            }

            if (nextArcId >= 0 && nextArcId <= bezierPath.Arcs.Count - 1)
            {
                m_curArcId = nextArcId;
                m_offsetLength = lengthInArc;
                transform.position =
                    bezierPath.Arcs[m_curArcId].CalculateCubicBezierPos(
                    bezierPath.Arcs[m_curArcId].MapToUniform(
                        (m_dirSgn > 0 ? m_offsetLength : (bezierPath.Arcs[m_curArcId].Length - m_offsetLength))
                        / bezierPath.Arcs[m_curArcId].Length));

                yield return null;
            }
            else
            {
                print("Has reached end with time: " + m_elapsedTime);
                if (onEndCallback != null)
                    onEndCallback.Invoke();
                yield break;
            }

            m_elapsedTime += Time.deltaTime;

            // Update speed based on curve
            if (mode == MoveMode.NodeBased)
            {
                if (referenceVelocity > 0)
                {
                    float interval = 1f / bezierPath.Arcs.Count;

                    actualVelocity =
                        referenceVelocity
                        * velocityCurve.Evaluate(interval * (m_curArcId 
                            + m_dirSgn * (m_dirSgn > 0 ? m_offsetLength : (bezierPath.Arcs[m_curArcId].Length - m_offsetLength))
                        / bezierPath.Arcs[m_curArcId].Length));
                }
            }
            else if (mode == MoveMode.DurationBased)
            {
                if (referenceVelocity > 0)
                {
                    float interval = 1f / bezierPath.Arcs.Count;

                    actualVelocity =
                        referenceVelocity
                        * velocityCurve.Evaluate((m_elapsedTime % duration) / duration);
                }
            }

        }
    }
}
