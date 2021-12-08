using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddShaderWrapper : MonoBehaviour
{
    [SerializeField]
    ComputeShader addShader;

    float[] A = new float[] { 1.0f, 2.0f, 3.0f };
    float[] B = new float[] { 1.0f, 2.0f, 3.0f };



    int ArrayDim;

    private MyComputeKernel.ComputeKernel addKernel;

    // Start is called before the first frame update
    void Start()
    {
        addKernel = new MyComputeKernel.ComputeKernel(addShader, "AddTest");

        addKernel.AddBufferAndFill("_A", A);
        addKernel.AddBufferAndFill("_B", B);
        addKernel.AddEmptyBuffer("_Result", A.Length);


        addKernel.AddGlobalConstantInt("_ArrayDim", A.Length);
    }

    public float[] RunAddTest()
    {

        addKernel.Dispatch(1, 1, 1);

        return addKernel.GetBufferData("_Result");

    }

    // Update is called once per frame
    void Update()
    {

    }
}
