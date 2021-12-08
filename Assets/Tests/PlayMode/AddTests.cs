using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
//using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Tests
{
    public class AddTests
    {
        [OneTimeSetUp]
        public void LoadScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TestsScene");
        }

        // A Test behaves as an ordinary method
        [UnityTest]
        public IEnumerator AddTest()
        {
            var testObject = GameObject.Find("TestObject");
            Assert.IsNotNull(testObject);

            AddTestScript testScript = testObject.GetComponent<AddTestScript>();

            float[] A = new float[] { 1.0f, 2.0f, 3.0f };
            float[] B = new float[] { 1.0f, 2.0f, 3.0f };

            float[] Result = testScript.RunAddTest(A, B);

            var comparer = new FloatEqualityComparer(10e-6f);

            
            while(true)
            {
                for (int i = 0; i < A.Length; i++)
                {
                    Assert.That(Result[i], Is.EqualTo(A[i] + B[i]).Using(comparer));

                }
                if(Time.realtimeSinceStartup > 5.0f)
                {
                    Assert.Fail("reached time out after 5 seconds!");
                }
                yield return new WaitForFixedUpdate();
            }
            

            

        }

    }
}