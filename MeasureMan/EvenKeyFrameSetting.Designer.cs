namespace MeasureMan
{
    partial class EvenKeyFrameSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EvenKeyFrameSetting));
            this.label11 = new System.Windows.Forms.Label();
            this.txtFps = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.btEnsure = new System.Windows.Forms.Button();
            this.btSkip = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtEndTime = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtStartTime = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtFrameInter = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFrameNumber = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtDuration = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbClipMode = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(235, 102);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 12);
            this.label11.TabIndex = 43;
            this.label11.Text = "帧/秒";
            // 
            // txtFps
            // 
            this.txtFps.Location = new System.Drawing.Point(129, 99);
            this.txtFps.Name = "txtFps";
            this.txtFps.Size = new System.Drawing.Size(100, 21);
            this.txtFps.TabIndex = 42;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(23, 102);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(53, 12);
            this.label10.TabIndex = 41;
            this.label10.Text = "视频帧率";
            // 
            // btEnsure
            // 
            this.btEnsure.Location = new System.Drawing.Point(196, 325);
            this.btEnsure.Name = "btEnsure";
            this.btEnsure.Size = new System.Drawing.Size(75, 23);
            this.btEnsure.TabIndex = 37;
            this.btEnsure.Text = "确认设置";
            this.btEnsure.UseVisualStyleBackColor = true;
            this.btEnsure.Click += new System.EventHandler(this.btEnsure_Click);
            // 
            // btSkip
            // 
            this.btSkip.Location = new System.Drawing.Point(25, 325);
            this.btSkip.Name = "btSkip";
            this.btSkip.Size = new System.Drawing.Size(75, 23);
            this.btSkip.TabIndex = 36;
            this.btSkip.Text = "跳过设置";
            this.btSkip.UseVisualStyleBackColor = true;
            this.btSkip.Click += new System.EventHandler(this.btSkip_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(233, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(17, 12);
            this.label8.TabIndex = 35;
            this.label8.Text = "秒";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(254, 278);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 34;
            this.label7.Text = "秒";
            // 
            // txtEndTime
            // 
            this.txtEndTime.Location = new System.Drawing.Point(129, 275);
            this.txtEndTime.Name = "txtEndTime";
            this.txtEndTime.Size = new System.Drawing.Size(119, 21);
            this.txtEndTime.TabIndex = 33;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(254, 233);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 32;
            this.label6.Text = "秒  到  ";
            // 
            // txtStartTime
            // 
            this.txtStartTime.Location = new System.Drawing.Point(129, 230);
            this.txtStartTime.Name = "txtStartTime";
            this.txtStartTime.Size = new System.Drawing.Size(119, 21);
            this.txtStartTime.TabIndex = 31;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 233);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 30;
            this.label5.Text = "视频截取时段";
            // 
            // txtFrameInter
            // 
            this.txtFrameInter.Location = new System.Drawing.Point(129, 138);
            this.txtFrameInter.Name = "txtFrameInter";
            this.txtFrameInter.Size = new System.Drawing.Size(100, 21);
            this.txtFrameInter.TabIndex = 29;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 28;
            this.label4.Text = "视频帧间隔";
            // 
            // txtFrameNumber
            // 
            this.txtFrameNumber.Location = new System.Drawing.Point(129, 60);
            this.txtFrameNumber.Name = "txtFrameNumber";
            this.txtFrameNumber.Size = new System.Drawing.Size(100, 21);
            this.txtFrameNumber.TabIndex = 27;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 26;
            this.label3.Text = "视频总帧数";
            // 
            // txtDuration
            // 
            this.txtDuration.Location = new System.Drawing.Point(129, 20);
            this.txtDuration.Name = "txtDuration";
            this.txtDuration.Size = new System.Drawing.Size(98, 21);
            this.txtDuration.TabIndex = 25;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 24;
            this.label2.Text = "视频总时长";
            // 
            // cbClipMode
            // 
            this.cbClipMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbClipMode.FormattingEnabled = true;
            this.cbClipMode.Items.AddRange(new object[] {
            "跳过视频段",
            "选择视频段"});
            this.cbClipMode.Location = new System.Drawing.Point(129, 181);
            this.cbClipMode.Name = "cbClipMode";
            this.cbClipMode.Size = new System.Drawing.Size(121, 20);
            this.cbClipMode.TabIndex = 23;
            this.cbClipMode.SelectedIndexChanged += new System.EventHandler(this.cbClipMode_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 184);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 22;
            this.label1.Text = "视频截取模式";
            // 
            // EvenKeyFrameSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 373);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txtFps);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btEnsure);
            this.Controls.Add(this.btSkip);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtEndTime);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtStartTime);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtFrameInter);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtFrameNumber);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtDuration);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbClipMode);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EvenKeyFrameSettings";
            this.Text = "等距关键帧提取设置";
            this.Load += new System.EventHandler(this.EvenKeyFrameSettings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtFps;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btEnsure;
        private System.Windows.Forms.Button btSkip;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtEndTime;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtStartTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFrameInter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtFrameNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtDuration;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbClipMode;
        private System.Windows.Forms.Label label1;
    }
}