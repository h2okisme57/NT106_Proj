namespace Battleship.Client
{
    partial class Profile
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
            btnBack = new Button();
            label1 = new Label();
            lblLose = new Label();
            lblID = new Label();
            lblUsername = new Label();
            lblWin = new Label();
            lblProfiletxt = new Label();
            SuspendLayout();
            // 
            // btnBack
            // 
            btnBack.BackColor = Color.Silver;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Location = new Point(3, 256);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(275, 34);
            btnBack.TabIndex = 1;
            btnBack.Text = "Back";
            btnBack.UseVisualStyleBackColor = false;
            btnBack.Click += btnBack_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(144, 178);
            label1.Name = "label1";
            label1.Size = new Size(16, 25);
            label1.TabIndex = 12;
            label1.Text = "|";
            // 
            // lblLose
            // 
            lblLose.ForeColor = Color.Firebrick;
            lblLose.Location = new Point(166, 178);
            lblLose.Name = "lblLose";
            lblLose.Size = new Size(88, 38);
            lblLose.TabIndex = 11;
            lblLose.Text = "Lose:";
            // 
            // lblID
            // 
            lblID.AutoSize = true;
            lblID.Location = new Point(27, 76);
            lblID.Name = "lblID";
            lblID.Size = new Size(34, 25);
            lblID.TabIndex = 8;
            lblID.Text = "ID:";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(27, 130);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(95, 25);
            lblUsername.TabIndex = 9;
            lblUsername.Text = "Username:";
            // 
            // lblWin
            // 
            lblWin.Anchor = AnchorStyles.None;
            lblWin.AutoSize = true;
            lblWin.ForeColor = Color.ForestGreen;
            lblWin.Location = new Point(70, 178);
            lblWin.Name = "lblWin";
            lblWin.RightToLeft = RightToLeft.No;
            lblWin.Size = new Size(52, 25);
            lblWin.TabIndex = 10;
            lblWin.Text = "Win: ";
            // 
            // lblProfiletxt
            // 
            lblProfiletxt.AutoSize = true;
            lblProfiletxt.Location = new Point(18, 22);
            lblProfiletxt.Name = "lblProfiletxt";
            lblProfiletxt.Size = new Size(62, 25);
            lblProfiletxt.TabIndex = 13;
            lblProfiletxt.Text = "Profile";
            lblProfiletxt.Click += lblProfiletxt_Click;
            // 
            // Profile
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(lblProfiletxt);
            Controls.Add(label1);
            Controls.Add(lblLose);
            Controls.Add(lblID);
            Controls.Add(lblUsername);
            Controls.Add(lblWin);
            Controls.Add(btnBack);
            Name = "Profile";
            Size = new Size(281, 293);
            Load += Profile_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnBack;
        private Label label1;
        private Label lblLose;
        private Label lblID;
        private Label lblUsername;
        private Label lblWin;
        private Label lblProfiletxt;
    }
}
