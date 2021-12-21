using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SPH
{
    public class BufferReleaser : MonoBehaviour
    {
        public static void ReleaseAllBuffers()
        {
            ComputeBufferWrapperFloat.ReleaseBuffers();
            ComputeBufferWrapperFloat3.ReleaseBuffers();
            ComputeBufferWrapperContainerWall.ReleaseBuffers();
        }

        public void OnDestroy()
        {
            ReleaseAllBuffers();
        }
    }

    public class ComputeBufferWrapperFloat
    {
        static private List<ComputeBuffer> _BufferPool;

        private int _bufferPoolIndex;
        private int _BufferDim;
        private int _BufferStride;

        public int dim => _BufferDim;
        static ComputeBufferWrapperFloat()
        {
            _BufferPool = new List<ComputeBuffer>();
        }

        public static void ReleaseBuffers()
        {
            foreach (ComputeBuffer b in ComputeBufferWrapperFloat._BufferPool)
            {
                b.Release();
            }
        }

        public ComputeBufferWrapperFloat(int dim)
        {

            float[] zero_initialized_data = new float[dim];

            _BufferDim = zero_initialized_data.Length;
            _BufferStride = sizeof(float);
            _bufferPoolIndex = ComputeBufferWrapperFloat._BufferPool.Count;

            ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
            buffer.SetData(zero_initialized_data);
            ComputeBufferWrapperFloat._BufferPool.Add(buffer);
        }

        public ComputeBufferWrapperFloat(float[] data)
        {


            _BufferDim = data.Length;
            _BufferStride = sizeof(float);
            _bufferPoolIndex = ComputeBufferWrapperFloat._BufferPool.Count;

            ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
            buffer.SetData(data);
            ComputeBufferWrapperFloat._BufferPool.Add(buffer);

        }



        public static implicit operator ComputeBuffer(ComputeBufferWrapperFloat b) => ComputeBufferWrapperFloat._BufferPool[b._bufferPoolIndex];
        public static implicit operator ComputeBufferWrapperFloat(float[] A) => new ComputeBufferWrapperFloat(A);
        public static implicit operator float[](ComputeBufferWrapperFloat ABuffer)
        {
            float[] A = new float[ABuffer.dim];
            ((ComputeBuffer)ABuffer).GetData(A);
            return A;
        }


    }

    public class ComputeBufferWrapperFloat3
    {
        static private List<ComputeBuffer> _BufferPool;

        private int _bufferPoolIndex;
        private int _BufferDim;
        private int _BufferStride;

        public int dim => _BufferDim;
        static ComputeBufferWrapperFloat3()
        {
            _BufferPool = new List<ComputeBuffer>();
        }

        public static void ReleaseBuffers()
        {
            foreach (ComputeBuffer b in ComputeBufferWrapperFloat3._BufferPool)
            {
                b.Release();
            }
        }

        public ComputeBufferWrapperFloat3(int dim)
        {

            Vector3[] zero_initialized_data = new Vector3[dim];
            for (int i = 0; i < dim; i++)
            {
                zero_initialized_data[i] = Vector3.zero;
            }

            _BufferDim = zero_initialized_data.Length;
            _BufferStride = sizeof(float) * 3;
            _bufferPoolIndex = ComputeBufferWrapperFloat3._BufferPool.Count;

            ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
            buffer.SetData(zero_initialized_data);
            ComputeBufferWrapperFloat3._BufferPool.Add(buffer);
        }

        public ComputeBufferWrapperFloat3(Vector3[] data)
        {


            _BufferDim = data.Length;
            _BufferStride = sizeof(float) * 3;
            _bufferPoolIndex = ComputeBufferWrapperFloat3._BufferPool.Count;

            ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
            buffer.SetData(data);
            ComputeBufferWrapperFloat3._BufferPool.Add(buffer);

        }



        public static implicit operator ComputeBuffer(ComputeBufferWrapperFloat3 b) => ComputeBufferWrapperFloat3._BufferPool[b._bufferPoolIndex];
        public static implicit operator ComputeBufferWrapperFloat3(Vector3[] A) => new ComputeBufferWrapperFloat3(A);
        public static implicit operator Vector3[](ComputeBufferWrapperFloat3 ABuffer)
        {
            Vector3[] A = new Vector3[ABuffer.dim];
            ((ComputeBuffer)ABuffer).GetData(A);
            return A;
        }


    }

    public class ComputeBufferWrapperContainerWall
    {
        static private List<ComputeBuffer> _BufferPool;

        private int _bufferPoolIndex;
        private int _BufferDim;
        private int _BufferStride;

        public int dim => _BufferDim;
        static ComputeBufferWrapperContainerWall()
        {
            _BufferPool = new List<ComputeBuffer>();
        }

        public static void ReleaseBuffers()
        {
            foreach (ComputeBuffer b in ComputeBufferWrapperContainerWall._BufferPool)
            {
                b.Release();
            }
        }

        public ComputeBufferWrapperContainerWall(CollisionContainerWall[] data)
        {
            _BufferDim = data.Length;
            _BufferStride = sizeof(float) * (3 * 2 + 1);
            _bufferPoolIndex = ComputeBufferWrapperContainerWall._BufferPool.Count;

            ComputeBuffer buffer = new ComputeBuffer(_BufferDim, _BufferStride);
            buffer.SetData(data);
            ComputeBufferWrapperContainerWall._BufferPool.Add(buffer);
        }



        public static implicit operator ComputeBuffer(ComputeBufferWrapperContainerWall b) => ComputeBufferWrapperContainerWall._BufferPool[b._bufferPoolIndex];
        public static implicit operator ComputeBufferWrapperContainerWall(CollisionContainerWall[] A) => new ComputeBufferWrapperContainerWall(A);
        public static implicit operator CollisionContainerWall[](ComputeBufferWrapperContainerWall ABuffer)
        {
            CollisionContainerWall[] A = new CollisionContainerWall[ABuffer.dim];
            ((ComputeBuffer)ABuffer).GetData(A);
            return A;
        }


    }

    public interface IKernelField
    {
        public void PreDispatch(ComputeShader shader, int kernelNameID);
    }

    public class KernelBufferFieldFloat : IKernelField
    {
        private string _codeName;
        private int _nameID;
        private ComputeBufferWrapperFloat? _attachedBuffer;
        private int _dim;

        public KernelBufferFieldFloat(ComputeKernel ck, string codeName)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = 0;

            ck.AddField(this);
        }

        public KernelBufferFieldFloat(ComputeKernel ck, string codeName, ComputeBufferWrapperFloat buffer)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = buffer.dim;
            _attachedBuffer = buffer;

            ck.AddField(this);
        }

        public void BindBuffer(ComputeBufferWrapperFloat buffer)
        {
            _dim = buffer.dim;
            _attachedBuffer = buffer;

        }

        public int dim => _dim;

        public static implicit operator ComputeBufferWrapperFloat(KernelBufferFieldFloat bf) => bf._attachedBuffer;



        public static implicit operator float[](KernelBufferFieldFloat bf)
        {
            if (bf._attachedBuffer is ComputeBufferWrapperFloat attachedBuffer)
            {
                float[] data = new float[bf._dim];
                //use a non-blocking call for this if doing anything other than testing
                ((ComputeBuffer)bf._attachedBuffer).GetData(data);
                return data;
            }
            else
                throw new System.Exception("Trying to read an unbound buffer from KernelBufferFieldFloat " + bf._codeName);
        }

        public void SetData(float[] data)
        {
            if (_attachedBuffer is ComputeBufferWrapperFloat attachedBuffer)
            {
                ((ComputeBuffer)attachedBuffer).SetData(data);
            }
            else
                throw new System.Exception("Trying to send data to an unbound buffer from KernelBufferFieldFloat" + _codeName);


        }

        public void PreDispatch(ComputeShader shader, int kernelNameID)
        {
            if (_attachedBuffer is null)
            {
                throw new System.Exception("You tried to launch a kernel with an unbound KernelBufferFieldFloat " + _codeName + ". Remember to Bind a buffer to this field before launching any dispatches.");
            }
            shader.SetBuffer(kernelNameID, _nameID, _attachedBuffer);
        }


    }

    public class KernelBufferFieldFloat3 : IKernelField
    {
        private string _codeName;
        private int _nameID;
        private ComputeBufferWrapperFloat3? _attachedBuffer;
        private int _dim;

        public KernelBufferFieldFloat3(ComputeKernel ck, string codeName)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = 0;

            ck.AddField(this);
        }

        public KernelBufferFieldFloat3(ComputeKernel ck, string codeName, ComputeBufferWrapperFloat3 buffer)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = buffer.dim;
            _attachedBuffer = buffer;

            ck.AddField(this);
        }

        public void BindBuffer(ComputeBufferWrapperFloat3 buffer)
        {
            _dim = buffer.dim;
            _attachedBuffer = buffer;

        }

        public int dim => _dim;

        public static implicit operator ComputeBufferWrapperFloat3(KernelBufferFieldFloat3 bf) => bf._attachedBuffer;



        public static implicit operator Vector3[](KernelBufferFieldFloat3 bf)
        {
            if (bf._attachedBuffer is ComputeBufferWrapperFloat3 attachedBuffer)
            {
                Vector3[] data = new Vector3[bf._dim];
                //use a non-blocking call for this if doing anything other than testing
                ((ComputeBuffer)bf._attachedBuffer).GetData(data);
                return data;
            }
            else
                throw new System.Exception("Trying to read an unbound buffer from KernelBufferFieldFloat3 " + bf._codeName);
        }

        public void SetData(Vector3[] data)
        {
            if (_attachedBuffer is ComputeBufferWrapperFloat3 attachedBuffer)
            {
                ((ComputeBuffer)attachedBuffer).SetData(data);
            }
            else
                throw new System.Exception("Trying to send data to an unbound buffer from KernelBufferFieldFloat" + _codeName);


        }

        public void PreDispatch(ComputeShader shader, int kernelNameID)
        {
            if (_attachedBuffer is null)
            {
                throw new System.Exception("You tried to launch a kernel with an unbound KernelBufferFieldFloat " + _codeName + ". Remember to Bind a buffer to this field before launching any dispatches.");
            }
            shader.SetBuffer(kernelNameID, _nameID, _attachedBuffer);
        }


    }

    public class KernelBufferFieldContainerWall : IKernelField
    {
        private string _codeName;
        private int _nameID;
        private ComputeBufferWrapperContainerWall? _attachedBuffer;
        private int _dim;

        public KernelBufferFieldContainerWall(ComputeKernel ck, string codeName)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = 0;

            ck.AddField(this);
        }

        public KernelBufferFieldContainerWall(ComputeKernel ck, string codeName, ComputeBufferWrapperContainerWall buffer)
        {
            _codeName = codeName;
            _nameID = Shader.PropertyToID(_codeName);
            _dim = buffer.dim;
            _attachedBuffer = buffer;

            ck.AddField(this);
        }

        public void BindBuffer(ComputeBufferWrapperContainerWall buffer)
        {
            _dim = buffer.dim;
            _attachedBuffer = buffer;

        }

        public int dim => _dim;

        public static implicit operator ComputeBufferWrapperContainerWall(KernelBufferFieldContainerWall bf) => bf._attachedBuffer;



        public static implicit operator CollisionContainerWall[](KernelBufferFieldContainerWall bf)
        {
            if (bf._attachedBuffer is ComputeBufferWrapperContainerWall attachedBuffer)
            {
                CollisionContainerWall[] data = new CollisionContainerWall[bf._dim];
                //use a non-blocking call for this if doing anything other than testing
                ((ComputeBuffer)bf._attachedBuffer).GetData(data);
                return data;
            }
            else
                throw new System.Exception("Trying to read an unbound buffer from KernelBufferFieldContainerWall " + bf._codeName);
        }

        public void SetData(CollisionContainerWall[] data)
        {
            if (_attachedBuffer is ComputeBufferWrapperContainerWall attachedBuffer)
            {
                ((ComputeBuffer)attachedBuffer).SetData(data);
            }
            else
                throw new System.Exception("Trying to send data to an unbound buffer from KernelBufferFieldContainerWall" + _codeName);


        }

        public void PreDispatch(ComputeShader shader, int kernelNameID)
        {
            if (_attachedBuffer is null)
            {
                throw new System.Exception("You tried to launch a kernel with an unbound KernelBufferContainerWall " + _codeName + ". Remember to Bind a buffer to this field before launching any dispatches.");
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

        public void SetFloat(float value)
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



        public ComputeKernel(ComputeShader computeShader, string kernelName)
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

        public int[] BlockDim()
        {
            uint X, Y, Z;
            _computeShader.GetKernelThreadGroupSizes(_kernelNameID, out X, out Y, out Z);
            int[] result = { (int)X, (int)Y, (int)Z };
            return result;
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
                _computeShader.SetInts(gridDimensionField.nameID, x, y, z);
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