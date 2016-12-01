namespace Microsoft.Samples.Kinect.HDFaceBasics
{
    partial class Form1
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
            this.nonverbal_webBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // nonverbal_webBrowser
            // 
            this.nonverbal_webBrowser.Location = new System.Drawing.Point(387, -1);
            this.nonverbal_webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.nonverbal_webBrowser.Name = "nonverbal_webBrowser";
            this.nonverbal_webBrowser.Size = new System.Drawing.Size(808, 300);
            this.nonverbal_webBrowser.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 704);
            this.Controls.Add(this.nonverbal_webBrowser);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser nonverbal_webBrowser;
    }
}