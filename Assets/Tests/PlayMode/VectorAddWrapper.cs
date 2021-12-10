using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorAddWrapper
{

    private MyComputeKernel.ComputeKernel _vectorAddKernel;
    private int _ArrayDim;


    // Start is called before the first frame update
    public VectorAddWrapper(ComputeShader computeShader, int arrayDim)
    {

        _vectorAddKernel = new MyComputeKernel.ComputeKernel(computeShader, "VectorAdd");
        _ArrayDim = arrayDim;

        _vectorAddKernel.AddGlobalConstantInt("_ArrayDim", _ArrayDim);

        _vectorAddKernel.CreateBuffer("_A", _ArrayDim);
        _vectorAddKernel.CreateBuffer("_B", _ArrayDim);
        _vectorAddKernel.CreateBuffer("_Result", _ArrayDim);
    }

    public float[] AddVectors(float[] A, float[] B)
    {
        _vectorAddKernel.FillBuffer("_A", A);
        _vectorAddKernel.FillBuffer("_B", B);

        _vectorAddKernel.Dispatch(10000, 1, 1);

        return _vectorAddKernel.GetBufferData("_Result");

    }


}
