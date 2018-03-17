using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BezierCurve : MonoBehaviour
{
    [SerializeField]
    private List<BezierPoint> m_points;
    private List<BezierFragment> m_fragments;

    public bool isAutoConnect;
    private LineRenderer m_lineRenderer;

    public int totalSampleCount = 10;

    public bool drawDebugPath;

    public float totalLength;

    public List<BezierFragment> Fragments
    {
        get
        {
            return m_fragments;
        }
    }

    public List<BezierPoint> Points
    {
        get
        {
            return m_points;
        }
    }

    void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();

        InitFragmentsFromPoints();
    }

    public void AddPoint(BezierPoint _point)
    {
        m_points.Add(_point);
        InitFragmentsFromPoints();
    }

    public void RemovePoint(BezierPoint _point)
    {
        m_points.Remove(_point);
        InitFragmentsFromPoints();
    }

    private void InitFragmentsFromPoints()
    {
        m_fragments = new List<BezierFragment>();
        for (int i = 0; i < m_points.Count - 1; i++)
        {
            m_fragments.Add(new BezierFragment(m_points[i], m_points[i + 1], totalSampleCount / m_points.Count));
        }

        if (isAutoConnect && m_points.Count > 1)
        {
            m_fragments.Add(new BezierFragment(m_points[m_points.Count - 1], m_points[0], totalSampleCount / m_points.Count));
        }

        totalLength = 0;
        foreach (BezierFragment frag in m_fragments)
        {
            totalLength += frag.Length;
        }
        //print("Total length: " + totalLength);
    }

    // Update is called once per frame
    void Update()
    {
        if (drawDebugPath)
            DrawDebugCurve();
        //ForceUpdateFrags();
    }

    /// <summary>
    /// Update anchors' position based on their localPosition
    /// Called every frame or everytime changed?
    /// </summary>
    public void UpdateAnchorsPosition()
    {
        for (int i = 0; i < Points.Count; ++i)
        {
            SetAnchorLocalPosition(i, Points[i].LocalPosition);
        }
    }

    public void SetAnchorPosition(int _id, Vector3 _position)
    {
        Points[_id].Position = _position;
        Points[_id].LocalPosition = Quaternion.Inverse(transform.rotation) * (_position - transform.position);
    }

    public void SetAnchorLocalPosition(int _id, Vector3 _localPosition)
    {
        Points[_id].LocalPosition = _localPosition;
        Points[_id].Position = transform.rotation * _localPosition + transform.position;
    }

    public void GetCurvePos(ref int _fragId, ref int _sampleId, float _speed, ref Vector3 _offset)
    {
        // Get offset's projection on vector of heading. A positive value means it's on the way if you are moving with a positive speed
        float offsetLength = Vector3.Dot(
            GetSampleVectorAmongAllFrags(_fragId, _sampleId, _speed.Sgn()).normalized * _speed.Sgn(), _offset);

        // It's the total movement mover should take from the startFragId and startSampleId in this frame
        float remainLength = _speed + offsetLength;
        int curFragId = _fragId;
        int curSampleId = _sampleId;

        while (remainLength.Sgn() != 0)
        {
            // Cut some of the remaining length if current fragment couldn't cover the whole length
            curSampleId = m_fragments[curFragId].GetSampleId(
                curSampleId, ref remainLength);

            // If remaining length has exceed the fragment
            if (m_fragments[curFragId].SampleIdWithinFragment(curSampleId + remainLength.Sgn()) == false)
            {
                curFragId += remainLength.Sgn();

                if (isAutoConnect)
                    curFragId = (curFragId + m_fragments.Count) % m_fragments.Count;

                curSampleId = remainLength.Sgn() > 0 ?
                    0 : m_fragments[curFragId].SamplePos.Count - 1;
            }
            // Remaining length can be finished at this fragment
            else
            {
                _offset = Mathf.Abs(remainLength) *
                    m_fragments[curFragId].GetSampleVector(curSampleId, remainLength.Sgn()).normalized;
                remainLength = 0;
            }

            if (curFragId < 0 || curFragId >= m_fragments.Count)
                break;
        }

        _fragId = curFragId;
        _sampleId = curSampleId;
    }

    /// <summary>
    /// Find a sample point's vector even it's on the boundary
    /// </summary>
    /// <param name="_fragId"></param>
    /// <param name="_sampleId"></param>
    /// <param name="_step"></param>
    /// <returns></returns>
    public Vector3 GetSampleVectorAmongAllFrags(int _fragId, int _sampleId, int _step)
    {
        int fragId = _fragId;
        int sampleId = _sampleId;

        if (m_fragments[fragId].SampleIdWithinFragment(sampleId + _step) == false)
        {
            if (isAutoConnect)
            {
                // Find connected frag
                fragId = (fragId + _step + m_fragments.Count) % m_fragments.Count;
                // Sample point can only be the head or rear of the new fragment
                sampleId = _step > 0 ? 0 : m_fragments[fragId].SampleCount - 1;
            }
            else
            {
                // Fallback: return the nearest vector
                return m_fragments[fragId].GetSampleVector(sampleId - _step, _step);
            }
        }

        return m_fragments[fragId].GetSampleVector(sampleId, _step);
    }

    public Vector3 GetNextSamplePosAmongAllFrags(int _fragId, int _sampleId, int _step)
    {
        int fragId = _fragId;
        int sampleId = _sampleId;

        if (m_fragments[fragId].SampleIdWithinFragment(sampleId + _step) == false)
        {
            if (isAutoConnect)
            {
                // Find connected frag
                fragId = (fragId + _step + m_fragments.Count) % m_fragments.Count;
                // Sample point can only be the head or rear of the new fragment
                sampleId = _step > 0 ? 0 : m_fragments[fragId].SampleCount - 1;
            }
            else
            {
                // Fallback: return the nearest vector
                return m_fragments[fragId].GetNextSamplePos(sampleId - _step, _step);
            }
        }

        return m_fragments[fragId].GetNextSamplePos(sampleId, _step);
    }

    /// <summary>
    /// Update all frags in list and return the total count of sample position
    /// </summary>
    /// <returns></returns>
    public int ForceUpdateFrags()
    {
        int totalPos = 1;
        foreach (var frag in m_fragments)
        {
            //frag.UpdateSamplePos();
            totalPos += frag.InitSampleCount - 1;
        }
        return totalPos;
    }

    public void ForceUpdateOneFrag(int _fragId)
    {
        m_fragments[_fragId].UpdateSamplePos();
        m_fragments[(_fragId + m_fragments.Count - 1) % m_fragments.Count].UpdateSamplePos();
        m_fragments[(_fragId + m_fragments.Count + 1) % m_fragments.Count].UpdateSamplePos();
    }

    private void DrawDebugCurve()
    {
        int totalPos = ForceUpdateFrags();
        m_lineRenderer.positionCount = totalPos;

        int curPos = 0;
        for (int i = 0; i < m_fragments.Count; ++i)
        {
            for (int j = 0; j < m_fragments[i].SamplePos.Count - 1; ++j)
            {
                m_lineRenderer.SetPosition(curPos + j, m_fragments[i].SamplePos[j]);
            }

            curPos += m_fragments[i].SamplePos.Count - 1;
        }

        List<Vector3> lastFragPoses = m_fragments[m_fragments.Count - 1].SamplePos;
        m_lineRenderer.SetPosition(totalPos - 1, lastFragPoses[lastFragPoses.Count - 1]);
    }

    public void UpdateAllPointPoses()
    {
        foreach (BezierPoint point in m_points)
        {
            point.UpdatePosition();
        }
    }
}
