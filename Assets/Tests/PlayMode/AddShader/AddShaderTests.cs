using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using System.Linq;

namespace Tests
{

    public class AddShaderTests
    {
        private AddShaderFactories addShaderFactories;

        [OneTimeSetUp]
        public void LoadScene()
        {
            //We can programmtically create scenes using SceneManager.CreateScene 
            //and load in assets using UnityEditor.ShaderImporter
            UnityEngine.SceneManagement.SceneManager.LoadScene("AddShaderTests");

            
        }

        [UnitySetUp]
        public IEnumerator LoadObjects()
        {
            yield return new EnterPlayMode();

            var testObject = GameObject.Find("AddShaderGameObject");
            Assert.IsNotNull(testObject);

            addShaderFactories = testObject.GetComponent<AddShaderFactories>();
            Assert.IsNotNull(addShaderFactories);
        }





        [UnityTest]
        public IEnumerator AddTestFor20Seconds()
        {



            float[] A = new float[] { 1.0f, 2.0f, 3.0f };
            float[] B = new float[] { 1.0f, 2.0f, 3.0f };

            float[] Result = new float[A.Length];

            VectorAddWrapper vectorAdder = addShaderFactories.VectorAddWrapperFactory(A.Length);



            var comparer = new UnityEngine.TestTools.Utils.FloatEqualityComparer(10e-6f);

            while (Time.realtimeSinceStartup < 20.0f)
            {
                Result = vectorAdder.AddVectors(A, B);

                yield return new WaitForFixedUpdate();
            }

            for (int i = 0; i < A.Length; i++)
            {
                Assert.That(Result[i], Is.EqualTo(A[i] + B[i]).Using(comparer));

            }

            vectorAdder.ReleaseBuffers();
            yield return null;
        }


        [Test]
        public void AddTestOnce()
        {

            int ArrayDim = 10_000_000;

            float[] A = new float[ArrayDim];
            float[] B = new float[ArrayDim];

            for(int i=0; i < ArrayDim; i++)
            {
                A[i] = i;
                B[i] = i * i;
            }

            VectorAddWrapper vectorAdder = addShaderFactories.VectorAddWrapperFactory(A.Length);

            float[] Result = vectorAdder.AddVectors(A, B);

            var comparer = new UnityEngine.TestTools.Utils.FloatEqualityComparer(10e-6f);


            for (int i = 0; i < A.Length; i++)
            {
                Assert.That(Result[i], Is.EqualTo(A[i] + B[i]).Using(comparer));

            }

            vectorAdder.ReleaseBuffers();
        }


        [Test]
        public void SumTest()
        {

            int ArrayDim = 512*2 -1 ;

            float[] A = new float[ArrayDim];

            for (int i = 0; i < ArrayDim; i++)
            {
                A[i] = 1.0f;
            }

            VectorSumWrapper vectorSummer = addShaderFactories.VectorSumWrapperFactory(A.Length);

            float Result = vectorSummer.SumVector(A);

            var comparer = new UnityEngine.TestTools.Utils.FloatEqualityComparer(10e-6f);

            Assert.That(Result, Is.EqualTo(ArrayDim).Using(comparer));

            vectorSummer.ReleaseBuffers();
            
        }


        //[UnityTearDown]
        //public IEnumerator UnLoadScene()
        //{
        //    yield return new ExitPlayMode();
        //}

    }

}