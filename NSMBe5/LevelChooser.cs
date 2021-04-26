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
using System.Windows.Forms;
using System.IO;
using NSMBe5.DSFileSystem;
using NSMBe5.Patcher;
using System.Diagnostics;

namespace NSMBe5 {
    public partial class LevelChooser : Form
    {
        public static ImageManagerWindow imgMgr;
        public TextInputForm textForm = new TextInputForm();
        // init has to be used because Winforms is setting the value of autoBackupTime before the form loads
        //   This causes it to be saved in the settings before the settings value is loaded.
        public bool init = false;

        public static void showImgMgr()
        {
            if (imgMgr == null || imgMgr.IsDisposed)
                imgMgr = new ImageManagerWindow();

            imgMgr.Show();
        }

        public LevelChooser()
        {
            InitializeComponent();
        }

        private void LevelChooser_Load(object sender, EventArgs e)
        {
            useMDI.Checked = Properties.Settings.Default.mdi;
            dlpCheckBox.Checked = Properties.Settings.Default.dlpMode;
            using_sb_asm_checkBox.Checked = Properties.Settings.Default.using_signboard_asm;
            chkAutoBackup.Checked = Properties.Settings.Default.AutoBackup > 0;
            if (chkAutoBackup.Checked)
                autoBackupTime.Value = Properties.Settings.Default.AutoBackup;
            init = true;

            filesystemBrowser1.Load(ROM.FS);

            LoadLevelNames();
            if (ROM.UserInfo != null)
                musicList.Items.AddRange(ROM.UserInfo.getFullList("Music").ToArray());

            LanguageManager.ApplyToContainer(this, "LevelChooser");
            openROMDialog.Filter = LanguageManager.Get("Filters", "rom");
            importLevelDialog.Filter = LanguageManager.Get("Filters", "level");
            exportLevelDialog.Filter = LanguageManager.Get("Filters", "level");
            openPatchDialog.Filter = LanguageManager.Get("Filters", "patch");
            savePatchDialog.Filter = LanguageManager.Get("Filters", "patch");
            openTextFileDialog.Filter = LanguageManager.Get("Filters", "text");
            saveTextFileDialog.Filter = LanguageManager.Get("Filters", "text");
            this.Activate();
            //Get Language Files
			string langDir = System.IO.Path.Combine(Application.StartupPath, "Languages");
            if (System.IO.Directory.Exists(langDir)) {
                string[] files = System.IO.Directory.GetFiles(langDir);
                for (int l = 0; l < files.Length; l++) {
                    if (files[l].EndsWith(".ini")) {
                        int startPos = files[l].LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1;
                        languageListBox.Items.Add(files[l].Substring(startPos, files[l].LastIndexOf('.') - startPos));
                    }
                }
            }

            // Load file backups from crash
            string backupPath = Path.Combine(Application.StartupPath, "Backup");
            if (ROM.fileBackups.Count > 0) {
                foreach (string filename in ROM.fileBackups) {
                    try
                    {
                        new LevelEditor(new NSMBLevel(LevelSource.getForBackupLevel(filename, backupPath))).Show();
                    }
                    catch (Exception) { }
                }
            }


            this.Text = "NSMB Editor 5.3.3 - " + ROM.filename;
            label3.Text = "NSMB Editor 5.3.3 " + Properties.Resources.version.Trim();
            this.Icon = Properties.Resources.nsmbe;

            if (!ROM.isNSMBRom)
            {
                tabControl1.TabPages.Remove(tabPage2);
                tabControl1.TabPages.Remove(tabPage5);
                tabControl1.TabPages.Remove(tabPage6);
                nsmbToolsGroupbox.Enabled = false;
                musicSlotsGrp.Enabled = false;
            }

            // new LevelEditor("A01_1", "LOL").Show();

            if (Properties.Settings.Default.UseFireflower)
            {
                makeinsert.Text = "Run 'fireflower' and insert";
                makeclean.Text = "Clean build";
            }
        }


