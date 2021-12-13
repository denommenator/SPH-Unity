using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupMemoryWrapper : MonoBehaviour
{
    public ComputeShader computeShader;
    public Vector3Int dispatchGroupDimensions = new Vector3Int(1,1,1);

    private MyComputeKernel.ComputeKernel groupMemoryTestKernel;



    // Start is called before the first frame update
    void Start()
    {
        groupMemoryTestKernel = MyComputeKernel.ComputeKernel.NewComputeKernel(computeShader, "GroupMemoryTest");
        groupMemoryTestKernel.CreateUninitializedBuffer("my_global_array");
        groupMemoryTestKernel.CreateUninitializedBuffer("read_result");

    }

    public void Update()
    {
        groupMemoryTestKernel._groupDim_x = dispatchGroupDimensions.x;
        groupMemoryTestKernel._groupDim_y = dispatchGroupDimensions.y;
        groupMemoryTestKernel._groupDim_z = dispatchGroupDimensions.z;

    }

    public ref MyComputeKernel.ComputeKernel GetKernel()
    {
        return ref groupMemoryTestKernel;
    }

}
