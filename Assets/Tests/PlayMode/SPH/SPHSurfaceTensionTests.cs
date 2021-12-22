using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.Linq;

namespace SPH
{
    public class SPHSurfaceForceTests
    {
        SPHMono sphMono;

        SurfaceTensionKernel surfaceTensionKernel;

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

            surfaceTensionKernel = sphMono.surfaceTensionKernel;

        }





        [Test]
        public void OneParticle()
        {
            //float hDensity = 1.0f;

            Vector3[] positions = { Vector3.zero };
            float[] densities = { 2000.0f };
            float[] pressures = { 2000.0f };


            Vector3[] surfaceForces = surfaceTensionKernel.ComputeSurfaceForce(positions, densities, 1.0f, 1.0f, 0.0f);

            Debug.Log(surfaceForces[0].ToString());

        }


    }
} // namespace SPH