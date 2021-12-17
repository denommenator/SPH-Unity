using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class DensityKernelInternal
    {
        ComputeKernel _computeKernel;
        private int _NBlocks;

        public KernelBufferFieldFloat3 _Positions;
        public KernelBufferFieldFloat _Densities;
        
        public GlobalFloat _hDensity;
        public GlobalInt _nBodies;

        GridDimensionField grid_dim;
        GroupDimensionField group_dim;
        


        public DensityKernelInternal(ComputeShader shader, int NBlocks)
        {
            _NBlocks = NBlocks;
            _computeKernel = new ComputeKernel(shader, "ComputeDensity");

            _Positions = new KernelBufferFieldFloat3(_computeKernel, "_Positions");
            _Densities = new KernelBufferFieldFloat(_computeKernel, "_Densities");

            _hDensity = new GlobalFloat(_computeKernel, "_hDensity");
            _nBodies = new GlobalInt(_computeKernel, "_nBodies");

            grid_dim = new GridDimensionField(_computeKernel, "grid_dim");
            group_dim = new GroupDimensionField(_computeKernel, "group_dim");

        }

        public void Dispatch()
        {
            _computeKernel.Dispatch(_NBlocks, 1, 1);
        }

    }


    public class DensityKernel
    {
        private DensityKernelInternal _densityKernelInternal;

        public DensityKernel(ComputeShader shader, int NBlocks)
        {
            int CappedNBlocks = Mathf.Min(NBlocks, 65_535);
            _densityKernelInternal = new DensityKernelInternal(shader, CappedNBlocks);
        }

        public ComputeBufferWrapperFloat ComputeDensity(ComputeBufferWrapperFloat3 Positions, float hDensity, ComputeBufferWrapperFloat? Densities = null)
        {
            _densityKernelInternal._Positions.BindBuffer(Positions);
            
            if (Densities is ComputeBufferWrapperFloat inDensities)
            {
                _densityKernelInternal._Densities.BindBuffer(inDensities);
            }
            else if (_densityKernelInternal._Densities.dim != Positions.dim)
            {
                //need to make a new densities buffer and bind it!
                ComputeBufferWrapperFloat outDensities = new ComputeBufferWrapperFloat(Positions.dim);
                _densityKernelInternal._Densities.BindBuffer(outDensities);
            }

            _densityKernelInternal._hDensity.SetFloat(hDensity);
            _densityKernelInternal._nBodies.SetInt(Positions.dim);

            _densityKernelInternal.Dispatch();

            return _densityKernelInternal._Densities;
        }
    }

} //namespace SPH