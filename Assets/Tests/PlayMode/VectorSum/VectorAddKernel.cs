using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPH;

public class VectorAddKernelInternal 
{
    private int _NBlocks;
    public KernelBufferFieldFloat A;
    public KernelBufferFieldFloat B;
    public KernelBufferFieldFloat AddResult;
    public GlobalInt ArrayDim;


    ComputeKernel _computeKernel;
    GroupDimensionField group_dim;
    GridDimensionField grid_dim;


    public VectorAddKernelInternal(ComputeShader shader, int NBlocks)
    {
        _NBlocks = NBlocks;
        _computeKernel = new ComputeKernel(shader, "VectorAdd");

        A = new KernelBufferFieldFloat(_computeKernel, "_A");
        B = new KernelBufferFieldFloat(_computeKernel, "_B");

        AddResult = new KernelBufferFieldFloat(_computeKernel, "_AddResult");

        ArrayDim = new GlobalInt(_computeKernel, "_ArrayDim");

        grid_dim = new GridDimensionField(_computeKernel, "grid_dim");
        group_dim = new GroupDimensionField(_computeKernel, "group_dim");
        
    }

    public void Dispatch()
    {
        _computeKernel.Dispatch(_NBlocks, 1, 1);
    }

}


public class VectorAddKernel
{
    private VectorAddKernelInternal _vectorAddKernelInternal;

    public VectorAddKernel(ComputeShader shader, int NBlocks = 2)
    {
        _vectorAddKernelInternal = new VectorAddKernelInternal(shader, NBlocks);
    }

    public ComputeBufferWrapperFloat VectorAdd(ComputeBufferWrapperFloat ABuffer, ComputeBufferWrapperFloat BBuffer, ComputeBufferWrapperFloat? ResultBuffer = null)
    {
        _vectorAddKernelInternal.A.BindBuffer(ABuffer);
        _vectorAddKernelInternal.B.BindBuffer(BBuffer);
        if(ResultBuffer is ComputeBufferWrapperFloat _ResultBuffer)
        {
            _vectorAddKernelInternal.AddResult.BindBuffer(_ResultBuffer);
        }

        else if(_vectorAddKernelInternal.AddResult.dim != ABuffer.dim)
        {
            ComputeBufferWrapperFloat AR = new ComputeBufferWrapperFloat(ABuffer.dim);
            _vectorAddKernelInternal.AddResult.BindBuffer(AR);
        }

        _vectorAddKernelInternal.ArrayDim.SetInt(ABuffer.dim);

        _vectorAddKernelInternal.Dispatch();

        return _vectorAddKernelInternal.AddResult;
    }
}