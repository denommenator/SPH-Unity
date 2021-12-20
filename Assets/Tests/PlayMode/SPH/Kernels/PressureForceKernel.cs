using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class PressureForceKernelInternal
    {
        ComputeKernel _computeKernel;

        public KernelBufferFieldFloat3 _Positions;
        public KernelBufferFieldFloat _Densities;
        public KernelBufferFieldFloat _Pressures;

        public KernelBufferFieldFloat3 _PressureForces;

        public GlobalFloat _hDensity;
        public GlobalInt _nBodies;

        GridDimensionField grid_dim;
        GroupDimensionField group_dim;
        


        public PressureForceKernelInternal(ComputeShader shader)
        {
            _computeKernel = new ComputeKernel(shader, "ComputePressureForces");

            _Positions = new KernelBufferFieldFloat3(_computeKernel, "_Positions");
            _Densities = new KernelBufferFieldFloat(_computeKernel, "_Densities");
            _Pressures = new KernelBufferFieldFloat(_computeKernel, "_Pressures");

            _PressureForces = new KernelBufferFieldFloat3(_computeKernel, "_PressureForces");

            _hDensity = new GlobalFloat(_computeKernel, "_hDensity");
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


    public class PressureForceKernel
    {
        private PressureForceKernelInternal _pressureForceKernelInternal;

        public PressureForceKernel(ComputeShader shader)
        {
            
            _pressureForceKernelInternal = new PressureForceKernelInternal(shader);
        }

        public ComputeBufferWrapperFloat3 ComputePressureForce(ComputeBufferWrapperFloat3 Positions, 
                                                              ComputeBufferWrapperFloat Densities,
                                                              ComputeBufferWrapperFloat Pressures,
                                                              float hDensity, 
                                                              int NBlocks = 0, 
                                                              ComputeBufferWrapperFloat3? PressureForces = null)
        {
            if(NBlocks == 0)
            {
                NBlocks = _pressureForceKernelInternal.NumBlocks(Positions.dim);
            }
            _pressureForceKernelInternal._Positions.BindBuffer(Positions);
            _pressureForceKernelInternal._Densities.BindBuffer(Densities);
            _pressureForceKernelInternal._Pressures.BindBuffer(Pressures);

            if (PressureForces is ComputeBufferWrapperFloat3 inPressureForces)
            {
                _pressureForceKernelInternal._PressureForces.BindBuffer(inPressureForces);
            }
            else if (_pressureForceKernelInternal._PressureForces.dim != Positions.dim)
            {
                //need to make a new densities buffer and bind it!
                ComputeBufferWrapperFloat3 outPressureForces = new ComputeBufferWrapperFloat3(Positions.dim);
                _pressureForceKernelInternal._PressureForces.BindBuffer(outPressureForces);
            }

            _pressureForceKernelInternal._hDensity.SetFloat(hDensity);
            _pressureForceKernelInternal._nBodies.SetInt(Positions.dim);

            _pressureForceKernelInternal.Dispatch(NBlocks);

            return _pressureForceKernelInternal._PressureForces;
        }
    }

} //namespace SPH