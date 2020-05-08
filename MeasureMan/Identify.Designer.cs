namespace MeasureMan
{
    partial class Identify
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Identify));
            this.rtbAttribute = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPosition = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLayer = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // rtbAttribute
            // 
            this.rtbAttribute.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.rtbAttribute.Location = new System.Drawing.Point(0, 102);
            this.rtbAttribute.Name = "rtbAttribute";
            this.rtbAttribute.Size = new System.Drawing.Size(251, 161);
            this.rtbAttribute.TabIndex = 0;
            this.rtbAttribute.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "坐标位置";
            // 
            // txtPosition
            // 
            this.txtPosition.Location = new System.Drawing.Point(75, 62);
            this.txtPosition.Name = "txtPosition";
            this.txtPosition.Size = new System.Drawing.Size(166, 21);
            this.txtPosition.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "所属图层";
            // 
            // txtLayer
            // 
            this.txtLayer.Location = new System.Drawing.Point(75, 20);
            this.txtLayer.Name = "txtLayer";
            this.txtLayer.Size = new System.Drawing.Size(166, 21);
            this.txtLayer.TabIndex = 4;
            // 
            // Identify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(251, 263);
            this.Controls.Add(this.txtLayer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPosition);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rtbAttribute);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Identify";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "标识";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbAttribute;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPosition;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLayer;
    }
}