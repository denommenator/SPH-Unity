using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.Linq;

namespace SPH
{
    public class SPHExplicitEulerTests
    {
        SPHMono sphMono;

        ExplicitEulerKernel explicitEulerKernel;

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

            explicitEulerKernel = sphMono.explicitEulerKernel;

        }





        [Test]
        public void OneParticle()
        {

            Vector3[] CurrentPositions = { Vector3.forward };
            Vector3[] CurrentVelocities = { Vector3.forward };
            float dt = 3.0f;


            Vector3[] nextPositions = explicitEulerKernel.ComputeNext(CurrentPositions, CurrentVelocities, dt);

            Assert.AreEqual(1, nextPositions.Length);

            Assert.AreEqual(0.0f, nextPositions[0].x, .001);
            Assert.AreEqual(0.0f, nextPositions[0].y, .001);
            Assert.AreEqual(4.0f, nextPositions[0].z, .001);

        }



        //[Test]
        //public void TwoParticles()
        //{
        //    float hDensity = 2.0f;

        //    Vector3[] positions = { Vector3.zero, Vector3.forward };
        //    float[] densities = { 2000.0f, 2000.0f };
        //    float[] pressures = { 2000.0f, 2000.0f };


        //    Vector3[] pressureForces = pressureForceKernel.ComputePressureForce(positions,
        //                                                                      densities,
        //                                                                      pressures,
        //                                                                      hDensity,
        //                                                                      1000);

        //    Assert.AreEqual(2, pressureForces.Length);
        //    Assert.AreNotSame(0.0f, pressureForces[0][2]);

        //    //equal and opposite forces!
        //    Assert.AreEqual(pressureForces[0][2], -pressureForces[1][2]);
        //    Debug.Log("Pressure Forces: " + pressureForces[0] + pressureForces[1]);

        //}

    }
} // namespace SPH