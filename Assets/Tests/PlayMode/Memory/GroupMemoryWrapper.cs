using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MyComputeKernel1;

public class GroupMemoryKernel
{
    ComputeKernel _computeKernel;
    KernelBufferField my_global_array;
    KernelBufferField read_result;
    _MyGlobalInt3 group_dim;
    _MyGlobalInt3 grid_dim;
    private int _dim;


    public GroupMemoryKernel(ComputeShader shader, int dim)
    {
        _dim = dim;
        _computeKernel = new ComputeKernel(shader, "GroupMemoryTest");

        my_global_array = new KernelBufferField(_computeKernel, "my_global_array", dim);
        read_result = new KernelBufferField(_computeKernel, "read_result", dim);

        group_dim = new _MyGlobalInt3(_computeKernel, "group_dim");
        grid_dim = new _MyGlobalInt3(_computeKernel, "grid_dim", new Vector3Int(1, 1, 1));



    }

    public void Dispatch(int x, int y, int z)
    {
        _computeKernel.Dispatch(x, y, z);
    }
}