        private void LoadLevelNames()
        {
            List<string> LevelNames = LanguageManager.GetList("LevelNames");

            TreeNode WorldNode = null;
            TreeNode LevelNode;
            TreeNode AreaNode;
            for (int NameIdx = 0; NameIdx < LevelNames.Count; NameIdx++) {
                LevelNames[NameIdx] = LevelNames[NameIdx].Trim();
                if (LevelNames[NameIdx] == "") continue;
                if (LevelNames[NameIdx][0] == '-') {
                    // Create a world
                    string[] ParseWorld = LevelNames[NameIdx].Substring(1).Split('|');
                    WorldNode = levelTreeView.Nodes.Add("ln" + NameIdx.ToString(), ParseWorld[0]);
                } else {
                    // Create a level
                    string[] ParseLevel = LevelNames[NameIdx].Split('|');
                    int AreaCount = ParseLevel.Length - 1;
                    if (AreaCount == 1) {
                        // One area only; no need for a subfolder here
                        LevelNode = WorldNode.Nodes.Add("ln" + NameIdx.ToString(), ParseLevel[0]);
                        LevelNode.Tag = ParseLevel[1];
                    } else {
                        // Create a subfolder
                        LevelNode = WorldNode.Nodes.Add("ln" + NameIdx.ToString(), ParseLevel[0]);
                        for (int AreaIdx = 1; AreaIdx <= AreaCount; AreaIdx++) {
                            AreaNode = LevelNode.Nodes.Add("ln" + NameIdx.ToString() + "a" + AreaIdx.ToString(), LanguageManager.Get("LevelChooser", "Area") + " " + AreaIdx.ToString());
                            AreaNode.Tag = ParseLevel[AreaIdx];
                        }
                    }
                }
            }
        }

        private void levelTreeView_AfterSelect(object sender, TreeViewEventArgs e) {
            bool enabled = e.Node.Tag != null;
            importLevelButton.Enabled = enabled;
            exportLevelButton.Enabled = enabled;
            editLevelButton.Enabled = enabled;
            hexEditLevelButton.Enabled = enabled;
            importClipboard.Enabled = enabled;
            exportClipboard.Enabled = enabled;
        }

        private void editLevelButton_Click(object sender, EventArgs e)
        {
            // Make a caption
            string EditorCaption = "";

            if (levelTreeView.SelectedNode.Parent.Parent == null) {
                EditorCaption += levelTreeView.SelectedNode.Text;
            } else {
                EditorCaption += levelTreeView.SelectedNode.Parent.Text + ", " + levelTreeView.SelectedNode.Text;
            }

            // Open it
            try
            {
                LevelEditor NewEditor = new LevelEditor(new NSMBLevel(new InternalLevelSource((string)levelTreeView.SelectedNode.Tag, EditorCaption)));
                NewEditor.Show();
            }
            catch (AlreadyEditingException)
            {
                MessageBox.Show(LanguageManager.Get("Errors", "Level"));
            }                
        }

        private void hexEditLevelButton_Click(object sender, EventArgs e) {
            if (levelTreeView.SelectedNode == null) return;

            // Make a caption
            string EditorCaption = LanguageManager.Get("General", "EditingSomething") + " ";

            if (levelTreeView.SelectedNode.Parent.Parent == null) {
                EditorCaption += levelTreeView.SelectedNode.Text;
            } else {
                EditorCaption += levelTreeView.SelectedNode.Parent.Text + ", " + levelTreeView.SelectedNode.Text;
            }

            // Open it
            try
            {
                LevelHexEditor NewEditor = new LevelHexEditor((string)levelTreeView.SelectedNode.Tag);
                NewEditor.Text = EditorCaption;
                NewEditor.Show();
            }
            catch (AlreadyEditingException)
            {
                MessageBox.Show(LanguageManager.Get("Errors", "Level"));
            }                
        }

        private void levelTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
            if (e.Node.Tag != null && editLevelButton.Enabled == true) {
                editLevelButton_Click(null, null);
            }
        }

