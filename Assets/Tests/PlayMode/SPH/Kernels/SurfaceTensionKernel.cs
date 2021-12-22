using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class SurfaceTensionKernelInternal
    {
        ComputeKernel _computeKernel;

        public KernelBufferFieldFloat3 _Positions;
        public KernelBufferFieldFloat _Densities;
        public KernelBufferFieldFloat3 _SurfaceForces;

        public GlobalFloat _hSurfaceTension;
        public GlobalFloat _sigma;
        public GlobalFloat _surfaceTensionThreshold;

        public GlobalInt _nBodies;

        GridDimensionField grid_dim;
        GroupDimensionField group_dim;
        


        public SurfaceTensionKernelInternal(ComputeShader shader)
        {
            _computeKernel = new ComputeKernel(shader, "ComputeSurfaceForce");

            _Positions = new KernelBufferFieldFloat3(_computeKernel, "_Positions");
            _Densities = new KernelBufferFieldFloat(_computeKernel, "_Densities");
            _SurfaceForces = new KernelBufferFieldFloat3(_computeKernel, "_SurfaceForces");

            _hSurfaceTension = new GlobalFloat(_computeKernel, "_hSurfaceTension");
            _sigma = new GlobalFloat(_computeKernel, "_sigma");
            _surfaceTensionThreshold = new GlobalFloat(_computeKernel, "_surfaceTensionThreshold");
            _nBodies = new GlobalInt(_computeKernel, "_nBodies");

            grid_dim = new GridDimensionField(_computeKernel, "grid_dim");
            group_dim = new GroupDimensionField(_computeKernel, "group_dim");

        }

        public int NumBlocks(int nParticles)
        {
            int[] block_dim = _computeKernel.BlockDim();
            return (nParticles + block_dim[0] - 1) / block_dim[0];
        }


        public void Dispatch(int NBlocks)
        {
            int CappedNBlocks = Mathf.Min(NBlocks, 65_535);
            _computeKernel.Dispatch(CappedNBlocks, 1, 1);
        }

    }


    public class SurfaceTensionKernel
    {
        private SurfaceTensionKernelInternal _kernelInternal;

        public SurfaceTensionKernel(ComputeShader shader)
        {

            _kernelInternal = new SurfaceTensionKernelInternal(shader);
        }

        public ComputeBufferWrapperFloat3 ComputeSurfaceForce(ComputeBufferWrapperFloat3 Positions, 
                                                          ComputeBufferWrapperFloat Densities, 
                                                          float hSurfaceTension,
                                                          float sigma,
                                                          float surfaceTensionThreshold,
                                                          ComputeBufferWrapperFloat3? SurfaceForces = null,
                                                          int NBlocks = 0
                                                          )
        {
            if (NBlocks == 0)
            {
                NBlocks = _kernelInternal.NumBlocks(Positions.dim);
            }
            _kernelInternal._Positions.BindBuffer(Positions);

            _kernelInternal._Densities.BindBuffer(Densities);

            if (SurfaceForces is ComputeBufferWrapperFloat3 inSurfaceForces)
            {
                _kernelInternal._SurfaceForces.BindBuffer(inSurfaceForces);
            }
            else if (_kernelInternal._SurfaceForces.dim != Positions.dim)
            {
                //need to make a new densities buffer and bind it!
                ComputeBufferWrapperFloat3 outSurfaceForces = new ComputeBufferWrapperFloat3(Positions.dim);
                _kernelInternal._SurfaceForces.BindBuffer(outSurfaceForces);
            }

            _kernelInternal._hSurfaceTension.SetFloat(hSurfaceTension);
            _kernelInternal._sigma.SetFloat(sigma);
            _kernelInternal._surfaceTensionThreshold.SetFloat(surfaceTensionThreshold);

            _kernelInternal._nBodies.SetInt(Positions.dim);

            _kernelInternal.Dispatch(NBlocks);

            return _kernelInternal._SurfaceForces;
        }
    }

} //namespace SPH