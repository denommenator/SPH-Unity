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

    SPH.BufferReleaser bufferReleaser;

    // Start is called before the first frame update
    void Start()
    {
        bufferReleaser = gameObject.AddComponent<SPH.BufferReleaser>();
    }

    public float VectorSum1(float[] A)
    {
       
        vectorSumKernel = new VectorSumKernel(shader);
        
        return ((float[])vectorSumKernel.VectorSum(A))[0];

    }

    public float[] VectorAdd1(float[] A, float[] B)
    {

        vectorAddKernel = new VectorAddKernel(shader);

        return vectorAddKernel.VectorAdd(A, B);

    }

    public float[] VectorAdd2(float[] A, float[] B)
    {
        SPH.ComputeBufferWrapperFloat ABuffer = new SPH.ComputeBufferWrapperFloat(A);
        SPH.ComputeBufferWrapperFloat BBuffer = new SPH.ComputeBufferWrapperFloat(B);
        SPH.ComputeBufferWrapperFloat ResultBuffer = new SPH.ComputeBufferWrapperFloat(A.Length);


        vectorAddKernel = new VectorAddKernel(shader);

        vectorAddKernel.VectorAdd(A, B, ResultBuffer);

        return ResultBuffer;

    }

    public float VectorAddSum(float[] A, float[] B)
    {

        SPH.ComputeBufferWrapperFloat ABuffer = new SPH.ComputeBufferWrapperFloat(A);
        SPH.ComputeBufferWrapperFloat BBuffer = new SPH.ComputeBufferWrapperFloat(B);
        SPH.ComputeBufferWrapperFloat AddResultBuffer = new SPH.ComputeBufferWrapperFloat(A.Length);

        vectorAddKernel = new VectorAddKernel(shader);

        vectorAddKernel.VectorAdd(ABuffer, BBuffer, AddResultBuffer);

        vectorSumKernel = new VectorSumKernel(shader);

        


        return ((float[])vectorSumKernel.VectorSum(AddResultBuffer))[0];

    }



}
