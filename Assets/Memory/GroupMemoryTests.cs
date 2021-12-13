using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupMemoryTests : MonoBehaviour
{
    [SerializeField]
    ComputeShader group_memory_tester;

    // Start is called before the first frame update
    void Start()
    {
        ComputeBuffer result_buffer = new ComputeBuffer(2, sizeof(int));
        ComputeBuffer read_buffer = new ComputeBuffer(2, sizeof(int));

        result_buffer.SetData(new int[] { 5, 5 });
        read_buffer.SetData(new int[] { 5, 5 });


        int kernelID = group_memory_tester.FindKernel("GroupMemoryTest");

        int result_bufferID = Shader.PropertyToID("read_result");
        int read_bufferID = Shader.PropertyToID("my_global_array");

        group_memory_tester.SetBuffer(kernelID, result_bufferID, result_buffer);
        group_memory_tester.SetBuffer(kernelID, read_bufferID, read_buffer);

        group_memory_tester.Dispatch(kernelID, 2, 1, 1);

        int[] result = new int[2];
        result_buffer.GetData(result);
        
        Debug.Log("Read: " +  result[0] + ", " + result[1]);

        result_buffer.Release();
        read_buffer.Release();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
