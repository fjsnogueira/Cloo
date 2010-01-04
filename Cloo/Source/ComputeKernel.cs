﻿/*

Copyright (c) 2009 Fatjon Sakiqi

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

*/

using System;
using System.Runtime.InteropServices;
using OpenTK.Cloo.CL10;

namespace Cloo
{
    public class ComputeKernel: ComputeResource
    {
        #region Fields

        private readonly ComputeContext context;
        private readonly string functionName;
        private readonly ComputeProgram program;

        #endregion

        #region Properties

        /// <summary>
        /// The ComputeContext associated with the kernel.
        /// </summary>
        public ComputeContext Context
        {
            get
            {
                return context;
            }
        }

        /// <summary>
        /// The kernel function name.
        /// </summary>
        public string FunctionName
        {
            get
            {
                return functionName;
            }
        }

        /// <summary>
        /// The ComputeProgram associated with kernel.
        /// </summary>
        public ComputeProgram Program
        {
            get
            {
                return program;
            }
        }

        #endregion

        #region Constructors

        internal ComputeKernel( IntPtr handle, ComputeProgram program )
        {
            this.Handle = handle;

            context = program.Context;
            functionName = GetStringInfo<KernelInfo>( KernelInfo.KernelFunctionName, CL.GetKernelInfo );
            this.program = program;
        }

        internal ComputeKernel( string functionName, ComputeProgram program )
        {
            ErrorCode error = ErrorCode.Success;
            Handle = CL.CreateKernel( program.Handle, functionName, out error );
            ComputeException.ThrowIfError( error );

            context = program.Context;
            this.functionName = functionName;
            this.program = program;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the amount of local memory in bytes used by the kernel.
        /// </summary>
        public long GetLocalMemorySize( ComputeDevice device )
        {
            return ( long )GetInfo<KernelWorkGroupInfo, ulong>(
                device, KernelWorkGroupInfo.KernelLocalMemSize, CL.GetKernelWorkGroupInfo );
        }

        /// <summary>
        /// The compile work-group size specified by the __attribute__((reqd_work_group_size(X, Y, Z))) qualifier. If the above qualifier is not specified (0, 0, 0) is returned.
        /// </summary>
        public long[] GetCompileWorkGroupSize( ComputeDevice device )
        {
            return ComputeTools.ConvertArray( 
                GetArrayInfo<KernelWorkGroupInfo, IntPtr>(
                    device, KernelWorkGroupInfo.KernelCompileWorkGroupSize, CL.GetKernelWorkGroupInfo ) );
        }

        /// <summary>
        /// The maximum work-group size that can be used to execute the kernel on the specified device.
        /// </summary>
        public long GetWorkGroupSize( ComputeDevice device )
        {
            return ( long )GetInfo<KernelWorkGroupInfo, IntPtr>(
                    device, KernelWorkGroupInfo.KernelWorkGroupSize, CL.GetKernelWorkGroupInfo );
        }

        /// <summary>
        /// Sets the value of a specific argument of the kernel.
        /// </summary>
        /// <param name="index">The index of the argument to set.</param>
        /// <param name="dataSize">The size in bytes of the data mapped to the argument.</param>
        /// <param name="dataAddr">The address of the data mapped to the argument.</param>
        public void SetArg( int index, IntPtr dataSize, IntPtr dataAddr )
        {
            int error = CL.SetKernelArg(
                Handle,
                index,
                dataSize,
                dataAddr );
            ComputeException.ThrowIfError( error );
        }

        /// <summary>
        /// Sets the specified kernel argument.
        /// </summary>
        public void SetMemoryArg( int index, ComputeMemory memObj )
        {
            SetValueArg<IntPtr>( index, memObj.Handle );
        }

        /// <summary>
        /// Sets the specified kernel argument.
        /// </summary>
        public void SetSamplerArg( int index, ComputeSampler sampler )
        {
            SetValueArg<IntPtr>( index, sampler.Handle );
        }        

        /// <summary>
        /// Sets the specified kernel argument.
        /// </summary>
        public void SetValueArg<T>( int index, T data ) where T : struct
        {
            GCHandle gcHandle = GCHandle.Alloc( data, GCHandleType.Pinned );            
            try
            {
                SetArg( 
                    index,
                    new IntPtr( Marshal.SizeOf( typeof( T ) ) ),
                    gcHandle.AddrOfPinnedObject() );
            }
            finally
            {
                gcHandle.Free();
            }            
        }

        /// <summary>
        /// Gets a string representation of this kernel.
        /// </summary>
        public override string ToString()
        {
            return "ComputeKernel" + base.ToString();
        }

        #endregion

        #region Protected methods

        protected override void Dispose( bool manual )
        {
            if( Handle != IntPtr.Zero )
            {
                CL.ReleaseKernel( Handle );
                Handle = IntPtr.Zero;
            }
        }

        #endregion
    }
}