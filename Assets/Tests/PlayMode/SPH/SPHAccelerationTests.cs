using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.Linq;

namespace SPH
{
    public class SPHAccelerationTests
    {
        SPHMono sphMono;

        AccelerationKernel accelerationKernel;

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

            accelerationKernel = sphMono.accelerationKernel;

        }





        [Test]
        public void OneParticleZeroG()
        {

            Vector3[] pressureForces = { Vector3.forward };
            Vector3[] viscosityForces = { Vector3.zero };
            Vector3[] surfaceForces = { Vector3.zero };

            float[] densities = { 2000.0f };


            Vector3[] accelerations = accelerationKernel.ComputeAcceleration(densities,
                                                                             pressureForces,
                                                                             viscosityForces,
                                                                             surfaceForces,
                                                                             0.0f);

            Assert.AreEqual(1, accelerations.Length);

            Assert.AreEqual(0.0f, accelerations[0][0], .001);
            Assert.AreEqual(0.0f, accelerations[0][1], .001);
            Assert.AreEqual(1.0f / 2_000.0f, accelerations[0][2], .001);

        }

        [Test]
        public void OneParticle()
        {

            Vector3[] pressureForces = { Vector3.forward };
            Vector3[] viscosityForces = { Vector3.zero };
            Vector3[] surfaceForces = { Vector3.zero };

            float[] densities = { 2000.0f };


            Vector3[] accelerations = accelerationKernel.ComputeAcceleration(densities,
                                                                             pressureForces,
                                                                             viscosityForces,
                                                                             surfaceForces,
                                                                             10.0f);

            Assert.AreEqual(1, accelerations.Length);

            Assert.AreEqual(0.0f, accelerations[0][0], .001);
            Assert.AreEqual(-10.0f, accelerations[0][1], .001);
            Assert.AreEqual(1.0f / 2_000.0f, accelerations[0][2], .001); ;

        }

        [Test]
        public void OneParticleOneBlockZeroG()
        {

            Vector3[] pressureForces = { Vector3.forward };
            Vector3[] viscosityForces = { Vector3.zero };
            Vector3[] surfaceForces = { Vector3.zero };

            float[] densities = { 2000.0f };


            Vector3[] accelerations = accelerationKernel.ComputeAcceleration(densities,
                                                                             pressureForces,
                                                                             viscosityForces,
                                                                             surfaceForces,
                                                                             0.0f, 1);

            Assert.AreEqual(1, accelerations.Length);

            Assert.AreEqual(0.0f, accelerations[0][0], .001);
            Assert.AreEqual(0.0f, accelerations[0][1], .001);
            Assert.AreEqual(1.0f / 2_000.0f, accelerations[0][2], .001);

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