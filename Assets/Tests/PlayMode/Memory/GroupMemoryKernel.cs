using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MyComputeKernel1;

public class GroupMemoryKernel
{
    public KernelBufferField my_global_array;
    public KernelBufferField read_result;


    ComputeKernel _computeKernel;
    GroupDimensionField group_dim;
    GridDimensionField grid_dim;


    public GroupMemoryKernel(ComputeShader shader)
    {
        _computeKernel = new ComputeKernel(shader, "GroupMemoryTest");

        my_global_array = new KernelBufferField(_computeKernel, "my_global_array", 2);
        read_result = new KernelBufferField(_computeKernel, "read_result", 2);

        group_dim = new GroupDimensionField(_computeKernel, "group_dim");
        grid_dim = new GridDimensionField(_computeKernel, "grid_dim");



    }

    public void Dispatch(int x, int y, int z)
    {
        _computeKernel.Dispatch(x, y, z);
    }

    public float[] GetResult()
    {
        float[] result = new float[2];
        ((ComputeBuffer)read_result).GetData(result);
        return result;


    }

}
