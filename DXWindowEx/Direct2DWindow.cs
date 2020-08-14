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
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;
using System.Drawing;
using WICBitmap = SharpDX.WIC.Bitmap;

using SharpDX.Direct2D1.Effects;
using Blend = SharpDX.Direct2D1.Effects.Blend;
using Image = SharpDX.Direct2D1.Image;
using SharpDX;

namespace D2D
{
    public class RenderableImage : IDrawable, IDisposable
    {
        private Size2 _Size;
        private Point _Position = new Point(0, 0);
        public Point Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = new Point(value.X, value.Y);
                if (!this.reltOutput.IsDisposed) this.reltOutput.Dispose();
                this.reltOutput = Output(rDc);
            }
        }
        private float _Opacity = 1.0f;
        public float Opacity 
        {
            get
            {
                return _Opacity;
            }

            set
            {
                _Opacity = value;
                if (!this.reltOutput.IsDisposed) this.reltOutput.Dispose();
                this.reltOutput = Output(rDc);
            }
        }

        private Image Output(DeviceContext dc)
        {
            var blEf = new SharpDX.Direct2D1.Effect(dc, Effect.Opacity);

            blEf.SetInput(0, image, new RawBool());
            blEf.SetValue(0, _Opacity);

            var result1 = blEf.Output;
            blEf.Dispose();

            return result1;
        }

        private D2DBitmap image = null;
        private Image reltOutput = null;

        private DeviceContext rDc = null;
        private bool disposedValue;

        static public RenderableImage CreateFromWIC(DeviceContext dc,WICBitmap im)
        {
            RenderableImage rt = new RenderableImage();

            rt.image = D2DBitmap.FromWicBitmap(dc, im);
            rt.reltOutput = D2DBitmap.FromWicBitmap(dc, im);
            rt.rDc = dc;
            rt._Size = new Size2(rt.image.PixelSize.Width, rt.image.PixelSize.Height);

            return rt;
        }


        public void Render()
        {
            rDc.DrawImage(reltOutput, null, new RawRectangleF(Position.X, Position.Y, Position.X + _Size.Width, Position.Y + _Size.Height), SharpDX.Direct2D1.InterpolationMode.Linear, CompositeMode.BoundedSourceCopy);
        }
        public void Render(DeviceContext dc)
        {
            dc.DrawImage(reltOutput, null, new RawRectangleF(Position.X, Position.Y, Position.X + _Size.Width, Position.Y + _Size.Height), SharpDX.Direct2D1.InterpolationMode.Linear, CompositeMode.BoundedSourceCopy);
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                this.image.Dispose();
                if(!this.reltOutput.IsDisposed) this.reltOutput.Dispose();
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~RenderableImage()
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
    }

    public interface IDrawable
    {
        void Render(DeviceContext dc);
        void Update();
    }
    public class ClosedGemo : IDrawable
    {
        public RawColor4 Border { get; set; }
        public RawColor4 Filler { get; set; }

        public int Thickness { get; set; } = 1;
        private readonly Point[] Path;
        public ClosedGemo(Point []Path,RawColor4 cBorder, RawColor4 cFiller)
        {
            this.Path = (Point[])Path.Clone();

            Border = new RawColor4(cBorder.R, cBorder.G, cBorder.B, cBorder.A);
            Filler = new RawColor4(cFiller.R, cFiller.G, cFiller.B, cFiller.A);
        }
        public void Render(DeviceContext dc)
        {
            using (SolidColorBrush BorderBrush = new SolidColorBrush(dc, Border), FillerBrush = new SolidColorBrush(dc, Filler)) 
            {
                for (int i = 0; i < Path.Length - 1; i++)
                {
                   
                    var area = new Mesh(dc, new Triangle[] { new Triangle() { Point1 = new RawVector2(Path[0].X, Path[0].Y), Point2 = new RawVector2(Path[i].X, Path[i].Y), Point3 = new RawVector2(Path[i + 1].X, Path[i + 1].Y) } });

                    dc.FillMesh(area, FillerBrush);
                    dc.DrawLine(new RawVector2(Path[i].X, Path[i].Y), new RawVector2(Path[i + 1].X, Path[i + 1].Y), BorderBrush, Thickness);
                    area.Dispose();
                }
                dc.DrawLine(new RawVector2(Path[Path.Length - 1].X, Path[Path.Length - 1].Y), new RawVector2(Path[0].X, Path[0].Y), BorderBrush, Thickness);
               
            }
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
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
        //tools

        public RenderableImage CreateImage(string path)
        {
            var im = Direct2DHelper.LoadBitmap(path);
            var result = RenderableImage.CreateFromWIC(m_info.View, im);

            im.Dispose();

            return result;
        }

        //********************************
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
            m_info = D2dInit(n_size, Handle, !_isfullwindow);
        }
        public void Run()
        {

          
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
                        m_info.swapChain.Present(0, PresentFlags.None);
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


            result.swapChainDesc.Width = size.Width;                           // use automatic sizing
            result.swapChainDesc.Height = size.Height;
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
            result.swapChainDesc.Flags = SwapChainFlags.AllowModeSwitch;
            var vb_ns = result.dxgiDevice.GetParent<Adapter>().GetParent<SharpDX.DXGI.Factory2>();
            result.swapChain = new SwapChain1(vb_ns, result.d3DDevice, handle, ref result.swapChainDesc);
            RawBool isFulls;
            Output opt;
            result.swapChain.GetFullscreenState(out isFulls, out opt);
            if (!isfullwindow && !isFulls)
            {
                ModeDescription res = new ModeDescription();
                res.Format = Format.B8G8R8A8_UNorm;
                res.Height = size.Height;
                res.Width = size.Width;
                res.Scaling = DisplayModeScaling.Stretched;
                res.RefreshRate = Rational.Empty;
                res.ScanlineOrdering = DisplayModeScanlineOrder.LowerFieldFirst;

                result.swapChain.ResizeTarget(ref res);
            
                result.swapChain.SetFullscreenState(!isfullwindow, opt);
                result.swapChain.ResizeBuffers(2, size.Width,size.Height,Format.B8G8R8A8_UNorm, SwapChainFlags.AllowModeSwitch);
            }
            result.backbuffer = Surface.FromSwapChain(result.swapChain, 0);

            result._bufferBack = new D2DBitmap(result.View, result.backbuffer);


            result.View.Target = result._bufferBack;
            result.View.AntialiasMode = AntialiasMode.Aliased;
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

        async public Task EndDrawProcess()
        {
            await Task.Factory.StartNew(e => {
                this.m_draw_thread.Abort();
                this.m_draw_thread.Join();
            }, null);
        }

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

    static class Direct2DHelper
    {
        public static WICBitmap LoadBitmap(string rele_path)
        {
            var Imgc = new ImagingFactory();
            var Demcoder = new BitmapDecoder(Imgc, rele_path, SharpDX.IO.NativeFileAccess.Read, DecodeOptions.CacheOnLoad);

            BitmapFrameDecode nm_opb = Demcoder.GetFrame(0);
            var convert = new FormatConverter(Imgc);
            convert.Initialize(nm_opb, SharpDX.WIC.PixelFormat.Format32bppPBGRA);

            var Init_action = new WICBitmap(Imgc, convert, BitmapCreateCacheOption.CacheOnLoad);

            Imgc.Dispose();
            Demcoder.Dispose();
            nm_opb.Dispose();
            convert.Dispose();
          
            return Init_action;
        }
    }
}
