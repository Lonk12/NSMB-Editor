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
using System.Text;
using System.Drawing;

namespace NSMBe5
{
    public class NSMBEntrance : LevelItem
    {
        //public byte[] Data;

        public int X;
        public int Y;
        public int CameraX;
        public int CameraY;
        public int Number;
        public int DestArea;
        public int ConnectedPipeID;
        public int DestEntrance;
        public int Type;
        public int Settings;
        public int Unknown1;
        public int EntryView;
        public int Unknown2;


        //LevelItem implementation.
        public int x { get { return X * snap; } set { X = value / snap; } }
        public int y { get { return Y * snap; } set { Y = value / snap; } }
        public int width { get { return 16; } set { } }
        public int height { get { return 16; } set { } }

        public int rx { get { return X * snap; } }
        public int ry { get { return Y * snap; } }
        public int rwidth { get { return 16; } }
        public int rheight { get { return 16; } }

        public bool isResizable { get { return false; } }
        public int snap { get { return 1; } }

        public NSMBEntrance() { }
        public NSMBEntrance(NSMBEntrance e)
        {
            X = e.X;
            Y = e.Y;
            CameraX = e.CameraX;
            CameraY = e.CameraY;
            Number = e.Number;
            DestArea = e.DestArea;
            ConnectedPipeID = e.ConnectedPipeID;
            DestEntrance = e.DestEntrance;
            Type = e.Type;
            Settings = e.Settings;
            Unknown1 = e.Unknown1;
            EntryView = e.EntryView;
            Unknown2 = e.Unknown2;
        }

        public void Render(Graphics g, LevelEditorControl edControl)
        {

            int EntranceArrowColour = 0;
            // connected pipes have the grey blob (or did, it's kind of pointless)
            /*if (((Type >= 3 && Type <= 6) || (Type >= 16 && Type <= 19) || (Type >= 22 && Type <= 25)) && (Settings & 8) != 0) {
                EntranceArrowColour = 2;
            }*/
            // doors and pipes can be exits, so mark them as one if they're not 128
            if (((Type >= 2 && Type <= 6) || (Type >= 16 && Type <= 19) || (Type >= 22 && Type <= 25)) && (Settings & 128) == 0) {
                EntranceArrowColour = 1;
            }

            g.DrawImage(Properties.Resources.entrances, new Rectangle(X, Y, 16, 16), new Rectangle(Math.Min(Type, 25) * 16, EntranceArrowColour * 16, 16, 16), GraphicsUnit.Pixel);
        }

        public override string ToString()
        {
            return String.Format("ENT:{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}:{11}:{12}", X, Y, CameraX, CameraY, Number, DestArea, 
                ConnectedPipeID, DestEntrance, Type, Settings, Unknown1, EntryView, Unknown2);
        }

        public string ToStringNormal()
        {
            return String.Format("{0}: {1} ({2},{3})", Number,
               LanguageManager.GetList("EntranceTypes")[Type],
               X, Y);
        }

        public static NSMBEntrance FromString(string[] strs, ref int idx, NSMBLevel lvl)
        {
            NSMBEntrance e = new NSMBEntrance();
            e.X = int.Parse(strs[1 + idx]);
            e.Y = int.Parse(strs[2 + idx]);
            e.CameraX = int.Parse(strs[3 + idx]);
            e.CameraY = int.Parse(strs[4 + idx]);
            e.Number = int.Parse(strs[5 + idx]);
            e.DestArea = int.Parse(strs[6 + idx]);
            e.ConnectedPipeID = int.Parse(strs[7 + idx]);
            e.DestEntrance = int.Parse(strs[8 + idx]);
            e.Type = int.Parse(strs[9 + idx]);
            e.Settings = int.Parse(strs[10 + idx]);
            e.Unknown1 = int.Parse(strs[11 + idx]);
            e.EntryView = int.Parse(strs[12 + idx]);
            e.Unknown2 = int.Parse(strs[13 + idx]);
            if (lvl.isEntranceNumberUsed(e.Number))
                e.Number = lvl.getFreeEntranceNumber();
            idx += 14;
            return e;
        }
    }
}
