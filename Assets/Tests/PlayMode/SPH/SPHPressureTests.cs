using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.Linq;

namespace SPH
{
    public class SPHPressureTests
    {
        SPHMono sphMono;

        PressureKernel pressureKernel;

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

            pressureKernel = sphMono.pressureKernel;

        }



        [Test]
        public void OneParticle()
        {
            float[] densities = { 1000.0f };
            float[] pressures = pressureKernel.ComputePressure(densities, 1, 1, 1);

            Assert.AreEqual(1, pressures.Length);
            Assert.AreEqual(0.0f, pressures[0], .0001);
        }

        [Test]
        public void ManyParticles()
        {
            int N = 1000;
            float[] densities = new float[1000];
            for(int i =0; i<N; i++)
            {
                densities[i] = 2_000;
            }

            float[] pressures = pressureKernel.ComputePressure(densities, 2, 1, 2000);

            Assert.AreEqual(N, pressures.Length);
            Debug.Log("Average Pressure: " + Enumerable.Average(pressures));
        }


    }
} // namespace SPH