using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddDummyGameObject : MonoBehaviour
{
    [SerializeField]
    ComputeShader shader;
    private VectorSumKernel vectorSumKernel;
    private VectorAddKernel vectorAddKernel;
    int N;

    // Start is called before the first frame update
    void Start()
    {
    }

    public float VectorSum(float[] A)
    {
        vectorSumKernel = new VectorSumKernel(shader, A.Length);
        vectorSumKernel.A.SetData(A);
        vectorSumKernel.Dispatch();

        return ((float[])vectorSumKernel.Sum)[0];

    }

    public float[] VectorAdd(float[] A, float[] B)
    {
        vectorAddKernel = new VectorAddKernel(shader, A.Length);
        vectorAddKernel.A.SetData(A);
        vectorAddKernel.B.SetData(B);
        vectorAddKernel.Dispatch();

        return vectorAddKernel.AddResult;

    }

    public float VectorAddSum1(float[] A, float[] B)
    {
        vectorAddKernel = new VectorAddKernel(shader, A.Length);
        vectorAddKernel.A.SetData(A);
        vectorAddKernel.B.SetData(B);
        vectorAddKernel.Dispatch();

        vectorSumKernel = new VectorSumKernel(shader, A.Length);
        vectorSumKernel.A.BindBuffer(vectorAddKernel.AddResult);
        vectorSumKernel.Dispatch();


        return ((float[])vectorSumKernel.Sum)[0];

    }

    public float VectorAddSum2(float[] A, float[] B)
    {
        

        vectorAddKernel = new VectorAddKernel(shader, A.Length);

        MyComputeKernel1.ComputeBufferWrapper temporary = vectorAddKernel.AddResult;

        vectorAddKernel.A.SetData(A);
        vectorAddKernel.B.SetData(B);
        vectorAddKernel.Dispatch();

        vectorSumKernel = new VectorSumKernel(shader, A.Length);
        vectorSumKernel.A.BindBuffer(temporary);
        vectorSumKernel.Dispatch();


        return ((float[])vectorSumKernel.Sum)[0];

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        MyComputeKernel1.ComputeBufferWrapper.ReleaseBuffers();
    }
}
