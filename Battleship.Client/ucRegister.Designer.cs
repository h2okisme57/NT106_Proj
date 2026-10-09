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
            lblErrorTxt = new Label();
            btnRegister = new Button();
            llblLogin = new LinkLabel();
            boxUsernameTxt = new TextBox();
            boxPasswordTxt = new TextBox();
            lblLoginLogoTxt = new Label();
            boxConfirmPassword = new TextBox();
            SuspendLayout();
            // 
            // lblErrorTxt
            // 
            lblErrorTxt.ForeColor = Color.Red;
            lblErrorTxt.Location = new Point(-1, 404);
            lblErrorTxt.Name = "lblErrorTxt";
            lblErrorTxt.Size = new Size(546, 35);
            lblErrorTxt.TabIndex = 13;
            lblErrorTxt.TextAlign = ContentAlignment.MiddleCenter;
            lblErrorTxt.Click += lblErrorTxt_Click;
            // 
            // btnRegister
            // 
            btnRegister.BackColor = Color.FromArgb(0, 139, 227);
            btnRegister.BackgroundImageLayout = ImageLayout.Center;
            btnRegister.FlatStyle = FlatStyle.Popup;
            btnRegister.Font = new Font("Times New Roman", 8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnRegister.ForeColor = Color.White;
            btnRegister.Location = new Point(197, 332);
            btnRegister.Margin = new Padding(0);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(135, 46);
            btnRegister.TabIndex = 12;
            btnRegister.Text = "REGISTER";
            btnRegister.UseVisualStyleBackColor = false;
            btnRegister.Click += btnRegister_Click;
            // 
            // llblLogin
            // 
            llblLogin.AutoSize = true;
            llblLogin.Cursor = Cursors.IBeam;
            llblLogin.LinkBehavior = LinkBehavior.NeverUnderline;
            llblLogin.LinkColor = Color.FromArgb(69, 149, 102);
            llblLogin.Location = new Point(298, 274);
            llblLogin.Name = "llblLogin";
            llblLogin.Size = new Size(144, 25);
            llblLogin.TabIndex = 11;
            llblLogin.TabStop = true;
            llblLogin.Text = "Đã có tài khoản?";
            llblLogin.LinkClicked += llblLogin_LinkClicked;
            // 
            // boxUsernameTxt
            // 
            boxUsernameTxt.BackColor = Color.White;
            boxUsernameTxt.BorderStyle = BorderStyle.FixedSingle;
            boxUsernameTxt.ForeColor = Color.FromArgb(5, 98, 155);
            boxUsernameTxt.Location = new Point(107, 140);
            boxUsernameTxt.Name = "boxUsernameTxt";
            boxUsernameTxt.RightToLeft = RightToLeft.No;
            boxUsernameTxt.Size = new Size(335, 31);
            boxUsernameTxt.TabIndex = 10;
            boxUsernameTxt.TextChanged += boxUsernameTxt_TextChanged;
            // 
            // boxPasswordTxt
            // 
            boxPasswordTxt.BackColor = Color.White;
            boxPasswordTxt.BorderStyle = BorderStyle.FixedSingle;
            boxPasswordTxt.ForeColor = Color.FromArgb(2, 69, 121);
            boxPasswordTxt.Location = new Point(107, 190);
            boxPasswordTxt.Name = "boxPasswordTxt";
            boxPasswordTxt.PasswordChar = '*';
            boxPasswordTxt.Size = new Size(335, 31);
            boxPasswordTxt.TabIndex = 9;
            boxPasswordTxt.TextChanged += boxPasswordTxt_TextChanged;
            // 
            // lblLoginLogoTxt
            // 
            lblLoginLogoTxt.AutoSize = true;
            lblLoginLogoTxt.BackColor = Color.Transparent;
            lblLoginLogoTxt.Font = new Font("Segoe UI", 15F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblLoginLogoTxt.ForeColor = Color.Black;
            lblLoginLogoTxt.Location = new Point(147, 22);
            lblLoginLogoTxt.Name = "lblLoginLogoTxt";
            lblLoginLogoTxt.Size = new Size(223, 41);
            lblLoginLogoTxt.TabIndex = 8;
            lblLoginLogoTxt.Text = "USER REGISTER";
            // 
            // boxConfirmPassword
            // 
            boxConfirmPassword.BackColor = Color.White;
            boxConfirmPassword.BorderStyle = BorderStyle.FixedSingle;
            boxConfirmPassword.ForeColor = Color.FromArgb(2, 69, 121);
            boxConfirmPassword.Location = new Point(107, 240);
            boxConfirmPassword.Name = "boxConfirmPassword";
            boxConfirmPassword.PasswordChar = '*';
            boxConfirmPassword.Size = new Size(335, 31);
            boxConfirmPassword.TabIndex = 14;
            boxConfirmPassword.TextChanged += boxConfirmPassword_TextChanged;
            // 
            // ucRegister
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(boxConfirmPassword);
            Controls.Add(lblErrorTxt);
            Controls.Add(btnRegister);
            Controls.Add(llblLogin);
            Controls.Add(boxUsernameTxt);
            Controls.Add(boxPasswordTxt);
            Controls.Add(lblLoginLogoTxt);
            Name = "ucRegister";
            Size = new Size(545, 439);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblErrorTxt;
        private Button btnRegister;
        private LinkLabel llblLogin;
        private TextBox boxUsernameTxt;
        private TextBox boxPasswordTxt;
        private Label lblLoginLogoTxt;
        private TextBox boxConfirmPassword;
    }
}
