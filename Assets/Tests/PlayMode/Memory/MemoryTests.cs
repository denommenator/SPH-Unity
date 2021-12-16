using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MemoryTests
{
    DummyGameObject dm;

    [OneTimeSetUp]
    public void LoadScene()
    {
        //We can programmtically create scenes using SceneManager.CreateScene 
        //and load in assets using UnityEditor.ShaderImporter
        UnityEngine.SceneManagement.SceneManager.LoadScene("MemoryTests");


    }

    [UnitySetUp]
    public IEnumerator LoadObjects()
    {
        yield return new EnterPlayMode();

        var testObject = GameObject.Find("MemoryGameObject");
        Assert.IsNotNull(testObject);

        dm = testObject.GetComponent<DummyGameObject>();
        Assert.IsNotNull(dm);

    }


    [Test]
    public void GroupMemoryTest()
    {

        float[] result = dm.GetResult();

        Assert.AreEqual(result[0], 0.0f, "Memory is not correct!");
        Assert.AreEqual(result[1], 1.0f, "Memory is not correct!");

    }
}
