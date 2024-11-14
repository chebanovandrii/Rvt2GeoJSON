
namespace Architexor.Forms.GeoJSON
{
	partial class FrmExport
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.Label label1;
			System.Windows.Forms.Label label2;
			System.Windows.Forms.Label label3;
			System.Windows.Forms.Label label4;
			this.txtLongitude = new System.Windows.Forms.TextBox();
			this.txtLatitude = new System.Windows.Forms.TextBox();
			this.txtAngle = new System.Windows.Forms.TextBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.txtSavePath = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.treeViewCategories = new System.Windows.Forms.TreeView();
			this.comboBoxWithCheckBox = new System.Windows.Forms.ComboBox();
			label1 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new System.Drawing.Point(12, 9);
			label1.Name = "label1";
			label1.Size = new System.Drawing.Size(57, 13);
			label1.TabIndex = 0;
			label1.Text = "Longitude:";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(212, 10);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(48, 13);
			label2.TabIndex = 2;
			label2.Text = "Latitude:";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(12, 36);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(37, 13);
			label3.TabIndex = 4;
			label3.Text = "Angle:";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(12, 570);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(51, 13);
			label4.TabIndex = 8;
			label4.Text = "Location:";
			// 
			// txtLongitude
			// 
			this.txtLongitude.Location = new System.Drawing.Point(75, 7);
			this.txtLongitude.Name = "txtLongitude";
			this.txtLongitude.Size = new System.Drawing.Size(122, 20);
			this.txtLongitude.TabIndex = 1;
			this.txtLongitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// txtLatitude
			// 
			this.txtLatitude.Location = new System.Drawing.Point(275, 8);
			this.txtLatitude.Name = "txtLatitude";
			this.txtLatitude.Size = new System.Drawing.Size(122, 20);
			this.txtLatitude.TabIndex = 3;
			this.txtLatitude.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// txtAngle
			// 
			this.txtAngle.Location = new System.Drawing.Point(75, 34);
			this.txtAngle.Name = "txtAngle";
			this.txtAngle.Size = new System.Drawing.Size(122, 20);
			this.txtAngle.TabIndex = 5;
			this.txtAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// btnOK
			// 
			this.btnOK.Location = new System.Drawing.Point(241, 594);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(75, 23);
			this.btnOK.TabIndex = 6;
			this.btnOK.Text = "Export";
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(322, 594);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 7;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// txtSavePath
			// 
			this.txtSavePath.Location = new System.Drawing.Point(75, 568);
			this.txtSavePath.Name = "txtSavePath";
			this.txtSavePath.ReadOnly = true;
			this.txtSavePath.Size = new System.Drawing.Size(241, 20);
			this.txtSavePath.TabIndex = 9;
			this.txtSavePath.Text = "D:\\";
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(322, 566);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(75, 23);
			this.btnBrowse.TabIndex = 10;
			this.btnBrowse.Text = "Browse";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// treeViewCategories
			// 
			this.treeViewCategories.CheckBoxes = true;
			this.treeViewCategories.Location = new System.Drawing.Point(9, 60);
			this.treeViewCategories.Name = "treeViewCategories";
			this.treeViewCategories.Size = new System.Drawing.Size(387, 500);
			this.treeViewCategories.TabIndex = 11;
			this.treeViewCategories.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewCategories_AfterCheck);
			// 
			// comboBoxWithCheckBox
			// 
			this.comboBoxWithCheckBox.FormattingEnabled = true;
			this.comboBoxWithCheckBox.Location = new System.Drawing.Point(9, 60);
			this.comboBoxWithCheckBox.Name = "comboBoxWithCheckBox";
			this.comboBoxWithCheckBox.Size = new System.Drawing.Size(167, 21);
			this.comboBoxWithCheckBox.TabIndex = 12;
			this.comboBoxWithCheckBox.Visible = false;
			// 
			// FrmExport
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(403, 623);
			this.Controls.Add(this.comboBoxWithCheckBox);
			this.Controls.Add(this.treeViewCategories);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.txtSavePath);
			this.Controls.Add(label4);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.txtAngle);
			this.Controls.Add(label3);
			this.Controls.Add(this.txtLatitude);
			this.Controls.Add(label2);
			this.Controls.Add(this.txtLongitude);
			this.Controls.Add(label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FrmExport";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Export";
			this.Load += new System.EventHandler(this.FrmExport_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtLongitude;
		private System.Windows.Forms.TextBox txtLatitude;
		private System.Windows.Forms.TextBox txtAngle;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TextBox txtSavePath;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.TreeView treeViewCategories;
		private System.Windows.Forms.ComboBox comboBoxWithCheckBox;
	}
}

