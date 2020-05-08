namespace MeasureMan
{
    partial class RasterRenderer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RasterRenderer));
            this.label1 = new System.Windows.Forms.Label();
            this.txtLayerName = new System.Windows.Forms.TextBox();
            this.cbSelectedBand = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtNumber = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btRender = new System.Windows.Forms.Button();
            this.axSymbologyControl1 = new ESRI.ArcGIS.Controls.AxSymbologyControl();
            this.cbColorRamp = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbMethod = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.axSymbologyControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "图层名称";
            // 
            // txtLayerName
            // 
            this.txtLayerName.Location = new System.Drawing.Point(106, 25);
            this.txtLayerName.Name = "txtLayerName";
            this.txtLayerName.Size = new System.Drawing.Size(143, 21);
            this.txtLayerName.TabIndex = 2;
            // 
            // cbSelectedBand
            // 
            this.cbSelectedBand.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSelectedBand.FormattingEnabled = true;
            this.cbSelectedBand.Location = new System.Drawing.Point(106, 74);
            this.cbSelectedBand.Name = "cbSelectedBand";
            this.cbSelectedBand.Size = new System.Drawing.Size(143, 20);
            this.cbSelectedBand.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "波段选择";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 133);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "分类数量";
            // 
            // txtNumber
            // 
            this.txtNumber.Location = new System.Drawing.Point(106, 130);
            this.txtNumber.Name = "txtNumber";
            this.txtNumber.Size = new System.Drawing.Size(143, 21);
            this.txtNumber.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(35, 188);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "色带选择";
            // 
            // btRender
            // 
            this.btRender.Location = new System.Drawing.Point(174, 290);
            this.btRender.Name = "btRender";
            this.btRender.Size = new System.Drawing.Size(75, 23);
            this.btRender.TabIndex = 8;
            this.btRender.Text = "渲染";
            this.btRender.UseVisualStyleBackColor = true;
            this.btRender.Click += new System.EventHandler(this.btRender_Click);
            // 
            // axSymbologyControl1
            // 
            this.axSymbologyControl1.Location = new System.Drawing.Point(12, 256);
            this.axSymbologyControl1.Name = "axSymbologyControl1";
            this.axSymbologyControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axSymbologyControl1.OcxState")));
            this.axSymbologyControl1.Size = new System.Drawing.Size(16, 17);
            this.axSymbologyControl1.TabIndex = 9;
            // 
            // cbColorRamp
            // 
            this.cbColorRamp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbColorRamp.FormattingEnabled = true;
            this.cbColorRamp.Location = new System.Drawing.Point(106, 185);
            this.cbColorRamp.Name = "cbColorRamp";
            this.cbColorRamp.Size = new System.Drawing.Size(143, 20);
            this.cbColorRamp.TabIndex = 11;
            this.cbColorRamp.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cbColorRamp_DrawItem);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(35, 242);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "分类方式";
            // 
            // cbMethod
            // 
            this.cbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMethod.FormattingEnabled = true;
            this.cbMethod.Items.AddRange(new object[] {
            "等间隔分级法",
            "自然间断点分级法",
            "几何间隔分级法",
            "分位数分级法"});
            this.cbMethod.Location = new System.Drawing.Point(106, 239);
            this.cbMethod.Name = "cbMethod";
            this.cbMethod.Size = new System.Drawing.Size(143, 20);
            this.cbMethod.TabIndex = 13;
            // 
            // RasterRenderer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 332);
            this.Controls.Add(this.cbMethod);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbColorRamp);
            this.Controls.Add(this.axSymbologyControl1);
            this.Controls.Add(this.btRender);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtNumber);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbSelectedBand);
            this.Controls.Add(this.txtLayerName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RasterRenderer";
            this.Text = "栅格渲染";
            this.Load += new System.EventHandler(this.RasterRenderer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axSymbologyControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLayerName;
        private System.Windows.Forms.ComboBox cbSelectedBand;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtNumber;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btRender;
        private ESRI.ArcGIS.Controls.AxSymbologyControl axSymbologyControl1;
        private System.Windows.Forms.ComboBox cbColorRamp;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbMethod;
    }
}