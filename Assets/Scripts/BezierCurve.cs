using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TasiYokan.Curve
{
    public class BezierCurve : MonoBehaviour
    {
        #region Fields

        public List<BezierPoint> Points;
        public bool isAutoConnect;
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

        public void SetPoints(List<BezierPoint> _points)
        {
            Points = _points;
            UpdateAnchorTransforms();
            InitArcsFromPoints();
        }

        /// <summary>
        /// Always append at tail
        /// </summary>
        /// <param name="_point"></param>
        public void AddPoint(BezierPoint _point)
        {
            if (Points == null)
                Points = new List<BezierPoint>();

            Points.Add(_point);
            UpdateAnchorTransformAt(Points.Count - 1);

            //Happens in Editor mode
            if (m_arcs == null)
                InitArcsFromPoints();
            else
            {
                if (isAutoConnect)
                {
                    m_arcs[m_arcs.Count - 1] = InitArcsFromPointsAt(Points.Count - 2);
                    m_arcs.Add(InitArcsFromPointsAt(Points.Count - 1));
                }
                else
                {
                    m_arcs.Add(InitArcsFromPointsAt(Points.Count - 1));
                }
            }
            //InitArcsFromPoints();
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
            for (int i = 0; i < Points.Count; i++)
            {
                BezierArc newArc = InitArcsFromPointsAt(i);
                if (newArc != null)
                    m_arcs.Add(newArc);
            }
        }

        public BezierArc InitArcsFromPointsAt(int _id)
        {
            Assert.IsNotNull(m_arcs, "m_arcs should not be null before set only one arc!");
            Assert.IsTrue(_id >= 0 && _id <= Points.Count - 1, "Invalid point id! " + _id);
            if (Points.Count <= 1)
            {
                Debug.Log("Count of Points should greater than 1");
                return null;
            }

            int endId;
            if (_id < Points.Count - 1)
            {
                endId = _id + 1;
            }
            else
            {
                if (isAutoConnect)
                    endId = 0;
                else
                    return null;
            }
            return new BezierArc(Points[_id], Points[endId]);
        }

        /// <summary>
        /// Update anchors' position based on their localPosition
        /// Called every frame or everytime changed?
        /// </summary>
        public void UpdateAnchorTransforms()
        {
            for (int i = 0; i < Points.Count; ++i)
            {
                UpdateAnchorTransformAt(i);
            }
        }

        /// <summary>
        /// Using point's LocalPosition to initialize Position
        /// </summary>
        /// <param name="_id"></param>
        public void UpdateAnchorTransformAt(int _id)
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

        /// <summary>
        /// Update its Position as well
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_localPosition"></param>
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

        /// <summary>
        /// Update all arc length
        /// </summary>
        public void ForceUpdateAllArcs()
        {
            totalLength = 0;
            for (int i = 0; i < m_arcs.Count; ++i)
            {
                ForceUpdateOneArc(i);
                totalLength += m_arcs[i].Length;
            }
        }

        public void ForceUpdateOneArc(int _arcId)
        {
            m_arcs[_arcId].UpdateLength();
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
            // To update arc length
            ForceUpdateAllArcs();
        }

        private void DrawDebugCurve()
        {
            int sampleCountInArc = 20;
            int arcCount = m_arcs.Count;
            m_lineRenderer.positionCount = sampleCountInArc * arcCount;

            for (int i = 0; i < arcCount; ++i)
            {
                for (int j = 0; j < sampleCountInArc; ++j)
                {
                    m_lineRenderer.SetPosition(i * sampleCountInArc + j,
                        m_arcs[i].CalculateCubicBezierPos((1f * j) / (sampleCountInArc - 1)));
                }
            }
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
                            UnityEditor.Handles.DrawBezier(index.Position, indexNext.Position, index.GetHandle(1).Position,
                            indexNext.GetHandle(0).Position, Color.gray, null, 5);
                        }
                        else if (isAutoConnect)
                        {
                            var index = Points[i];
                            var indexNext = Points[0];
                            UnityEditor.Handles.DrawBezier(index.Position, indexNext.Position, index.GetHandle(1).Position,
                            indexNext.GetHandle(0).Position, Color.gray, null, 5);
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
}
