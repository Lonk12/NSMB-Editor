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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NSMBe5 {
    public partial class LevelMinimap : Form
    {

        private NSMBLevel Level;
        private LevelEditorControl EdControl;
        private Brush UnViewableBlocksBrush;

        public LevelMinimap(NSMBLevel Level, LevelEditorControl EdControl)
        {
            InitializeComponent();
            LanguageManager.ApplyToContainer(this, "LevelMinimap");

            this.Level = Level;
            this.EdControl = EdControl;
            UnViewableBlocksBrush = new SolidBrush(Color.FromArgb(120, Color.DarkSlateGray.R, Color.DarkSlateGray.G, Color.DarkSlateGray.B));
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            e.Graphics.Clear(Color.LightSlateGray);
            // Draws object, but draws with background color if it is object 0
            foreach (NSMBTile obj in Level.Objects)
                if (obj.TileID == 0 && obj.Tileset == 0)
                    e.Graphics.FillRectangle(Brushes.SlateGray, obj.X, obj.Y, obj.Width, obj.Height);
                else
                    e.Graphics.FillRectangle(Brushes.White, obj.X, obj.Y, obj.Width, obj.Height);

            foreach (NSMBStageObj s in Level.Sprites)
                e.Graphics.FillRectangle(Brushes.Chartreuse, s.GetMinimapBounds());
            foreach (NSMBView v in Level.Views)
                e.Graphics.DrawRectangle(Pens.LightSteelBlue, v.X / 16, v.Y / 16, v.Width / 16, v.Height / 16);
            foreach (NSMBView v in Level.Zones)
                e.Graphics.DrawRectangle(Pens.PaleGreen, v.X / 16, v.Y / 16, v.Width / 16, v.Height / 16);

            // Draw viewable area
            e.Graphics.FillRectangle(UnViewableBlocksBrush, EdControl.ViewableBlocks);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            pictureBox1_MouseMove(sender, e);
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                // Calculate new position
                Rectangle va = EdControl.ViewableBlocks;
                Rectangle NewArea = new Rectangle(0, 0, (int)(va.Width * EdControl.zoom), (int)(va.Height * EdControl.zoom));
                NewArea.X = (int)(e.X * EdControl.zoom) - (NewArea.Width / 2);
                NewArea.Y = (int)(e.Y * EdControl.zoom) - (NewArea.Height / 2);
                // Set it
                EdControl.ScrollEditor(NewArea.Location);
                pictureBox1.Invalidate();
                //ScrollEditor(ViewableBlocks.Location);
            }
        }

        private void onFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
