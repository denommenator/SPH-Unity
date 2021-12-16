using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AddShaderTests
{
    AddDummyGameObject dm;

    [OneTimeSetUp]
    public void LoadScene()
    {
        //We can programmtically create scenes using SceneManager.CreateScene 
        //and load in assets using UnityEditor.ShaderImporter
        UnityEngine.SceneManagement.SceneManager.LoadScene("VectorSumTests");


    }

    [UnitySetUp]
    public IEnumerator LoadObjects()
    {
        yield return new EnterPlayMode();

        var testObject = GameObject.Find("TestGameObject");
        Assert.IsNotNull(testObject);

        dm = testObject.GetComponent<AddDummyGameObject>();
        Assert.IsNotNull(dm);

    }


    [Test]
    public void VectorSumTest()
    {
        int N = 1_024;
        float[] A = new float[N];
        for(int i=0; i<N; i++)
        {
            A[i] = 1.0f;
        }
        float result = dm.VectorSum(A);

        Assert.AreEqual(N, result, "Sum is not correct!");

    }

    [Test]
    public void VectorAddTest()
    {
        int N = 1_024;
        float[] A = new float[N];
        float[] B = new float[N];
        for (int i = 0; i < N; i++)
        {
            A[i] = 1.0f;
            B[i] = 10.0f;
        }
        float[] result = dm.VectorAdd(A, B);
        for (int i = 0; i < N; i++)
        {
            Assert.AreEqual(11.0f, result[i], "Sum is not correct!");
        }

    }

    [Test]
    public void VectorAddSumTest()
    {
        int N = 1_024;
        float[] A = new float[N];
        float[] B = new float[N];
        for (int i = 0; i < N; i++)
        {
            A[i] = 1.0f;
            B[i] = 10.0f;
        }
        float result = dm.VectorAddSum2(A, B);
        Assert.AreEqual(11.0f * N, result, "AddSum is not correct!");
        

    }
}
