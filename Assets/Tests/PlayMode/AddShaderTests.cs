using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{

    public class AddShaderTests
    {
        [OneTimeSetUp]
        public void LoadScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("AddShaderTests");
        }

        [UnityTest]
        public IEnumerator AddTestFor5Seconds()
        {
            var testObject = GameObject.Find("AddShaderTest");
            Assert.IsNotNull(testObject);

            AddShaderWrapper addShaderWrapper = testObject.GetComponent<AddShaderWrapper>();

            float[] A = new float[] { 1.0f, 2.0f, 3.0f };
            float[] B = new float[] { 1.0f, 2.0f, 3.0f };

            float[] Result = addShaderWrapper.RunAddTest();

            var comparer = new UnityEngine.TestTools.Utils.FloatEqualityComparer(10e-6f);

            while (true)
            {
                for (int i = 0; i < A.Length; i++)
                {
                    Assert.That(Result[i], Is.EqualTo(A[i] + B[i]).Using(comparer));

                }
                if (Time.realtimeSinceStartup > 5.0f)
                {
                    Assert.Fail("reached time out after 5 seconds!");
                }
                yield return new WaitForFixedUpdate();
            }
        }


        [UnityTest]
        public IEnumerator AddTestOnce()
        {
            var testObject = GameObject.Find("AddShaderTest");
            Assert.IsNotNull(testObject);

            AddShaderWrapper addShaderWrapper = testObject.GetComponent<AddShaderWrapper>();

            float[] A = new float[] { 1.0f, 2.0f, 3.0f };
            float[] B = new float[] { 1.0f, 2.0f, 3.0f };

            float[] Result = addShaderWrapper.RunAddTest();

            var comparer = new UnityEngine.TestTools.Utils.FloatEqualityComparer(10e-6f);


            for (int i = 0; i < A.Length; i++)
            {
                Assert.That(Result[i], Is.EqualTo(A[i] + B[i]).Using(comparer));

            }

            yield return null;
        }
    }

}