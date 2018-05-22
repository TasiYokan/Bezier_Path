using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BezierCurve : MonoBehaviour
{
    #region Fields

    public List<BezierPoint> Points;
    public bool isAutoConnect;
    public int totalSampleCount = 10;
    public bool drawDebugPath;
    public float totalLength;
    private List<BezierArc> m_arcs;
    private LineRenderer m_lineRenderer;

    #endregion Fields

    #region Properties

    public List<BezierArc> Arcs
    {
        get
        {
            return m_arcs;
        }
    }

    #endregion Properties

    #region Methods

    public void AddPoint(BezierPoint _point)
    {
        if (Points == null)
            Points = new List<BezierPoint>();

        Points.Add(_point);
        UpdateAnchorTransform(Points.Count - 1);
        // TODO: Actually, only need to update the last arc
        InitArcsFromPoints();
    }

    public void RemovePoint(BezierPoint _point)
    {
        Points.Remove(_point);
        InitArcsFromPoints();
    }

    /// <summary>
    /// It needs position info of points. Make sure get position from localPosition before initing arcs
    /// </summary>
    public void InitArcsFromPoints()
    {
        m_arcs = new List<BezierArc>();
        for (int i = 0; i < Points.Count - 1; i++)
        {
            m_arcs.Add(new BezierArc(Points[i], Points[i + 1], totalSampleCount / Points.Count));
        }

        if (isAutoConnect && Points.Count > 1)
        {
            m_arcs.Add(new BezierArc(Points[Points.Count - 1], Points[0], totalSampleCount / Points.Count));
        }

        totalLength = 0;
        foreach (BezierArc arc in m_arcs)
        {
            totalLength += arc.Length;
        }
        //print("Total length: " + totalLength);
    }

    /// <summary>
    /// Update anchors' position based on their localPosition
    /// Called every frame or everytime changed?
    /// </summary>
    public void UpdateAnchorTransforms()
    {
        for (int i = 0; i < Points.Count; ++i)
        {
            UpdateAnchorTransform(i);
        }
    }

    public void UpdateAnchorTransform(int _id)
    {
        SetAnchorLocalRotation(_id, Points[_id].LocalRotation);
        SetAnchorLocalPosition(_id, Points[_id].LocalPosition);
    }

    public void SetAnchorPosition(int _id, Vector3 _position)
    {
        Points[_id].Position = _position;
        Points[_id].LocalPosition =
            Vector3.Scale(transform.localScale.Reciprocal(),
            Quaternion.Inverse(transform.rotation)
            * (_position - transform.position));
    }

    public void SetAnchorLocalPosition(int _id, Vector3 _localPosition)
    {
        Points[_id].LocalPosition = _localPosition;
        Points[_id].Position =
            transform.position
            + transform.rotation
            * Vector3.Scale(transform.localScale, _localPosition);
    }

    public void SetAnchorRotation(int _id, Quaternion _rotation)
    {
        Points[_id].Rotation = _rotation;
        Points[_id].LocalRotation = Quaternion.Inverse(transform.rotation) * _rotation;
    }

    public void SetAnchorLocalRotation(int _id, Quaternion _localRotation)
    {
        Points[_id].LocalRotation = _localRotation;
        Points[_id].Rotation = transform.rotation * _localRotation;
    }

    //public void GetCurvePos(ref int _arcId, ref int _sampleId, float _speed, ref Vector3 _offset)
    //{
    //    // Get offset's projection on vector of heading. A positive value means it's on the way if you are moving with a positive speed
    //    float offsetLength = Vector3.Dot(
    //        GetSampleVectorAmongAllArcs(_arcId, _sampleId, _speed.Sgn()).normalized * _speed.Sgn(), _offset);

    //    // It's the total movement mover should take from the startArcId and startSampleId in this frame
    //    float remainLength = _speed + offsetLength;
    //    int curArcId = _arcId;
    //    int curSampleId = _sampleId;

    //    while (remainLength.Sgn() != 0)
    //    {
    //        // Cut some of the remaining length if current arc couldn't cover the whole length
    //        curSampleId = m_arcs[curArcId].GetSampleId(
    //            curSampleId, ref remainLength);

    //        // If remaining length has exceed the arc
    //        if (m_arcs[curArcId].SampleIdWithinArc(curSampleId + remainLength.Sgn()) == false)
    //        {
    //            curArcId += remainLength.Sgn();

    //            if (isAutoConnect)
    //                curArcId = (curArcId + m_arcs.Count) % m_arcs.Count;

    //            curSampleId = remainLength.Sgn() > 0 ?
    //                0 : m_arcs[curArcId].SamplePos.Count - 1;
    //        }
    //        // Remaining length can be finished at this arc
    //        else
    //        {
    //            _offset = Mathf.Abs(remainLength) *
    //                m_arcs[curArcId].GetSampleVector(curSampleId, remainLength.Sgn()).normalized;
    //            remainLength = 0;
    //        }

    //        if (curArcId < 0 || curArcId >= m_arcs.Count)
    //            break;
    //    }

    //    _arcId = curArcId;
    //    _sampleId = curSampleId;
    //}

    //public Vector3 GetSamplePos(int _arcId, int _sampleId)
    //{
    //    return Arcs[_arcId].SamplePos[_sampleId];
    //}

    /// <summary>
    /// Find a sample point's vector even it's on the boundary
    /// </summary>
    /// <param name="_arcId"></param>
    /// <param name="_sampleId"></param>
    /// <param name="_step"></param>
    /// <returns></returns>
    //public Vector3 GetSampleVectorAmongAllArcs(int _arcId, int _sampleId, int _step)
    //{
    //    int arcId = _arcId;
    //    int sampleId = _sampleId;

    //    if (m_arcs[arcId].SampleIdWithinArc(sampleId + _step) == false)
    //    {
    //        if (isAutoConnect)
    //        {
    //            // Find connected arc
    //            arcId = (arcId + _step + m_arcs.Count) % m_arcs.Count;
    //            // Sample point can only be the head or rear of the new arc
    //            sampleId = _step > 0 ? 0 : m_arcs[arcId].SampleCount - 1;
    //        }
    //        else
    //        {
    //            // Fallback: return the nearest vector
    //            return m_arcs[arcId].GetSampleVector(sampleId - _step, _step);
    //        }
    //    }

    //    return m_arcs[arcId].GetSampleVector(sampleId, _step);
    //}

    //public void GetNextId(ref int _arcId, ref int _sampleId, int _step)
    //{
    //    if (m_arcs[_arcId].SampleIdWithinArc(_sampleId + _step) == false)
    //    {
    //        if (isAutoConnect)
    //        {
    //            // Find connected arc
    //            _arcId = (_arcId + _step + m_arcs.Count) % m_arcs.Count;
    //            // Sample point can only be the head or rear of the new arc
    //            _sampleId = _step > 0 ? 0 : m_arcs[_arcId].SampleCount - 1;
    //        }
    //        else
    //        {
    //            // Fallback: clamp on the boundary
    //            if (_arcId + _step < 0 || _arcId + _step > m_arcs.Count - 1)
    //                _sampleId = _step > 0 ? m_arcs[_arcId].SampleCount - 1 : 0;
    //            else
    //                _sampleId = _step < 0 ? m_arcs[_arcId].SampleCount - 1 : 0;

    //            _arcId = Mathf.Clamp(_arcId + _step, 0, m_arcs.Count - 1);
    //        }
    //    }

    //    _sampleId += _step;
    //    return;
    //}

    //public Vector3 GetNextSamplePosAmongAllArcs(int _arcId, int _sampleId, int _step)
    //{
    //    int arcId = _arcId;
    //    int sampleId = _sampleId;

    //    if (m_arcs[arcId].SampleIdWithinArc(sampleId + _step) == false)
    //    {
    //        if (isAutoConnect)
    //        {
    //            // Find connected arc
    //            arcId = (arcId + _step + m_arcs.Count) % m_arcs.Count;
    //            // Sample point can only be the head or rear of the new arc
    //            sampleId = _step > 0 ? 0 : m_arcs[arcId].SampleCount - 1;
    //        }
    //        else
    //        {
    //            // Fallback: return the nearest vector
    //            return m_arcs[arcId].GetNextSamplePos(sampleId - _step, _step);
    //        }
    //    }

    //    return m_arcs[arcId].GetNextSamplePos(sampleId, _step);
    //}

    /// <summary>
    /// Update all arc length
    /// </summary>
    public void ForceUpdateAllArcs()
    {
        //int totalPos = 1;
        //foreach (var arc in m_arcs)
        //{
        //    arc.UpdateSamplePos();
        //    totalPos += arc.InitSampleCount - 1;
        //}

        for (int i = 0; i < m_arcs.Count; ++i)
        {
            ForceUpdateOneArc(i);
        }
    }

    public void ForceUpdateOneArc(int _arcId)
    {
        //m_arcs[_arcId].UpdateSamplePos();
        //m_arcs[(_arcId + m_arcs.Count - 1) % m_arcs.Count].UpdateSamplePos();
        //m_arcs[(_arcId + m_arcs.Count + 1) % m_arcs.Count].UpdateSamplePos();

        m_arcs[_arcId].UpdateLength();
    }

    public void UpdateAllPointPoses()
    {
        foreach (BezierPoint point in Points)
        {
            point.UpdatePosition();
        }
    }

    void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        if (m_lineRenderer == null)
            m_lineRenderer = gameObject.AddComponent<LineRenderer>();

        UpdateAnchorTransforms();
        InitArcsFromPoints();
    }
    // Update is called once per frame
    void Update()
    {
        if (drawDebugPath)
            DrawDebugCurve();
        // To update sample poses
        ForceUpdateAllArcs();
    }
    //private int GetTotalSampleCount()
    //{
    //    int totalPos = 1;
    //    foreach (var arc in m_arcs)
    //    {
    //        totalPos += arc.InitSampleCount - 1;
    //    }
    //    return totalPos;
    //}
    private void DrawDebugCurve()
    {
        //int totalPos = GetTotalSampleCount();
        //m_lineRenderer.positionCount = totalPos;

        //int curPos = 0;
        //for (int i = 0; i < m_arcs.Count; ++i)
        //{
        //    for (int j = 0; j < m_arcs[i].SamplePos.Count - 1; ++j)
        //    {
        //        m_lineRenderer.SetPosition(curPos + j, m_arcs[i].SamplePos[j]);
        //    }

        //    curPos += m_arcs[i].SamplePos.Count - 1;
        //}

        //List<Vector3> lastArcPoses = m_arcs[m_arcs.Count - 1].SamplePos;
        //m_lineRenderer.SetPosition(totalPos - 1, lastArcPoses[lastArcPoses.Count - 1]);
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (UnityEditor.Selection.activeGameObject != gameObject)
        {
            UpdateAnchorTransforms();
            if (Points.Count >= 2)
            {
                for (int i = 0; i < Points.Count; i++)
                {
                    if (i < Points.Count - 1)
                    {
                        var index = Points[i];
                        var indexNext = Points[i + 1];
                        UnityEditor.Handles.DrawBezier(index.Position, indexNext.Position, index.GetHandle(0).Position,
                        indexNext.GetHandle(1).Position, Color.gray, null, 5);
                    }
                    else if (isAutoConnect)
                    {
                        var index = Points[i];
                        var indexNext = Points[0];
                        UnityEditor.Handles.DrawBezier(index.Position, indexNext.Position, index.GetHandle(0).Position,
                        indexNext.GetHandle(1).Position, Color.gray, null, 5);
                    }
                }
            }

            float size = HandleUtility.GetHandleSize(gameObject.transform.position) * 0.1f;
            for (int i = 0; i < Points.Count; i++)
            {
                Gizmos.matrix = Matrix4x4.TRS(Points[i].Position, Points[i].Rotation, Vector3.one);
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(Vector3.zero, size);
                Gizmos.matrix = Matrix4x4.identity;
            }
        }
    }
#endif

    #endregion Methods
}
