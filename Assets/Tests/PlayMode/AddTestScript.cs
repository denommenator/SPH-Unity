using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using NUnit.Framework;


public class AddTestScript : MonoBehaviour
{
    [SerializeField]
    ComputeShader addTestShader;
    // Start is called before the first frame update
    void Start()
    {
        // Use the Assert class to test conditions



    }

    [Test]
    public void RunTest()
    {
        float[] A = new float[] { 1.0f, 2.0f, 3.0f };
        float[] B = new float[] { 1.0f, 2.0f, 3.0f };

        float[] Result = new float[A.Length];

        ComputeBuffer ABuffer = new ComputeBuffer(A.Length, sizeof(float));
        ComputeBuffer BBuffer = new ComputeBuffer(B.Length, sizeof(float));
        ComputeBuffer ResultBuffer = new ComputeBuffer(Result.Length, sizeof(float));

        int addTestKernelID = addTestShader.FindKernel("AddTest");

        int arrayDimID = Shader.PropertyToID("_ArrayDim");

        int ABufferID = Shader.PropertyToID("_A");
        int BBufferID = Shader.PropertyToID("_B");
        int ResultBufferID = Shader.PropertyToID("_Result");

        addTestShader.SetInt(arrayDimID, A.Length);

        ABuffer.SetData(A);
        BBuffer.SetData(B);

        addTestShader.SetBuffer(addTestKernelID, ABufferID, ABuffer);
        addTestShader.SetBuffer(addTestKernelID, BBufferID, ABuffer);
        addTestShader.SetBuffer(addTestKernelID, ResultBufferID, ResultBuffer);

        addTestShader.Dispatch(addTestKernelID, 1, 1, 1);


        ResultBuffer.GetData(Result);


        var comparer = new FloatEqualityComparer(10e-6f);

        for (int i = 0; i < A.Length; i++)
        {
            Assert.That(Result[i], Is.EqualTo(A[i] + B[i]).Using(comparer));

        }

        ABuffer.Release();
        BBuffer.Release();
        ResultBuffer.Release();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
