using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class ExplicitEulerKernelInternal
    {
        ComputeKernel _computeKernel;

        public KernelBufferFieldFloat3 _Current;
        public KernelBufferFieldFloat3 _CurrentDot;
        public KernelBufferFieldFloat3 _Next;

        public GlobalFloat _dt;
        public GlobalInt _nBodies;


        GridDimensionField grid_dim;
        GroupDimensionField group_dim;
        


        public ExplicitEulerKernelInternal(ComputeShader shader)
        {
            _computeKernel = new ComputeKernel(shader, "ComputeNext");

            _Current = new KernelBufferFieldFloat3(_computeKernel, "_Current");
            _CurrentDot = new KernelBufferFieldFloat3(_computeKernel, "_CurrentDot");
            _Next = new KernelBufferFieldFloat3(_computeKernel, "_Next");


            _nBodies = new GlobalInt(_computeKernel, "_nBodies");
            _dt = new GlobalFloat(_computeKernel, "_dt");

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


    public class ExplicitEulerKernel
    {
        private ExplicitEulerKernelInternal _kernelInternal;

        public ExplicitEulerKernel(ComputeShader shader)
        {

            _kernelInternal = new ExplicitEulerKernelInternal(shader);
        }

        public ComputeBufferWrapperFloat3 ComputeNext(ComputeBufferWrapperFloat3 Current, ComputeBufferWrapperFloat3 CurrentDot, float dt, int NBlocks = 0, ComputeBufferWrapperFloat3? Next = null)
        {
            if (NBlocks == 0)
            {
                NBlocks = _kernelInternal.NumBlocks(Current.dim);
            }
            _kernelInternal._Current.BindBuffer(Current);
            _kernelInternal._CurrentDot.BindBuffer(CurrentDot);

            if (Next is ComputeBufferWrapperFloat3 inNext)
            {
                _kernelInternal._Next.BindBuffer(inNext);
            }
            else if (_kernelInternal._Next.dim != Current.dim)
            {
                //need to make a new pressures buffer and bind it!
                ComputeBufferWrapperFloat3 outNext = new ComputeBufferWrapperFloat3(Current.dim);
                _kernelInternal._Next.BindBuffer(outNext);
            }

            _kernelInternal._dt.SetFloat(dt);
            _kernelInternal._nBodies.SetInt(Current.dim);

            _kernelInternal.Dispatch(NBlocks);

            return _kernelInternal._Next;
        }
    }

} //namespace SPH