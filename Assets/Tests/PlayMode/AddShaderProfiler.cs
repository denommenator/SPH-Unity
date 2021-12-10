using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddShaderProfiler : MonoBehaviour
{


    [SerializeField]
    ComputeShader addShader;

    private VectorAddWrapper vectorAdder;

    private float[] A;
    private float[] B;



    //Wrappers wrap kernels, not the entire shader, which might have multiple kernels
    public VectorAddWrapper VectorAddWrapperFactory(int arrayDim)
    {
        VectorAddWrapper vectorAdder = new VectorAddWrapper(addShader, arrayDim);

        return vectorAdder;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        int ArrayDim = 100_000;

        A = new float[ArrayDim];
        B = new float[ArrayDim];

        vectorAdder = VectorAddWrapperFactory(ArrayDim);


        for (int i = 0; i < ArrayDim; i++)
        {
            A[i] = i;
            B[i] = i * i;
        }


    }

    // Update is called once per frame
    void Update()
    {

        float[] Result = vectorAdder.AddVectors(A, B);

        
    }
}
