using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class SPHTestMono : MonoBehaviour
    {
        [SerializeField]
        ComputeShader Densityshader;

        BufferReleaser bufferReleaser;

        //attach test kernels here
        public DensityKernel densityKernel;

        // Start is called before the first frame update
        void Start()
        {
            bufferReleaser = gameObject.AddComponent<BufferReleaser>();
            densityKernel = new DensityKernel(Densityshader);

        }

    }
} // namespace SPH