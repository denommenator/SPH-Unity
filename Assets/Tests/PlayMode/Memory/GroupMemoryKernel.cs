using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SPH;

public class GroupMemoryKernel
{
    public KernelBufferFieldFloat my_global_array;
    public KernelBufferFieldFloat read_result;


    ComputeKernel _computeKernel;
    GroupDimensionField group_dim;
    GridDimensionField grid_dim;


    public GroupMemoryKernel(ComputeShader shader)
    { 
        _computeKernel = new ComputeKernel(shader, "GroupMemoryTest");

        my_global_array = new KernelBufferFieldFloat(_computeKernel, "my_global_array");
        read_result = new KernelBufferFieldFloat(_computeKernel, "read_result");

        group_dim = new GroupDimensionField(_computeKernel, "group_dim");
        grid_dim = new GridDimensionField(_computeKernel, "grid_dim");



    }

    public void Dispatch(int x, int y, int z)
    {
        _computeKernel.Dispatch(x, y, z);
    }


}
