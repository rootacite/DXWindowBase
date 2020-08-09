using System;


using Device = SharpDX.Direct3D11.Device1;

using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using DeviceContext = SharpDX.Direct2D1.DeviceContext;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using D2DFactory = SharpDX.Direct2D1.Factory1;
using SharpDX.DXGI;

using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

using D2DBitmap = SharpDX.Direct2D1.Bitmap1;
using Size = System.Drawing.Size;

namespace D2D
{
    public struct Direct2DInformation
    {
        public ImagingFactory ImagingFacy;
        public D2DFactory D2DFacy;
        public D2DBitmap _bufferBack;//用于D2D绘图的WIC图片
        public SharpDX.Direct3D11.Device d3DDevice;// = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
        public SharpDX.DXGI.Device dxgiDevice;// = d3DDevice.QueryInterface<Device>().QueryInterface<SharpDX.DXGI.Device>();

        public SharpDX.Direct2D1.Device d2DDevice;
        public SwapChainDescription1 swapChainDesc;
        public Surface backbuffer;
        public SwapChain1 swapChain;
        public DeviceContext View { get; set; }//绘图目标

    }
    enum DrawResult
    {
        Commit,
        Death,
        Ignore
    }
    class Direct2DWindow : IDisposable
    {
        [DllImport("winmm")]
        static extern void timeBeginPeriod(int t);
        [DllImport("winmm")]
        static extern void timeEndPeriod(int t);
        bool _isfullwindow;
        private Size n_size;
        public delegate DrawResult _DrawProc(DeviceContext dc, Size size);
        public event _DrawProc DrawProc;

        public IntPtr Handle { get; private set; } = (IntPtr)0;
        public Direct2DWindow(Size size,IntPtr Handle, bool isfullwindow = false)
        {
            n_size = size;
            _isfullwindow = isfullwindow;
            this.Handle = Handle;
            timeBeginPeriod(1);
        }
        public void Run()
        {

            m_info = D2dInit(n_size, Handle, !_isfullwindow);
            m_draw_thread = new Thread(DrawingEvent) { IsBackground = true };
            m_draw_thread.Start();

        }
        private Thread m_draw_thread = null;
        private Stopwatch timer = new Stopwatch();
        private int _Frames = 60;
        public int Frames
        {
            get
            {
                return _Frames;
            }
            set
            {
                _Frames = value;
            }
        }

        public void Commit()
        {
            m_info.swapChain.Present(1, PresentFlags.None);
        }
        private void DrawingEvent()
        {
            while (true)
            {
                timer.Start();
                m_info.View.BeginDraw();
                var result = DrawProc?.Invoke(m_info.View, n_size);
                m_info.View.EndDraw();
                if (result == DrawResult.Death)
                {
                    Dispose();
                    return;
                }
                if (result == DrawResult.Commit)
                {
                        m_info.swapChain.Present(1, PresentFlags.None);
                }
                timer.Stop();
                decimal time = timer.ElapsedTicks / (decimal)Stopwatch.Frequency * 1000;
                decimal wait_time = 1000.0M / (decimal)Frames - time;

                timer.Reset();
                //    _renderForm.Text = time.ToString();
                if (wait_time <= 0) wait_time = 0;
                Thread.Sleep((int)wait_time);
            }
        }
        #region dxrender
        static public Direct2DInformation D2dInit(Size size, IntPtr handle, bool isfullwindow)
        {
            Direct2DInformation result = new Direct2DInformation();

            result.d3DDevice = new SharpDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);
            var __dxgiDevice = result.d3DDevice.QueryInterface<Device>();
            result.dxgiDevice = __dxgiDevice.QueryInterface<SharpDX.DXGI.Device2>();
            __dxgiDevice.Dispose();

            result.D2DFacy = new D2DFactory();
            result.d2DDevice = new SharpDX.Direct2D1.Device(result.D2DFacy, result.dxgiDevice);

            result.View = new DeviceContext(result.d2DDevice, DeviceContextOptions.EnableMultithreadedOptimizations);
            result.ImagingFacy = new ImagingFactory();

            result.swapChainDesc = new SwapChainDescription1();


            result.swapChainDesc.Width = 0;                           // use automatic sizing
            result.swapChainDesc.Height = 0;
            result.swapChainDesc.Format = Format.B8G8R8A8_UNorm; // this is the most common swapchain format
            result.swapChainDesc.Stereo = false;
            result.swapChainDesc.SampleDescription = new SampleDescription()
            {
                Count = 1,
                Quality = 0
            };
            result.swapChainDesc.Usage = Usage.RenderTargetOutput;
            result.swapChainDesc.BufferCount = 2;                     // use double buffering to enable flip
            result.swapChainDesc.Scaling = Scaling.None;
            result.swapChainDesc.SwapEffect = SwapEffect.FlipSequential; // all apps must use this SwapEffect
            result.swapChainDesc.Flags = 0;
            var vb_ns = result.dxgiDevice.GetParent<Adapter>().GetParent<SharpDX.DXGI.Factory2>();
            result.swapChain = new SwapChain1(vb_ns, result.d3DDevice, handle, ref result.swapChainDesc);

            result.backbuffer = Surface.FromSwapChain(result.swapChain, 0);

            result._bufferBack = new D2DBitmap(result.View, result.backbuffer);


            result.View.Target = result._bufferBack;

            vb_ns.Dispose();

            return result;
        }
        static public void D2dRelease(Direct2DInformation info)
        {
            info._bufferBack?.Dispose();
            info.View?.Dispose();

            info.ImagingFacy?.Dispose();


            info.d2DDevice?.Dispose();
            info.D2DFacy?.Dispose();
            info.dxgiDevice?.Dispose();
            info.d3DDevice?.Dispose();
            info.backbuffer.Dispose();

            info.swapChain.Dispose();
        }

        Direct2DInformation m_info;
        #endregion

        #region Dispose
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }
                m_draw_thread.Abort();
                D2dRelease(m_info);
                timeEndPeriod(1);
                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~Direct2DWindow()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
