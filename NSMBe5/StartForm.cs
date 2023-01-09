﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NSMBe5.DSFileSystem;

namespace NSMBe5
{
    public partial class StartForm : Form
    {
        bool close = true;

        public StartForm()
        {
            InitializeComponent();

            LanguageManager.ApplyToContainer(this, "StartForm");

            this.Icon = Properties.Resources.nsmbe;

            Program.ApplyFontToControls(Controls);
        }

        private void openRomButton_Click(object sender, EventArgs e)
        {

            string path = "";
            string[] backups = null;

            if (Properties.Settings.Default.BackupFiles != "" &&
                MessageBox.Show(LanguageManager.Get("StartForm", "OpenBackups"), LanguageManager.Get("StartForm", "OpenBackupsTitle"), MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                backups = Properties.Settings.Default.BackupFiles.Split(';');
                path = backups[0];
            }
            else
            {
                OpenFileDialog openROMDialog = new OpenFileDialog();
                openROMDialog.Filter = LanguageManager.Get("Filters", "rom");
                if (Properties.Settings.Default.ROMFolder != "")
                    openROMDialog.InitialDirectory = Properties.Settings.Default.ROMFolder;
                if (openROMDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    path = openROMDialog.FileName;
            }

            if (path == "")
                return;

            try
            {
                NitroROMFilesystem fs = new NitroROMFilesystem(path);
                Properties.Settings.Default.ROMPath = path;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.ROMFolder = System.IO.Path.GetDirectoryName(path);
                Properties.Settings.Default.Save();

                if (backups != null)
                    for (int l = 1; l < backups.Length; l++)
                        ROM.fileBackups.Add(backups[l]);

                run(fs);

                //DEBUG PRINT SHIT
                /*foreach(File file in fs.allFiles)
                {
                    Directory lastDir = file.parentDir;
                    while (true)
                    {
                        if(lastDir != null)
                        {
                            Console.Write("/" + lastDir.name);
                        }
                        lastDir = lastDir.parentDir;
                        break;
                    }
                    Console.WriteLine("/" + file.name + " = " + (file.id - 131) + ",");
                }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            NetFilesystem fs = new NetFilesystem(hostTextBox.Text, Int32.Parse(portTextBox.Text));
            run(fs);
        }

        private void run(Filesystem fs)
        {
            ROM.load(fs);

            SpriteData.Load();
            new LevelChooser().Show();

            close = false;
            Close();
        }

        private void StartForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(close)
                Application.Exit();
        }

    }
}
