using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MyComputeKernel1
{
    public class ComputeBufferWrapper
    {
        static private List<ComputeBuffer> _BufferPool;

        private int _bufferPoolIndex;
        private int _BufferDim;
        private int _BufferStride;

        public int dim => _BufferDim;
        static ComputeBufferWrapper()
        {
            _BufferPool = new List<ComputeBuffer>();
        }

        public static void ReleaseBuffers()
        {
            foreach (ComputeBuffer b in _BufferPool)
            {
                b.Release();
            }
        }

        public ComputeBufferWrapper(float[] data)
        {
            

            _BufferDim = data.Length;
            _BufferStride = sizeof(float);
            _bufferPoolIndex = ComputeBufferWrapper._BufferPool.Count;

            ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
            buffer.SetData(data);
            ComputeBufferWrapper._BufferPool.Add(buffer);

        }

        public ComputeBufferWrapper(int dim)
        {

            float[] zero_initialized_data = new float[dim];

            _BufferDim = zero_initialized_data.Length;
            _BufferStride = sizeof(float);
            _bufferPoolIndex = ComputeBufferWrapper._BufferPool.Count;

            ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
            buffer.SetData(zero_initialized_data);
            ComputeBufferWrapper._BufferPool.Add(buffer);
        }

        public static implicit operator ComputeBuffer(ComputeBufferWrapper b) => ComputeBufferWrapper._BufferPool[b._bufferPoolIndex];

        

    }

    public interface IKernelField
    {
        public void PreDispatch(ComputeShader shader, int kernelNameID);
    }

    public class KernelBufferField : IKernelField
    {
        private string _codeName;
        private int _nameID;
        private ComputeBufferWrapper _attachedBuffer;
        private int _dim;

        public KernelBufferField(ComputeKernel ck, string codeName, int dim)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = dim;
            _attachedBuffer = new ComputeBufferWrapper(dim);

            ck.AddField(this);
        }

        public KernelBufferField(ComputeKernel ck, string codeName, ComputeBufferWrapper preexistingBuffer)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = preexistingBuffer.dim;
            _attachedBuffer = preexistingBuffer;

            ck.AddField(this);
        }

        public void BindBuffer(ComputeBufferWrapper buffer)
        {
            _dim = buffer.dim;
            _attachedBuffer = buffer;

        }

        public static implicit operator ComputeBufferWrapper(KernelBufferField bf) => bf._attachedBuffer;
        
        public static implicit operator float[](KernelBufferField bf)
        {
            float[] data = new float[bf._dim];
            //use a non-blocking call for this if doing anything other than testing
            ((ComputeBuffer)bf._attachedBuffer).GetData(data);
            return data;
        }

        public void SetData(float[] data)
        {
            ((ComputeBuffer)_attachedBuffer).SetData(data);
        }

        public void PreDispatch(ComputeShader shader, int kernelNameID)
        {
            shader.SetBuffer(kernelNameID, _nameID, _attachedBuffer);
        }


    }


    public struct _MyGlobalInt : IKernelField
    {
        private string _name;
        private int _nameID;
        private int _value;

        public _MyGlobalInt(ComputeKernel ck, string name, int value = 0)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = value;

            ck.AddField(this);
        }


        public static implicit operator int(_MyGlobalInt i) => i._value;

        public void PreDispatch(ComputeShader shader, int kernelID)
        {
            shader.SetInt(_nameID, _value);
        }
    }

    public struct _MyGlobalFloat : IKernelField
    {
        private string _name;
        private int _nameID;
        private float _value;

        public _MyGlobalFloat(ComputeKernel ck, string name, float value = 0.0f)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = value;

            ck.AddField(this);
        }


        public static implicit operator float(_MyGlobalFloat f) => f._value;

        public void PreDispatch(ComputeShader shader, int kernelID)
        {
            shader.SetFloat(_nameID, _value);
        }

    }

    public struct _MyGlobalInt3 : IKernelField
    {
        private string _name;
        private int _nameID;
        private Vector3Int _value;

        public _MyGlobalInt3(ComputeKernel ck, string name)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = Vector3Int.zero;

            ck.AddField(this);
        }

        public _MyGlobalInt3(ComputeKernel ck, string name, Vector3Int value)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = value;

            ck.AddField(this);
        }

        public static implicit operator Vector3Int(_MyGlobalInt3 i) => i._value;
        public static implicit operator int[](_MyGlobalInt3 i) => new int[] { i._value.x, i._value.y, i._value.z };

        public void PreDispatch(ComputeShader shader, int kernelNameID)
        {
            shader.SetInts(_nameID, this);
        }


    }

    public struct GridDimensionField
    {
        private string _name;
        private int _nameID;

        public GridDimensionField(ComputeKernel ck, string name)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);

            ck.AddGridDimensionField(this);
        }

        public int nameID => _nameID;

    }

    public struct GroupDimensionField
    {
        private string _name;
        private int _nameID;

        public GroupDimensionField(ComputeKernel ck, string name)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);

            ck.AddGroupDimensionField(this);
        }

        public int nameID => _nameID;

    }




    public class ComputeKernel
    {
        private string _kernelName;
        private int _kernelNameID;
        private ComputeShader _computeShader;
        private List<IKernelField> _kernelFields;
        public List<GridDimensionField> _gridDimensionField;
        public List<GroupDimensionField> _groupDimensionField;



        public  ComputeKernel(ComputeShader computeShader, string kernelName)
        {

            _computeShader = computeShader;
            
            _kernelName = kernelName;
            _kernelNameID = computeShader.FindKernel(kernelName);

            _kernelFields = new List<IKernelField>();
            _groupDimensionField = new List<GroupDimensionField>();
            _gridDimensionField = new List<GridDimensionField>();

        }

        public void AddField(IKernelField field)
        {
            _kernelFields.Add(field);
        }

        public void AddGroupDimensionField(GroupDimensionField field)
        {
            _groupDimensionField.Add(field);
        }

        public void AddGridDimensionField(GridDimensionField field)
        {
            _gridDimensionField.Add(field);
        }


        public void Dispatch(int x, int y, int z)
        {

            

            if (_gridDimensionField.Count != 0)
            {
                _computeShader.SetInts(_gridDimensionField[0].nameID,  x, y, z);
            }

            if (_groupDimensionField.Count != 0)
            {
                uint X, Y, Z;
                _computeShader.GetKernelThreadGroupSizes(_kernelNameID, out X, out Y, out Z);
                _computeShader.SetInts(_groupDimensionField[0].nameID, (int)X, (int)Y, (int)Z);
            }



            foreach (IKernelField field in _kernelFields)
            {
                field.PreDispatch(_computeShader, _kernelNameID);
            }

            _computeShader.Dispatch(_kernelNameID, x, y, z);
        }






    }
}