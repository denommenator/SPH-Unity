using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScript : MonoBehaviour
{
    [SerializeField, Range(2, 1_000)]
    public int nBodies = 10;

    ComputeBuffer positionBuffer, nextPositionBuffer;
    ComputeBuffer velocityBuffer, nextVelocityBuffer;
    ComputeBuffer accelerationBuffer, nextAccelerationBuffer;

    ComputeBuffer densityBuffer, pressureBuffer;

    [SerializeField, Range(0, 1_000)]
    public float spiralScale = 1;

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

    static readonly int accelerationsID = Shader.PropertyToID("_Accelerations");
    static readonly int nextAccelerationsID = Shader.PropertyToID("_NextAccelerations");

    static readonly int densitiesID = Shader.PropertyToID("_Densities");
    static readonly int pressuresID = Shader.PropertyToID("_Pressures");

    static readonly int deltaTimeID = Shader.PropertyToID("_DeltaTime");
    static readonly int nBodiesID = Shader.PropertyToID("_nBodies");

    private float containerSize;

    

    List<GameObject> bodies;

    void OnEnable()
    {
        containerSize = spiralScale * Mathf.Sqrt(nBodies);

        densityBuffer = new ComputeBuffer(nBodies, sizeof(float));
        pressureBuffer = new ComputeBuffer(nBodies, sizeof(float));

        positionBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        nextPositionBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));

        Vector3[] initialPositions = new Vector3[nBodies];

        for (int id = 0; id < nBodies; id++)
        {
            initialPositions[id] = getFibonacciPosition(id);
        }

        positionBuffer.SetData(initialPositions);


        velocityBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        nextVelocityBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));

        Vector3[] init_velocity = new Vector3[nBodies];
        for (int i = 0; i < nBodies; i++)
            init_velocity[i] = Vector3.zero;

        velocityBuffer.SetData(init_velocity);

        accelerationBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));
        nextAccelerationBuffer = new ComputeBuffer(nBodies, 3 * sizeof(float));

        accelerationBuffer.SetData(init_velocity);

    }

    void OnDisable()
    {
        positionBuffer.Release();
        nextPositionBuffer.Release();

        velocityBuffer.Release();
        nextVelocityBuffer.Release();

        accelerationBuffer.Release();
        nextAccelerationBuffer.Release();

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


    

    // Update is called once per frame
    void Update()
    {
        moveShader.SetInt(nBodiesID, nBodies);

        moveShader.SetBuffer(moveShader.FindKernel("ComputeDensity"), positionsID, positionBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("ComputeDensity"), densitiesID, densityBuffer);

        moveShader.Dispatch(moveShader.FindKernel("ComputeDensity"), nBodies, 1, 1);

        moveShader.SetBuffer(moveShader.FindKernel("ComputePressure"), densitiesID, densityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("ComputePressure"), pressuresID, pressureBuffer);

        moveShader.Dispatch(moveShader.FindKernel("ComputePressure"), nBodies, 1, 1);


        moveShader.SetFloat(deltaTimeID, Time.deltaTime);


        moveShader.SetBuffer(moveShader.FindKernel("Main"), densitiesID, densityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("Main"), pressuresID, pressureBuffer);

        moveShader.SetBuffer(moveShader.FindKernel("Main"), positionsID, positionBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("Main"), nextPositionsID, nextPositionBuffer);


        moveShader.SetBuffer(moveShader.FindKernel("Main"), velocitiesID, velocityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("Main"), nextVelocitiesID, nextVelocityBuffer);

        moveShader.SetBuffer(moveShader.FindKernel("Main"), accelerationsID, accelerationBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("Main"), nextAccelerationsID, nextAccelerationBuffer);


        moveShader.Dispatch(moveShader.FindKernel("Main"), nBodies, 1, 1);

        //copy data from next to current
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), positionsID, positionBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), nextPositionsID, nextPositionBuffer);
                                                        
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), velocitiesID, velocityBuffer);
        moveShader.SetBuffer(moveShader.FindKernel("CopyBuffers"), nextVelocitiesID, nextVelocityBuffer);

        moveShader.Dispatch(moveShader.FindKernel("CopyBuffers"), nBodies, 1, 1);



        material.SetBuffer(positionsID, positionBuffer);

        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * containerSize);
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, nBodies);
    }
}
