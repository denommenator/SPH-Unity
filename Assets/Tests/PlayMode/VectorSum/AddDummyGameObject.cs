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
        SPH.ComputeBufferWrapper ABuffer = new SPH.ComputeBufferWrapper(A);
        vectorSumKernel = new VectorSumKernel(shader);
        vectorSumKernel.A.BindBuffer(ABuffer);
        vectorSumKernel.ArrayDim.SetInt(A.Length);
        vectorSumKernel.Dispatch();

        return ((float[])vectorSumKernel.Sum)[0];

    }

    public float[] VectorAdd1(float[] A, float[] B)
    {

        vectorAddKernel = new VectorAddKernel(shader);

        return vectorAddKernel.VectorAdd(A, B);

    }

    public float[] VectorAdd2(float[] A, float[] B)
    {
        SPH.ComputeBufferWrapper ABuffer = new SPH.ComputeBufferWrapper(A);
        SPH.ComputeBufferWrapper BBuffer = new SPH.ComputeBufferWrapper(B);
        SPH.ComputeBufferWrapper ResultBuffer = new SPH.ComputeBufferWrapper(A.Length);


        vectorAddKernel = new VectorAddKernel(shader);

        vectorAddKernel.VectorAdd(A, B, ResultBuffer);

        return ResultBuffer;

    }

    public float VectorAddSum(float[] A, float[] B)
    {

        SPH.ComputeBufferWrapper ABuffer = new SPH.ComputeBufferWrapper(A);
        SPH.ComputeBufferWrapper BBuffer = new SPH.ComputeBufferWrapper(B);
        SPH.ComputeBufferWrapper AddResultBuffer = new SPH.ComputeBufferWrapper(A.Length);

        vectorAddKernel = new VectorAddKernel(shader);

        vectorAddKernel.VectorAdd(ABuffer, BBuffer, AddResultBuffer);

        vectorSumKernel = new VectorSumKernel(shader);
        vectorSumKernel.A.BindBuffer(AddResultBuffer);
        vectorSumKernel.ArrayDim.SetInt(A.Length);
        vectorSumKernel.Dispatch();


        return ((float[])vectorSumKernel.Sum)[0];

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        SPH.ComputeBufferWrapper.ReleaseBuffers();
    }
}
