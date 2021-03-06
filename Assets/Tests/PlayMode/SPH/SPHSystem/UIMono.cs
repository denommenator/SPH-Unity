using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class UIMono : MonoBehaviour
    {
        private SPHMono sphMono;

        [SerializeField]
        public int iterationsPerFrame = 1;

        public float containerHeight = 40;
        public float containerWidth = 70;
        public float containerDepth = 5;
        float initialParticleSeparation = 1.0f;

        //location is a percentage of containerWidth
        [SerializeField, Range(0.0f, 0.25f)]
        public float pushWallLocation = 0.0f;

        [SerializeField, Range(0.0f, 1.0f)]
        public float wall_elasticity = 0.7f;


        [SerializeField, Range(0, 50)]
        public float initialParticleHeight = 10.0f;

        [SerializeField, Range(0, 10.0f)]
        float hDensity = 2.0f;

        [SerializeField, Range(0, 10.0f)]
        float hViscosity = 2.0f;

        [SerializeField, Range(0, 10.0f)]
        float hSurfaceTension = 2.0f;

        [SerializeField, Range(0, 10_000.0f)]
        float mu = 1500.0f;

        [SerializeField, Range(0, 10_000.0f)]
        float sigma = 1500.0f;

        [SerializeField, Range(0, 1_000.0f)]
        float k = 150.0f;

        [SerializeField, Range(0, 10.0f)]
        float g = 9.81f;

        [SerializeField, Range(0.0f, 2.0f)]
        float surfaceTensionThreshold = 0.2f;

        [SerializeField]
        Material sphMaterial;
        static readonly int sphMaterialPositionsID = Shader.PropertyToID("_Positions");


        [SerializeField]
        Mesh mesh;

        private ComputeBufferWrapperFloat3 _PositionsA;
        private ComputeBufferWrapperFloat3 _PositionsB;
        private ComputeBufferWrapperFloat3 _VelocitiesA;
        private ComputeBufferWrapperFloat3 _VelocitiesB;
        private enum currentBufferState
        { A,
          B
        }
        private currentBufferState _currentBufferState;

        private ComputeBufferWrapperFloat3 _Accelerations;
        private ComputeBufferWrapperFloat _Densities;
        private ComputeBufferWrapperFloat _Pressures;
        private ComputeBufferWrapperFloat3 _PressureForces;
        private ComputeBufferWrapperFloat3 _ViscosityForces;
        private ComputeBufferWrapperFloat3 _SurfaceForces;

        private ComputeBufferWrapperFloat3 _CurrentPositions;
        private ComputeBufferWrapperFloat3 _NextPositions;
        private ComputeBufferWrapperFloat3 _CurrentVelocities;
        private ComputeBufferWrapperFloat3 _NextVelocities;

        private ComputeBufferWrapperContainerWall _ContainerWalls;


        Vector3[] getInitialPositions()
        {
            List<Vector3> initialPositionsList = new List<Vector3>();
            for (Vector3 depth = Vector3.zero; -depth.z < containerDepth; depth += initialParticleSeparation * Vector3.back)
            {
                for (Vector3 width = containerWidth / 2 * Vector3.left; width.x < containerWidth / 2; width += initialParticleSeparation * Vector3.right)
                {
                    for (Vector3 height = Vector3.zero; height.y < initialParticleHeight; height += initialParticleSeparation * Vector3.up)
                    {
                        initialPositionsList.Add(depth + width + height);
                    }
                }
            }
            return initialPositionsList.ToArray();
        }

        // Start is called before the first frame update
        void Start()
        {
            //JANKY
            sphMono = gameObject.GetComponentInParent<SPHMono>();

            _ContainerWalls = new ComputeBufferWrapperContainerWall(CollisionContainers.BoxContainer(containerWidth, containerHeight, containerDepth, wall_elasticity, pushWallLocation));


            Vector3[] initialPositions = getInitialPositions();
            Debug.Log("Simulating " + initialPositions.Length + " particles.\n");

            //allocate/initialize all the buffers!
            _PositionsA = new ComputeBufferWrapperFloat3(initialPositions);

            _PositionsB = new ComputeBufferWrapperFloat3(initialPositions.Length);

            _VelocitiesA = new ComputeBufferWrapperFloat3(initialPositions.Length);
            _VelocitiesB = new ComputeBufferWrapperFloat3(initialPositions.Length);

            _ViscosityForces = new ComputeBufferWrapperFloat3(initialPositions.Length);
            _SurfaceForces = new ComputeBufferWrapperFloat3(initialPositions.Length);

            _CurrentPositions = _PositionsA;
            _NextPositions = _PositionsB;
            _CurrentVelocities = _VelocitiesA;
            _NextVelocities = _VelocitiesB;


            _Accelerations = new ComputeBufferWrapperFloat3(initialPositions.Length);
            _Densities = new ComputeBufferWrapperFloat(initialPositions.Length);
            _Pressures = new ComputeBufferWrapperFloat(initialPositions.Length);
            _PressureForces = new ComputeBufferWrapperFloat3(initialPositions.Length);

        }

        public void SwapNextCurrent()
        {
            if(_currentBufferState == currentBufferState.A)
            {
                _CurrentPositions = _PositionsB;
                _CurrentVelocities = _VelocitiesB;

                _NextPositions = _PositionsA;
                _NextVelocities = _VelocitiesA;

                _currentBufferState = currentBufferState.B;
            }
            else if(_currentBufferState == currentBufferState.B)
            {
                _CurrentPositions = _PositionsA;
                _CurrentVelocities = _VelocitiesA;

                _NextPositions = _PositionsB;
                _NextVelocities = _VelocitiesB;

                _currentBufferState = currentBufferState.A;
            }

        }

        // Update is called once per frame
        void FixedUpdate()
        {

            float dt = Time.fixedDeltaTime / iterationsPerFrame;

            for (int i = 0; i < iterationsPerFrame; i++)
            {
                sphMono.densityKernel.ComputeDensity(_CurrentPositions, hDensity, 0, _Densities);
                sphMono.pressureKernel.ComputePressure(_Densities, k, 1, 0, _Pressures);
                sphMono.pressureForceKernel.ComputePressureForce(_CurrentPositions, _Densities, _Pressures, hDensity, 0, _PressureForces);
                sphMono.viscosityKernel.ComputeViscosityForce(_CurrentPositions, _CurrentVelocities, _Densities, hViscosity, mu, _ViscosityForces);
                sphMono.surfaceTensionKernel.ComputeSurfaceForce(_CurrentPositions, _Densities, hSurfaceTension, sigma, surfaceTensionThreshold, _SurfaceForces);
                sphMono.accelerationKernel.ComputeAcceleration(_Densities, _PressureForces, _ViscosityForces, _SurfaceForces, g, 0, _Accelerations);

                //Explicit-Euler
                sphMono.explicitEulerKernel.ComputeNext(_CurrentVelocities, _Accelerations, dt, 0, _NextVelocities);

                sphMono.explicitEulerKernel.ComputeNext(_CurrentPositions, _CurrentVelocities, dt, 0, _NextPositions);
                // sphMono.explicitEulerKernel.ComputeNext(_CurrentVelocities, _Accelerations, dt, 0, _NextVelocities);

                sphMono.collisionsKernel.ResolveCollisions(_NextPositions, _NextVelocities, _ContainerWalls);

                SwapNextCurrent();
            }
        }

        void Update()
        {
            _ContainerWalls.SetData(CollisionContainers.BoxContainer(containerWidth, containerHeight, containerDepth, wall_elasticity, pushWallLocation));

            sphMaterial.SetBuffer(sphMaterialPositionsID, _CurrentPositions);
            float max = Mathf.Max(containerDepth, containerHeight, containerWidth);
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one * max);
            Graphics.DrawMeshInstancedProcedural(mesh, 0, sphMaterial, bounds, _CurrentPositions.dim);
        }

       
    }




} //namespace SPH