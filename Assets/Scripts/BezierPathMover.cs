using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

namespace TasiYokan.Curve
{
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
        //private int m_curSampleId = 0;
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
            //m_curSampleId = 0;
            m_dirSgn = 0;
            m_offset = Vector3.zero;
            m_elapsedTime = 0;

            bezierPath.UpdateAnchorTransforms();
            bezierPath.InitArcsFromPoints();
            bezierPath.ForceUpdateAllArcs();

            //print("Start time: " + Time.time);
            //if (duration > 0)
            //{
            //    actualVelocity = bezierPath.totalLength / duration;
            //    referenceVelocity = actualVelocity;
            //}

            actualVelocity = referenceVelocity;
            m_dirSgn = 1;
            m_offsetLength = 0;

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
                    if (bezierPath.Arcs[nextArcId].Length >= lengthInArc)
                        break;

                    lengthInArc -= bezierPath.Arcs[nextArcId].Length;

                    nextArcId = m_curArcId + m_dirSgn;
                    if (bezierPath.isAutoConnect)
                        nextArcId = (nextArcId + bezierPath.Arcs.Count) % bezierPath.Arcs.Count;

                    // If it moves too fast, we will trigger all nodes that reach in one frame
                    //if (onEveryNodeComplete != null)
                    //    onEveryNodeComplete(nextArcId);
                }

                if (nextArcId >= 0 && nextArcId <= bezierPath.Arcs.Count - 1)
                {
                    m_curArcId = nextArcId;
                    m_offsetLength = lengthInArc;
                    float uniformT = bezierPath.Arcs[m_curArcId].MapToUniform(
                            (m_dirSgn > 0 ? m_offsetLength : (bezierPath.Arcs[m_curArcId].Length - m_offsetLength))
                            / bezierPath.Arcs[m_curArcId].Length);
                    transform.position =
                        bezierPath.Arcs[m_curArcId].CalculateCubicBezierPos(uniformT);
                    transform.forward = bezierPath.Arcs[m_curArcId].CalculateCubicBezierVelocity(uniformT)
                        * (alwaysForward ? 1 : actualVelocity.Sgn());


                    if (alwaysForward == true && actualVelocity.Sgn() < 0)
                    {
                        //bezierPath.GetNextId(ref curArcId, ref curSampleId, -1);
                    }

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
                    //if (referenceVelocity > 0)
                    {
                        float interval = 1f / bezierPath.Arcs.Count;

                        actualVelocity =
                            referenceVelocity
                            * velocityCurve.Evaluate(interval * (m_curArcId
                                + (m_dirSgn > 0 ? m_offsetLength : (bezierPath.Arcs[m_curArcId].Length - m_offsetLength))
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
}