using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyComputeKernel
{
    class _MyComputeBuffer
    {
        private string _name;
        private int _ID;
        private int _Dim;
        private ComputeBuffer _Buffer;

        public _MyComputeBuffer(string name, int dim)
        {
            
            _name = name;
            _ID = Shader.PropertyToID(name);
            _Dim = dim;
            InitializeBuffer(dim);

        }

        public _MyComputeBuffer(string name)
        {

            _name = name;
            _ID = Shader.PropertyToID(name);
            _Dim = 0;

            Debug.Log("Creating the uninitialized buffer: " + _name);
        }


        public static implicit operator ComputeBuffer(_MyComputeBuffer b) => b._Buffer;

        ~_MyComputeBuffer()
        {
            if (_Buffer.IsValid())
            {
                Debug.Log("Are you sure you meant to not release this buffer?!" + _name);
                _Buffer.Release();
            }
        }

        public int ID => _ID;

        public void InitializeBuffer(int dim)
        {
            _Dim = dim;
            _Buffer = new ComputeBuffer(_Dim, sizeof(float));
        }

        public void SetData(float[] data)
        {
            _Buffer.SetData(data);
        }
        

        public float[] GetData()
        {
            float[] array = new float[_Dim];
            _Buffer.GetData(array);
            return array;
        }

        public void Release()
        {
            _Buffer.Release();
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

    class ComputeKernel
    {
        private ComputeShader _computeShader;
        private string _kernelName;
        private int _kernelNameID;
        

        private Dictionary<string, _MyGlobalInt> _globalConstantInts;
        private Dictionary<string, _MyGlobalInt> _globalInts;
        private Dictionary<string, _MyGlobalInt3> _globalInt3s;
        private Dictionary<string, _MyGlobalFloat> _globalFloats;

        private Dictionary<string, _MyComputeBuffer> _computeBuffers;


        public ComputeKernel(ComputeShader computeShader, string kernelName)
        {
            _computeShader = computeShader;
            _kernelName = kernelName;
            _kernelNameID = _computeShader.FindKernel(kernelName);
            _computeBuffers = new Dictionary<string, _MyComputeBuffer>();
            _globalConstantInts = new Dictionary<string, _MyGlobalInt>();
            _globalInts = new Dictionary<string, _MyGlobalInt>();
            _globalInt3s = new Dictionary<string, _MyGlobalInt3>();
            _globalFloats = new Dictionary<string, _MyGlobalFloat>();

            _globalInt3s.Add("grid_dim", new _MyGlobalInt3("grid_dim", new Vector3Int(1,1,1)));
        }



        public void AddBufferAndFill(string codeName, float[] inputArray)
        {
            _MyComputeBuffer buffer = new _MyComputeBuffer(codeName, inputArray.Length);
            buffer.SetData(inputArray);
            _computeBuffers.Add(codeName, buffer);

        }

        public void CreateUninitializedBuffer(string codeName)
        {
            _MyComputeBuffer buffer = new _MyComputeBuffer(codeName);
            _computeBuffers.Add(codeName, buffer);
        }

        public void CreateBuffer(string codeName, int dim)
        {
            _MyComputeBuffer buffer = new _MyComputeBuffer(codeName, dim);
            _computeBuffers.Add(codeName, buffer);
        }

        public void InitializeBuffer(string codeName, int dim)
        {
            _computeBuffers[codeName].InitializeBuffer(dim);
        }

        public void FillBuffer(string codeName, float[] A)
        {
            _computeBuffers[codeName].SetData(A);
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

        public void Dispatch(int x, int y, int z)
        {
            _MyGlobalInt3 grid_dim = new _MyGlobalInt3("grid_dim", new Vector3Int(x, y, z));
            _globalInt3s["grid_dim"] = grid_dim; //accessors return a copy of the struct, so can't modify in place

            foreach(_MyComputeBuffer buffer in _computeBuffers.Values)
            {
                _computeShader.SetBuffer(_kernelNameID, buffer.ID, buffer);
            }

            foreach (_MyGlobalInt i in _globalInts.Values)
            {
                _computeShader.SetInt(i.ID, i);
            }

            foreach (_MyGlobalInt3 i3 in _globalInt3s.Values)
            {
                _computeShader.SetInts(i3.ID, i3);
            }

            foreach (_MyGlobalFloat f in _globalFloats.Values)
            {
                _computeShader.SetFloat(f.ID, f);
            }


            _computeShader.Dispatch(_kernelNameID, x, y, z);
        }

        //oh god, there is no GetFloat...
        //public float GetFloat(string codeName)
        //{
        //    return _globalFloats[codeName].GetFloat();
        //}

        public float[] GetBufferData(string codeName)
        {
            return _computeBuffers[codeName].GetData();
        }

        public void ReleaseBuffers()
        {
            foreach(_MyComputeBuffer b in _computeBuffers.Values)
            {
                b.Release();
            }
        }

    }
}