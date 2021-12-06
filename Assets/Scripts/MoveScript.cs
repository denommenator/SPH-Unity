using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScript : MonoBehaviour
{
    private int thread_groups_dim = 64;
    private int n_thread_groups;


    ComputeBuffer positionBuffer, nextPositionBuffer;
    ComputeBuffer velocityBuffer, nextVelocityBuffer;

    ComputeBuffer viscocityFieldBuffer;

    ComputeBuffer accelerationBuffer;

    ComputeBuffer fixedPointIterationPositionBuffer, fixedPointIterationVelocityBuffer;

    ComputeBuffer densityBuffer, pressureBuffer;

    ComputeBuffer colorFieldGradientBuffer, colorFieldLaplacianBuffer;


    public float containerHeight = 15;
    public float containerWidth = 40;
    public float containerDepth = 5;


    [SerializeField, Range(0, 1_000)]
    public float initialParticleHeight = 5.0f;

    //[SerializeField, Range(2, 1_000)]
    private int nBodies;

    [SerializeField, Range(1, 40)]
    private int stepsPerUpdate = 4;

    [SerializeField, Range(1, 40)]
    private int fixedPointIterations = 8;

    [SerializeField, Range(0, 1)]
    float wall_elasticity = 0.5f;

    [SerializeField, Range(0, 9.8f)]
    float g = 0.0f;

    [SerializeField, Range(0, 10.0f)]
    float hDensity = 2.0f;

    [SerializeField, Range(0, 10.0f)]
    float hViscocity = 2.0f;

    [SerializeField, Range(0, 10.0f)]
    float hSurfaceTension = 2.0f;


    [SerializeField, Range(0, 10.0f)]
    float initialParticleSeparation = 1.0f;

    [SerializeField, Range(0, 10.0f)]
    float k = 10.0f;

    [SerializeField, Range(0, 1_000_000.0f)]
    float mu = 500.0f;

    [SerializeField, Range(0, 10.0f)]
    float surfaceTensionThreshold = 0.5f;

    [SerializeField, Range(0, 10000.0f)]
    float sigma = 1500.0f;

    [SerializeField, Range(0, 1.0f)]
    float densityFactor = 0.5f;

    [SerializeField]
    Material material;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    ComputeShader moveShader;

    static readonly int positionsID = Shader.PropertyToID("_Positions");
    static readonly int nextPositionsID = Shader.PropertyToID("_NextPositions");

    static readonly int velocitiesID = Shader.PropertyToID("_Velocities");
    static readonly int nextVelocitiesID = Shader.PropertyToID("_NextVelocities");

    static readonly int viscocityFieldID = Shader.PropertyToID("_ViscocityField");

    static readonly int accelerationsID = Shader.PropertyToID("_Accelerations");

    static readonly int fixedPointIterationsPositionsID = Shader.PropertyToID("_FixedPointIterationPositions");
    static readonly int fixedPointIterationsVelocitiesID = Shader.PropertyToID("_FixedPointIterationVelocities");


    static readonly int copyBufferOriginID = Shader.PropertyToID("_CopyBufferOrigin");
    static readonly int copyBufferDestinationID = Shader.PropertyToID("_CopyBufferDestination");

    

    static readonly int densitiesID = Shader.PropertyToID("_Densities");
    static readonly int pressuresID = Shader.PropertyToID("_Pressures");

    static readonly int colorFieldGradientID = Shader.PropertyToID("_ColorFieldGradient");
    static readonly int colorFieldLaplacianID = Shader.PropertyToID("_ColorFieldLaplacian");

    static readonly int deltaTimeID = Shader.PropertyToID("_DeltaTime");
    static readonly int nBodiesID = Shader.PropertyToID("_nBodies");

    static readonly int hDensityID = Shader.PropertyToID("_hDensity");
    static readonly int hViscocityID = Shader.PropertyToID("_hViscocity");
    static readonly int hSurfaceTensionID = Shader.PropertyToID("_hSurfaceTension");

    static readonly int gID = Shader.PropertyToID("_g");
    static readonly int kID = Shader.PropertyToID("_k");
    static readonly int muID = Shader.PropertyToID("_mu");
    static readonly int sigmaID = Shader.PropertyToID("_sigma");
    static readonly int densityFactorID = Shader.PropertyToID("_densityFactor");

    static readonly int surfaceTensionThresholdID = Shader.PropertyToID("_surfaceTensionThreshold");

    static readonly int collisionContainerWallsID = Shader.PropertyToID("_CollisionContainerWalls");
    static readonly int nCollisionContainerWallsID = Shader.PropertyToID("_nCollisionContainerWalls");

    static readonly int currentWallID = Shader.PropertyToID("_CurrentWallID");





    //refactor to 3 buffers
    public struct CollisionContainerWall
    {
        public Vector3 inward_normal;
        public Vector3 point;
        public float elasticity;
    }

    private int nCollisionContainerWalls;

    private CollisionContainerWall[] collisionContainerWalls;
    ComputeBuffer CollisionContainerWallBuffer;

    CollisionContainerWall[] BoxContainer(float width, float height, float depth, float wallElasticity)
    {
        CollisionContainerWall wall_lower, wall_upper, wall_left, wall_right, wall_back, wall_front;

        wall_lower = new CollisionContainerWall
            {
                inward_normal = Vector3.up,
                point = Vector3.zero,
                elasticity = wallElasticity
        };

        wall_upper = new CollisionContainerWall
        {
            inward_normal = Vector3.down,
            point = height * Vector3.up,
            elasticity = wallElasticity
        };

        wall_left = new CollisionContainerWall
        {
            inward_normal = Vector3.right,
            point = width / 2 * Vector3.left,
            elasticity = wallElasticity
        };

        wall_right = new CollisionContainerWall
        {
            inward_normal = Vector3.left,
            point = width / 2 * Vector3.right,
            elasticity = wallElasticity
        };

        wall_back = new CollisionContainerWall
        {
            inward_normal = Vector3.forward,
            point = depth * Vector3.back,
            elasticity = wallElasticity
        };

        wall_front = new CollisionContainerWall
        {
            inward_normal = Vector3.back,
            point = Vector3.zero,
            elasticity = wallElasticity
        };


        CollisionContainerWall[] walls = new CollisionContainerWall[]
            { wall_lower, wall_upper, wall_left, wall_right, wall_back, wall_front };

        return walls;

    }

    
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


    void Start()
    {
        

    

        Vector3[] initialPositions = getInitialPositions();

        nBodies = initialPositions.Length;
        Debug.Log("Simulating " + nBodies + " particles.\n");
        n_thread_groups = Mathf.CeilToInt((float)nBodies / thread_groups_dim);


        //allocate & initialize buffers

        densityBuffer = new ComputeBuffer(nBodies, sizeof(float));
        pressureBuffer = new ComputeBuffer(nBodies, sizeof(float));

        colorFieldGradientBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        colorFieldLaplacianBuffer = new ComputeBuffer(nBodies, sizeof(float));

        viscocityFieldBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));

        //initial positions & velocities
        positionBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        nextPositionBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));

        fixedPointIterationPositionBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        fixedPointIterationVelocityBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));



        positionBuffer.SetData(initialPositions);


        velocityBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        nextVelocityBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));

        Vector3[] zero_vectors = new Vector3[nBodies];
        for (int i = 0; i < nBodies; i++)
            zero_vectors[i] = Vector3.zero;

        velocityBuffer.SetData(zero_vectors);


        accelerationBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        accelerationBuffer.SetData(zero_vectors);

        //set up collision container

        collisionContainerWalls = BoxContainer(containerWidth, containerHeight, containerDepth, wall_elasticity);
        nCollisionContainerWalls = collisionContainerWalls.Length;

        CollisionContainerWallBuffer = new ComputeBuffer(nCollisionContainerWalls, sizeof(float) * (3 * 2 + 1));
        CollisionContainerWallBuffer.SetData(collisionContainerWalls);




    }

    void OnDestroy()
    {
        positionBuffer.Release();
        nextPositionBuffer.Release();

        velocityBuffer.Release();
        nextVelocityBuffer.Release();

        accelerationBuffer.Release();

        viscocityFieldBuffer.Release();

        fixedPointIterationPositionBuffer.Release();
        fixedPointIterationVelocityBuffer.Release();

        densityBuffer.Release();
        pressureBuffer.Release();

        colorFieldGradientBuffer.Release();
        colorFieldLaplacianBuffer.Release();

        CollisionContainerWallBuffer.Release();

    }





    void FixedUpdate()
    {

        for (int step = 0; step < stepsPerUpdate; step++)
            StepSimulation();
    }

    void StepSimulation()
    {
        moveShader.SetInt(nBodiesID, nBodies);
        moveShader.SetFloat(hDensityID, hDensity);
        moveShader.SetFloat(hViscocityID, hViscocity);
        moveShader.SetFloat(hSurfaceTensionID, hSurfaceTension);

        moveShader.SetFloat(gID, g);        
        moveShader.SetFloat(kID, k);
        moveShader.SetFloat(muID, mu);
        moveShader.SetFloat(sigmaID, sigma);
        moveShader.SetFloat(surfaceTensionThresholdID, surfaceTensionThreshold);
        moveShader.SetFloat(densityFactorID, densityFactor);




        //start fixed-point iteration loop for backwards euler to compute q^(i+1), q_dot^(i+1)
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, positionBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, fixedPointIterationPositionBuffer);

        moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), n_thread_groups, 1, 1);

        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, velocityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, fixedPointIterationVelocityBuffer);

        moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), n_thread_groups, 1, 1);

        for (int i = 0; i < fixedPointIterations; i++)
        {
            //first, update density, pressure and color fields
            moveShader.SetBuffer(moveShader.FindKernel("ComputeDensity"), fixedPointIterationsPositionsID, fixedPointIterationPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeDensity"), densitiesID, densityBuffer);

            moveShader.Dispatch(moveShader.FindKernel("ComputeDensity"), n_thread_groups, 1, 1);

            moveShader.SetBuffer(moveShader.FindKernel("ComputePressure"), densitiesID, densityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputePressure"), pressuresID, pressureBuffer);

            moveShader.Dispatch(moveShader.FindKernel("ComputePressure"), n_thread_groups, 1, 1);

            moveShader.SetBuffer(moveShader.FindKernel("ComputeColorFieldGradient"), densitiesID, densityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeColorFieldGradient"), fixedPointIterationsPositionsID, fixedPointIterationPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeColorFieldGradient"), colorFieldGradientID, colorFieldGradientBuffer);
            
            moveShader.Dispatch(moveShader.FindKernel("ComputeColorFieldGradient"), n_thread_groups, 1, 1);


            moveShader.SetBuffer(moveShader.FindKernel("ComputeColorFieldLaplacian"), densitiesID, densityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeColorFieldLaplacian"), fixedPointIterationsPositionsID, fixedPointIterationPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeColorFieldLaplacian"), colorFieldLaplacianID, colorFieldLaplacianBuffer);
            
            moveShader.Dispatch(moveShader.FindKernel("ComputeColorFieldLaplacian"), n_thread_groups, 1, 1);


            moveShader.SetBuffer(moveShader.FindKernel("ComputeViscocityField"), densitiesID, densityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeViscocityField"), fixedPointIterationsPositionsID, fixedPointIterationPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeViscocityField"), fixedPointIterationsVelocitiesID, fixedPointIterationVelocityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeViscocityField"), viscocityFieldID, viscocityFieldBuffer);

            moveShader.Dispatch(moveShader.FindKernel("ComputeViscocityField"), n_thread_groups, 1, 1);
            ///


            //compute the acceleration
            moveShader.SetFloat(deltaTimeID, Time.fixedDeltaTime / stepsPerUpdate);


            moveShader.SetBuffer(moveShader.FindKernel("ComputeAcceleration"), densitiesID, densityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeAcceleration"), pressuresID, pressureBuffer);
                                                        
            moveShader.SetBuffer(moveShader.FindKernel("ComputeAcceleration"), colorFieldGradientID, colorFieldGradientBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeAcceleration"), colorFieldLaplacianID, colorFieldLaplacianBuffer);

            moveShader.SetBuffer(moveShader.FindKernel("ComputeAcceleration"), viscocityFieldID, viscocityFieldBuffer);

            moveShader.SetBuffer(moveShader.FindKernel("ComputeAcceleration"), fixedPointIterationsPositionsID, fixedPointIterationPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeAcceleration"), fixedPointIterationsVelocitiesID, fixedPointIterationVelocityBuffer);

            moveShader.SetBuffer(moveShader.FindKernel("ComputeAcceleration"), accelerationsID, accelerationBuffer);

            moveShader.Dispatch(moveShader.FindKernel("ComputeAcceleration"), n_thread_groups, 1, 1);


            //compute the new positions and velocities
            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), positionsID, positionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), velocitiesID, velocityBuffer);


            //moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), fixedPointIterationsPositionsID, fixedPointIterationPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), fixedPointIterationsVelocitiesID, fixedPointIterationVelocityBuffer);

            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), accelerationsID, accelerationBuffer);

            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), nextPositionsID, nextPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), nextVelocitiesID, nextVelocityBuffer);

            moveShader.Dispatch(moveShader.FindKernel("FixedPointIteration"), n_thread_groups, 1, 1);

            //update i->i+1 in the fixed point position/velocity buffers
            moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, nextPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, fixedPointIterationPositionBuffer);

            moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), n_thread_groups, 1, 1);

            moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, nextVelocityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, fixedPointIterationVelocityBuffer);

            moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), n_thread_groups, 1, 1);
        }

        //collision detection / resolution
        moveShader.SetInt(nCollisionContainerWallsID, nCollisionContainerWalls);
        moveShader.SetBuffer(moveShader.FindKernel("ResolveWallCollisions"), nextPositionsID, nextPositionBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("ResolveWallCollisions"), nextVelocitiesID, nextVelocityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("ResolveWallCollisions"), collisionContainerWallsID, CollisionContainerWallBuffer);
        for (int wall_id = 0; wall_id < nCollisionContainerWalls; wall_id++)
        {
            moveShader.SetInt(currentWallID, wall_id);
            moveShader.Dispatch(moveShader.FindKernel("ResolveWallCollisions"), n_thread_groups, 1, 1);
        }
        



        //copy data from next position/velocity to current position/velocity
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, nextPositionBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, positionBuffer);

        moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), n_thread_groups, 1, 1);

        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, nextVelocityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, velocityBuffer);

        moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), n_thread_groups, 1, 1);


        



        
    }

    void Update()
    {
        material.SetBuffer(positionsID, positionBuffer);
        float max = Mathf.Max(containerDepth, containerHeight, containerWidth);
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * max);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, nBodies);
    }
}
