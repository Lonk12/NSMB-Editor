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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NSMBe5.DSFileSystem;


namespace NSMBe5 {
    public partial class LevelConfig : UserControl {

        private static readonly uint[] ObjectBankSlots = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 15 };

        private const int ObjectBanksCount = 10;


        public LevelConfig(LevelEditorControl EdControl) {
            InitializeComponent();
            this.EdControl = EdControl;
            this.Level = EdControl.Level;

            tabControl1.SelectTab(0);

            LanguageManager.ApplyToContainer(this, "LevelConfig");

            // Load lists
            loadList("Foregrounds", bgTopLayerComboBox);
            loadList("Backgrounds", bgBottomLayerComboBox);
            loadList("Tilesets", tilesetComboBox);

            ComboBox[] comboBoxes = new ComboBox[] {
                slot0ComboBox, slot1ComboBox, slot2ComboBox, slot3ComboBox,
                slot4ComboBox, slot5ComboBox, slot6ComboBox, slot7ComboBox,
                slot8ComboBox, slot9ComboBox, slot15ComboBox
            };

            var stageObjBanks = ROM.GetInlineFile(ROM.Data.File_Modifiers);
            int[,] bankToOverlayTable = ROM.GetObjectBankOverlays();

            var stageObjsInOvs = new Dictionary<int, List<int>>();

            for (int obj = 0; obj < ROM.NativeStageObjCount; obj++)
            {
                int slot = stageObjBanks[obj * 2];
                int bank = stageObjBanks[obj * 2 + 1];

                // Skip bank zero and illegal slots
                if (slot < 0 || (slot > 9 && slot != 15) || bank == 0)
                    continue;

                int ov = bankToOverlayTable[slot, bank - 1];

                if (!stageObjsInOvs.ContainsKey(ov))
                    stageObjsInOvs.Add(ov, new List<int>());

                stageObjsInOvs[ov].Add(obj);
            }

            // Load object bank slot lists
            for (int i = 0; i < ObjectBankSlots.Length; i++)
            {
                uint slot = ObjectBankSlots[i];

                var items = new string[ObjectBanksCount + 1];

                items[0] = "0: None";

                foreach (string name in LanguageManager.GetList($"ObjectBankSlot{slot}"))
                {
                    string[] split = name.Split('=');

                    int index = int.Parse(split[0].Trim());
                    
                    // Skip out of bounds elements
                    if (index <= 0 || index >= items.Length)
                        continue;

                    items[index] = split[1].Trim();
                }

                for (int bank = 1; bank < items.Length; bank++)
                {
                    int ov = bankToOverlayTable[slot, bank - 1];

                    string suffix = $"[ov{ov}";

                    if (stageObjsInOvs.ContainsKey(ov))
                    {
                        suffix += ": ";
                        suffix += string.Join(", ", stageObjsInOvs[ov].ToArray());
                    }

                    suffix += "]";

                    items[bank] = $"{bank}: {items[bank]} {suffix}";
                }

                comboBoxes[i].Items.AddRange(items);
            }

        }

        private NSMBLevel Level;
        private LevelEditorControl EdControl;

        public delegate void ReloadTilesetDelegate();
        public event ReloadTilesetDelegate ReloadTileset;

        public delegate void RefreshMainWindowDelegate();
        public event RefreshMainWindowDelegate RefreshMainWindow;

        private void loadList(string name, ComboBox dest)
        {
            dest.Items.Clear();
            foreach (string s in ROM.UserInfo.getFullList(name))
            {
                string ss = s.Trim();
                if (ss == "") continue;

                string[] nameArray = ss.Split('@');

                /*if (nameArray[0].Split(':')[1] == "")
                    continue;*/

                if (nameArray.Length > 6 && nameArray[6] == "not_level_bg")
                    continue;

                dest.Items.Add(nameArray[0]);
            }
        }

