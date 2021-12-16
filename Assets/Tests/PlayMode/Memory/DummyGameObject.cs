using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyGameObject : MonoBehaviour
{
    [SerializeField]
    ComputeShader shader;
    private GroupMemoryKernel groupMemoryKernel;
    int N;

    // Start is called before the first frame update
    void Start()
    {
        SPH.ComputeBufferWrapper my_global_array_buffer = new SPH.ComputeBufferWrapper(2);
        SPH.ComputeBufferWrapper read_result_buffer = new SPH.ComputeBufferWrapper(2);

        groupMemoryKernel = new GroupMemoryKernel(shader);

        groupMemoryKernel.my_global_array.BindBuffer(my_global_array_buffer);
        groupMemoryKernel.read_result.BindBuffer(read_result_buffer);


    }

    public float[] GetResult()
    {
        groupMemoryKernel.Dispatch(2, 1, 1);

        return groupMemoryKernel.read_result;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        SPH.ComputeBufferWrapper.ReleaseBuffers();
    }
}
