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
        [SerializeField]
        ComputeShader ExplicitEulerShader;

        BufferReleaser bufferReleaser;

        //attach kernels here
        public DensityKernel densityKernel;
        public PressureKernel pressureKernel;
        public PressureForceKernel pressureForceKernel;
        public AccelerationKernel accelerationKernel;
        public ExplicitEulerKernel explicitEulerKernel;

        // Start is called before the first frame update
        void Start()
        {
            bufferReleaser = gameObject.AddComponent<BufferReleaser>();
            densityKernel = new DensityKernel(Densityshader);
            pressureKernel = new PressureKernel(PressureShader);
            pressureForceKernel = new PressureForceKernel(PressureForceShader);
            accelerationKernel = new AccelerationKernel(AccelerationShader);
            explicitEulerKernel = new ExplicitEulerKernel(ExplicitEulerShader);

        }

    }
} // namespace SPH