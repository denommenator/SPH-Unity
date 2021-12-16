using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddDummyGameObject : MonoBehaviour
{
    [SerializeField]
    ComputeShader shader;
    private VectorSumKernel vectorSumKernel;
    int N;

    // Start is called before the first frame update
    void Start()
    {
    }

    public float VectorSum(float[] A)
    {
        vectorSumKernel = new VectorSumKernel(shader, A.Length);
        ((ComputeBuffer)vectorSumKernel.A).SetData(A);
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
