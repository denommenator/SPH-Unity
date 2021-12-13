using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class VectorSumWrapper 
{

    private MyComputeKernel.ComputeKernel _vectorSumKernel;
    private int _ArrayDim;

    public VectorSumWrapper(ComputeShader computeShader, int arrayDim)
    {
        _vectorSumKernel = new MyComputeKernel.ComputeKernel(computeShader, "VectorSum");
        _ArrayDim = arrayDim;

        _vectorSumKernel.AddGlobalConstantInt("_ArrayDim", arrayDim);

        //_vectorSumKernel.CreateBuffer("_Sum_Result", 1);

        _vectorSumKernel.CreateBuffer("_A", _ArrayDim);
        _vectorSumKernel.CreateUninitializedBuffer("_Block_Sums");
        _vectorSumKernel.CreateBuffer("_Sum", 1);

    }

    public float SumVector(float[] A)
    {
        int nBlocks = 2;
        _vectorSumKernel.FillBuffer("_A", A);
        _vectorSumKernel.InitializeBuffer("_Block_Sums", nBlocks);

        _vectorSumKernel.Dispatch(nBlocks, 1, 1);

        float[] sum_array = _vectorSumKernel.GetBufferData("_Sum");
        float[] block_sum_array = _vectorSumKernel.GetBufferData("_Block_Sums");

        return sum_array[0];

            //total cop out!
        //return block_sum_array.Sum();

    }

    public void ReleaseBuffers()
    {
        _vectorSumKernel.ReleaseBuffers();
    }
}
