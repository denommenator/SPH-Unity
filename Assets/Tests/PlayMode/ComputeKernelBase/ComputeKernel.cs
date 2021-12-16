using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SPH
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

        public ComputeBufferWrapper(float[] data)
        {
            

            _BufferDim = data.Length;
            _BufferStride = sizeof(float);
            _bufferPoolIndex = ComputeBufferWrapper._BufferPool.Count;

            ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
            buffer.SetData(data);
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
        private ComputeBufferWrapper? _attachedBuffer;
        private int _dim;

        public KernelBufferField(ComputeKernel ck, string codeName)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = 0;

            ck.AddField(this);
        }

        public KernelBufferField(ComputeKernel ck, string codeName, ComputeBufferWrapper buffer)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = buffer.dim;
            _attachedBuffer = buffer;

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
            if (bf._attachedBuffer is ComputeBufferWrapper attachedBuffer)
            {
                float[] data = new float[bf._dim];
                //use a non-blocking call for this if doing anything other than testing
                ((ComputeBuffer)bf._attachedBuffer).GetData(data);
                return data;
            }
            else
                throw new System.Exception("Trying to read an unbound buffer from a KernelBufferField");
        }

        public void SetData(float[] data)
        {
            if (_attachedBuffer is ComputeBufferWrapper attachedBuffer)
            {
                ((ComputeBuffer)attachedBuffer).SetData(data);
            }
            else
                throw new System.Exception("Trying to send data to an unbound buffer from a KernelBufferField");

            
        }

        public void PreDispatch(ComputeShader shader, int kernelNameID)
        {
            if(_attachedBuffer is null)
            {
                throw new System.Exception("You tried to launch a kernel with an unbound KernelBufferField. Remember to Bind a buffer to this field before launching any dispatches.");
            }
            shader.SetBuffer(kernelNameID, _nameID, _attachedBuffer);
        }


    }


    public class GlobalInt : IKernelField
    {
        private string _name;
        private int _nameID;
        private int? _value;

        public GlobalInt(ComputeKernel ck, string name, int value)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = value;

            ck.AddField(this);
        }

        public GlobalInt(ComputeKernel ck, string name)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = null;

            ck.AddField(this);
        }

        public void SetInt(int value)
        {
            _value = value;
        }

        //public static implicit operator int(MyGlobalInt i)
        //{
        //    i._value;
        //}
        //there is no read-int from the GPU

        public void PreDispatch(ComputeShader shader, int kernelID)
        {
            if (_value is int value)
            {
                shader.SetInt(_nameID, value);
            }
            else
                throw new System.Exception("Trying to launch a Kernel with integer " + _name + " unbound");
            
        }
    }

    public class GlobalFloat : IKernelField
    {
        private string _name;
        private int _nameID;
        private float? _value;

        public GlobalFloat(ComputeKernel ck, string name, float value)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = value;

            ck.AddField(this);
        }

        public GlobalFloat(ComputeKernel ck, string name)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = null;

            ck.AddField(this);
        }

        public void SetFloat(int value)
        {
            _value = value;
        }

        //public static implicit operator int(MyGlobalInt i)
        //{
        //    i._value;
        //}
        //there is no read-int from the GPU

        public void PreDispatch(ComputeShader shader, int kernelID)
        {
            if (_value is float value)
            {
                shader.SetFloat(_nameID, value);
            }
            else
                throw new System.Exception("Trying to launch a Kernel with float " + _name + " unbound");

        }
    }

    public class GlobalInt3 : IKernelField
    {
        private string _name;
        private int _nameID;
        private Vector3Int? _value;

        public GlobalInt3(ComputeKernel ck, string name, Vector3Int value)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = value;

            ck.AddField(this);
        }

        public GlobalInt3(ComputeKernel ck, string name)
        {
            _name = name;
            _nameID = Shader.PropertyToID(name);
            _value = null;

            ck.AddField(this);
        }

        public void SetInt3(Vector3Int value)
        {
            _value = value;
        }

        //public static implicit operator int(MyGlobalInt i)
        //{
        //    i._value;
        //}
        //there is no read-int from the GPU

        public void PreDispatch(ComputeShader shader, int kernelID)
        {
            if (_value is Vector3Int value)
            {
                shader.SetInts(_nameID, value.x, value.y, value.z);
            }
            else
                throw new System.Exception("Trying to launch a Kernel with int3 " + _name + " unbound");

        }
    }

    public class GridDimensionField
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

    public class GroupDimensionField
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
        public GridDimensionField? _gridDimensionField;
        public GroupDimensionField? _groupDimensionField;



        public  ComputeKernel(ComputeShader computeShader, string kernelName)
        {

            _computeShader = computeShader;
            
            _kernelName = kernelName;
            _kernelNameID = computeShader.FindKernel(kernelName);

            _kernelFields = new List<IKernelField>();

        }

        public void AddField(IKernelField field)
        {
            _kernelFields.Add(field);
        }

        public void AddGroupDimensionField(GroupDimensionField field)
        {
            _groupDimensionField = field;
        }

        public void AddGridDimensionField(GridDimensionField field)
        {
            _gridDimensionField = field;
        }


        public void Dispatch(int x, int y, int z)
        {

            

            if (_gridDimensionField is GridDimensionField gridDimensionField)
            {
                _computeShader.SetInts(gridDimensionField.nameID,  x, y, z);
            }

            if (_groupDimensionField is GroupDimensionField groupDimensionField)
            {
                uint X, Y, Z;
                _computeShader.GetKernelThreadGroupSizes(_kernelNameID, out X, out Y, out Z);
                _computeShader.SetInts(groupDimensionField.nameID, (int)X, (int)Y, (int)Z);
            }



            foreach (IKernelField field in _kernelFields)
            {
                field.PreDispatch(_computeShader, _kernelNameID);
            }

            _computeShader.Dispatch(_kernelNameID, x, y, z);
        }






    }
}