namespace Battleship.Client
{
    partial class ucLogin
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblLoginLogoTxt = new Label();
            boxPasswordTxt = new TextBox();
            boxUsernameTxt = new TextBox();
            llblRegister = new LinkLabel();
            btnLogin = new Button();
            lblErrorTxt = new Label();
            SuspendLayout();
            // 
            // lblLoginLogoTxt
            // 
            lblLoginLogoTxt.AutoSize = true;
            lblLoginLogoTxt.Font = new Font("Segoe UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblLoginLogoTxt.ForeColor = SystemColors.ActiveCaptionText;
            lblLoginLogoTxt.Location = new Point(159, 24);
            lblLoginLogoTxt.Name = "lblLoginLogoTxt";
            lblLoginLogoTxt.Size = new Size(183, 41);
            lblLoginLogoTxt.TabIndex = 0;
            lblLoginLogoTxt.Text = "USER LOGIN";
            // 
            // boxPasswordTxt
            // 
            boxPasswordTxt.BackColor = Color.White;
            boxPasswordTxt.BorderStyle = BorderStyle.FixedSingle;
            boxPasswordTxt.ForeColor = Color.FromArgb(2, 69, 121);
            boxPasswordTxt.Location = new Point(85, 189);
            boxPasswordTxt.Name = "boxPasswordTxt";
            boxPasswordTxt.PasswordChar = '*';
            boxPasswordTxt.Size = new Size(335, 31);
            boxPasswordTxt.TabIndex = 3;
            // 
            // boxUsernameTxt
            // 
            boxUsernameTxt.BackColor = Color.White;
            boxUsernameTxt.BorderStyle = BorderStyle.FixedSingle;
            boxUsernameTxt.ForeColor = Color.FromArgb(5, 98, 155);
            boxUsernameTxt.Location = new Point(85, 137);
            boxUsernameTxt.Name = "boxUsernameTxt";
            boxUsernameTxt.RightToLeft = RightToLeft.No;
            boxUsernameTxt.Size = new Size(335, 31);
            boxUsernameTxt.TabIndex = 4;
            // 
            // llblRegister
            // 
            llblRegister.AutoSize = true;
            llblRegister.Cursor = Cursors.IBeam;
            llblRegister.LinkBehavior = LinkBehavior.NeverUnderline;
            llblRegister.LinkColor = Color.FromArgb(69, 149, 102);
            llblRegister.Location = new Point(257, 243);
            llblRegister.Name = "llblRegister";
            llblRegister.Size = new Size(163, 25);
            llblRegister.TabIndex = 5;
            llblRegister.TabStop = true;
            llblRegister.Text = "Chưa có tài khoản?";
            llblRegister.LinkClicked += llblRegister_LinkClicked;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(236, 45, 51);
            btnLogin.BackgroundImageLayout = ImageLayout.Center;
            btnLogin.FlatStyle = FlatStyle.Popup;
            btnLogin.Font = new Font("Times New Roman", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(186, 305);
            btnLogin.Margin = new Padding(0);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(132, 46);
            btnLogin.TabIndex = 6;
            btnLogin.Text = "LOGIN";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // lblErrorTxt
            // 
            lblErrorTxt.ForeColor = Color.Red;
            lblErrorTxt.Location = new Point(0, 371);
            lblErrorTxt.Name = "lblErrorTxt";
            lblErrorTxt.Size = new Size(546, 35);
            lblErrorTxt.TabIndex = 7;
            lblErrorTxt.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ucLogin
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(lblErrorTxt);
            Controls.Add(btnLogin);
            Controls.Add(llblRegister);
            Controls.Add(boxUsernameTxt);
            Controls.Add(boxPasswordTxt);
            Controls.Add(lblLoginLogoTxt);
            Name = "ucLogin";
            Size = new Size(545, 406);
            Load += uclogin_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblLoginLogoTxt;
        private TextBox boxPasswordTxt;
        private TextBox boxUsernameTxt;
        private LinkLabel llblRegister;
        private Button btnLogin;
        private Label lblErrorTxt;
    }
}