        private void importLevelButton_Click(object sender, EventArgs e) {
            if (levelTreeView.SelectedNode == null) return;

            // Figure out what file to import
            if (importLevelDialog.ShowDialog() == DialogResult.Cancel)
                return;

            // Get the files
            string LevelFilename = (string)levelTreeView.SelectedNode.Tag;
            DSFileSystem.File LevelFile = ROM.getLevelFile(LevelFilename);
            DSFileSystem.File BGFile = ROM.getBGDatFile(LevelFilename);

            // Load it
            try
            {
                ExternalLevelSource level = new ExternalLevelSource(importLevelDialog.FileName);
                level.level.Import(LevelFile, BGFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void exportLevelButton_Click(object sender, EventArgs e) {
            if (levelTreeView.SelectedNode == null) return;

            // Figure out what file to export to
            if (exportLevelDialog.ShowDialog() == DialogResult.Cancel)
                return;

            // Get the files
            string LevelFilename = (string)levelTreeView.SelectedNode.Tag;
            DSFileSystem.File LevelFile = ROM.getLevelFile(LevelFilename);
            DSFileSystem.File BGFile = ROM.getBGDatFile(LevelFilename);

            // Load it
            FileStream fs = new FileStream(exportLevelDialog.FileName, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            new ExportedLevel(LevelFile, BGFile).Write(bw);
            bw.Close();
        }

        private void openLevel_Click(object sender, EventArgs e)
        {
            if (importLevelDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            try {
                new LevelEditor(new NSMBLevel(new ExternalLevelSource(importLevelDialog.FileName))).Show();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void importClipboard_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show((LanguageManager.Get("LevelChooser", "replaceclipboard")), (LanguageManager.Get("LevelChooser", "replaceclipboardtitle")), MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                return;
            try
            {
                string LevelFilename = (string)levelTreeView.SelectedNode.Tag;
                DSFileSystem.File LevelFile = ROM.getLevelFile(LevelFilename);
                DSFileSystem.File BGFile = ROM.getBGDatFile(LevelFilename);
                ClipboardLevelSource level = new ClipboardLevelSource();
                level.level.Import(LevelFile, BGFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show((LanguageManager.Get("LevelChooser", "clipinvalidlevel")));
            }
        }

        private void exportClipboard_Click(object sender, EventArgs e)
        {
            string LevelFilename = (string)levelTreeView.SelectedNode.Tag;
            DSFileSystem.File LevelFile = ROM.getLevelFile(LevelFilename);
            DSFileSystem.File BGFile = ROM.getBGDatFile(LevelFilename);

            ByteArrayInputStream strm = new ByteArrayInputStream(new byte[0]);
            BinaryWriter bw = new BinaryWriter(strm);

            new ExportedLevel(LevelFile, BGFile).Write(bw);
            ClipboardLevelSource.copyData(strm.getData());
            bw.Close();
        }

        private void openClipboard_Click(object sender, EventArgs e)
        {
            try
            {
                new LevelEditor(new NSMBLevel(new ClipboardLevelSource())).Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(LanguageManager.Get("LevelChooser", "clipinvalidlevel"));
            }
        }

        private DataFinder DataFinderForm;

        private void dataFinderButton_Click(object sender, EventArgs e) {
            if (DataFinderForm == null || DataFinderForm.IsDisposed) {
                DataFinderForm = new DataFinder();
            }

            DataFinderForm.Show();
            DataFinderForm.Activate();
        }


        /**
         * PATCH FILE FORMAT
         * 
         * - String "NSMBe5 Exported Patch"
         * - Some files (see below)
         * - byte 0
         * 
         * STRUCTURE OF A FILE
         * - byte 1
         * - File name as a string
         * - File ID as ushort (to check for different versions, only gives a warning)
         * - File length as uint
         * - File contents as byte[]
         */

        const string oldPatchHeader = "NSMBe4 Exported Patch";
        public const string patchHeader = "NSMBe5 Exported Patch";

        private void patchExport_Click(object sender, EventArgs e)
        {
            //output to show to the user
            bool differentRomsWarning = false; // tells if we have shown the warning
            int fileCount = 0;

            //load the original rom
            MessageBox.Show(LanguageManager.Get("Patch", "SelectROM"), LanguageManager.Get("Patch", "Export"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (openROMDialog.ShowDialog() == DialogResult.Cancel)
                return;
            NitroROMFilesystem origROM = new NitroROMFilesystem(openROMDialog.FileName);

            //open the output patch
            MessageBox.Show(LanguageManager.Get("Patch", "SelectLocation"), LanguageManager.Get("Patch", "Export"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (savePatchDialog.ShowDialog() == DialogResult.Cancel)
                return;

            FileStream fs = new FileStream(savePatchDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
            
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(patchHeader);

            //DO THE PATCH!!
            ProgressWindow progress = new ProgressWindow(LanguageManager.Get("Patch", "ExportProgressTitle"));
            progress.Show();
            progress.SetMax(ROM.FS.allFiles.Count);
            int progVal = 0;
            MessageBox.Show(LanguageManager.Get("Patch", "StartingPatch"), LanguageManager.Get("Patch", "Export"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            foreach (NSMBe5.DSFileSystem.File f in ROM.FS.allFiles)
            {
                if (f.isSystemFile) continue;

                Console.Out.WriteLine("Checking " + f.name);
                progress.SetCurrentAction(string.Format(LanguageManager.Get("Patch", "ComparingFile"), f.name));

                NSMBe5.DSFileSystem.File orig = origROM.getFileByName(f.name);
                //check same version
                if(orig == null)
                {
                    new ErrorMSGBox("", "", "In this case it is recommended that you continue.", "This ROM has more files than the original clean ROM or a file was renamed!\n\nPlease make an XDelta patch instead.\n\nExport will end now.").ShowDialog();
                    bw.Write((byte)0);
                    bw.Close();
                    origROM.close();
                    progress.SetCurrentAction("");
                    progress.WriteLine(string.Format(LanguageManager.Get("Patch", "ExportReady"), fileCount));
                    return;
                }
                else if(!differentRomsWarning && f.id != orig.id)
                {
                    if (MessageBox.Show(LanguageManager.Get("Patch", "ExportDiffVersions"), LanguageManager.Get("General", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        differentRomsWarning = true;
                    else
                    {
                        fs.Close();
                        return;
                    }
                }

                byte[] oldFile = orig.getContents();
                byte[] newFile = f.getContents();

                if (!arrayEqual(oldFile, newFile))
                {
                    //include file in patch
                    string fileName = orig.name;
                    Console.Out.WriteLine("Including: " + fileName);
                    progress.WriteLine(string.Format(LanguageManager.Get("Patch", "IncludedFile"), fileName));
                    fileCount++;

                    bw.Write((byte)1);
                    bw.Write(fileName);
                    bw.Write((ushort)f.id);
                    bw.Write((uint)newFile.Length);
                    bw.Write(newFile, 0, newFile.Length);
                }
                progress.setValue(++progVal);
            }
            bw.Write((byte)0);
            bw.Close();
            origROM.close();
            progress.SetCurrentAction("");
            progress.WriteLine(string.Format(LanguageManager.Get("Patch", "ExportReady"), fileCount));
        }

        public bool arrayEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;

            return true;
        }

        private void patchImport_Click(object sender, EventArgs e)
        {
            //output to show to the user
            bool differentRomsWarning = false; // tells if we have shown the warning
            int fileCount = 0;

            //open the input patch
            if (openPatchDialog.ShowDialog() == DialogResult.Cancel)
                return;

            FileStream fs = new FileStream(openPatchDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader br = new BinaryReader(fs);

            string header = br.ReadString();
            if (!(header == patchHeader || header == oldPatchHeader))
            {
                MessageBox.Show(
                    LanguageManager.Get("Patch", "InvalidFile"),
                    LanguageManager.Get("Patch", "Unreadable"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                br.Close();
                return;
            }


            ProgressWindow progress = new ProgressWindow(LanguageManager.Get("Patch", "ImportProgressTitle"));
            progress.Show();

            byte filestartByte = br.ReadByte();
            try
            {
                while (filestartByte == 1)
                {
                    string fileName = br.ReadString();
                    progress.WriteLine(string.Format(LanguageManager.Get("Patch", "ReplacingFile"), fileName));
                    ushort origFileID = br.ReadUInt16();
                    NSMBe5.DSFileSystem.File f = ROM.FS.getFileByName(fileName);
                    uint length = br.ReadUInt32();

                    byte[] newFile = new byte[length];
                    br.Read(newFile, 0, (int)length);
                    filestartByte = br.ReadByte();

                    if (f != null)
                    {
                        ushort fileID = (ushort)f.id;

                        if (!differentRomsWarning && origFileID != fileID)
                        {
                            MessageBox.Show(LanguageManager.Get("Patch", "ImportDiffVersions"), LanguageManager.Get("General", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            differentRomsWarning = true;
                        }
                        if (!f.isSystemFile)
                        {
                            Console.Out.WriteLine("Replace " + fileName);
                            f.beginEdit(this);
                            f.replace(newFile, this);
                            f.endEdit(this);
                        }
                        fileCount++;
                    }
                }
            }
            catch (AlreadyEditingException)
            {
                MessageBox.Show(string.Format(LanguageManager.Get("Patch", "Error"), fileCount), LanguageManager.Get("General", "Completed"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            br.Close();
            MessageBox.Show(string.Format(LanguageManager.Get("Patch", "ImportReady"), fileCount), LanguageManager.Get("General", "Completed"), MessageBoxButtons.OK, MessageBoxIcon.Information);
//            progress.Close();
        }

        private void mpPatch_Click(object sender, EventArgs e)
        {
            NarcReplace("Dat_Field.narc",    "J01_1.bin");
            NarcReplace("Dat_Basement.narc", "J02_1.bin");
            NarcReplace("Dat_Ice.narc",      "J03_1.bin");
            NarcReplace("Dat_Pipe.narc",     "J04_1.bin");
            NarcReplace("Dat_Fort.narc",     "J05_1.bin");
            NarcReplace("Dat_Field.narc",    "J01_1_bgdat.bin");
            NarcReplace("Dat_Basement.narc", "J02_1_bgdat.bin");
            NarcReplace("Dat_Ice.narc",      "J03_1_bgdat.bin");
            NarcReplace("Dat_Pipe.narc",     "J04_1_bgdat.bin");
            NarcReplace("Dat_Fort.narc",     "J05_1_bgdat.bin");

            MessageBox.Show(LanguageManager.Get("General", "Completed"));
        }

        private void mpPatch2_Click(object sender, EventArgs e)
        {
            NSMBLevel lvl = new NSMBLevel(new InternalLevelSource("J01_1", ""));
            NarcReplace("Dat_Field.narc", "I_M_nohara.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT));
            NarcReplace("Dat_Field.narc", "I_M_nohara_hd.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT_HD));
            NarcReplace("Dat_Field.narc", "d_2d_PA_I_M_nohara.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_PNL));
            NarcReplace("Dat_Field.narc", "NoHaRaMainUnitChangeData.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_CHK));
            NarcReplace("Dat_Field.narc", "d_2d_I_M_tikei_nohara_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCG));
            NarcReplace("Dat_Field.narc", "d_2d_I_M_tikei_nohara_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCL));

            NarcReplace("Dat_Field.narc", "d_2d_I_M_back_nohara_VS_UR_nsc.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NSC));
            NarcReplace("Dat_Field.narc", "d_2d_I_M_back_nohara_VS_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCG));
            NarcReplace("Dat_Field.narc", "d_2d_I_M_back_nohara_VS_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCL));

            NarcReplace("Dat_Field.narc", "d_2d_I_M_free_nohara_VS_UR_nsc.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NSC));
            NarcReplace("Dat_Field.narc", "d_2d_I_M_free_nohara_VS_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NCG));
            NarcReplace("Dat_Field.narc", "d_2d_I_M_free_nohara_VS_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NCL));


            lvl = new NSMBLevel(new InternalLevelSource("J02_1", ""));
            NarcReplace("Dat_Basement.narc", "I_M_chika3.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT));
            NarcReplace("Dat_Basement.narc", "I_M_chika3_hd.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT_HD));
            NarcReplace("Dat_Basement.narc", "d_2d_PA_I_M_chika3.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_PNL));
            NarcReplace("Dat_Basement.narc", "ChiKa3MainUnitChangeData.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_CHK));
            NarcReplace("Dat_Basement.narc", "d_2d_I_M_tikei_chika3_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCG));
            NarcReplace("Dat_Basement.narc", "d_2d_I_M_tikei_chika3_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCL));

            NarcReplace("Dat_Basement.narc", "d_2d_I_M_back_chika3_R_nsc.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NSC));
            NarcReplace("Dat_Basement.narc", "d_2d_I_M_back_chika3_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCG));
            NarcReplace("Dat_Basement.narc", "d_2d_I_M_back_chika3_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCL));


            lvl = new NSMBLevel(new InternalLevelSource("J03_1", ""));
            NarcReplace("Dat_Ice.narc", "I_M_setsugen2.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT));
            NarcReplace("Dat_Ice.narc", "I_M_setsugen2_hd.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT_HD));
            NarcReplace("Dat_Ice.narc", "d_2d_PA_I_M_setsugen2.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_PNL));
            NarcReplace("Dat_Ice.narc", "SeTsuGen2MainUnitChangeData.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_CHK));
            NarcReplace("Dat_Ice.narc", "d_2d_I_M_tikei_setsugen2_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCG));
            NarcReplace("Dat_Ice.narc", "d_2d_I_M_tikei_setsugen2_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCL));

            NarcReplace("Dat_Ice.narc", "d_2d_I_M_back_setsugen2_UR_nsc.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NSC));
            NarcReplace("Dat_Ice.narc", "d_2d_I_M_back_setsugen2_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCG));
            NarcReplace("Dat_Ice.narc", "d_2d_I_M_back_setsugen2_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCL));

            NarcReplace("Dat_Ice.narc", "d_2d_I_M_free_setsugen2_UR_nsc.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NSC));
            NarcReplace("Dat_Ice.narc", "d_2d_I_M_free_setsugen2_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NCG));
            NarcReplace("Dat_Ice.narc", "d_2d_I_M_free_setsugen2_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NCL));


            lvl = new NSMBLevel(new InternalLevelSource("J04_1", ""));
            NarcReplace("Dat_Pipe.narc", "W_M_dokansoto.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT));
            NarcReplace("Dat_Pipe.narc", "W_M_dokansoto_hd.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT_HD));
            NarcReplace("Dat_Pipe.narc", "d_2d_PA_W_M_dokansoto.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_PNL));
            NarcReplace("Dat_Pipe.narc", "DoKaNSoToMainUnitChangeData.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_CHK));
            NarcReplace("Dat_Pipe.narc", "d_2d_W_M_tikei_dokansoto_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCG));
            NarcReplace("Dat_Pipe.narc", "d_2d_W_M_tikei_dokansoto_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCL));

            NarcReplace("Dat_Pipe.narc", "d_2d_W_M_back_dokansoto_R_nsc.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NSC));
            NarcReplace("Dat_Pipe.narc", "d_2d_W_M_back_dokansoto_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCG));
            NarcReplace("Dat_Pipe.narc", "d_2d_W_M_back_dokansoto_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCL));

            NarcReplace("Dat_Pipe.narc", "d_2d_W_M_free_dokansoto_R_nsc.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NSC));
            NarcReplace("Dat_Pipe.narc", "d_2d_W_M_free_dokansoto_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NCG));
            NarcReplace("Dat_Pipe.narc", "d_2d_W_M_free_dokansoto_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NCL));


            lvl = new NSMBLevel(new InternalLevelSource("J05_1", ""));
            NarcReplace("Dat_Fort.narc", "I_M_yakata.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT));
            NarcReplace("Dat_Fort.narc", "I_M_yakata_hd.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_UNT_HD));
            NarcReplace("Dat_Fort.narc", "d_2d_PA_I_M_yakata.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_PNL));
            NarcReplace("Dat_Fort.narc", "YaKaTaMainUnitChangeData.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_CHK));
            NarcReplace("Dat_Fort.narc", "d_2d_I_M_tikei_yakata_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCG));
            NarcReplace("Dat_Fort.narc", "d_2d_I_M_tikei_yakata_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0xC], ROM.Data.Table_TS_NCL));

            NarcReplace("Dat_Fort.narc", "d_2d_I_M_back_yakata_UR_nsc.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NSC));
            NarcReplace("Dat_Fort.narc", "d_2d_I_M_back_yakata_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCG));
            NarcReplace("Dat_Fort.narc", "d_2d_I_M_back_yakata_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x6], ROM.Data.Table_BG_NCL));


            NarcReplace("Dat_Fort.narc", "d_2d_I_M_free_yakata_UR_nsc.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NSC));
            NarcReplace("Dat_Fort.narc", "d_2d_I_M_free_yakata_ncg.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NCG));
            NarcReplace("Dat_Fort.narc", "d_2d_I_M_free_yakata_ncl.bin", ROM.GetFileIDFromTable(lvl.Blocks[0][0x12], ROM.Data.Table_FG_NCL));

            MessageBox.Show(LanguageManager.Get("General", "Completed"));
        }

        //WTF was this for?!
        //private void NarcReplace(string NarcName, string f1, string f2) { }

        private void NarcReplace(string NarcName, string f1, ushort f2)
        {
            NarcFilesystem fs = new NarcFilesystem(ROM.FS.getFileByName(NarcName));

            NSMBe5.DSFileSystem.File f = fs.getFileByName(f1);
            if (f == null)
                Console.Out.WriteLine("No File: " + NarcName + "/" + f1);
            else
            {
                f.beginEdit(this);
                f.replace(ROM.FS.getFileById(f2).getContents(), this);
                f.endEdit(this);
            }
            fs.close();            
        }

        private void NarcReplace(string NarcName, string f1)
        {
            NarcFilesystem fs = new NarcFilesystem(ROM.FS.getFileByName(NarcName));

            NSMBe5.DSFileSystem.File f = fs.getFileByName(f1);
            f.beginEdit(this);
            f.replace(ROM.FS.getFileByName(f1).getContents(), this);
            f.endEdit(this);

            fs.close();
        }

		//ASM Tools
        private void decompArm9Bin_Click(object sender, EventArgs e)
        {
            Arm9BinaryHandler bh = new Arm9BinaryHandler();
            bh.decompress();
        }

        private void makeinsert_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.UseFireflower)
            {
                PatchMakerFF pmff = new PatchMakerFF(ROM.romfile.Directory);
                pmff.compilePatch();
                pmff.importPatch();
            }
            else
            {
                PatchMaker pm = new PatchMaker(ROM.romfile.Directory);
                string hookmap;
                string replaces;
                pm.insertCode(ROM.romfile.Directory, out hookmap, out replaces);
                pm.restore();
                pm.compilePatch();
                pm.generatePatch(hookmap, replaces);
            }
        }

        private void makeclean_Click(object sender, EventArgs e)
        {
            PatchCompiler.cleanPatch(ROM.romfile.Directory);
        }
        
        //Settings
        private void useMDI_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.mdi = useMDI.Checked;
            Properties.Settings.Default.Save();
        }

        private void updateSpriteDataButton_Click(object sender, EventArgs e)
        {
            SpriteData.Update();
        }

        private void changeLanguageButton_Click(object sender, EventArgs e) {
            if (languageListBox.SelectedItem != null) {
                Properties.Settings.Default.LanguageFile = languageListBox.SelectedItem.ToString();
                Properties.Settings.Default.Save();

                    MessageBox.Show(
                        LanguageManager.Get("LevelChooser", "LangChanged"),
                        LanguageManager.Get("LevelChooser", "LangChangedTitle"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        //Other crap
        private void dumpMapButton_Click(object sender, EventArgs e)
        {
/*            if (saveTextFileDialog.ShowDialog() != DialogResult.OK) return;

            TextWriter tw = new StreamWriter(new FileStream(saveTextFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite));
            ROM.FS.dumpFilesOrdered(tw);
            tw.Close();*/
        }
        
        private void LevelChooser_FormClosing(object sender, FormClosingEventArgs e)
        {
            ROM.close();
            Console.Out.WriteLine(e.CloseReason.ToString());
            if (MdiParentForm.instance != null && e.CloseReason != CloseReason.MdiFormClosing)
                MdiParentForm.instance.Close();
            Properties.Settings.Default.BackupFiles = "";
            Properties.Settings.Default.Save();
        }
        
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://github.com/Dirbaio/NSMB-Editor");
        }

        private void linkLabel2_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://nsmbhd.net/");
        }

        private void renameBtn_Click(object sender, EventArgs e)
        {
            if (musicList.SelectedIndex == -1)
                return;
            string newName;
            string oldName = musicList.SelectedItem.ToString();
            oldName = oldName.Substring(oldName.IndexOf(" ") + 1);
            if (textForm.ShowDialog(LanguageManager.Get("LevelChooser", "rnmmusic"), oldName, out newName) == DialogResult.OK)
            {
                if (newName == string.Empty)
                {
                    ROM.UserInfo.removeListItem("Music", musicList.SelectedIndex, true);
                    musicList.Items[musicList.SelectedIndex] = string.Format("{0}: {1}", musicList.SelectedIndex, LanguageManager.GetList("Music")[musicList.SelectedIndex].Split('=')[1]);
                    return;
                }
                ROM.UserInfo.setListItem("Music", musicList.SelectedIndex, string.Format("{0}={1}", musicList.SelectedIndex, newName), true);
                musicList.Items[musicList.SelectedIndex] = string.Format("{0}: {1}", musicList.SelectedIndex, newName);
            }
        }

        private void autoBackupTime_ValueChanged(object sender, EventArgs e)
        {
            if (init)
            {
                Properties.Settings.Default.AutoBackup = chkAutoBackup.Checked ? (int)autoBackupTime.Value : 0;
                Properties.Settings.Default.Save();
            }
        }

        private void deleteBackups_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show((LanguageManager.Get("LevelChooser", "delbackup")), (LanguageManager.Get("LevelChooser", "delbacktitle")), MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                String backupPath = Path.Combine(Application.StartupPath, "Backup");
                if (System.IO.Directory.Exists(backupPath))
                    System.IO.Directory.Delete(backupPath, true);
            }
        }

        private void dlpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ROM.dlpMode = dlpCheckBox.Checked;
            Properties.Settings.Default.dlpMode = dlpCheckBox.Checked;
            Properties.Settings.Default.Save();
        }

        private void LevelChooser_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void Xdelta_export_Click(object sender, EventArgs e)
        {
            MessageBox.Show(LanguageManager.Get("Patch", "XSelectROM") + LanguageManager.Get("Patch", "XRestartAfterApplied"), LanguageManager.Get("Patch", "XExport"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            OpenFileDialog cleanROM_openFileDialog = new OpenFileDialog();
            cleanROM_openFileDialog.Title = "Please select a clean NSMB ROM file";
            cleanROM_openFileDialog.Filter = "Nintendo DS ROM (*.nds)|*.nds|All files (*.*)|*.*";

            if (cleanROM_openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            SaveFileDialog xdelta_saveFileDialog = new SaveFileDialog();
            xdelta_saveFileDialog.Title = "Please select where to save the XDelta patch";
            xdelta_saveFileDialog.Filter = "XDelta Patch (*.xdelta)|*.xdelta|All files (*.*)|*.*";

            if (xdelta_saveFileDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                ROM.close();

                string str = " -f -s \"" + cleanROM_openFileDialog.FileName + "\" \"" + Properties.Settings.Default.ROMPath + "\" \"" + xdelta_saveFileDialog.FileName + "\"";
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "xdelta3.exe";
                process.StartInfo.Arguments = str;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                string end = process.StandardError.ReadToEnd();
                if (end == "")
                {
                    MessageBox.Show("Patch created successfully!", "Success!");
                }
                else
                {
                    MessageBox.Show(end, "Something went wrong!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                process.WaitForExit();
            }
            catch(Exception ex)
            {
                MessageBox.Show("An unexpected exception has occured!\nDetails:\n" + ex, LanguageManager.Get("SpriteData", "ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //Restart NSMBe
                Process process2 = new Process();
                process2.StartInfo.FileName = Application.ExecutablePath;
                process2.StartInfo.Arguments = "\"" + Properties.Settings.Default.ROMPath + "\"";
                process2.Start();
                Application.Exit();
            }
        }

        private void Xdelta_import_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("All of the ROM contents will be replaced with the XDelta patch, unlike the NSMBe patches, this one overwrites the ROM entirely!\n\nNSMBe is going to restart after the import has finished.\n\nDo you still want to contiue?", LanguageManager.Get("General", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.No)
                return;

            MessageBox.Show(LanguageManager.Get("Patch", "XSelectROM"), LanguageManager.Get("Patch", "XImport"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            OpenFileDialog cleanROM_openFileDialog = new OpenFileDialog();
            cleanROM_openFileDialog.Title = "Please select a clean NSMB ROM file";
            cleanROM_openFileDialog.Filter = "Nintendo DS ROM (*.nds)|*.nds|All files (*.*)|*.*";

            if (cleanROM_openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            OpenFileDialog xdelta_openFileDialog = new OpenFileDialog();
            xdelta_openFileDialog.Title = "Please select the XDelta patch to import";
            xdelta_openFileDialog.Filter = "XDelta Patch (*.xdelta)|*.xdelta|All files (*.*)|*.*";

            if (xdelta_openFileDialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                ROM.close();

                string str = " -d -f -s \"" + cleanROM_openFileDialog.FileName + "\" \"" + xdelta_openFileDialog.FileName + "\" \"" + Properties.Settings.Default.ROMPath + "\"";
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "xdelta3.exe";
                process.StartInfo.Arguments = str;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                string end = process.StandardError.ReadToEnd();
                if (end == "")
                {
                    MessageBox.Show("ROM patched successfully, restarting NSMBe...");
                }
                else
                {
                    MessageBox.Show(end + "\n\nRestarting NSMBe!", "Something went wrong!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                process.WaitForExit();

                //Restart NSMBe
                Process process2 = new Process();
                process2.StartInfo.FileName = Application.ExecutablePath;
                process2.StartInfo.Arguments = "\"" + Properties.Settings.Default.ROMPath + "\"";
                process2.Start();
                Application.Exit();
            }
            catch(Exception ex)
            {
                MessageBox.Show("An unexpected exception has occured, restarting!\nDetails:\n" + ex, LanguageManager.Get("SpriteData", "ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Restart NSMBe
                Process process2 = new Process();
                process2.StartInfo.FileName = Application.ExecutablePath;
                process2.StartInfo.Arguments = "\"" + Properties.Settings.Default.ROMPath + "\"";
                process2.Start();
                Application.Exit();
            }
        }

        private void Using_sb_asm_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.using_signboard_asm = using_sb_asm_checkBox.Checked;
            Properties.Settings.Default.Save();

            SpriteData.Load();
        }
    }
}
