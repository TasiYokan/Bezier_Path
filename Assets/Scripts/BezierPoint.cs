using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace TasiYokan.Curve
{
    [System.Serializable]
    public class BezierPoint
    {
        #region Fields

        private int activeHandleId = 1;

        [SerializeField]
        private BezierHandle[] m_handles = new BezierHandle[2];

        [SerializeField]
        private Vector3 m_localPosition;
        private Vector3 m_position;

        [SerializeField]
        private Quaternion m_localRotation;
        private Quaternion m_rotation;

        [SerializeField]
        private bool m_isAutoSmooth;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Avoid misset values when deserializing.
        /// Only used when click add point button or create one in game.
        /// </summary>
        /// <param name="_isFirstTime"></param>
        public BezierPoint(Vector3 _localPos, Quaternion _localRot, bool _isFirstTime = true)
        {
            if (_isFirstTime == false)
                return;

            m_localPosition = _localPos;
            m_localRotation = _localRot;

            m_handles[0] = new BezierHandle();
            m_handles[1] = new BezierHandle();

            // TODO: Based on its rotation
            SetHandleLocalPosition(0, Vector3.back);
            SetHandleLocalPosition(1, Vector3.forward);
        }

        #endregion Constructors

        #region Properties

        public bool IsAutoSmooth
        {
            get
            {
                return m_isAutoSmooth;
            }

            set
            {
                m_isAutoSmooth = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return m_position;
            }

            set
            {
                m_position = value;
                UpdateHandlesPosition();
            }
        }

        public Vector3 LocalPosition
        {
            get
            {
                return m_localPosition;
            }

            set
            {
                m_localPosition = value;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return m_rotation;
            }

            set
            {
                m_rotation = value;
            }
        }

        public Quaternion LocalRotation
        {
            get
            {
                return m_localRotation;
            }

            set
            {
                m_localRotation = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Update handles' position based on their localPosition
        /// Called every frame or everytime changed?
        /// </summary>
        public void UpdateHandlesPosition()
        {
            SetHandleLocalPosition(0, m_handles[0].LocalPosition);
            SetHandleLocalPosition(1, m_handles[1].LocalPosition);
        }

        public BezierHandle GetHandle(int _id)
        {
            return m_handles[_id];
        }

        public void SetHandlePosition(int _id, Vector3 _position, bool _autoSmooth = false)
        {
            m_handles[_id].Position = _position;
            m_handles[_id].LocalPosition = Quaternion.Inverse(this.Rotation) * (_position - this.Position);

            if (_autoSmooth)
            {
                SetHandleLocalPosition(1 - _id, -m_handles[_id].LocalPosition, false);
            }
        }

        public void SetHandleLocalPosition(int _id, Vector3 _localPosition, bool _autoSmooth = false)
        {
            m_handles[_id].LocalPosition = _localPosition;
            m_handles[_id].Position = this.Rotation * _localPosition + this.Position;

            if (_autoSmooth)
            {
                SetHandleLocalPosition(1 - _id, -m_handles[_id].LocalPosition, false);
            }
        }

        public void UpdatePosition()
        {
            if (IsAutoSmooth)
            {
                SmoothHandle(activeHandleId == 0);
            }
        }

        /// <summary>
        /// Adjsut secondary handle to the opposite postion of primary one.
        /// </summary>
        private void SmoothHandle(bool _basedOnPrimary = true)
        {
            int refId = _basedOnPrimary ? 0 : 1;

            m_handles[1 - refId].Position = 2 * Position - m_handles[refId].Position;
        }

        #endregion Methods
    }
}