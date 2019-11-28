using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.OpenGL;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SkiaSharp;
using AlphaMode = SharpDX.DXGI.AlphaMode;
using Device = SharpDX.Direct3D11.Device;

namespace AvaloniaApplication2
{
    public class EGLInterop
    {
        private Texture2D texture;
        private RenderTargetView _renderTargetView;

        private delegate IntPtr CreateEGLSurfaceDelegate(IntPtr display, int bufferType, IntPtr bufferHandle, IntPtr config, int[] attr); 

        private delegate bool GetConfigDelegate(IntPtr display, IntPtr context, int attribute, int[] values);

        private delegate bool GetDeviceDelegate(IntPtr displayHandle, int type, out IntPtr ptr);

        private Device _device;
        
        public EGLInterop()
        {
            var glFeature = AvaloniaLocator.Current.GetService<IWindowingPlatformGlFeature>();
            var eglContext = glFeature.ImmediateContext as EglContext;

            var pointer = GetEglD3Device(eglContext);
            
            _device = new Device(pointer);
        }
        
        public IntPtr CreateEglSurface()
        {
            var glFeature = AvaloniaLocator.Current.GetService<IWindowingPlatformGlFeature>();
            var eglContext = glFeature.ImmediateContext as EglContext;

            var textureSharedHandle = InitTexture(_device);

            // getting display handle 
            var displayHandle = (eglContext.Display as EglDisplay).Handle;
            
            // getting config handle
            var configHandle = GetConfigHandle(eglContext, displayHandle);
            
            // create egl surface handle
            var eglSurface = CreateEglSurface(eglContext, displayHandle, textureSharedHandle, configHandle);


            return eglSurface;
        }
        
        private static IntPtr CreateEglSurface(IGlContext eglContext, IntPtr displayHandle, IntPtr sharedHandle, IntPtr configHandle)
        {
            // getting handle to eglCreatePbufferFromClientBuffer function
            var pBufferFunctionHandle =
                eglContext.Display.GlInterface.GetProcAddress("eglCreatePbufferFromClientBuffer");

            
            // create attributes
            var pBufferAttributes = new[]
            {
                EglConsts.EGL_TEXTURE_FORMAT, EglConsts.EGL_NO_TEXTURE,
                EglConsts.EGL_TEXTURE_TARGET, EglConsts.EGL_NONE
            };


            // create function by handle
            var createEGLSurface =
                (CreateEGLSurfaceDelegate) Marshal.GetDelegateForFunctionPointer(pBufferFunctionHandle,
                    typeof(CreateEGLSurfaceDelegate));

            // create and return egl surface            
            var eglSurfaceHandle =
                createEGLSurface(displayHandle, 0x33A3, sharedHandle, configHandle, null);

            return eglSurfaceHandle;
        }

        private IntPtr GetConfigHandle(EglContext eglContext, IntPtr displayHandle)
        {
            var eglQueryContextHandle = eglContext.Display.GlInterface.GetProcAddress("eglQueryContext");

            // create function by pointer
            var getConfig =
                (GetConfigDelegate) Marshal.GetDelegateForFunctionPointer(eglQueryContextHandle,
                    typeof(GetConfigDelegate));

            // store config ID
            var values = new int[1];

            // getting config ID
            getConfig(displayHandle, eglContext.Context, EglConsts.EGL_CONFIG_ID, values);


            var config = IntPtr.Zero;
            var attribs = new[]
            {
                EglConsts.EGL_CONFIG_ID, values[0],
                EglConsts.EGL_NONE
            };

            // getting config by ID
            ((EglDisplay) eglContext.Display).EglInterface.ChooseConfig(displayHandle, attribs, out config, 1,
                out var numConfigs);

            return config;
        }

        private IntPtr InitTexture(Device sharpDxD3dDevice)
        {
            texture = new Texture2D(sharpDxD3dDevice, new Texture2DDescription
            {
                Width = 600,
                Height = 800,
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.Shared,
                SampleDescription = new SampleDescription(1, 0)
            });
            
            return texture.NativePointer;
        }

        private IntPtr GetEglD3Device(IGlContext eglContext)
        {
            var eglQueryDeviceAttribEXTPointer = eglContext.Display.GlInterface.GetProcAddress("eglQueryDisplayAttribEXT");
            var getDevice =
                (GetDeviceDelegate) Marshal.GetDelegateForFunctionPointer(eglQueryDeviceAttribEXTPointer,
                    typeof(GetDeviceDelegate));

            var eglDeviceHandle = IntPtr.Zero; 
            var eglDevice = getDevice((eglContext.Display as EglDisplay).Handle, 0x322C,  out eglDeviceHandle);

            
            var d3dQueryDeviceAttribEXTPointer = eglContext.Display.GlInterface.GetProcAddress("eglQueryDeviceAttribEXT");
            var getD3dDevice =
                (GetDeviceDelegate) Marshal.GetDelegateForFunctionPointer(d3dQueryDeviceAttribEXTPointer,
                    typeof(GetDeviceDelegate));

            var d3dDeviceHandle = IntPtr.Zero;
            var d3dDevice = getD3dDevice(eglDeviceHandle, 0x33A1, out d3dDeviceHandle);

            return d3dDeviceHandle;
        }
    }
}