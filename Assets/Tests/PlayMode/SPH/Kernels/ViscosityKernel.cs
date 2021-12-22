using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class ViscosityKernelInternal
    {
        ComputeKernel _computeKernel;

        public KernelBufferFieldFloat3 _Positions;
        public KernelBufferFieldFloat3 _Velocities;
        public KernelBufferFieldFloat _Densities;
        public KernelBufferFieldFloat3 _ViscosityForces;

        public GlobalFloat _hViscosity;
        public GlobalFloat _mu;
        public GlobalInt _nBodies;

        GridDimensionField grid_dim;
        GroupDimensionField group_dim;
        


        public ViscosityKernelInternal(ComputeShader shader)
        {
            _computeKernel = new ComputeKernel(shader, "ComputeViscosity");

            _Positions = new KernelBufferFieldFloat3(_computeKernel, "_Positions");
            _Densities = new KernelBufferFieldFloat(_computeKernel, "_Densities");
            _Velocities = new KernelBufferFieldFloat3(_computeKernel, "_Velocities");
            _ViscosityForces = new KernelBufferFieldFloat3(_computeKernel, "_ViscosityForces");

            _hViscosity = new GlobalFloat(_computeKernel, "_hViscosity");
            _mu = new GlobalFloat(_computeKernel, "_mu");
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


    public class ViscosityKernel
    {
        private ViscosityKernelInternal _kernelInternal;

        public ViscosityKernel(ComputeShader shader)
        {

            _kernelInternal = new ViscosityKernelInternal(shader);
        }

        public ComputeBufferWrapperFloat3 ComputeViscosityForce(ComputeBufferWrapperFloat3 Positions, 
                                                          ComputeBufferWrapperFloat3 Velocities, 
                                                          ComputeBufferWrapperFloat Densities, 
                                                          float hViscosity,
                                                          float mu,
                                                          ComputeBufferWrapperFloat3? ViscosityForces = null,
                                                          int NBlocks = 0
                                                          )
        {
            if (NBlocks == 0)
            {
                NBlocks = _kernelInternal.NumBlocks(Positions.dim);
            }
            _kernelInternal._Positions.BindBuffer(Positions);
            _kernelInternal._Densities.BindBuffer(Densities);
            _kernelInternal._Velocities.BindBuffer(Velocities);

            if (ViscosityForces is ComputeBufferWrapperFloat3 inViscosityForces)
            {
                _kernelInternal._ViscosityForces.BindBuffer(inViscosityForces);
            }
            else if (_kernelInternal._ViscosityForces.dim != Positions.dim)
            {
                //need to make a new densities buffer and bind it!
                ComputeBufferWrapperFloat3 outViscosityForces = new ComputeBufferWrapperFloat3(Positions.dim);
                _kernelInternal._ViscosityForces.BindBuffer(outViscosityForces);
            }

            _kernelInternal._hViscosity.SetFloat(hViscosity);
            _kernelInternal._mu.SetFloat(mu);

            _kernelInternal._nBodies.SetInt(Positions.dim);

            _kernelInternal.Dispatch(NBlocks);

            return _kernelInternal._ViscosityForces;
        }
    }

} //namespace SPH