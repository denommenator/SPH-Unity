using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPH;

public class VectorSumKernelInternal
{
    private int _NBlocks;
    public KernelBufferField A;
    public KernelBufferField Block_Sums;
    public KernelBufferField Sum;
    public GlobalInt ArrayDim;


    ComputeKernel _computeKernel;
    GroupDimensionField group_dim;
    GridDimensionField grid_dim;


    public VectorSumKernelInternal(ComputeShader shader, int NBlocks)
    {
        _NBlocks = NBlocks;
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

    public int NBlocks => _NBlocks;

    public void Dispatch()
    {
        _computeKernel.Dispatch(_NBlocks, 1, 1);
    }


}


public class VectorSumKernel
{
    private VectorSumKernelInternal _vectorSumKernelInternal;

    public VectorSumKernel(ComputeShader shader, int NBlocks = 2)
    {
        _vectorSumKernelInternal = new VectorSumKernelInternal(shader, NBlocks);
    }

    public ComputeBufferWrapper VectorSum(ComputeBufferWrapper ABuffer, ComputeBufferWrapper? ResultSumBuffer = null)
    {
        int NBlocks = _vectorSumKernelInternal.NBlocks;

        _vectorSumKernelInternal.A.BindBuffer(ABuffer);
        _vectorSumKernelInternal.Block_Sums.BindBuffer(new ComputeBufferWrapper(NBlocks));


        if (ResultSumBuffer is ComputeBufferWrapper _ResultBuffer)
        {
            _vectorSumKernelInternal.Sum.BindBuffer(_ResultBuffer);
        }

        else if (_vectorSumKernelInternal.Sum.dim != 1)
        {
            ComputeBufferWrapper SumBuffer = new ComputeBufferWrapper(1);
            _vectorSumKernelInternal.Sum.BindBuffer(SumBuffer);
        }

        _vectorSumKernelInternal.ArrayDim.SetInt(ABuffer.dim);

        _vectorSumKernelInternal.Dispatch();

        return _vectorSumKernelInternal.Sum;
    }
}