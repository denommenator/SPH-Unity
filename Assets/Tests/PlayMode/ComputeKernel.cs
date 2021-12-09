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
            _Buffer = new ComputeBuffer(_Dim, sizeof(float));

            Debug.Log("Creating the buffer: " + _name);
        }

        public static implicit operator ComputeBuffer(_MyComputeBuffer b) => b._Buffer;

        ~_MyComputeBuffer()
        {
            Debug.Log( "Destroying the buffer:" + _name);
            _Buffer.Release();
        }

        public int ID => _ID;

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

        public int ID => _ID;


    }

    class ComputeKernel
    {
        private ComputeShader _computeShader;
        private string _kernelName;
        private int _kernelNameID;

        private Dictionary<string, _MyGlobalInt> _globalConstantInts;

        private Dictionary<string, _MyComputeBuffer> _computeBuffers;


        public ComputeKernel(ComputeShader computeShader, string kernelName)
        {
            _computeShader = computeShader;
            _kernelName = kernelName;
            _kernelNameID = _computeShader.FindKernel(kernelName);
            _computeBuffers = new Dictionary<string, _MyComputeBuffer>();
            _globalConstantInts = new Dictionary<string, _MyGlobalInt>();
        }



        public void AddBufferAndFill(string codeName, float[] inputArray)
        {
            _MyComputeBuffer buffer = new _MyComputeBuffer(codeName, inputArray.Length);
            buffer.SetData(inputArray);
            _computeBuffers.Add(codeName, buffer);

        }

        public void CreateBuffer(string codeName, int dim)
        {
            _MyComputeBuffer buffer = new _MyComputeBuffer(codeName, dim);
            _computeBuffers.Add(codeName, buffer);
        }

        public void FillBuffer(string codeName, float[] A)
        {
            _computeBuffers[codeName].SetData(A);
        }

        public void AddGlobalConstantInt(string codeName, int inputValue)
        {
            _MyGlobalInt MyInt = new _MyGlobalInt(codeName, inputValue);
            _globalConstantInts.Add(codeName, MyInt);
            _computeShader.SetInt(MyInt.ID, MyInt.value);
        }

        public void Dispatch(int x, int y, int z)
        {
            foreach(_MyComputeBuffer buffer in _computeBuffers.Values)
            {
                _computeShader.SetBuffer(_kernelNameID, buffer.ID, buffer);
            }

            _computeShader.Dispatch(_kernelNameID, x, y, z);
        }

        public float[] GetBufferData(string codeName)
        {
            return _computeBuffers[codeName].GetData();
        }

    }
}