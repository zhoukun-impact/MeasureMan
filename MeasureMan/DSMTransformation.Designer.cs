namespace MeasureMan
{
    partial class DSMTransformation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DSMTransformation));
            this.txtGridSize = new System.Windows.Forms.TextBox();
            this.lbGridSize = new System.Windows.Forms.Label();
            this.btEnsure = new System.Windows.Forms.Button();
            this.plSettings = new System.Windows.Forms.Panel();
            this.cbxInterpol = new System.Windows.Forms.ComboBox();
            this.cbxWeightFunc = new System.Windows.Forms.ComboBox();
            this.txtNeighbor = new System.Windows.Forms.TextBox();
            this.txtSearchR = new System.Windows.Forms.TextBox();
            this.btCancel = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btExportPath = new System.Windows.Forms.Button();
            this.txtExportPath = new System.Windows.Forms.TextBox();
            this.plSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtGridSize
            // 
            this.txtGridSize.Location = new System.Drawing.Point(100, 14);
            this.txtGridSize.Name = "txtGridSize";
            this.txtGridSize.Size = new System.Drawing.Size(91, 21);
            this.txtGridSize.TabIndex = 15;
            // 
            // lbGridSize
            // 
            this.lbGridSize.AutoSize = true;
            this.lbGridSize.Location = new System.Drawing.Point(15, 20);
            this.lbGridSize.Name = "lbGridSize";
            this.lbGridSize.Size = new System.Drawing.Size(83, 12);
            this.lbGridSize.TabIndex = 14;
            this.lbGridSize.Text = "像元大小(m*m)";
            // 
            // btEnsure
            // 
            this.btEnsure.Location = new System.Drawing.Point(350, 77);
            this.btEnsure.Name = "btEnsure";
            this.btEnsure.Size = new System.Drawing.Size(64, 23);
            this.btEnsure.TabIndex = 13;
            this.btEnsure.Text = "执行";
            this.btEnsure.UseVisualStyleBackColor = true;
            this.btEnsure.Click += new System.EventHandler(this.btEnsure_Click);
            // 
            // plSettings
            // 
            this.plSettings.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.plSettings.Controls.Add(this.cbxInterpol);
            this.plSettings.Controls.Add(this.cbxWeightFunc);
            this.plSettings.Controls.Add(this.txtNeighbor);
            this.plSettings.Controls.Add(this.txtSearchR);
            this.plSettings.Controls.Add(this.btCancel);
            this.plSettings.Controls.Add(this.label4);
            this.plSettings.Controls.Add(this.btEnsure);
            this.plSettings.Controls.Add(this.label3);
            this.plSettings.Controls.Add(this.label2);
            this.plSettings.Controls.Add(this.label1);
            this.plSettings.Controls.Add(this.lbGridSize);
            this.plSettings.Controls.Add(this.txtGridSize);
            this.plSettings.Location = new System.Drawing.Point(9, 53);
            this.plSettings.Margin = new System.Windows.Forms.Padding(2);
            this.plSettings.Name = "plSettings";
            this.plSettings.Size = new System.Drawing.Size(442, 117);
            this.plSettings.TabIndex = 22;
            // 
            // cbxInterpol
            // 
            this.cbxInterpol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxInterpol.FormattingEnabled = true;
            this.cbxInterpol.Items.AddRange(new object[] {
            "snap",
            "nearestNeighbour",
            "delaunayTriangulation",
            "movingAverage",
            "movingPlanes",
            "robMovingPlanes",
            "movingParaboloid",
            "naturalNeighbour"});
            this.cbxInterpol.Location = new System.Drawing.Point(290, 17);
            this.cbxInterpol.Margin = new System.Windows.Forms.Padding(2);
            this.cbxInterpol.Name = "cbxInterpol";
            this.cbxInterpol.Size = new System.Drawing.Size(124, 20);
            this.cbxInterpol.TabIndex = 27;
            this.cbxInterpol.SelectedIndexChanged += new System.EventHandler(this.cbxInterpol_SelectedIndexChanged);
            // 
            // cbxWeightFunc
            // 
            this.cbxWeightFunc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxWeightFunc.FormattingEnabled = true;
            this.cbxWeightFunc.Items.AddRange(new object[] {
            "不使用",
            "IDW1",
            "IDW2",
            "IDW3"});
            this.cbxWeightFunc.Location = new System.Drawing.Point(322, 49);
            this.cbxWeightFunc.Margin = new System.Windows.Forms.Padding(2);
            this.cbxWeightFunc.Name = "cbxWeightFunc";
            this.cbxWeightFunc.Size = new System.Drawing.Size(92, 20);
            this.cbxWeightFunc.TabIndex = 26;
            // 
            // txtNeighbor
            // 
            this.txtNeighbor.Location = new System.Drawing.Point(100, 80);
            this.txtNeighbor.Name = "txtNeighbor";
            this.txtNeighbor.Size = new System.Drawing.Size(91, 21);
            this.txtNeighbor.TabIndex = 25;
            // 
            // txtSearchR
            // 
            this.txtSearchR.Location = new System.Drawing.Point(101, 49);
            this.txtSearchR.Name = "txtSearchR";
            this.txtSearchR.Size = new System.Drawing.Size(91, 21);
            this.txtSearchR.TabIndex = 24;
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(228, 78);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(64, 23);
            this.btCancel.TabIndex = 23;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(226, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 19;
            this.label4.Text = "反距离加权函数";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(226, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 18;
            this.label3.Text = "插值方法";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(15, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 17;
            this.label2.Text = "邻域大小";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 16;
            this.label1.Text = "搜索半径";
            // 
            // btExportPath
            // 
            this.btExportPath.Location = new System.Drawing.Point(9, 17);
            this.btExportPath.Name = "btExportPath";
            this.btExportPath.Size = new System.Drawing.Size(92, 21);
            this.btExportPath.TabIndex = 24;
            this.btExportPath.Text = "输出DSM路径";
            this.btExportPath.UseVisualStyleBackColor = true;
            this.btExportPath.Click += new System.EventHandler(this.btExportPath_Click);
            // 
            // txtExportPath
            // 
            this.txtExportPath.Location = new System.Drawing.Point(106, 19);
            this.txtExportPath.Name = "txtExportPath";
            this.txtExportPath.ReadOnly = true;
            this.txtExportPath.Size = new System.Drawing.Size(345, 21);
            this.txtExportPath.TabIndex = 23;
            // 
            // DSMTransformation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 187);
            this.Controls.Add(this.btExportPath);
            this.Controls.Add(this.txtExportPath);
            this.Controls.Add(this.plSettings);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DSMTransformation";
            this.Text = "DSM转换";
            this.plSettings.ResumeLayout(false);
            this.plSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtGridSize;
        private System.Windows.Forms.Label lbGridSize;
        private System.Windows.Forms.Button btEnsure;
        private System.Windows.Forms.Panel plSettings;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbxInterpol;
        private System.Windows.Forms.ComboBox cbxWeightFunc;
        private System.Windows.Forms.TextBox txtNeighbor;
        private System.Windows.Forms.TextBox txtSearchR;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btExportPath;
        private System.Windows.Forms.TextBox txtExportPath;
    }
}