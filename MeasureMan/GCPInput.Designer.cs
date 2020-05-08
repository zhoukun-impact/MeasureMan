namespace MeasureMan
{
    partial class GCPInput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GCPInput));
            this.label1 = new System.Windows.Forms.Label();
            this.txtLng = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLat = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtElevation = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btEnsure = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "经度";
            // 
            // txtLng
            // 
            this.txtLng.Location = new System.Drawing.Point(77, 28);
            this.txtLng.Name = "txtLng";
            this.txtLng.Size = new System.Drawing.Size(133, 21);
            this.txtLng.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(216, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "°";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(216, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "°";
            // 
            // txtLat
            // 
            this.txtLat.Location = new System.Drawing.Point(77, 81);
            this.txtLat.Name = "txtLat";
            this.txtLat.Size = new System.Drawing.Size(133, 21);
            this.txtLat.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(40, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "纬度";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(216, 143);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(11, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "m";
            // 
            // txtElevation
            // 
            this.txtElevation.Location = new System.Drawing.Point(77, 140);
            this.txtElevation.Name = "txtElevation";
            this.txtElevation.Size = new System.Drawing.Size(133, 21);
            this.txtElevation.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(40, 143);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 6;
            this.label6.Text = "高程";
            // 
            // btEnsure
            // 
            this.btEnsure.Location = new System.Drawing.Point(152, 192);
            this.btEnsure.Name = "btEnsure";
            this.btEnsure.Size = new System.Drawing.Size(75, 23);
            this.btEnsure.TabIndex = 9;
            this.btEnsure.Text = "确定";
            this.btEnsure.UseVisualStyleBackColor = true;
            this.btEnsure.Click += new System.EventHandler(this.btEnsure_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(42, 192);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 10;
            this.btCancel.Text = "取消";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // GCPInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 251);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btEnsure);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtElevation);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtLat);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtLng);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GCPInput";
            this.Text = "控制点坐标输入";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLng;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLat;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtElevation;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btEnsure;
        private System.Windows.Forms.Button btCancel;

    }
}