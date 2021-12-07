using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
//using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace Tests
{
    public class ComputeKernelsTests
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
            testScript.RunTest();


            yield return null;


        }

    }
}