using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.Linq;

namespace SPH
{
    public class SPHCollisionTests
    {
        SPHMono sphMono;

        CollisionsKernel collisionsKernel;

        [OneTimeSetUp]
        public void LoadScene()
        {
            //We can programmtically create scenes using SceneManager.CreateScene 
            //and load in assets using UnityEditor.ShaderImporter
            UnityEngine.SceneManagement.SceneManager.LoadScene("SPHTestScene");


        }

        [UnitySetUp]
        public IEnumerator LoadObjects()
        {
            yield return new EnterPlayMode();

            var testObject = GameObject.Find("SPHTestGameObject");
            Assert.IsNotNull(testObject);

            sphMono = testObject.GetComponent<SPHMono>();
            Assert.IsNotNull(sphMono);

            collisionsKernel = sphMono.collisionsKernel;

        }



        [Test]
        public void OneWall()
        {
            float wall_elasticity = 0.1f;
            CollisionContainerWall wall = new CollisionContainerWall
            {
                inward_normal = new Vector3(1,0,0),
                point = Vector3.zero,
                elasticity = wall_elasticity
            };

            ComputeBufferWrapperContainerWall walls = new ComputeBufferWrapperContainerWall(new CollisionContainerWall[] { wall });


            Vector3 outside = new Vector3(-10, 0, 0);
            Vector3 inside = new Vector3(10, 0, 0);

            Vector3 moving_away_dot = new Vector3(-1, 0, 0);
            Vector3 moving_in_dot = new Vector3(1, 0, 0);


            Vector3[] positions = new Vector3[] { outside,
                                                  outside,
                                                  inside ,
                                                  inside };

            Vector3[] velocities = new Vector3[] { moving_away_dot,
                                                   moving_in_dot,
                                                   moving_away_dot,
                                                   moving_in_dot};

            ComputeBufferWrapperFloat3 positionsBuffer = (ComputeBufferWrapperFloat3)positions;
            ComputeBufferWrapperFloat3 velocitiesBuffer = (ComputeBufferWrapperFloat3)velocities;

            collisionsKernel.ResolveCollisions(positionsBuffer, velocitiesBuffer, walls);

            Vector3[] resolvedPositions = (Vector3[])positionsBuffer;
            Vector3[] resolvedVelocities = (Vector3[])velocitiesBuffer;

            Assert.AreEqual(resolvedPositions[2], positions[2]);
            Assert.AreEqual(resolvedPositions[3], positions[3]);
            Assert.AreEqual(resolvedVelocities[2], velocities[2]);
            Assert.AreEqual(resolvedVelocities[3], velocities[3]);

            Assert.AreEqual(resolvedVelocities[1], velocities[1]);
            Assert.That(resolvedPositions[1][0] > 0.0f);

            Assert.AreEqual(-wall_elasticity * velocities[0][0], resolvedVelocities[0][0], .00001);
            Assert.AreEqual(-wall_elasticity * velocities[0][1], resolvedVelocities[0][1], .00001);
            Assert.AreEqual(-wall_elasticity * velocities[0][2], resolvedVelocities[0][2], .00001);

            Assert.That(resolvedPositions[0][0] > 0.0f);

        }


    }
} // namespace SPH