using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class AccelerationKernelInternal
    {
        ComputeKernel _computeKernel;

        public KernelBufferFieldFloat3 _PressureForces;
        public KernelBufferFieldFloat3 _ViscosityForces;
        public KernelBufferFieldFloat _Densities;
        public KernelBufferFieldFloat3 _Accelerations;

        public GlobalInt _nBodies;
        public GlobalFloat _g;


        GridDimensionField grid_dim;
        GroupDimensionField group_dim;
        


        public AccelerationKernelInternal(ComputeShader shader)
        {
            _computeKernel = new ComputeKernel(shader, "ComputeAcceleration");

            _Densities = new KernelBufferFieldFloat(_computeKernel, "_Densities");
            _PressureForces = new KernelBufferFieldFloat3(_computeKernel, "_PressureForces");
            _ViscosityForces = new KernelBufferFieldFloat3(_computeKernel, "_ViscosityForces");
            _Accelerations = new KernelBufferFieldFloat3(_computeKernel, "_Accelerations");

            _nBodies = new GlobalInt(_computeKernel, "_nBodies");
            _g = new GlobalFloat(_computeKernel, "_g");

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


    public class AccelerationKernel
    {
        private AccelerationKernelInternal _accelerationKernelInternal;

        public AccelerationKernel(ComputeShader shader)
        {

            _accelerationKernelInternal = new AccelerationKernelInternal(shader);
        }

        public ComputeBufferWrapperFloat3 ComputeAcceleration(ComputeBufferWrapperFloat Densities, ComputeBufferWrapperFloat3 PressureForces, ComputeBufferWrapperFloat3 ViscosityForces, float g, int NBlocks = 0, ComputeBufferWrapperFloat3? Accelerations = null)
        {
            if(NBlocks == 0)
            {
                NBlocks = _accelerationKernelInternal.NumBlocks(Densities.dim);
            }
            _accelerationKernelInternal._Densities.BindBuffer(Densities);
            _accelerationKernelInternal._PressureForces.BindBuffer(PressureForces);
            _accelerationKernelInternal._ViscosityForces.BindBuffer(ViscosityForces);

            if (Accelerations is ComputeBufferWrapperFloat3 inAccelerations)
            {
                _accelerationKernelInternal._Accelerations.BindBuffer(inAccelerations);
            }
            else if (_accelerationKernelInternal._Accelerations.dim != Densities.dim)
            {
                //need to make a new pressures buffer and bind it!
                ComputeBufferWrapperFloat3 outAccelerations = new ComputeBufferWrapperFloat3(Densities.dim);
                _accelerationKernelInternal._Accelerations.BindBuffer(outAccelerations);
            }

            _accelerationKernelInternal._nBodies.SetInt(Densities.dim);
            _accelerationKernelInternal._g.SetFloat(g);

            _accelerationKernelInternal.Dispatch(NBlocks);

            return _accelerationKernelInternal._Accelerations;
        }
    }

} //namespace SPH