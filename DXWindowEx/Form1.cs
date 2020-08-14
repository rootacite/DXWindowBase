using D2D;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DXWindowEx
{
    public partial class Form1 : Form
    {
        ClosedGemo gemo = new ClosedGemo(new Point[] { new Point(500,500),new Point(800,600),new Point(800,200), new Point(500, 200) }, new SharpDX.Mathematics.Interop.RawColor4(1, 0, 0, 1), new SharpDX.Mathematics.Interop.RawColor4(1, 1, 1, 1));
        RenderableImage image = null;
        public Form1()
        {
            InitializeComponent();
            gemo.Thickness = 1;
            float a = 0, b = 1;
            bool res = false;
            var DX = new Direct2DWindow(new Size(1280, 720),  this.Handle,false);
            this.KeyPress += (e, v) =>
            {
                image.Opacity = 0.7f;
                image.Position = new Point(300, 300);
            };
            DX.Frames = 60;
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

                gemo.Render(dc);
                image.Render();
                return DrawResult.Commit;
            };
            image = DX.CreateImage(@"C:\Users\14980\Desktop\test.jpg");
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