        bool updating = false;
        public void LoadSettings() {
            updating = true;
            startEntranceUpDown.Value = Level.Blocks[0][0];
            midwayEntranceUpDown.Value = Level.Blocks[0][1];
            timeLimitUpDown.Value = Level.Blocks[0][4] | (Level.Blocks[0][5] << 8);
            soundSetUpDown.Value = Level.Blocks[0][26];
            levelWrapCheckBox.Checked = ((Level.Blocks[0][2] & 0x20) != 0);
            forceMiniCheckBox.Checked = ((Level.Blocks[0][2] & 0x01) != 0);
            miniMarioPhysicsCheckBox.Checked = ((Level.Blocks[0][2] & 0x02) != 0);

            tilesetComboBox.SelectedIndex = Level.Blocks[0][0xC];
            int FGIndex = Level.Blocks[0][0x12];
            if (FGIndex == 255) FGIndex = bgTopLayerComboBox.Items.Count - 1;
            bgTopLayerComboBox.SelectedIndex = FGIndex;
            int BGIndex = Level.Blocks[0][6];
            if (BGIndex == 255) BGIndex = bgBottomLayerComboBox.Items.Count - 1;
            bgBottomLayerComboBox.SelectedIndex = BGIndex;

            ComboBox[] comboBoxes = new ComboBox[] {
                slot0ComboBox, slot1ComboBox, slot2ComboBox, slot3ComboBox,
                slot4ComboBox, slot5ComboBox, slot6ComboBox, slot7ComboBox,
                slot8ComboBox, slot9ComboBox, slot15ComboBox
            };

            if (Level.Blocks[13].Length == 0) {
                // works around levels like 1-4 area 2 which have a blank modifier block
                Level.Blocks[13] = new byte[16];
            }

            for (int i = 0; i < ObjectBankSlots.Length; i++)
            {
                int bank = Level.Blocks[13][ObjectBankSlots[i]];
                comboBoxes[i].SelectedIndex = bank;
            }

            updating = false;
        }

        #region Previews
        private void tilesetPreviewButton_Click(object sender, EventArgs e) {
            ushort GFXFileID = ROM.GetFileIDFromTable(tilesetComboBox.SelectedIndex, ROM.Data.Table_TS_NCG);
            ushort PalFileID = ROM.GetFileIDFromTable(tilesetComboBox.SelectedIndex, ROM.Data.Table_TS_NCL);
        }

        private Bitmap RenderBackground(File GFXFile, File PalFile, File LayoutFile, int offs, int palOffs)
        {
            GFXFile = new CompressedFile(GFXFile, CompressedFile.CompressionType.MaybeCompressed);
            LayoutFile = new CompressedFile(LayoutFile, CompressedFile.CompressionType.MaybeCompressed);
            PalFile = new CompressedFile(PalFile, CompressedFile.CompressionType.MaybeCompressed);

            Image2D i = new Image2D(GFXFile, 256, false);
            Palette pal1 = new FilePalette(new InlineFile(PalFile, 0, 512, PalFile.name));
            Palette pal2 = new FilePalette(new InlineFile(PalFile, 512, 512, PalFile.name));

            Tilemap t = new Tilemap(LayoutFile, 64, i, new Palette[] { pal1, pal2 }, offs, palOffs);
            t.render();
            return t.buffer;
        }

        private void bgTopLayerPreviewButton_Click(object sender, EventArgs e) {
            if (bgTopLayerComboBox.SelectedIndex == bgTopLayerComboBox.Items.Count - 1) {
                MessageBox.Show(LanguageManager.Get("LevelConfig", "BlankBG"));
                return;
            }

            ushort GFXFileID = ROM.GetFileIDFromTable(bgTopLayerComboBox.SelectedIndex, ROM.Data.Table_FG_NCG);
            ushort PalFileID = ROM.GetFileIDFromTable(bgTopLayerComboBox.SelectedIndex, ROM.Data.Table_FG_NCL);
            ushort LayoutFileID = ROM.GetFileIDFromTable(bgTopLayerComboBox.SelectedIndex, ROM.Data.Table_FG_NSC);

            File GFXFile = ROM.FS.getFileById(GFXFileID);
            File PalFile = ROM.FS.getFileById(PalFileID);
            File LayoutFile = ROM.FS.getFileById(LayoutFileID);

            if (GFXFile == null || PalFile == null || LayoutFile == null)
            {
                MessageBox.Show(LanguageManager.Get("LevelConfig", "BrokenBG"));
                return;
            }

            new ImagePreviewer(RenderBackground(GFXFile, PalFile, LayoutFile, 256, 8)).Show();
        }

