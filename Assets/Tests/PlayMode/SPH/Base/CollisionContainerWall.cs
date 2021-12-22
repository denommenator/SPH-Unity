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

    public class CollisionContainers
    {

        static public CollisionContainerWall[] BoxContainer(float width, float height, float depth, float wallElasticity, float pushWallLocation = 0.0f)
        {
            CollisionContainerWall wall_lower, wall_upper, wall_left, wall_right, wall_back, wall_front;

            wall_lower = new CollisionContainerWall
            {
                inward_normal = Vector3.up,
                point = Vector3.zero,
                elasticity = wallElasticity
            };

            wall_upper = new CollisionContainerWall
            {
                inward_normal = Vector3.down,
                point = height * Vector3.up,
                elasticity = wallElasticity
            };

            wall_left = new CollisionContainerWall
            {
                inward_normal = Vector3.right,
                point = width / 2 * Vector3.left + pushWallLocation * width * Vector3.right,
                elasticity = wallElasticity
            };

            wall_right = new CollisionContainerWall
            {
                inward_normal = Vector3.left,
                point = width / 2 * Vector3.right,
                elasticity = wallElasticity
            };

            wall_back = new CollisionContainerWall
            {
                inward_normal = Vector3.forward,
                point = depth * Vector3.back,
                elasticity = wallElasticity
            };

            wall_front = new CollisionContainerWall
            {
                inward_normal = Vector3.back,
                point = Vector3.zero,
                elasticity = wallElasticity
            };


            CollisionContainerWall[] walls = new CollisionContainerWall[]
                { wall_lower, wall_upper, wall_left, wall_right, wall_back, wall_front };

            return walls;

        }
    }
}