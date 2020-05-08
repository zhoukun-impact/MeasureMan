namespace MeasureMan
{
    partial class AboutAuthor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutAuthor));
            this.rtbAuthors = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rtbAuthors
            // 
            this.rtbAuthors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbAuthors.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rtbAuthors.Location = new System.Drawing.Point(0, 0);
            this.rtbAuthors.Name = "rtbAuthors";
            this.rtbAuthors.ReadOnly = true;
            this.rtbAuthors.Size = new System.Drawing.Size(284, 213);
            this.rtbAuthors.TabIndex = 0;
            this.rtbAuthors.Text = "";
            // 
            // AboutAuthors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 213);
            this.Controls.Add(this.rtbAuthors);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AboutAuthors";
            this.Text = "关于作者";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbAuthors;
    }
}