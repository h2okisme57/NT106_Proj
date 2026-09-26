namespace Battleship.Client
{
    partial class ucRegister
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
            llblLogin = new LinkLabel();
            boxUsername = new TextBox();
            boxPassword = new TextBox();
            lblPasswordTxt = new Label();
            lblUsernameTxt = new Label();
            lblRegisterLogoTxt = new Label();
            btnRegister = new Button();
            boxPasswordCofirm = new TextBox();
            lblPasswordConfirmtxt = new Label();
            lblErrorTxt = new Label();
            SuspendLayout();
            // 
            // llblLogin
            // 
            llblLogin.AutoSize = true;
            llblLogin.Location = new Point(405, 562);
            llblLogin.Name = "llblLogin";
            llblLogin.Size = new Size(237, 25);
            llblLogin.TabIndex = 11;
            llblLogin.TabStop = true;
            llblLogin.Text = "Đã có tài khoản? Đăng nhập";
            llblLogin.LinkClicked += llblLogin_LinkClicked;
            // 
            // boxUsername
            // 
            boxUsername.Location = new Point(481, 217);
            boxUsername.Name = "boxUsername";
            boxUsername.Size = new Size(226, 31);
            boxUsername.TabIndex = 10;
            // 
            // boxPassword
            // 
            boxPassword.Location = new Point(481, 298);
            boxPassword.Name = "boxPassword";
            boxPassword.PasswordChar = '*';
            boxPassword.Size = new Size(226, 31);
            boxPassword.TabIndex = 9;
            boxPassword.TextChanged += boxPassword_TextChanged;
            // 
            // lblPasswordTxt
            // 
            lblPasswordTxt.AutoSize = true;
            lblPasswordTxt.Location = new Point(319, 298);
            lblPasswordTxt.Name = "lblPasswordTxt";
            lblPasswordTxt.Size = new Size(87, 25);
            lblPasswordTxt.TabIndex = 8;
            lblPasswordTxt.Text = "Password";
            // 
            // lblUsernameTxt
            // 
            lblUsernameTxt.AutoSize = true;
            lblUsernameTxt.Location = new Point(319, 214);
            lblUsernameTxt.Name = "lblUsernameTxt";
            lblUsernameTxt.Size = new Size(91, 25);
            lblUsernameTxt.TabIndex = 7;
            lblUsernameTxt.Text = "Username";
            // 
            // lblRegisterLogoTxt
            // 
            lblRegisterLogoTxt.AutoSize = true;
            lblRegisterLogoTxt.Location = new Point(468, 136);
            lblRegisterLogoTxt.Name = "lblRegisterLogoTxt";
            lblRegisterLogoTxt.Size = new Size(79, 25);
            lblRegisterLogoTxt.TabIndex = 6;
            lblRegisterLogoTxt.Text = "SIGN UP";
            // 
            // btnRegister
            // 
            btnRegister.Location = new Point(468, 455);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(112, 34);
            btnRegister.TabIndex = 12;
            btnRegister.Text = "Sign up";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            // 
            // boxPasswordCofirm
            // 
            boxPasswordCofirm.Location = new Point(481, 371);
            boxPasswordCofirm.Name = "boxPasswordCofirm";
            boxPasswordCofirm.PasswordChar = '*';
            boxPasswordCofirm.Size = new Size(226, 31);
            boxPasswordCofirm.TabIndex = 14;
            // 
            // lblPasswordConfirmtxt
            // 
            lblPasswordConfirmtxt.AutoSize = true;
            lblPasswordConfirmtxt.Location = new Point(319, 371);
            lblPasswordConfirmtxt.Name = "lblPasswordConfirmtxt";
            lblPasswordConfirmtxt.Size = new Size(156, 25);
            lblPasswordConfirmtxt.TabIndex = 13;
            lblPasswordConfirmtxt.Text = "Password Confirm";
            // 
            // lblErrorTxt
            // 
            lblErrorTxt.ForeColor = Color.Red;
            lblErrorTxt.Location = new Point(142, 514);
            lblErrorTxt.Name = "lblErrorTxt";
            lblErrorTxt.Size = new Size(702, 25);
            lblErrorTxt.TabIndex = 16;
            lblErrorTxt.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ucRegister
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblErrorTxt);
            Controls.Add(boxPasswordCofirm);
            Controls.Add(lblPasswordConfirmtxt);
            Controls.Add(btnRegister);
            Controls.Add(llblLogin);
            Controls.Add(boxUsername);
            Controls.Add(boxPassword);
            Controls.Add(lblPasswordTxt);
            Controls.Add(lblUsernameTxt);
            Controls.Add(lblRegisterLogoTxt);
            Name = "ucRegister";
            Size = new Size(978, 644);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private LinkLabel llblLogin;
        private TextBox boxUsername;
        private TextBox boxPassword;
        private Label lblPasswordTxt;
        private Label lblUsernameTxt;
        private Label lblRegisterLogoTxt;
        private Button btnRegister;
        private TextBox boxPasswordCofirm;
        private Label lblPasswordConfirmtxt;
        private Label lblErrorTxt;
    }
}
