using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class SPHMono : MonoBehaviour
    {
        [SerializeField]
        ComputeShader Densityshader;
        [SerializeField]
        ComputeShader PressureShader;
        [SerializeField]
        ComputeShader PressureForceShader;
        [SerializeField]
        ComputeShader AccelerationShader;

        BufferReleaser bufferReleaser;

        //attach kernels here
        public DensityKernel densityKernel;
        public PressureKernel pressureKernel;
        public PressureForceKernel pressureForceKernel;
        public AccelerationKernel accelerationKernel;

        // Start is called before the first frame update
        void Start()
        {
            bufferReleaser = gameObject.AddComponent<BufferReleaser>();
            densityKernel = new DensityKernel(Densityshader);
            pressureKernel = new PressureKernel(PressureShader);
            pressureForceKernel = new PressureForceKernel(PressureForceShader);
            accelerationKernel = new AccelerationKernel(AccelerationShader);

        }

    }
} // namespace SPH