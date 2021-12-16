using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPH;

public class VectorAddKernel
{
    private int _NBlocks;
    public KernelBufferField A;
    public KernelBufferField B;
    public KernelBufferField AddResult;
    public GlobalInt ArrayDim;


    ComputeKernel _computeKernel;
    GroupDimensionField group_dim;
    GridDimensionField grid_dim;


    public VectorAddKernel(ComputeShader shader)
    {
        _NBlocks = 2;
        _computeKernel = new ComputeKernel(shader, "VectorAdd");

        A = new KernelBufferField(_computeKernel, "_A");
        B = new KernelBufferField(_computeKernel, "_B");

        AddResult = new KernelBufferField(_computeKernel, "_AddResult");

        ArrayDim = new GlobalInt(_computeKernel, "_ArrayDim");

        grid_dim = new GridDimensionField(_computeKernel, "grid_dim");
        group_dim = new GroupDimensionField(_computeKernel, "group_dim");
        
    }

    public void Dispatch()
    {
        _computeKernel.Dispatch(_NBlocks, 1, 1);
    }

    //public float[] GetResult()
    //{
    //    return AddResult;

    //}

}