        private void bgBottomLayerPreviewButton_Click(object sender, EventArgs e) {
            if (bgBottomLayerComboBox.SelectedIndex == bgBottomLayerComboBox.Items.Count - 1) {
                MessageBox.Show(LanguageManager.Get("LevelConfig", "BlankBG"));
                return;
            }

            ushort GFXFileID = ROM.GetFileIDFromTable(bgBottomLayerComboBox.SelectedIndex, ROM.Data.Table_BG_NCG);
            ushort PalFileID = ROM.GetFileIDFromTable(bgBottomLayerComboBox.SelectedIndex, ROM.Data.Table_BG_NCL);
            ushort LayoutFileID = ROM.GetFileIDFromTable(bgBottomLayerComboBox.SelectedIndex, ROM.Data.Table_BG_NSC);
            File GFXFile = ROM.FS.getFileById(GFXFileID);
            File PalFile = ROM.FS.getFileById(PalFileID);
            File LayoutFile = ROM.FS.getFileById(LayoutFileID);

            if (GFXFile == null || PalFile == null || LayoutFile == null) {
                MessageBox.Show(LanguageManager.Get("LevelConfig", "BrokenBG"));
                return;
            }

            new ImagePreviewer(RenderBackground(GFXFile, PalFile, LayoutFile, 576, 10)).Show();
        }

        #endregion

        private void saveSettings() {
            if (updating) return;
            byte[][] newData = ChangeLevelSettingsAction.Clone(Level.Blocks);
            newData[0][0] = (byte)startEntranceUpDown.Value;
            newData[0][1] = (byte)midwayEntranceUpDown.Value;
            newData[0][4] = (byte)((int)timeLimitUpDown.Value & 255);
            newData[0][5] = (byte)((int)timeLimitUpDown.Value >> 8);
            newData[0][26] = (byte)((int)soundSetUpDown.Value & 255);

            byte settingsByte = 0x00;

            if (levelWrapCheckBox.Checked)
                settingsByte |= 0x20;
            if (forceMiniCheckBox.Checked)
                settingsByte |= 0x01;
            if (miniMarioPhysicsCheckBox.Checked)
                settingsByte |= 0x02;
            newData[0][2] = settingsByte;

            int oldTileset = newData[0][0xC];
            int oldBottomBg = newData[0][6];

            newData[0][0xC] = (byte)tilesetComboBox.SelectedIndex; // ncg
            newData[3][4] = (byte)tilesetComboBox.SelectedIndex; // ncl

            int FGIndex = bgTopLayerComboBox.SelectedIndex;
            if (FGIndex == bgTopLayerComboBox.Items.Count - 1) FGIndex = 0xFFFF;
            newData[0][0x12] = (byte)FGIndex; // ncg
            newData[0][0x13] = (byte)(FGIndex>>8); // ncg
            newData[4][4] = (byte)FGIndex; // ncl
            newData[4][5] = (byte)(FGIndex >> 8); // ncg
            newData[4][2] = (byte)FGIndex; // nsc
            newData[4][3] = (byte)(FGIndex >> 8); // ncg

            int BGIndex = bgBottomLayerComboBox.SelectedIndex;
            if (BGIndex == bgBottomLayerComboBox.Items.Count - 1) BGIndex = 0xFFFF;
            newData[0][6] = (byte)BGIndex; // ncg
            newData[0][7] = (byte)(BGIndex >> 8); // ncg
            newData[2][4] = (byte)BGIndex; // ncl
            newData[2][5] = (byte)(BGIndex >> 8); // ncg
            newData[2][2] = (byte)BGIndex; // nsc
            newData[2][3] = (byte)(BGIndex >> 8); // ncg


            ComboBox[] comboBoxes = new ComboBox[] {
                slot0ComboBox, slot1ComboBox, slot2ComboBox, slot3ComboBox,
                slot4ComboBox, slot5ComboBox, slot6ComboBox, slot7ComboBox,
                slot8ComboBox, slot9ComboBox, slot15ComboBox
            };

            for (int i = 0; i < ObjectBankSlots.Length; i++)
            {
                byte bank = (byte)comboBoxes[i].SelectedIndex;
                newData[13][ObjectBankSlots[i]] = bank;
            }

            EdControl.UndoManager.Do(new ChangeLevelSettingsAction(newData), true);
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            saveSettings();
        }
    }
}
