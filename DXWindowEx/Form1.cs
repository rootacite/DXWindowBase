using D2D;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DXWindowEx
{
    static class MTolls
    {
        static public bool OfInterval(this double vle, double top, double bottom)
        {
            if (vle > top && vle < bottom)
                return true;
            return false;
        }
    }
    public partial class Form1 : Form
    {
        ClosedGemo gemo = new ClosedGemo(new Point[] { new Point(500,500),new Point(800,600),new Point(800,200), new Point(500, 200) }, new SharpDX.Mathematics.Interop.RawColor4(1, 0, 0, 1), new SharpDX.Mathematics.Interop.RawColor4(1, 1, 1, 1));
        InteractiveObject obj = null;
        InteractiveObject obj2 = null;
        public Form1()
        {
            InitializeComponent();
            gemo.Thickness = 1;
            float a = 0, b = 1;
            bool res = false;
            var DX = new Direct2DWindow(new Size(1280, 720), this.Handle, false);
            double rate = 0.5;
            this.KeyPress += (e, v) =>
            {
                //    image.Opacity = 0.7f;
                //    image.Position = new Point(50, 50);
                //  image.Size = new SharpDX.Size2(100, 100);
                //    image.Orientation = Math.PI * rate;
                //    image.Saturation = 1f;
                if (v.KeyChar == 'a')
                    obj.Move(new System.Windows.Vector(-2, 0));
                if (v.KeyChar == 'd')
                    obj.Move(new System.Windows.Vector(2, 0));
                if (v.KeyChar == 'w')
                    obj.Move(new System.Windows.Vector(0, -2));
                if (v.KeyChar == 's')
                    obj.Move(new System.Windows.Vector(0, 2));

                if (v.KeyChar == '-')
                    obj.Opacity -= 0.05f;
                if (v.KeyChar == '=')
                    obj.Opacity += 0.05f ;

                if (v.KeyChar == ' ')
                    obj2.Dispose();

                if (v.KeyChar == 'c')
                    obj2.NextCharacter();


                this.Text = DX.FrameRate.ToString();
            };
            DX.AskedFrames = 60;
            DX.DrawProc += (dc, pw) =>
            {
              

                if (a <= 1 && !res)
                {
                    a += 0.05f;
                    b -= 0.05f;
                }
                else if (!res)
                {
                    res = true;
                    goto draw;
                }
                if (a >= 0 && res)
                {
                    a -= 0.05f;
                    b += 0.05f;
                }
                else if (res)
                {
                    res = false;
                    goto draw;
                }
            draw:
                dc.Clear(new SharpDX.Mathematics.Interop.RawColor4(a, 1, b, 1));

                obj.Render();
               if(!obj2.IsDisposed) obj2.Render();
                return DrawResultW.Commit;
            };
            using (var im = Direct2DHelper.LoadBitmap(@"E:\note\Untitled.png"))
                obj = new InteractiveObject(DX.DC, im);
            var sz = obj.Size;
            sz.Width /= 3;
            sz.Height /= 3;
            obj.Size = sz;


            using (SharpDX.WIC.Bitmap im = Direct2DHelper.LoadBitmap(@"C:\Users\14980\Desktop\devicecontextdiagram.png"), im2 = Direct2DHelper.LoadBitmap(@"E:\note\Untitled.png"))
                obj2 = new InteractiveObject(DX.DC, new SharpDX.WIC.Bitmap[] { im, im2 })
                {
                    Position = new Point(600, 300),
                    Size = new SharpDX.Size2(200, 200)
                };

            obj.Collision += (o, r) =>
            {
               
                if (r.OfInterval(Math.PI / 4d, Math.PI * (3d / 4d)))
                {
                    o.Move(new System.Windows.Vector(0, 2));
                }

                if (r > Math.PI * (3d / 4d) && r < Math.PI * (5 / 4d))
                {
                    o.Move(new System.Windows.Vector(2, 0));
                }

                if (r > Math.PI * (5d / 4d) && r < Math.PI * (7d / 4d))
                {
                    o.Move(new System.Windows.Vector(0, -2));
                }

                if (r > Math.PI * (7d / 4d) || r < Math.PI * (1d / 4d))
                {
                    o.Move(new System.Windows.Vector(-2, 0));
                }
                return false;
            };
            DX.Run();
            this.Paint += (e, v) =>
            {
             //  DX.Commit();
            };

            
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            return;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
        
        }
    }
}
