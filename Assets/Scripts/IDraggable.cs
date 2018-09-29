using System;
using System.Collections.Generic;
using UnityEngine;

namespace TasiYokan.Curve
{
    public interface IDraggable
    {
        Vector3 DragOffset { get; set; }
        void OnDragged(Vector3 _dragStartPos);
        void OnDropped(Vector3 _dragEndPos);
        void OnDragStay(Vector3 _dragCurPos);
    }
}