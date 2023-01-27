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
    public partial class ObjectEditor : UserControl
    {
        public List<LevelItem> SelectedObjects;
        public LevelEditorControl EdControl;
        private bool DataUpdateFlag = false;

        public ObjectEditor(LevelEditorControl EdControl)
        {
            InitializeComponent();
            LanguageManager.ApplyToContainer(this, "ObjectEditor");
            tileset0picker.Initialise(EdControl.GFX, 0);
            tileset1picker.Initialise(EdControl.GFX, 1);
            tileset2picker.Initialise(EdControl.GFX, 2);
            this.EdControl = EdControl;
            UpdateInfo();
        }

        public void SelectObjects(List<LevelItem> objs)
        {
            SelectedObjects = objs;
            UpdateInfo();
        }

        public void UpdateInfo()
        {
            if (SelectedObjects == null || SelectedObjects.Count == 0)
                return;
            NSMBTile o = SelectedObjects[0] as NSMBTile;
            DataUpdateFlag = true;

            if (o.Tileset != 0) tileset0picker.selectObjectNumber(-1);
            if (o.Tileset != 1) tileset1picker.selectObjectNumber(-1);
            if (o.Tileset != 2) tileset2picker.selectObjectNumber(-1);

            if (o.Tileset == 0) tileset0picker.selectObjectNumber(o.TileID);
            if (o.Tileset == 1) tileset1picker.selectObjectNumber(o.TileID);
            if (o.Tileset == 2) tileset2picker.selectObjectNumber(o.TileID);

            tabControl1.SelectedIndex = o.Tileset;
            DataUpdateFlag = false;
        }

        public void ReloadObjectPicker() {
            tileset0picker.reload();
            tileset1picker.reload();
            tileset2picker.reload();
        }

        private void setObjectType(int til, int obj)
        {
            if (til != 0) tileset0picker.selectObjectNumber(-1);
            if (til != 1) tileset1picker.selectObjectNumber(-1);
            if (til != 2) tileset2picker.selectObjectNumber(-1);

            EdControl.UndoManager.Do(new ChangeObjectTypeAction(SelectedObjects, til, obj));
        }

        public int getObjectType()
        {
            return Math.Max(Math.Max(tileset0picker.SelectedObject, tileset1picker.SelectedObject), tileset2picker.SelectedObject);
        }

        public int getTilesetNum()
        {
            if (tileset0picker.SelectedObject != -1) return 0;
            if (tileset1picker.SelectedObject != -1) return 1;
            if (tileset2picker.SelectedObject != -1) return 2;
            return -1;
        }

        private void tileset0picker_ObjectSelected()
        {
            setObjectType(0, tileset0picker.SelectedObject);
        }

        private void tileset1picker_ObjectSelected()
        {
            setObjectType(1, tileset1picker.SelectedObject);
        }

        private void tileset2picker_ObjectSelected()
        {
            setObjectType(2, tileset2picker.SelectedObject);
        }

    }
}
