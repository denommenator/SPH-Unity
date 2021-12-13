using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MemoryTests
{
    GroupMemoryWrapper groupMemoryTest;
    MyComputeKernel.ComputeKernel memoryTestKernel;

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

        groupMemoryTest = testObject.GetComponent<GroupMemoryWrapper>();
        Assert.IsNotNull(groupMemoryTest);

        memoryTestKernel = groupMemoryTest.GetKernel();
    }


    [Test]
    public void GroupMemoryTest()
    {

        float[] A = { 5, 5 };
        memoryTestKernel.FillBuffer("my_global_array", A);
        memoryTestKernel.FillBuffer("read_results", A);

        memoryTestKernel.Dispatch();
        float[] B = memoryTestKernel.GetBufferData("read_results");

        Assert.That(B[0] == 0, "First index is correct!");
        Assert.That(B[0] == 0, "Second index is correct!");

    }
}
