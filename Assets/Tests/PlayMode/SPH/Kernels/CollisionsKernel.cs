using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class CollisionsKernelInternal
    {
        ComputeKernel _computeKernel;

        public KernelBufferFieldFloat3 _Positions;
        public KernelBufferFieldFloat3 _Velocities;
        public KernelBufferFieldContainerWall _CollisionContainerWalls;

        public GlobalInt _nBodies;
        public GlobalInt _nContainerWalls;


        GridDimensionField grid_dim;
        GroupDimensionField group_dim;
        


        public CollisionsKernelInternal(ComputeShader shader)
        {
            _computeKernel = new ComputeKernel(shader, "ResolveCollisions");

            _Positions = new KernelBufferFieldFloat3(_computeKernel, "_Positions");
            _Velocities = new KernelBufferFieldFloat3(_computeKernel, "_Velocities");
            _CollisionContainerWalls = new KernelBufferFieldContainerWall(_computeKernel, "_CollisionContainerWalls");

            _nBodies = new GlobalInt(_computeKernel, "_nBodies");
            _nContainerWalls = new GlobalInt(_computeKernel, "_nContainerWalls");

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


    public class CollisionsKernel
    {
        private CollisionsKernelInternal _kernelInternal;

        public CollisionsKernel(ComputeShader shader)
        {

            _kernelInternal = new CollisionsKernelInternal(shader);
        }

        public void ResolveCollisions(ComputeBufferWrapperFloat3 Positions, ComputeBufferWrapperFloat3 Velocities, ComputeBufferWrapperContainerWall Walls, int NBlocks = 0)
        {
            if(NBlocks == 0)
            {
                NBlocks = _kernelInternal.NumBlocks(Positions.dim);
            }
            _kernelInternal._Positions.BindBuffer(Positions);
            _kernelInternal._Velocities.BindBuffer(Velocities);
            _kernelInternal._CollisionContainerWalls.BindBuffer(Walls);

            _kernelInternal._nBodies.SetInt(Positions.dim);
            _kernelInternal._nContainerWalls.SetInt(Walls.dim);

            _kernelInternal.Dispatch(NBlocks);

        }
    }

} //namespace SPH