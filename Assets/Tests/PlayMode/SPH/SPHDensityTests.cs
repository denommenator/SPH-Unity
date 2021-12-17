using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System.Linq;

namespace SPH
{
    public class SPHDensityTests
    {
        SPHTestMono SPHMono;

        DensityKernel densityKernel;

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

            SPHMono = testObject.GetComponent<SPHTestMono>();
            Assert.IsNotNull(SPHMono);

            densityKernel = SPHMono.densityKernel;

        }



        [Test]
        public void SmallCubeTest()
        {
            int NGrid = 10;
            float delta = 1.0f;

            List<Vector3> Positions = new List<Vector3>();

            for (int i = 0; i < NGrid; i++)
                for (int j = 0; j < NGrid; j++)
                    for (int k = 0; k < NGrid; k++)
                        Positions.Add(new Vector3(i * delta, j * delta, k * delta));

            float[] Densities = densityKernel.ComputeDensity(Positions.ToArray(), 1, NGrid * NGrid * NGrid);

            Debug.Log("Average Density: " + Enumerable.Average(Densities));
            for (int p_id = 0; p_id < Densities.Length; p_id++)
            {
                Debug.Log(Densities[p_id].ToString());
            }
        }

    }
} // namespace SPH