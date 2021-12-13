using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyComputeKernel
{
    public class MyComputeBuffer : MonoBehaviour
    {
        private string _name;
        private int _ID;
        private int _Dim;
        private ComputeBuffer _Buffer;

        public static MyComputeBuffer NewMyComputeBuffer(string name, int dim)
        {
            MyComputeBuffer cb = new MyComputeBuffer();

            cb._name = name;
            cb._ID = Shader.PropertyToID(name);
            cb._Dim = dim;
            cb.InitializeBuffer(dim);
            return cb;

        }

        public static MyComputeBuffer NewMyComputeBuffer(string name)
        {
            MyComputeBuffer cb = new MyComputeBuffer();
            cb._name = name;
            cb._ID = Shader.PropertyToID(name);
            cb._Dim = 0;
            return cb;
        }


        public static implicit operator ComputeBuffer(MyComputeBuffer b) => b._Buffer;

        public void OnDestroy()
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

    public class ComputeKernel : MonoBehaviour
    {
        private ComputeShader _computeShader;
        private string _kernelName;
        private int _kernelNameID;

        public int _groupDim_x = 1, _groupDim_y = 1, _groupDim_z = 1;

        

        private Dictionary<string, _MyGlobalInt> _globalConstantInts;
        private Dictionary<string, _MyGlobalInt> _globalInts;
        private Dictionary<string, _MyGlobalInt3> _globalInt3s;
        private Dictionary<string, _MyGlobalFloat> _globalFloats;

        public Dictionary<string, MyComputeBuffer> computeBuffers;

        public void Start()
        {
            
        }

        public static ComputeKernel NewComputeKernel(ComputeShader computeShader, string kernelName)
        {
            ComputeKernel ck = new ComputeKernel();

            ck._computeShader = computeShader;
            ck._kernelName = kernelName;
            ck._kernelNameID = computeShader.FindKernel(kernelName);
            ck.computeBuffers = new Dictionary<string, MyComputeBuffer>();
            ck._globalConstantInts = new Dictionary<string, _MyGlobalInt>();
            ck._globalInts = new Dictionary<string, _MyGlobalInt>();
            ck._globalInt3s = new Dictionary<string, _MyGlobalInt3>();
            ck._globalFloats = new Dictionary<string, _MyGlobalFloat>();
            
            ck._globalInt3s.Add("grid_dim", new _MyGlobalInt3("grid_dim", new Vector3Int(1,1,1)));
            ck._globalInt3s.Add("block_dim", new _MyGlobalInt3("block_dim", new Vector3Int(1, 1, 1)));


            return ck;
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

        public void Dispatch()
        {
            int x = _groupDim_x, y = _groupDim_y, z = _groupDim_z;
            _MyGlobalInt3 grid_dim = new _MyGlobalInt3("grid_dim", new Vector3Int(x, y, z));
            _globalInt3s["grid_dim"] = grid_dim; //accessors return a copy of the struct, so can't modify in place

            uint group_dim_x, group_dim_y, group_dim_z;
            _computeShader.GetKernelThreadGroupSizes(_kernelNameID, out group_dim_x, out group_dim_y, out group_dim_z);
            _MyGlobalInt3 block_dim = new _MyGlobalInt3("group_dim", new Vector3Int((int)group_dim_x, (int)group_dim_y, (int)group_dim_z));
            _globalInt3s["group_dim"] = block_dim; //accessors return a copy of the struct, so can't modify in place



            foreach (MyComputeBuffer buffer in computeBuffers.Values)
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
            return computeBuffers[codeName].GetData();
        }


    }
}