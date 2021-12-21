using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    //refactor to 3 buffers
    public struct CollisionContainerWall
    {
        public Vector3 inward_normal;
        public Vector3 point;
        public float elasticity;
    }
}