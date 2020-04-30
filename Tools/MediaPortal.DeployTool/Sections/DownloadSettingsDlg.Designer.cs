namespace MediaPortal.DeployTool.Sections
{
  partial class DownloadSettingsDlg
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
      this.rb32bit = new System.Windows.Forms.Label();
      this.rb64bit = new System.Windows.Forms.Label();
      this.listViewLang = new MediaPortal.DeployTool.MPListView();
      this.columnCountry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnLang3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.b32bit = new System.Windows.Forms.Button();
      this.b64bit = new System.Windows.Forms.Button();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // labelSectionHeader
      // 
      this.labelSectionHeader.Location = new System.Drawing.Point(330, 31);
      this.labelSectionHeader.MaximumSize = new System.Drawing.Size(400, 0);
      this.labelSectionHeader.MinimumSize = new System.Drawing.Size(400, 0);
      this.labelSectionHeader.Size = new System.Drawing.Size(400, 17);
      this.labelSectionHeader.Text = "Please select settings for downloads:";
      // 
      // rb32bit
      // 
      this.rb32bit.AutoSize = true;
      this.rb32bit.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rb32bit.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rb32bit.ForeColor = System.Drawing.Color.White;
      this.rb32bit.Location = new System.Drawing.Point(387, 392);
      this.rb32bit.Name = "rb32bit";
      this.rb32bit.Size = new System.Drawing.Size(70, 13);
      this.rb32bit.TabIndex = 13;
      this.rb32bit.Text = "32bit (x86)";
      this.rb32bit.Click += new System.EventHandler(this.b32bit_Click);
      // 
      // rb64bit
      // 
      this.rb64bit.AutoSize = true;
      this.rb64bit.Cursor = System.Windows.Forms.Cursors.Hand;
      this.rb64bit.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.rb64bit.ForeColor = System.Drawing.Color.White;
      this.rb64bit.Location = new System.Drawing.Point(582, 392);
      this.rb64bit.Name = "rb64bit";
      this.rb64bit.Size = new System.Drawing.Size(70, 13);
      this.rb64bit.TabIndex = 14;
      this.rb64bit.Text = "64bit (x64)";
      this.rb64bit.Click += new System.EventHandler(this.b64bit_Click);
      // 
      // listViewLang
      // 
      this.listViewLang.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(111)))), ((int)(((byte)(152)))));
      this.listViewLang.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnCountry,
            this.columnID,
            this.columnLang3});
      this.listViewLang.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.listViewLang.ForeColor = System.Drawing.Color.White;
      this.listViewLang.FullRowSelect = true;
      this.listViewLang.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewLang.HideSelection = false;
      this.listViewLang.Location = new System.Drawing.Point(333, 62);
      this.listViewLang.MultiSelect = false;
      this.listViewLang.Name = "listViewLang";
      this.listViewLang.Size = new System.Drawing.Size(603, 309);
      this.listViewLang.Sorting = System.Windows.Forms.SortOrder.Ascending;
      this.listViewLang.TabIndex = 1;
      this.listViewLang.UseCompatibleStateImageBehavior = false;
      this.listViewLang.View = System.Windows.Forms.View.Details;
      // 
      // columnCountry
      // 
      this.columnCountry.Text = "Country";
      this.columnCountry.Width = 63;
      // 
      // columnID
      // 
      this.columnID.Text = "ID";
      // 
      // columnLang3
      // 
      this.columnLang3.Text = "Code";
      this.columnLang3.Width = 102;
      // 
      // b32bit
      // 
      this.b32bit.FlatAppearance.BorderSize = 0;
      this.b32bit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.b32bit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.b32bit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.b32bit.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.b32bit.Location = new System.Drawing.Point(344, 387);
      this.b32bit.Name = "b32bit";
      this.b32bit.Size = new System.Drawing.Size(37, 23);
      this.b32bit.TabIndex = 2;
      this.b32bit.UseVisualStyleBackColor = true;
      this.b32bit.Click += new System.EventHandler(this.b32bit_Click);
      // 
      // b64bit
      // 
      this.b64bit.FlatAppearance.BorderSize = 0;
      this.b64bit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
      this.b64bit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
      this.b64bit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.b64bit.Image = global::MediaPortal.DeployTool.Images.Choose_button_off;
      this.b64bit.Location = new System.Drawing.Point(539, 387);
      this.b64bit.Name = "b64bit";
      this.b64bit.Size = new System.Drawing.Size(37, 23);
      this.b64bit.TabIndex = 3;
      this.b64bit.UseVisualStyleBackColor = true;
      this.b64bit.Click += new System.EventHandler(this.b64bit_Click);
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.DeployTool.Images.Internet_connection;
      this.pictureBox1.Location = new System.Drawing.Point(3, 62);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(324, 309);
      this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox1.TabIndex = 22;
      this.pictureBox1.TabStop = false;
      // 
      // DownloadSettingsDlg
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.BackgroundImage = global::MediaPortal.DeployTool.Images.Background_middle_empty;
      this.Controls.Add(this.pictureBox1);
      this.Controls.Add(this.b64bit);
      this.Controls.Add(this.b32bit);
      this.Controls.Add(this.listViewLang);
      this.Controls.Add(this.rb64bit);
      this.Controls.Add(this.rb32bit);
      this.Name = "DownloadSettingsDlg";
      this.Controls.SetChildIndex(this.rb32bit, 0);
      this.Controls.SetChildIndex(this.rb64bit, 0);
      this.Controls.SetChildIndex(this.listViewLang, 0);
      this.Controls.SetChildIndex(this.labelSectionHeader, 0);
      this.Controls.SetChildIndex(this.b32bit, 0);
      this.Controls.SetChildIndex(this.b64bit, 0);
      this.Controls.SetChildIndex(this.pictureBox1, 0);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label rb32bit;
    private System.Windows.Forms.Label rb64bit;
    private MPListView listViewLang;
    private System.Windows.Forms.ColumnHeader columnCountry;
    private System.Windows.Forms.ColumnHeader columnID;
    private System.Windows.Forms.ColumnHeader columnLang3;
    private System.Windows.Forms.Button b32bit;
    private System.Windows.Forms.Button b64bit;
    private System.Windows.Forms.PictureBox pictureBox1;
  }
}
