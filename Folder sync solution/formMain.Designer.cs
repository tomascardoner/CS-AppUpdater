
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
            this.textboxStatus = new System.Windows.Forms.TextBox();
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
            // textboxStatus
            // 
            this.textboxStatus.Location = new System.Drawing.Point(108, 12);
            this.textboxStatus.MaxLength = 0;
            this.textboxStatus.Multiline = true;
            this.textboxStatus.Name = "textboxStatus";
            this.textboxStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textboxStatus.Size = new System.Drawing.Size(364, 97);
            this.textboxStatus.TabIndex = 1;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(484, 123);
            this.Controls.Add(this.textboxStatus);
            this.Controls.Add(this.pictureboxCompanyLogo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
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
        private System.Windows.Forms.TextBox textboxStatus;
    }
}

