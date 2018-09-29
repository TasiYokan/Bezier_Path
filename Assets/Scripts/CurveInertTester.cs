using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TasiYokan.Curve
{
    public class CurveInertTester : MonoBehaviour
    {
        public BezierCurve curve;
        public int count = 10;
        public List<GameObject> goes;

        // Use this for initialization
        void Start()
        {
            if (curve == null)
                curve = FindObjectOfType<BezierCurve>();

            goes = new List<GameObject>();
            for (int ai = 0; ai < curve.Arcs.Count; ++ai)
                for (int i = 0; i < count; ++i)
                    goes.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
        }

        // Update is called once per frame
        void OnDrawGizmos()
        {
            if (Application.isPlaying == false)
                return;

            int totalId = 0;
            for (int ai = 0; ai < curve.Arcs.Count; ++ai)
                for (int i = 0; i < count; ++i)
                {
                    float mapped = curve.Arcs[ai].MapToUniform(i * 1f / count);
                    Vector3 pos = curve.Arcs[ai].CalculateCubicBezierPos(mapped);
                    goes[totalId].transform.position = pos;
                    Vector3 tangent = curve.Arcs[ai].CalculateCubicBezierVelocity(mapped) / 15;
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(pos, pos + tangent);

                    totalId++;
                }
        }
    }
}