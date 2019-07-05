﻿namespace NSMBe5.DSFileSystem
{
    partial class FilesystemBrowser
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.selectionInfo = new System.Windows.Forms.Label();
            this.selectedFileInfo = new System.Windows.Forms.Label();
            this.decompressFileButton = new System.Windows.Forms.Button();
            this.compressFileButton = new System.Windows.Forms.Button();
            this.replaceFileButton = new System.Windows.Forms.Button();
            this.extractFileButton = new System.Windows.Forms.Button();
            this.fileTreeView = new System.Windows.Forms.TreeView();
            this.extractFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.replaceFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.hexEdButton = new System.Windows.Forms.Button();
            this.decompressOverlayButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.extractDirectoryDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // selectionInfo
            // 
            this.selectionInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.selectionInfo.AutoSize = true;
            this.selectionInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectionInfo.Location = new System.Drawing.Point(4, 5);
            this.selectionInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.selectionInfo.Name = "selectionInfo";
            this.selectionInfo.Size = new System.Drawing.Size(120, 17);
            this.selectionInfo.TabIndex = 14;
            this.selectionInfo.Text = "<SelectionInfo>";
            // 
            // selectedFileInfo
            // 
            this.selectedFileInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.selectedFileInfo.AutoSize = true;
            this.selectedFileInfo.Location = new System.Drawing.Point(132, 5);
            this.selectedFileInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.selectedFileInfo.Name = "selectedFileInfo";
            this.selectedFileInfo.Size = new System.Drawing.Size(18, 17);
            this.selectedFileInfo.TabIndex = 13;
            this.selectedFileInfo.Text = "--";
            // 
            // decompressFileButton
            // 
            this.decompressFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.decompressFileButton.Location = new System.Drawing.Point(547, 25);
            this.decompressFileButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.decompressFileButton.Name = "decompressFileButton";
            this.decompressFileButton.Size = new System.Drawing.Size(128, 28);
            this.decompressFileButton.TabIndex = 12;
            this.decompressFileButton.Text = "<decompressFileButton>";
            this.decompressFileButton.UseVisualStyleBackColor = true;
            this.decompressFileButton.Click += new System.EventHandler(this.decompressFileButton_Click);
            // 
            // compressFileButton
            // 
            this.compressFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.compressFileButton.Location = new System.Drawing.Point(436, 25);
            this.compressFileButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.compressFileButton.Name = "compressFileButton";
            this.compressFileButton.Size = new System.Drawing.Size(103, 28);
            this.compressFileButton.TabIndex = 11;
            this.compressFileButton.Text = "<compressFileButton>";
            this.compressFileButton.UseVisualStyleBackColor = true;
            this.compressFileButton.Click += new System.EventHandler(this.compressFileButton_Click);
            // 
            // replaceFileButton
            // 
            this.replaceFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.replaceFileButton.Location = new System.Drawing.Point(112, 25);
            this.replaceFileButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.replaceFileButton.Name = "replaceFileButton";
            this.replaceFileButton.Size = new System.Drawing.Size(100, 28);
            this.replaceFileButton.TabIndex = 10;
            this.replaceFileButton.Text = "<replaceFileButton>";
            this.replaceFileButton.UseVisualStyleBackColor = true;
            this.replaceFileButton.Click += new System.EventHandler(this.replaceFileButton_Click);
            // 
            // extractFileButton
            // 
            this.extractFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.extractFileButton.Location = new System.Drawing.Point(4, 25);
            this.extractFileButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.extractFileButton.Name = "extractFileButton";
            this.extractFileButton.Size = new System.Drawing.Size(100, 28);
            this.extractFileButton.TabIndex = 9;
            this.extractFileButton.Text = "<extractFileButton>";
            this.extractFileButton.UseVisualStyleBackColor = true;
            this.extractFileButton.Click += new System.EventHandler(this.extractFileButton_Click);
            // 
            // fileTreeView
            // 
            this.fileTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTreeView.Location = new System.Drawing.Point(0, 0);
            this.fileTreeView.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.fileTreeView.Name = "fileTreeView";
            this.fileTreeView.Size = new System.Drawing.Size(679, 288);
            this.fileTreeView.TabIndex = 8;
            this.fileTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.fileTreeView_ItemDrag);
            this.fileTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.fileTreeView_AfterSelect);
            this.fileTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.fileTreeView_NodeMouseDoubleClick);
            // 
            // hexEdButton
            // 
            this.hexEdButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.hexEdButton.Location = new System.Drawing.Point(325, 25);
            this.hexEdButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.hexEdButton.Name = "hexEdButton";
            this.hexEdButton.Size = new System.Drawing.Size(103, 28);
            this.hexEdButton.TabIndex = 11;
            this.hexEdButton.Text = "<hexEdButton>";
            this.hexEdButton.UseVisualStyleBackColor = true;
            this.hexEdButton.Click += new System.EventHandler(this.hexEdButton_Click);
            // 
            // decompressOverlayButton
            // 
            this.decompressOverlayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.decompressOverlayButton.Location = new System.Drawing.Point(27, 62);
            this.decompressOverlayButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.decompressOverlayButton.Name = "decompressOverlayButton";
            this.decompressOverlayButton.Size = new System.Drawing.Size(167, 28);
            this.decompressOverlayButton.TabIndex = 11;
            this.decompressOverlayButton.Text = "<Decompress overlay>";
            this.decompressOverlayButton.UseVisualStyleBackColor = true;
            this.decompressOverlayButton.Click += new System.EventHandler(this.decompressOverlayButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.selectionInfo);
            this.panel1.Controls.Add(this.extractFileButton);
            this.panel1.Controls.Add(this.selectedFileInfo);
            this.panel1.Controls.Add(this.replaceFileButton);
            this.panel1.Controls.Add(this.compressFileButton);
            this.panel1.Controls.Add(this.decompressFileButton);
            this.panel1.Controls.Add(this.hexEdButton);
            this.panel1.Controls.Add(this.decompressOverlayButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 288);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(679, 100);
            this.panel1.TabIndex = 15;
            // 
            // FilesystemBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fileTreeView);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FilesystemBrowser";
            this.Size = new System.Drawing.Size(679, 388);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label selectionInfo;
        private System.Windows.Forms.Label selectedFileInfo;
        private System.Windows.Forms.Button decompressFileButton;
        private System.Windows.Forms.Button compressFileButton;
        private System.Windows.Forms.Button replaceFileButton;
        private System.Windows.Forms.Button extractFileButton;
        private System.Windows.Forms.TreeView fileTreeView;
        private System.Windows.Forms.SaveFileDialog extractFileDialog;
        private System.Windows.Forms.OpenFileDialog replaceFileDialog;
        private System.Windows.Forms.Button hexEdButton;
        private System.Windows.Forms.Button decompressOverlayButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.FolderBrowserDialog extractDirectoryDialog;
    }
}
