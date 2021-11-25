using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScript : MonoBehaviour
{
    

    ComputeBuffer positionBuffer, nextPositionBuffer;
    ComputeBuffer velocityBuffer, nextVelocityBuffer;

    ComputeBuffer fixedPointIterationPositionBuffer, fixedPointIterationVelocityBuffer;

    ComputeBuffer densityBuffer, pressureBuffer;



    [SerializeField, Range(0, 1_000)]
    public float spiralScale = 10;

    //[SerializeField, Range(2, 1_000)]
    private int nBodies;

    [SerializeField, Range(1, 40)]
    private int stepsPerUpdate = 1;

    [SerializeField, Range(1, 40)]
    private int fixedPointIterations = 1;

    [SerializeField, Range(0, 1)]
    float wall_elasticity = 1.0f;

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

    static readonly int fixedPointIterationsPositionsID = Shader.PropertyToID("_FixedPointIterationPositions");
    static readonly int fixedPointIterationsVelocitiesID = Shader.PropertyToID("_FixedPointIterationVelocities");


    static readonly int copyBufferOriginID = Shader.PropertyToID("_CopyBufferOrigin");
    static readonly int copyBufferDestinationID = Shader.PropertyToID("_CopyBufferDestination");

    

    static readonly int densitiesID = Shader.PropertyToID("_Densities");
    static readonly int pressuresID = Shader.PropertyToID("_Pressures");

    static readonly int deltaTimeID = Shader.PropertyToID("_DeltaTime");
    static readonly int nBodiesID = Shader.PropertyToID("_nBodies");

    static readonly int collisionContainerWallsID = Shader.PropertyToID("_CollisionContainerWalls");
    static readonly int nCollisionContainerWallsID = Shader.PropertyToID("_nCollisionContainerWalls");

    static readonly int currentWallID = Shader.PropertyToID("_CurrentWallID");

    




    private float containerSize;

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

    CollisionContainerWall[] BoxContainer(float containerSize)
    {
        CollisionContainerWall wall_lower, wall_upper, wall_left, wall_right;

        wall_lower = new CollisionContainerWall
            {
                inward_normal = Vector3.up,
                point = containerSize / 2 * Vector3.down,
                elasticity = wall_elasticity
            };

        wall_upper = new CollisionContainerWall
        {
            inward_normal = Vector3.down,
            point = containerSize / 2 * Vector3.up,
            elasticity = wall_elasticity
        };

        wall_left = new CollisionContainerWall
        {
            inward_normal = Vector3.right,
            point = containerSize / 2 * Vector3.left,
            elasticity = wall_elasticity
        };

        wall_right = new CollisionContainerWall
        {
            inward_normal = Vector3.left,
            point = containerSize / 2 * Vector3.right,
            elasticity = wall_elasticity
        };


        CollisionContainerWall[] walls = new CollisionContainerWall[]
            { wall_lower, wall_upper, wall_left, wall_right };

        return walls;

    }


    void Start()
    {
        nBodies = Mathf.FloorToInt(1.0f * Mathf.PI * Mathf.Pow(spiralScale, 2));

        containerSize = spiralScale * Mathf.Sqrt(nBodies);
        nCollisionContainerWalls = 5;

        //allocate & initialize buffers

        densityBuffer = new ComputeBuffer(nBodies, sizeof(float));
        pressureBuffer = new ComputeBuffer(nBodies, sizeof(float));


        //initial positions & velocities
        positionBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        nextPositionBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));

        fixedPointIterationPositionBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        fixedPointIterationVelocityBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));

        Vector3[] initialPositions = new Vector3[nBodies];

        for (int id = 0; id < nBodies; id++)
        {
            initialPositions[id] = getFibonacciPosition(id);
        }

        positionBuffer.SetData(initialPositions);


        velocityBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        nextVelocityBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));

        Vector3[] zero_vectors = new Vector3[nBodies];
        for (int i = 0; i < nBodies; i++)
            zero_vectors[i] = Vector3.zero;

        velocityBuffer.SetData(zero_vectors);

        //collision container

        collisionContainerWalls = BoxContainer(containerSize);
        //
        CollisionContainerWallBuffer = new ComputeBuffer(nCollisionContainerWalls, sizeof(float) * (3 * 2 + 1));
        CollisionContainerWallBuffer.SetData(collisionContainerWalls);


    }

    void OnDestroy()
    {
        positionBuffer.Release();
        nextPositionBuffer.Release();

        velocityBuffer.Release();
        nextVelocityBuffer.Release();

        fixedPointIterationPositionBuffer.Release();
        fixedPointIterationVelocityBuffer.Release();

        densityBuffer.Release();
        pressureBuffer.Release();

        CollisionContainerWallBuffer.Release();

    }

    Vector3 getFibonacciPosition(int n)
    {
        float phi = (1 + Mathf.Sqrt(5.0f)) / 2.0f;
        float theta_0 = 2.0f * Mathf.PI / phi;

        float r = Mathf.Sqrt(n);
        float x = r * Mathf.Cos(n * theta_0);
        float y = r * Mathf.Sin(n * theta_0);


        return new Vector3(spiralScale * x / Mathf.Sqrt(nBodies), spiralScale * y / Mathf.Sqrt(nBodies), 0);

    }





    void FixedUpdate()
    {

        for (int step = 0; step < stepsPerUpdate; step++)
            StepSimulation();
    }

    void StepSimulation()
    {
        moveShader.SetInt(nBodiesID, nBodies);

        


        

        //start fixed-point iteration loop for backwards euler to compute q^(i+1), q_dot^(i+1)
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, positionBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, fixedPointIterationPositionBuffer);

        moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), nBodies, 1, 1);

        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, velocityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, fixedPointIterationVelocityBuffer);

        moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), nBodies, 1, 1);

        for (int i = 0; i < fixedPointIterations; i++)
        {
            //first, update density and pressure fields
            moveShader.SetBuffer(moveShader.FindKernel("ComputeDensity"), fixedPointIterationsPositionsID, fixedPointIterationPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputeDensity"), densitiesID, densityBuffer);

            moveShader.Dispatch(moveShader.FindKernel("ComputeDensity"), nBodies, 1, 1);

            moveShader.SetBuffer(moveShader.FindKernel("ComputePressure"), densitiesID, densityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("ComputePressure"), pressuresID, pressureBuffer);

            moveShader.Dispatch(moveShader.FindKernel("ComputePressure"), nBodies, 1, 1);

            //compute next fixed point iteration
            moveShader.SetFloat(deltaTimeID, Time.fixedDeltaTime / stepsPerUpdate);


            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), densitiesID, densityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), pressuresID, pressureBuffer);

            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), positionsID, positionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), nextPositionsID, nextPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), fixedPointIterationsPositionsID, fixedPointIterationPositionBuffer);


            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), velocitiesID, velocityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), nextVelocitiesID, nextVelocityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("FixedPointIteration"), fixedPointIterationsVelocitiesID, fixedPointIterationVelocityBuffer);

            moveShader.Dispatch(moveShader.FindKernel("FixedPointIteration"), nBodies, 1, 1);

            //update i->i+1 in the fixed point position/velocity buffers
            moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, nextPositionBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, fixedPointIterationPositionBuffer);

            moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), nBodies, 1, 1);

            moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, nextVelocityBuffer);
            moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, fixedPointIterationVelocityBuffer);

            moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), nBodies, 1, 1);
        }

        //collision detection / resolution
        moveShader.SetInt(nCollisionContainerWallsID, nCollisionContainerWalls);
        moveShader.SetBuffer(moveShader.FindKernel("ResolveWallCollisions"), nextPositionsID, nextPositionBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("ResolveWallCollisions"), nextVelocitiesID, nextVelocityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("ResolveWallCollisions"), collisionContainerWallsID, CollisionContainerWallBuffer);
        for (int wall_id = 0; wall_id < nCollisionContainerWalls; wall_id++)
        {
            moveShader.SetInt(currentWallID, wall_id);
            moveShader.Dispatch(moveShader.FindKernel("ResolveWallCollisions"), nBodies, 1, 1);
        }
        



        //copy data from next position/velocity to current position/velocity
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, nextPositionBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, positionBuffer);

        moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), nBodies, 1, 1);

        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferOriginID, nextVelocityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), copyBufferDestinationID, velocityBuffer);

        moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), nBodies, 1, 1);


        



        
    }

    void Update()
    {
        material.SetBuffer(positionsID, positionBuffer);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * containerSize);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, nBodies);
    }
}
