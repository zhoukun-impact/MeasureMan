namespace MeasureMan
{
    partial class CreateProject
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateProject));
            this.label1 = new System.Windows.Forms.Label();
            this.cbDataType = new System.Windows.Forms.ComboBox();
            this.btGetPath = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btCancel = new System.Windows.Forms.Button();
            this.btEnsure = new System.Windows.Forms.Button();
            this.cbFeatureType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "数据类型";
            // 
            // cbDataType
            // 
            this.cbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDataType.FormattingEnabled = true;
            this.cbDataType.Items.AddRange(new object[] {
            "图像数据",
            "视频数据"});
            this.cbDataType.Location = new System.Drawing.Point(99, 32);
            this.cbDataType.Name = "cbDataType";
            this.cbDataType.Size = new System.Drawing.Size(121, 20);
            this.cbDataType.TabIndex = 2;
            // 
            // btGetPath
            // 
            this.btGetPath.Location = new System.Drawing.Point(253, 137);
            this.btGetPath.Name = "btGetPath";
            this.btGetPath.Size = new System.Drawing.Size(31, 21);
            this.btGetPath.TabIndex = 4;
            this.btGetPath.Text = "...";
            this.btGetPath.UseVisualStyleBackColor = true;
            this.btGetPath.Click += new System.EventHandler(this.btGetPath_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 140);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "保存路径";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(99, 137);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(148, 21);
            this.txtPath.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(30, 195);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 7;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btEnsure
            // 
            this.btEnsure.Location = new System.Drawing.Point(209, 195);
            this.btEnsure.Name = "btEnsure";
            this.btEnsure.Size = new System.Drawing.Size(75, 23);
            this.btEnsure.TabIndex = 8;
            this.btEnsure.Text = "确定";
            this.btEnsure.UseVisualStyleBackColor = true;
            this.btEnsure.Click += new System.EventHandler(this.btEnsure_Click);
            // 
            // cbFeatureType
            // 
            this.cbFeatureType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFeatureType.FormattingEnabled = true;
            this.cbFeatureType.Items.AddRange(new object[] {
            "精细建模",
            "快速建模"});
            this.cbFeatureType.Location = new System.Drawing.Point(99, 83);
            this.cbFeatureType.Name = "cbFeatureType";
            this.cbFeatureType.Size = new System.Drawing.Size(121, 20);
            this.cbFeatureType.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "建模方式";
            // 
            // CreateProject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(310, 251);
            this.Controls.Add(this.cbFeatureType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btEnsure);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btGetPath);
            this.Controls.Add(this.cbDataType);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CreateProject";
            this.Text = "创建工程文件";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbDataType;
        private System.Windows.Forms.Button btGetPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btEnsure;
        private System.Windows.Forms.ComboBox cbFeatureType;
        private System.Windows.Forms.Label label2;
    }
}