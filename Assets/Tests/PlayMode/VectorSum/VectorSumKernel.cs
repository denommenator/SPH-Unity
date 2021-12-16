using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MyComputeKernel1;

public class VectorSumKernel
{
    private int _dim;
    private int _NBlocks;
    public KernelBufferField A;
    public KernelBufferField Block_Sums;
    public KernelBufferField Sum;
    public _MyGlobalInt ArrayDim;


    ComputeKernel _computeKernel;
    GroupDimensionField group_dim;
    GridDimensionField grid_dim;


    public VectorSumKernel(ComputeShader shader, int dim)
    {
        _dim = dim;
        _NBlocks = 2;
        _computeKernel = new ComputeKernel(shader, "VectorSum");

        A = new KernelBufferField(_computeKernel, "_A", _dim);
        Block_Sums = new KernelBufferField(_computeKernel, "_Block_Sums", _NBlocks);
        Sum = new KernelBufferField(_computeKernel, "_Sum", 1);

        ArrayDim = new _MyGlobalInt(_computeKernel, "_ArrayDim", _dim);

        grid_dim = new GridDimensionField(_computeKernel, "grid_dim");
        group_dim = new GroupDimensionField(_computeKernel, "group_dim");
        



    }

    public void Dispatch()
    {
        _computeKernel.Dispatch(_NBlocks, 1, 1);
    }

    public float GetResult()
    {
        float[] result = new float[1];
        ((ComputeBuffer)Sum).GetData(result);
        return result[0];


    }

}
