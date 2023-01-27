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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace NSMBe5
{
    public partial class ObjectPickerControl : UserControl
    {

        private NSMBTile[][] TilesetObjects;
        private bool inited = false;
        private NSMBGraphics GFX;
        private bool Ready = false;

        // save countless dictionary lookups every repaint
        private string ObjectString = LanguageManager.Get("ObjectPickerControl", "Object");
        private string InvalidObjectString = LanguageManager.Get("ObjectPickerControl", "InvalidObject");

        public ObjectPickerControl()
        {
            InitializeComponent();
            vScrollBar.Visible = true;
            SelectedObject = 0;
            UpdateScrollbars();
            DrawingArea.Invalidate();
        }

        public void Initialise(NSMBGraphics GFXd)
        {
            if (inited) return;
            inited = true;
            GFX = GFXd;
            Ready = true;
            LoadObjects();
        }

        private void LoadObjects()
        {
            TilesetObjects = new NSMBTile[][] { null, null, null };
            TilesetObjects[0] = new NSMBTile[256];
            TilesetObjects[1] = new NSMBTile[256];
            TilesetObjects[2] = new NSMBTile[256];
            for (int TSIdx = 0; TSIdx < 3; TSIdx++)
            {
                for (int ObjIdx = 0; ObjIdx < 256; ObjIdx++)
                {
                    TilesetObjects[TSIdx][ObjIdx] = new NSMBTile(ObjIdx, TSIdx, 0, 0, 5, 3, GFX);
                }
            }
        }

        public void ReRenderAll(int Tileset)
        {
            for (int ObjIdx = 0; ObjIdx < 256; ObjIdx++)
            {
                try
                {
                    TilesetObjects[Tileset][ObjIdx].UpdateTileCache();
                }
                catch (Exception) { }
            }
            DrawingArea.Invalidate();
        }

        #region Scrolling
        private void UpdateScrollbars()
        {
            ViewableHeight = (int)Math.Ceiling((float)DrawingArea.Height / 54);

            vScrollBar.Maximum = ((int)Math.Ceiling((float)(256 - ViewableHeight) / 4) * 4) + 1;
        }

        private void ObjectPickerControl_Resize(object sender, EventArgs e)
        {
            UpdateScrollbars();
            DrawingArea.Invalidate();
        }

        private void vScrollBar_ValueChanged(object sender, ScrollEventArgs e)
        {
            UpdateScrollbars();
            DrawingArea.Invalidate();
        }

        public void EnsureObjVisible(int ObjNum)
        {
            if (ObjNum < vScrollBar.Value)
            {
                vScrollBar.Value = ObjNum;
            }
            else if (ObjNum > (vScrollBar.Value + ViewableHeight - 2))
            {
                vScrollBar.Value = ObjNum - ViewableHeight + 2;
            }
        }

        private int ViewableHeight;
        #endregion


        public int SelectedObject;
        public int CurrentTileset;


        public delegate void ObjectSelectedDelegate();
        public event ObjectSelectedDelegate ObjectSelected;

        private void DrawingArea_Paint(object sender, PaintEventArgs e)
        {
            if (!Ready) return;

            e.Graphics.Clear(Color.Silver);

            int CurrentDrawY = 2;
            int RealObjIdx = vScrollBar.Value;

            for (int ObjIdx = 0; ObjIdx < ViewableHeight; ObjIdx++)
            {
                e.Graphics.FillRectangle((RealObjIdx == SelectedObject) ? Brushes.WhiteSmoke : Brushes.Gainsboro, 2, CurrentDrawY, DrawingArea.Width - 4, 52);
                e.Graphics.DrawString(ObjectString + " " + RealObjIdx.ToString(), NSMBGraphics.SmallInfoFont, Brushes.Black, 86, (float)CurrentDrawY);
                if (!GFX.Tilesets[CurrentTileset].objectExists(RealObjIdx))
                {
                    // Invalid object
                    e.Graphics.DrawImage(NSMBe5.Properties.Resources.warning, DrawingArea.Width - 22, CurrentDrawY + 2);
                    e.Graphics.DrawString(InvalidObjectString, NSMBGraphics.SmallInfoFont, Brushes.Black, 86, (float)CurrentDrawY + 14);
                }
                if (GFX.Tilesets[CurrentTileset].UseNotes && RealObjIdx < GFX.Tilesets[CurrentTileset].ObjNotes.Length)
                {
                    //e.Graphics.DrawString(GFX.Tilesets[CurrentTileset].ObjNotes[RealObjIdx], NSMBGraphics.SmallInfoFont, Brushes.Black, 86, (float)CurrentDrawY + 14);
                    e.Graphics.DrawString(GFX.Tilesets[CurrentTileset].ObjNotes[RealObjIdx], NSMBGraphics.SmallInfoFont, Brushes.Black, new Rectangle(86, CurrentDrawY + 14, DrawingArea.Width - 86, 34));
                }
                CurrentDrawY += 54;
                RealObjIdx++;
                if (RealObjIdx == 256) break;
            }

            CurrentDrawY = 4;
            RealObjIdx = vScrollBar.Value;

            for (int ObjIdx = 0; ObjIdx < ViewableHeight; ObjIdx++)
            {
                TilesetObjects[CurrentTileset][RealObjIdx].RenderPlain(e.Graphics, 4, CurrentDrawY);
                CurrentDrawY += 54;
                RealObjIdx++;
                if (RealObjIdx == 256) break;
            }
        }

        private void DrawingArea_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int OldSelection = SelectedObject;

                SelectedObject = (int)Math.Floor((double)(e.Y - 2) / 54) + vScrollBar.Value;
                if (SelectedObject < 0) SelectedObject = 0;
                if (SelectedObject > 255) SelectedObject = 255;

                if (SelectedObject != OldSelection)
                {
                    Invalidate(true);
                    ObjectSelected();
                }
            }
        }

        private void DrawingArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Y < 16 && vScrollBar.Value > 0)
                {
                    vScrollBar.Value -= 1;
                    Invalidate(true);
                }
                if (e.Y > (DrawingArea.Height - 16) && vScrollBar.Value < vScrollBar.Maximum)
                {
                    vScrollBar.Value += 1;
                    Invalidate(true);
                }
            }
            DrawingArea_MouseDown(sender, e);
        }
    }
}
