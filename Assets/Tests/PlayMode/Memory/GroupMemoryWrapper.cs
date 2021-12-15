using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MyComputeKernel1;

public class GroupMemoryWrapper : MonoBehaviour
{
    public ComputeShader computeShader;
    public Vector3Int dispatchGroupDimensions = new Vector3Int(1,1,1);

    private ComputeKernel groupMemoryTestKernel;
    private KernelFields kernelFields;



    // Start is called before the first frame update
    void Start()
    {
        groupMemoryTestKernel = gameObject.GetComponent<ComputeKernel>();
        groupMemoryTestKernel.InitializeComputeKernel(computeShader, "GroupMemoryTest");

        groupMemoryTestKernel.NewBufferVariable("my_global_array");
        groupMemoryTestKernel.NewBufferVariable("read_result");

    }

    public void InitializeGlobalArray(float[] A)
    {

    }

    public void Dispatch()
    {
        groupMemoryTestKernel.Dispatch(dispatchGroupDimensions.x, dispatchGroupDimensions.y, dispatchGroupDimensions.z);
    }
}
