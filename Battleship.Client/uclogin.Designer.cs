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
            lblUsernameTxt = new Label();
            lblPasswordTxt = new Label();
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
            lblLoginLogoTxt.Location = new Point(433, 89);
            lblLoginLogoTxt.Name = "lblLoginLogoTxt";
            lblLoginLogoTxt.Size = new Size(68, 25);
            lblLoginLogoTxt.TabIndex = 0;
            lblLoginLogoTxt.Text = "LOG IN";
            // 
            // lblUsernameTxt
            // 
            lblUsernameTxt.AutoSize = true;
            lblUsernameTxt.Location = new Point(274, 224);
            lblUsernameTxt.Name = "lblUsernameTxt";
            lblUsernameTxt.Size = new Size(91, 25);
            lblUsernameTxt.TabIndex = 1;
            lblUsernameTxt.Text = "Username";
            // 
            // lblPasswordTxt
            // 
            lblPasswordTxt.AutoSize = true;
            lblPasswordTxt.Location = new Point(274, 308);
            lblPasswordTxt.Name = "lblPasswordTxt";
            lblPasswordTxt.Size = new Size(87, 25);
            lblPasswordTxt.TabIndex = 2;
            lblPasswordTxt.Text = "Password";
            // 
            // boxPasswordTxt
            // 
            boxPasswordTxt.Location = new Point(409, 308);
            boxPasswordTxt.Name = "boxPasswordTxt";
            boxPasswordTxt.PasswordChar = '*';
            boxPasswordTxt.Size = new Size(226, 31);
            boxPasswordTxt.TabIndex = 3;
            // 
            // boxUsernameTxt
            // 
            boxUsernameTxt.Location = new Point(409, 224);
            boxUsernameTxt.Name = "boxUsernameTxt";
            boxUsernameTxt.Size = new Size(226, 31);
            boxUsernameTxt.TabIndex = 4;
            // 
            // llblRegister
            // 
            llblRegister.AutoSize = true;
            llblRegister.Location = new Point(338, 502);
            llblRegister.Name = "llblRegister";
            llblRegister.Size = new Size(278, 25);
            llblRegister.TabIndex = 5;
            llblRegister.TabStop = true;
            llblRegister.Text = "Chưa có tài khoản? Đăng ký ngay";
            llblRegister.LinkClicked += llblRegister_LinkClicked;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(433, 388);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(112, 34);
            btnLogin.TabIndex = 6;
            btnLogin.Text = "Log in";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // lblErrorTxt
            // 
            lblErrorTxt.ForeColor = Color.Red;
            lblErrorTxt.Location = new Point(155, 455);
            lblErrorTxt.Name = "lblErrorTxt";
            lblErrorTxt.Size = new Size(702, 25);
            lblErrorTxt.TabIndex = 7;
            lblErrorTxt.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ucLogin
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblErrorTxt);
            Controls.Add(btnLogin);
            Controls.Add(llblRegister);
            Controls.Add(boxUsernameTxt);
            Controls.Add(boxPasswordTxt);
            Controls.Add(lblPasswordTxt);
            Controls.Add(lblUsernameTxt);
            Controls.Add(lblLoginLogoTxt);
            Name = "ucLogin";
            Size = new Size(978, 644);
            Load += uclogin_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblLoginLogoTxt;
        private Label lblUsernameTxt;
        private Label lblPasswordTxt;
        private TextBox boxPasswordTxt;
        private TextBox boxUsernameTxt;
        private LinkLabel llblRegister;
        private Button btnLogin;
        private Label lblErrorTxt;
    }
}
