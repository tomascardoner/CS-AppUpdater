
namespace CSAppUpdater
{
    partial class formMain
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
            this.pictureboxCompanyLogo = new System.Windows.Forms.PictureBox();
            this.textboxLog = new System.Windows.Forms.TextBox();
            this.progressbarStatus = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureboxCompanyLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureboxCompanyLogo
            // 
            this.pictureboxCompanyLogo.Image = global::CSAppUpdater.Properties.Resources.IsotipoCardonerSistemas;
            this.pictureboxCompanyLogo.Location = new System.Drawing.Point(12, 12);
            this.pictureboxCompanyLogo.Name = "pictureboxCompanyLogo";
            this.pictureboxCompanyLogo.Size = new System.Drawing.Size(90, 97);
            this.pictureboxCompanyLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureboxCompanyLogo.TabIndex = 0;
            this.pictureboxCompanyLogo.TabStop = false;
            // 
            // textboxLog
            // 
            this.textboxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxLog.Location = new System.Drawing.Point(108, 12);
            this.textboxLog.MaxLength = 0;
            this.textboxLog.Multiline = true;
            this.textboxLog.Name = "textboxLog";
            this.textboxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textboxLog.Size = new System.Drawing.Size(464, 97);
            this.textboxLog.TabIndex = 1;
            this.textboxLog.Visible = false;
            // 
            // progressbarStatus
            // 
            this.progressbarStatus.Location = new System.Drawing.Point(108, 39);
            this.progressbarStatus.Name = "progressbarStatus";
            this.progressbarStatus.Size = new System.Drawing.Size(464, 26);
            this.progressbarStatus.TabIndex = 2;
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(108, 69);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(464, 31);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(584, 123);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.progressbarStatus);
            this.Controls.Add(this.textboxLog);
            this.Controls.Add(this.pictureboxCompanyLogo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Title";
            this.Shown += new System.EventHandler(this.formMain_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureboxCompanyLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureboxCompanyLogo;
        private System.Windows.Forms.TextBox textboxLog;
        private System.Windows.Forms.ProgressBar progressbarStatus;
        private System.Windows.Forms.Label labelStatus;
    }
}

