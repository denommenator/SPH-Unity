using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class PressureKernelInternal
    {
        ComputeKernel _computeKernel;

        public KernelBufferFieldFloat _Pressures;
        public KernelBufferFieldFloat _Densities;
        
        public GlobalFloat _k;
        public GlobalFloat _densityFactor;
        public GlobalInt _nBodies;


        GridDimensionField grid_dim;
        GroupDimensionField group_dim;
        


        public PressureKernelInternal(ComputeShader shader)
        {
            _computeKernel = new ComputeKernel(shader, "ComputePressure");

            _Densities = new KernelBufferFieldFloat(_computeKernel, "_Densities");
            _Pressures = new KernelBufferFieldFloat(_computeKernel, "_Pressures");

            _k = new GlobalFloat(_computeKernel, "_k");
            _densityFactor = new GlobalFloat(_computeKernel, "_densityFactor");
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


    public class PressureKernel
    {
        private PressureKernelInternal _pressureKernelInternal;

        public PressureKernel(ComputeShader shader)
        {
            
            _pressureKernelInternal = new PressureKernelInternal(shader);
        }

        public ComputeBufferWrapperFloat ComputePressure(ComputeBufferWrapperFloat Densities, float k, float densityFactor, int NBlocks = 0, ComputeBufferWrapperFloat? Pressures = null)
        {
            if (NBlocks == 0)
            {
                NBlocks = _pressureKernelInternal.NumBlocks(Densities.dim);
            }
            _pressureKernelInternal._Densities.BindBuffer(Densities);
            
            if (Pressures is ComputeBufferWrapperFloat inPressures)
            {
                _pressureKernelInternal._Pressures.BindBuffer(inPressures);
            }
            else if (_pressureKernelInternal._Pressures.dim != Densities.dim)
            {
                //need to make a new pressures buffer and bind it!
                ComputeBufferWrapperFloat outPressures = new ComputeBufferWrapperFloat(Densities.dim);
                _pressureKernelInternal._Pressures.BindBuffer(outPressures);
            }

            _pressureKernelInternal._k.SetFloat(k);
            _pressureKernelInternal._densityFactor.SetFloat(densityFactor);
            _pressureKernelInternal._nBodies.SetInt(Densities.dim);

            _pressureKernelInternal.Dispatch(NBlocks);

            return _pressureKernelInternal._Pressures;
        }
    }

} //namespace SPH