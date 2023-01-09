﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace NSMBe5
{
    class BNXLButton : Control
    {
        public int Opacity = 192;

        public BNXLButton()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, true);
            BackColor = Color.FromArgb(Opacity, 0xE1, 0xE1, 0xE1);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //base.OnPaintBackground(pevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(Opacity, BackColor.R, BackColor.G, BackColor.B)), ClientRectangle);
            pevent.Graphics.DrawRectangle(new Pen(Color.FromArgb(255, Color.Blue.R, Color.Blue.G, Color.Blue.B), 2f), ClientRectangle);

            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            pevent.Graphics.DrawString(Text, new Font(Font.Name, Font.Size), new SolidBrush(ForeColor), ClientRectangle, sf);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            if (Parent != null)
            {
                Parent.Invalidate(this.Bounds, true);
            }
            base.OnBackColorChanged(e);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if (Parent != null)
            {
                Parent.Invalidate(this.Bounds, true);
            }
            base.OnLocationChanged(e);
        }
    }
}
