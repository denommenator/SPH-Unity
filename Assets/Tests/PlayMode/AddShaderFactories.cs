using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AddShaderFactories : MonoBehaviour
{

    [SerializeField]
    ComputeShader addShader;

    //Wrappers wrap kernels, not the entire shader, which might have multiple kernels
    public VectorAddWrapper VectorAddWrapperFactory(int arrayDim)
    {
        VectorAddWrapper vectorAdder = new VectorAddWrapper(addShader, arrayDim);

        return vectorAdder;
    }

}



