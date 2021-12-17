using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPH;

public class VectorSumKernelInternal
{
    private int _NBlocks;
    public KernelBufferFieldFloat A;
    public KernelBufferFieldFloat Block_Sums;
    public KernelBufferFieldFloat Sum;
    public GlobalInt ArrayDim;


    ComputeKernel _computeKernel;
    GroupDimensionField group_dim;
    GridDimensionField grid_dim;


    public VectorSumKernelInternal(ComputeShader shader, int NBlocks)
    {
        _NBlocks = NBlocks;
        _computeKernel = new ComputeKernel(shader, "VectorSum");

        A = new KernelBufferFieldFloat(_computeKernel, "_A");

        ComputeBufferWrapperFloat Block_Sums_Buffer = new ComputeBufferWrapperFloat(_NBlocks);
        Block_Sums = new KernelBufferFieldFloat(_computeKernel, "_Block_Sums", Block_Sums_Buffer);

        ComputeBufferWrapperFloat SumBuffer = new ComputeBufferWrapperFloat(1);
        Sum = new KernelBufferFieldFloat(_computeKernel, "_Sum", SumBuffer);

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

    public ComputeBufferWrapperFloat VectorSum(ComputeBufferWrapperFloat ABuffer, ComputeBufferWrapperFloat? ResultSumBuffer = null)
    {
        int NBlocks = _vectorSumKernelInternal.NBlocks;

        _vectorSumKernelInternal.A.BindBuffer(ABuffer);
        _vectorSumKernelInternal.Block_Sums.BindBuffer(new ComputeBufferWrapperFloat(NBlocks));


        if (ResultSumBuffer is ComputeBufferWrapperFloat _ResultBuffer)
        {
            _vectorSumKernelInternal.Sum.BindBuffer(_ResultBuffer);
        }

        else if (_vectorSumKernelInternal.Sum.dim != 1)
        {
            ComputeBufferWrapperFloat SumBuffer = new ComputeBufferWrapperFloat(1);
            _vectorSumKernelInternal.Sum.BindBuffer(SumBuffer);
        }

        _vectorSumKernelInternal.ArrayDim.SetInt(ABuffer.dim);

        _vectorSumKernelInternal.Dispatch();

        return _vectorSumKernelInternal.Sum;
    }
}