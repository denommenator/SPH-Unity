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
        groupMemoryKernel = new GroupMemoryKernel(shader);
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
        MyComputeKernel1.ComputeBufferWrapper.ReleaseBuffers();
    }
}
