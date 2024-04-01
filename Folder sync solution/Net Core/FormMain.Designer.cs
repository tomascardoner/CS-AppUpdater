namespace CSAppUpdater
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            PictureBoxCompanyLogo = new PictureBox();
            TextBoxLog = new TextBox();
            ProgressBarStatus = new ProgressBar();
            LabelStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)PictureBoxCompanyLogo).BeginInit();
            SuspendLayout();
            // 
            // PictureBoxCompanyLogo
            // 
            PictureBoxCompanyLogo.Image = Properties.Resources.IsotipoCardonerSistemas;
            PictureBoxCompanyLogo.Location = new Point(12, 12);
            PictureBoxCompanyLogo.Name = "PictureBoxCompanyLogo";
            PictureBoxCompanyLogo.Size = new Size(90, 97);
            PictureBoxCompanyLogo.SizeMode = PictureBoxSizeMode.Zoom;
            PictureBoxCompanyLogo.TabIndex = 0;
            PictureBoxCompanyLogo.TabStop = false;
            // 
            // TextBoxLog
            // 
            TextBoxLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TextBoxLog.Location = new Point(108, 12);
            TextBoxLog.MaxLength = 0;
            TextBoxLog.Multiline = true;
            TextBoxLog.Name = "TextBoxLog";
            TextBoxLog.ScrollBars = ScrollBars.Vertical;
            TextBoxLog.Size = new Size(464, 97);
            TextBoxLog.TabIndex = 1;
            TextBoxLog.Visible = false;
            // 
            // ProgressBarStatus
            // 
            ProgressBarStatus.Location = new Point(108, 39);
            ProgressBarStatus.Name = "ProgressBarStatus";
            ProgressBarStatus.Size = new Size(464, 26);
            ProgressBarStatus.TabIndex = 2;
            // 
            // LabelStatus
            // 
            LabelStatus.Location = new Point(108, 69);
            LabelStatus.Name = "LabelStatus";
            LabelStatus.Size = new Size(464, 31);
            LabelStatus.TabIndex = 3;
            LabelStatus.TextAlign = ContentAlignment.TopCenter;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(584, 123);
            Controls.Add(LabelStatus);
            Controls.Add(ProgressBarStatus);
            Controls.Add(TextBoxLog);
            Controls.Add(PictureBoxCompanyLogo);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Title";
            Shown += This_Shown;
            ((System.ComponentModel.ISupportInitialize)PictureBoxCompanyLogo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox PictureBoxCompanyLogo;
        private TextBox TextBoxLog;
        private ProgressBar ProgressBarStatus;
        private Label LabelStatus;
    }
}
