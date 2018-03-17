using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

/// <summary>
/// Single fragment curve with 2 points which consists the whole curve
/// </summary>
public class BezierFragment
{
    public BezierPoint startPoint;
    public BezierPoint endPoint;

    private int m_initSampleCount;
    private List<Vector3> m_samplePoses;
    private float m_length;

    public List<Vector3> SamplePos
    {
        get
        {
            return m_samplePoses;
        }

        set
        {
            m_samplePoses = value;
        }
    }

    public int SampleCount
    {
        get
        {
            return m_samplePoses.Count;
        }
    }

    public int InitSampleCount
    {
        get
        {
            return m_initSampleCount;
        }
    }

    public float Length
    {
        get
        {
            return m_length;
        }

        private set
        {
            m_length = value;
        }
    }

    public BezierFragment(BezierPoint _start, BezierPoint _end, int _sampleCount = 10)
    {
        startPoint = _start;
        endPoint = _end;
        m_initSampleCount = _sampleCount;

        SamplePos = new List<Vector3>();
        UpdateSamplePos();
    }

    public void UpdateSamplePos()
    {
        Length = 0;
        SamplePos.Clear();
        for (int i = 0; i < m_initSampleCount; ++i)
        {
            Vector3 pos = CalculateCubicBezierPos(i / (float)(m_initSampleCount - 1));

            if (i > 0)
                Length += (pos - SamplePos[i - 1]).magnitude;

            SamplePos.Add(pos);
        }
    }

    private Vector3 CalculateCubicBezierPos(float _t)
    {
        float u = 1 - _t;
        float t2 = _t * _t;
        float u2 = u * u;
        float u3 = u2 * u;
        float t3 = t2 * _t;

        Vector3 p = u3 * startPoint.Position
            + t3 * endPoint.Position
            + 3 * u2 * _t * startPoint.GetHandle(0).Position
            + 3 * u * t2 * endPoint.GetHandle(1).Position;

        return p;
    }

    public int GetSampleId(int _startId, ref float _remainLength)
    {
        if (Mathf.Abs(_remainLength).Sgn() == 0)
            return _startId;

        int step = _remainLength.Sgn();
        float totalDistance = 0;
        float previousDistance = 0;

        //float offsetLength =
        //    Vector3.Cross(_offset, GetSampleVector(_startId, step)).magnitude;

        int curId = _startId;
        while (totalDistance.FloatLess(Mathf.Abs(_remainLength)))
        {
            previousDistance = totalDistance;

            if (SampleIdWithinFragment(curId + step))
            {
                totalDistance += GetSampleVector(curId, step).magnitude;
            }
            else
            {
                // Even the remain length/moving speed can't cover offset
                _remainLength -= step * previousDistance;
                return curId;
            }
            curId += step;
        }

        _remainLength -= step * previousDistance;
        return curId - step;
    }

    public Vector3 GetSampleVector(int _id, int _step)
    {
        Assert.IsTrue(SampleIdWithinFragment(_id + _step));

        return m_samplePoses[_id + _step] - m_samplePoses[_id];
    }

    public Vector3 GetNextSamplePos(int _id, int _step)
    {
        Assert.IsTrue(SampleIdWithinFragment(_id + _step));

        return m_samplePoses[_id + _step];
    }

    public bool SampleIdWithinFragment(int _id)
    {
        return _id >= 0 && _id < m_samplePoses.Count;
    }

    public int FindNearestSampleOnFrag(Vector3 _pos, ref Vector3 _offset)
    {
        int nearestSampleId = 0;
        float shortestDist = (_pos - m_samplePoses[nearestSampleId]).sqrMagnitude;
        for (int i = 1; i < m_samplePoses.Count; ++i)
        {
            if ((_pos - m_samplePoses[i]).sqrMagnitude.FloatLess(shortestDist))
            {
                nearestSampleId = i;
                _offset = _pos - m_samplePoses[i];
                shortestDist = _offset.sqrMagnitude;
            }
        }

        return nearestSampleId;
    }
}
