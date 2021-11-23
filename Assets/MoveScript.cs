using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScript : MonoBehaviour
{
    

    ComputeBuffer positionBuffer, nextPositionBuffer;
    ComputeBuffer velocityBuffer, nextVelocityBuffer;
    ComputeBuffer accelerationBuffer, nextAccelerationBuffer;

    ComputeBuffer fixedPointIterationPositionBuffer, fixedPointIterationVelocityBuffer;

    ComputeBuffer densityBuffer, pressureBuffer;

    [SerializeField, Range(0, 1_000)]
    public float spiralScale = 10;

    //[SerializeField, Range(2, 1_000)]
    private int nBodies;

    [SerializeField, Range(1, 40)]
    private uint fixedPointIterations = 1;

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

    static readonly int copyBufferOriginID = Shader.PropertyToID("_CopyBufferOrigin");
    static readonly int copyBufferDestinationID = Shader.PropertyToID("_CopyBufferDestination");

    static readonly int fixedPointIterationsPositionsID = Shader.PropertyToID("_FixedPointIterationPositions");
    static readonly int fixedPointIterationsVelocitiesID = Shader.PropertyToID("_FixedPointIterationVelocities");

    static readonly int densitiesID = Shader.PropertyToID("_Densities");
    static readonly int pressuresID = Shader.PropertyToID("_Pressures");

    static readonly int deltaTimeID = Shader.PropertyToID("_DeltaTime");
    static readonly int nBodiesID = Shader.PropertyToID("_nBodies");

    private float containerSize;

    

    List<GameObject> bodies;

    void OnEnable()
    {
        nBodies = Mathf.FloorToInt(1.0f * Mathf.PI * Mathf.Pow(spiralScale, 2));

        containerSize = spiralScale * Mathf.Sqrt(nBodies);

        densityBuffer = new ComputeBuffer(nBodies, sizeof(float));
        pressureBuffer = new ComputeBuffer(nBodies, sizeof(float));

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

    }

    void OnDisable()
    {
        positionBuffer.Release();
        nextPositionBuffer.Release();

        velocityBuffer.Release();
        nextVelocityBuffer.Release();

        fixedPointIterationPositionBuffer.Release();
        fixedPointIterationVelocityBuffer.Release();

        densityBuffer.Release();
        pressureBuffer.Release();

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




    // Start is called before the first frame update
    void Start()
    {

    }


    
    void FixedUpdate()
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
            moveShader.SetFloat(deltaTimeID, Time.fixedDeltaTime / fixedPointIterations);


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
