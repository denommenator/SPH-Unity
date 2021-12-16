using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPH;

public class VectorSumKernel
{
    private int _NBlocks;
    public KernelBufferField A;
    public KernelBufferField Block_Sums;
    public KernelBufferField Sum;
    public GlobalInt ArrayDim;


    ComputeKernel _computeKernel;
    GroupDimensionField group_dim;
    GridDimensionField grid_dim;


    public VectorSumKernel(ComputeShader shader)
    {
        _NBlocks = 2;
        _computeKernel = new ComputeKernel(shader, "VectorSum");

        A = new KernelBufferField(_computeKernel, "_A");

        ComputeBufferWrapper Block_Sums_Buffer = new ComputeBufferWrapper(_NBlocks);
        Block_Sums = new KernelBufferField(_computeKernel, "_Block_Sums", Block_Sums_Buffer);

        ComputeBufferWrapper SumBuffer = new ComputeBufferWrapper(1);
        Sum = new KernelBufferField(_computeKernel, "_Sum", SumBuffer);

        ArrayDim = new GlobalInt(_computeKernel, "_ArrayDim");

        grid_dim = new GridDimensionField(_computeKernel, "grid_dim");
        group_dim = new GroupDimensionField(_computeKernel, "group_dim");
        



    }

    public void Dispatch()
    {
        _computeKernel.Dispatch(_NBlocks, 1, 1);
    }


}
