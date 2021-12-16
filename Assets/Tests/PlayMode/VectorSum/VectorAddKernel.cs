using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MyComputeKernel1;

public class VectorAddKernel
{
    private int _dim;
    private int _NBlocks;
    public KernelBufferField A;
    public KernelBufferField B;
    public KernelBufferField AddResult;
    public _MyGlobalInt ArrayDim;


    ComputeKernel _computeKernel;
    GroupDimensionField group_dim;
    GridDimensionField grid_dim;


    public VectorAddKernel(ComputeShader shader, int dim)
    {
        _dim = dim;
        _NBlocks = 2;
        _computeKernel = new ComputeKernel(shader, "VectorAdd");

        A = new KernelBufferField(_computeKernel, "_A", _dim);
        B = new KernelBufferField(_computeKernel, "_B", _dim);
        AddResult = new KernelBufferField(_computeKernel, "_AddResult", _dim);

        ArrayDim = new _MyGlobalInt(_computeKernel, "_ArrayDim", _dim);

        grid_dim = new GridDimensionField(_computeKernel, "grid_dim");
        group_dim = new GroupDimensionField(_computeKernel, "group_dim");
        



    }

    public void Dispatch()
    {
        _computeKernel.Dispatch(_NBlocks, 1, 1);
    }

    public float[] GetResult()
    {
        return AddResult;

    }

}
