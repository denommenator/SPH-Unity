using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH
{
    public class UIMono : MonoBehaviour
    {
        private SPHMono sphMono;

        public float containerHeight = 15;
        public float containerWidth = 40;
        public float containerDepth = 5;


        [SerializeField, Range(0, 50)]
        public float initialParticleHeight = 5.0f;

        [SerializeField, Range(0, 10.0f)]
        float hDensity = 2.0f;


        [SerializeField, Range(0, 10.0f)]
        float initialParticleSeparation = 1.0f;

        [SerializeField, Range(0, 10.0f)]
        float k = 10.0f;

        [SerializeField, Range(0, 1.0f)]
        float densityFactor = 1.0f;

        [SerializeField]
        Material material;

        [SerializeField]
        Mesh mesh;


        private ComputeBufferWrapperFloat3 _PositionsA;
        private ComputeBufferWrapperFloat3 _PositionsB;
        private ComputeBufferWrapperFloat3 _VelocitiesA;
        private ComputeBufferWrapperFloat3 _VelocitiesB;
        private ComputeBufferWrapperFloat3 _Accelerations;
        private ComputeBufferWrapperFloat _Densities;
        private ComputeBufferWrapperFloat _Pressures;
        private ComputeBufferWrapperFloat3 _PressureForces;

        private ComputeBufferWrapperFloat3 _CurrentPosition;
        private ComputeBufferWrapperFloat3 _NextPosition;
        private ComputeBufferWrapperFloat3 _CurrentVelocity;
        private ComputeBufferWrapperFloat3 _NextVelocity;



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


            Vector3[] initialPositions = getInitialPositions();
            Debug.Log("Simulating " + initialPositions.Length + " particles.\n");

            //allocate/initialize all the buffers!
            _PositionsA = new ComputeBufferWrapperFloat3(initialPositions);

            _PositionsB = new ComputeBufferWrapperFloat3(initialPositions.Length);

            _VelocitiesA = new ComputeBufferWrapperFloat3(initialPositions.Length);
            _VelocitiesB = new ComputeBufferWrapperFloat3(initialPositions.Length);

            _CurrentPosition = _PositionsA;
            _NextPosition = _PositionsB;
            _CurrentVelocity = _VelocitiesA;
            _NextVelocity = _VelocitiesB;


            _Accelerations = new ComputeBufferWrapperFloat3(initialPositions.Length);
            _Densities = new ComputeBufferWrapperFloat(initialPositions.Length);
            _Pressures = new ComputeBufferWrapperFloat(initialPositions.Length);
            _PressureForces = new ComputeBufferWrapperFloat3(initialPositions.Length);

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
} //namespace SPH