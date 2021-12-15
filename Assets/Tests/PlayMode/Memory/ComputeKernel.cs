using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyComputeKernel1
{
    public class ComputeBufferWrapper : MonoBehaviour
    {
        static private List<ComputeBuffer> _BufferPool;

        private int _bufferPoolIndex;
        private int _BufferDim;
        private int _BufferStride;


        static ComputeBufferWrapper NewFloatBuffer(GameObject parentObject, float[] data)
        {
            ComputeBufferWrapper bufferWrapper = parentObject.GetComponent<ComputeBufferWrapper>();

            bufferWrapper._BufferDim = data.Length;
            bufferWrapper._BufferStride = sizeof(float);
            bufferWrapper._bufferPoolIndex = ComputeBufferWrapper._BufferPool.Count;

            ComputeBuffer buffer = new ComputeBuffer(bufferWrapper._BufferDim, bufferWrapper._BufferStride);
            buffer.SetData(data);
            ComputeBufferWrapper._BufferPool.Add(buffer);

            return bufferWrapper;
        }

        static ComputeBufferWrapper NewFloatBuffer(GameObject parentObject, int dim)
        {

            float[] zero_initialized_data = new float[dim];
            return NewFloatBuffer(parentObject, zero_initialized_data);
        }

        public static implicit operator ComputeBuffer(ComputeBufferWrapper b) => ComputeBufferWrapper._BufferPool[b._bufferPoolIndex];

        public void ReleaseBuffers()
        {
            foreach(ComputeBuffer b in _BufferPool)
            {
                b.Release();
            }
        }

    }

    public class KernelBufferField
    {
        private string _codeName;
        private ComputeBufferWrapper _attachedBuffer;
        private int _dim;

        public Instantiate(int dim)
        {
            _dim = dim;
            _attachedBuffer = new ComputeBufferWrapper(dim, sizeof(float));
        }

        public static operator << (KernelBufferField bf, float[] A)
        {
            bf._attachedBuffer.SetData(A);
        }

        public static operator float[](KernelBufferField bf)
        {
            float[] data = new float[_dim];
            //use a non-blocking call for this if doing anything other than testing
            bf._attachedBuffer.GetData(data);
        }



    }

    public class KernelFields
    {
        private Dictionary<string, _MyGlobalInt> _globalConstantInts;
        private Dictionary<string, _MyGlobalInt> _globalInts;
        private Dictionary<string, _MyGlobalInt3> _globalInt3s;
        private Dictionary<string, _MyGlobalFloat> _globalFloats;

        public Dictionary<string, ComputeBufferWrapper> computeBuffers;

        public void InitializeKernelProperties()
        {
            computeBuffers = new Dictionary<string, MyComputeBuffer>();
            _globalConstantInts = new Dictionary<string, _MyGlobalInt>();
            _globalInts = new Dictionary<string, _MyGlobalInt>();
            _globalInt3s = new Dictionary<string, _MyGlobalInt3>();
            _globalFloats = new Dictionary<string, _MyGlobalFloat>();

            _globalInt3s.Add("grid_dim", new _MyGlobalInt3("grid_dim", new Vector3Int(1, 1, 1)));
            _globalInt3s.Add("group_dim", new _MyGlobalInt3("group_dim", new Vector3Int(1, 1, 1)));
        }


    }


    struct _MyGlobalInt
    {
        private string _name;
        private int _ID;
        public int value;

        public _MyGlobalInt(string name)
        {
            _name = name;
            _ID = Shader.PropertyToID(name);
            value = 0;
        }

        public _MyGlobalInt(string name, int value)
        {
            _name = name;
            _ID = Shader.PropertyToID(name);
            this.value = value;
        }

        public static implicit operator int(_MyGlobalInt i) => i.value;

        public int ID => _ID;

    }

    struct _MyGlobalFloat
    {
        private string _name;
        private int _ID;
        public float value;

        public _MyGlobalFloat(string name)
        {
            _name = name;
            _ID = Shader.PropertyToID(name);
            value = 0.0f;
        }

        public _MyGlobalFloat(string name, float value)
        {
            _name = name;
            _ID = Shader.PropertyToID(name);
            this.value = value;
        }

        public static implicit operator float(_MyGlobalFloat f) => f.value;

        public int ID => _ID;

    }

    struct _MyGlobalInt3
    {
        private string _name;
        private int _ID;
        public Vector3Int value;

        public _MyGlobalInt3(string name)
        {
            _name = name;
            _ID = Shader.PropertyToID(name);
            value = new Vector3Int(0, 0, 0);
        }

        public _MyGlobalInt3(string name, Vector3Int value)
        {
            _name = name;
            _ID = Shader.PropertyToID(name);
            this.value = value;
        }

        public static implicit operator Vector3Int(_MyGlobalInt3 i) => i.value;
        public static implicit operator int[](_MyGlobalInt3 i) => new int[] { i.value.x, i.value.y, i.value.z };

        public int ID => _ID;


    }

    


    public class ComputeKernel
    {
        private string _kernelName;
        private int _kernelNameID;

        public int _groupDim_x = 1, _groupDim_y = 1, _groupDim_z = 1;

        

        

        public void Start()
        {
            
        }

        public  ComputeKernel(ComputeShader computeShader, string kernelName)
        {
            

            
            _kernelName = kernelName;
            _kernelNameID = computeShader.FindKernel(kernelName);
            

        }



        public void AddBufferAndFill(string codeName, float[] inputArray)
        {
            MyComputeBuffer buffer = MyComputeBuffer.NewMyComputeBuffer(codeName, inputArray.Length);
            buffer.SetData(inputArray);
            computeBuffers.Add(codeName, buffer);

        }

        public void CreateUninitializedBuffer(string codeName)
        {
            MyComputeBuffer buffer = MyComputeBuffer.NewMyComputeBuffer(codeName);
            computeBuffers.Add(codeName, buffer);
        }

        public void CreateBuffer(string codeName, int dim)
        {
            MyComputeBuffer buffer = MyComputeBuffer.NewMyComputeBuffer(codeName, dim);
            computeBuffers.Add(codeName, buffer);
        }

        public void InitializeBuffer(string codeName, int dim)
        {
            computeBuffers[codeName].InitializeBuffer(dim);
        }

        public void FillBuffer(string codeName, float[] A)
        {
            computeBuffers[codeName].SetData(A);
        }

        public void AddGlobalConstantInt(string codeName, int inputValue)
        {
            _MyGlobalInt myInt = new _MyGlobalInt(codeName, inputValue);
            _globalConstantInts.Add(codeName, myInt);
            _computeShader.SetInt(myInt.ID, myInt);
        }

        public void AddGlobalFloat(string codeName)
        {
            _MyGlobalFloat myFloat = new _MyGlobalFloat(codeName);
            _globalFloats.Add(codeName, myFloat);
        }

        public void Dispatch(ComputeShader computeShader, int x, int y, int z)
        {
            _MyGlobalInt3 grid_dim = new _MyGlobalInt3("grid_dim", new Vector3Int(x, y, z));
            _globalInt3s["grid_dim"] = grid_dim; //accessors return a copy of the struct, so can't modify in place

            uint group_dim_x, group_dim_y, group_dim_z;
            computeShader.GetKernelThreadGroupSizes(_kernelNameID, out group_dim_x, out group_dim_y, out group_dim_z);
            _MyGlobalInt3 block_dim = new _MyGlobalInt3("group_dim", new Vector3Int((int)group_dim_x, (int)group_dim_y, (int)group_dim_z));
            _globalInt3s["group_dim"] = block_dim; //accessors return a copy of the struct, so can't modify in place



            foreach (MyComputeBuffer buffer in computeBuffers.Values)
            {
                computeShader.SetBuffer(_kernelNameID, buffer.ID, buffer);
            }

            foreach (_MyGlobalInt i in _globalInts.Values)
            {
                computeShader.SetInt(i.ID, i);
            }

            foreach (_MyGlobalInt3 i3 in _globalInt3s.Values)
            {
                computeShader.SetInts(i3.ID, i3);
            }

            foreach (_MyGlobalFloat f in _globalFloats.Values)
            {
                computeShader.SetFloat(f.ID, f);
            }


            computeShader.Dispatch(_kernelNameID, x, y, z);
        }


        //oh god, there is no GetFloat...
        //public float GetFloat(string codeName)
        //{
        //    return _globalFloats[codeName].GetFloat();
        //}

        public float[] GetBufferData(string codeName)
        {
            return computeBuffers[codeName].GetData();
        }


    }
}