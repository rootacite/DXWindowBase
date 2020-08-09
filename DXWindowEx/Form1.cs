using D2D;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DXWindowEx
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            float a = 0, b = 1;
            bool res = false;
            var DX = new Direct2DWindow(new Size(1280, 720),  this.Handle,true);
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
                return DrawResult.Commit;
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
    }
}
