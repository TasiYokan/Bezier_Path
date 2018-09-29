using System;
using System.Collections.Generic;
using UnityEngine;

namespace TasiYokan.Curve
{
    public interface IBezierPos
    {
        Vector3 Position { get; set; }
        Vector3 LocalPosition { get; set; }
    }
}