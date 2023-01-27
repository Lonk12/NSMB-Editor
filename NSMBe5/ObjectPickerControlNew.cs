﻿/*
*   This file is part of NSMB Editor 5.
*
*   NSMB Editor 5 is free software: you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   NSMB Editor 5 is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.
*
*   You should have received a copy of the GNU General Public License
*   along with NSMB Editor 5.  If not, see <http://www.gnu.org/licenses/>.
*/

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace NSMBe5
{
    public partial class ObjectPickerControlNew : UserControl
    {
        public delegate void ObjectSelectedDelegate();
        public event ObjectSelectedDelegate ObjectSelected;
        bool inited = false;
        NSMBGraphics gfx;
        int tileset;
        NSMBTile selected = null;
        List<NSMBTile> objects = new List<NSMBTile>();

        int tileWidth = -1;
        public int SelectedObject = 0;
        bool selecting = false;

        public ObjectPickerControlNew()
        {
            InitializeComponent();
        }

        public void Initialise(NSMBGraphics GFXd, int tileset)
        {
            if (inited) return;
            inited = true;
            gfx = GFXd;
            this.tileset = tileset;
            LoadObjects();
        }

        public void reload()
        {
            tileWidth = -1;
            LoadObjects();
        }
        private void LoadObjects()
        {
            int nw = (Width-40) / 16;
            if (nw < 5) nw = 5;

            if (tileWidth == nw) return;
            if (!inited) return;

            objects.Clear();

            tileWidth = nw;
            int x = 0;
            int y = 0;

            int rowheight = 1;
            for(int i = 0; i < 256; i++)
            {
                if (gfx.Tilesets[tileset].Objects[i] == null) continue;
                int ow = gfx.Tilesets[tileset].Objects[i].getWidth();
                int oh = gfx.Tilesets[tileset].Objects[i].getHeight();
                if (ow > tileWidth) ow = tileWidth;
                if (oh > 5) oh = 5;

                if (x + ow > tileWidth)
                {
                    //New line
                    x = 0;
                    y += rowheight + 1;
                    rowheight = 1;
                }

                NSMBTile o = new NSMBTile(i, tileset, x, y, ow, oh, gfx);
                if (i == SelectedObject) selected = o;

                x += ow + 1;
                if (oh > rowheight) rowheight = oh;

                if (!o.badTile)
                    objects.Add(o);
            }

            if(x != 0)
                y += rowheight + 1;

            int scrollheight = y * 16 - Height + 16;
            if (this.Height > 0)
                vScrollBar1.LargeChange = this.Height;
            vScrollBar1.Maximum = scrollheight + this.Height;
            vScrollBar1.Enabled = vScrollBar1.LargeChange < vScrollBar1.Maximum;
            Invalidate();
        }

        private void ObjectPickerControlNew_Load(object sender, EventArgs e)
        {

        }

        private void ObjectPickerControlNew_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rec = e.ClipRectangle;

            e.Graphics.FillRectangle(Brushes.White, rec);
            rec.Y += vScrollBar1.Value;
            System.Drawing.Drawing2D.Matrix tr = e.Graphics.Transform;
            e.Graphics.TranslateTransform(0, -vScrollBar1.Value);

            foreach (NSMBTile obj in objects)
            {
                Rectangle or = new Rectangle(obj.X * 16 + 8, obj.Y * 16 + 8, obj.Width * 16, obj.Height * 16);
                Rectangle or2 = Rectangle.Inflate(or, 4, 4);
                if (or2.IntersectsWith(rec))
                {
                    e.Graphics.FillRectangle(obj == selected ? Brushes.OrangeRed : Brushes.LightBlue, or2);
                    e.Graphics.FillRectangle(Brushes.LightSlateGray, or);
                    obj.RenderPlain(e.Graphics, obj.X * 16 + 8, obj.Y * 16 + 8);
                }
            }
            e.Graphics.Transform = tr;
        }
        
        int xDown, yDown;
        int xScr, yScr;

        private void ObjectPickerControlNew_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                int x = e.X;
                int y = e.Y + vScrollBar1.Value;

                int oldSel = SelectedObject;

                foreach (NSMBTile obj in objects)
                {
                    Rectangle or = new Rectangle(obj.X * 16 + 8, obj.Y * 16 + 8, obj.Width * 16, obj.Height * 16);
                    or.Inflate(8, 8);
                    if (or.Contains(x, y))
                    {
                        selected = obj;
                        SelectedObject = obj.TileID;
                    }
                }

                if (oldSel != SelectedObject)
                {
                    Invalidate();
                    selecting = true;
                    if(ObjectSelected != null)
                        ObjectSelected();
                    selecting = false;
                }
            }
            else
            {
                yDown = e.Y;
                yScr = vScrollBar1.Value;
            }
        }

        private void ObjectPickerControlNew_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                if (!vScrollBar1.Enabled) return;
                int newval = yScr - (e.Y - yDown)*2;
                if (newval < vScrollBar1.Minimum) newval = vScrollBar1.Minimum;
                if (newval > vScrollBar1.Maximum - vScrollBar1.LargeChange) newval = vScrollBar1.Maximum - vScrollBar1.LargeChange;

                vScrollBar1.Value = newval;
                objectHovered(null);
                Invalidate();
            }
            else
            {
                int x = e.X;
                int y = e.Y + vScrollBar1.Value;

                bool hov = false;
                foreach (NSMBTile obj in objects)
                {
                    Rectangle or = new Rectangle(obj.X * 16 + 8, obj.Y * 16 + 8, obj.Width * 16, obj.Height * 16);
                    or.Inflate(8, 8);
                    if (or.Contains(x, y))
                    {
                        objectHovered(obj);
                        hov = true;
                    }
                }

                if (!hov) objectHovered(null);
            }
        }

        private NSMBTile hover = null;
        void objectHovered(NSMBTile obj)
        {
            if (obj == hover) return;
            hover = obj;

            if (obj == null)
                toolTip1.Hide(this);
            else
            {
                int x = Width - 16;
                int y = obj.Y * 16 - 8 + obj.Height * 8 - vScrollBar1.Value;
                toolTip1.ToolTipTitle = LanguageManager.Get("ObjectPickerControl", "Object") + obj.TileID;
                string text = "";
                if (gfx.Tilesets[tileset].UseNotes && obj.TileID < gfx.Tilesets[tileset].ObjNotes.Length)
                    text = gfx.Tilesets[tileset].ObjNotes[obj.TileID];

                toolTip1.Show(text+" ", this, x, y);
            }

        }
        private void ObjectPickerControlNew_Resize(object sender, EventArgs e)
        {
            LoadObjects();
        }

        public void selectObjectNumber(int objectNum)
        {
            SelectedObject = -1;
            selected = null;

            foreach(NSMBTile o in objects)
                if (o.TileID == objectNum)
                {
                    selected = o;
                    SelectedObject = objectNum;
                }

            if (selected != null && !selecting)
                vScrollBar1.Value = Math.Max(vScrollBar1.Minimum, Math.Min(vScrollBar1.Maximum - vScrollBar1.LargeChange + 1, selected.Y * 16 - Height / 2));
            Invalidate();
        }

        private void ObjectPickerControlNew_MouseLeave(object sender, EventArgs e)
        {
            objectHovered(null);
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        private void ObjectPickerControlNew_SizeChanged(object sender, EventArgs e)
        {
            reload();
            vScrollBar1.Value = Math.Max(0, Math.Min(vScrollBar1.Maximum - vScrollBar1.LargeChange, vScrollBar1.Value));
        }
    }
}
