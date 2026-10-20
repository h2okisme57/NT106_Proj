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
            tabProfile = new TabControl();
            tabPageProfile = new TabPage();
            label1 = new Label();
            lblLose = new Label();
            lblID = new Label();
            lblUsername = new Label();
            lblWin = new Label();
            tabPageFriend = new TabPage();
            flpFriend = new FlowLayoutPanel();
            pnlFriend = new FlowLayoutPanel();
            btnBack = new Button();
            tabProfile.SuspendLayout();
            tabPageProfile.SuspendLayout();
            tabPageFriend.SuspendLayout();
            flpFriend.SuspendLayout();
            SuspendLayout();
            // 
            // tabProfile
            // 
            tabProfile.Controls.Add(tabPageProfile);
            tabProfile.Controls.Add(tabPageFriend);
            tabProfile.Location = new Point(-10, 3);
            tabProfile.Name = "tabProfile";
            tabProfile.SelectedIndex = 0;
            tabProfile.Size = new Size(305, 252);
            tabProfile.SizeMode = TabSizeMode.Fixed;
            tabProfile.TabIndex = 0;
            // 
            // tabPageProfile
            // 
            tabPageProfile.BackColor = Color.Transparent;
            tabPageProfile.Controls.Add(label1);
            tabPageProfile.Controls.Add(lblLose);
            tabPageProfile.Controls.Add(lblID);
            tabPageProfile.Controls.Add(lblUsername);
            tabPageProfile.Controls.Add(lblWin);
            tabPageProfile.Location = new Point(4, 34);
            tabPageProfile.Name = "tabPageProfile";
            tabPageProfile.Padding = new Padding(3);
            tabPageProfile.Size = new Size(297, 214);
            tabPageProfile.TabIndex = 0;
            tabPageProfile.Text = "Profile";
            tabPageProfile.Click += tabPageProfile_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(134, 147);
            label1.Name = "label1";
            label1.Size = new Size(16, 25);
            label1.TabIndex = 7;
            label1.Text = "|";
            label1.Click += this.label1_Click;
            // 
            // lblLose
            // 
            lblLose.ForeColor = Color.Firebrick;
            lblLose.Location = new Point(156, 147);
            lblLose.Name = "lblLose";
            lblLose.Size = new Size(88, 38);
            lblLose.TabIndex = 6;
            lblLose.Text = "Lose:";
            lblLose.Click += this.lblLose_Click;
            // 
            // lblID
            // 
            lblID.AutoSize = true;
            lblID.Location = new Point(17, 45);
            lblID.Name = "lblID";
            lblID.Size = new Size(34, 25);
            lblID.TabIndex = 2;
            lblID.Text = "ID:";
            lblID.Click += this.lblID_Click;
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(17, 99);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(95, 25);
            lblUsername.TabIndex = 3;
            lblUsername.Text = "Username:";
            lblUsername.Click += this.lblUsername_Click;
            // 
            // lblWin
            // 
            lblWin.Anchor = AnchorStyles.None;
            lblWin.AutoSize = true;
            lblWin.ForeColor = Color.ForestGreen;
            lblWin.Location = new Point(60, 147);
            lblWin.Name = "lblWin";
            lblWin.RightToLeft = RightToLeft.No;
            lblWin.Size = new Size(52, 25);
            lblWin.TabIndex = 5;
            lblWin.Text = "Win: ";
            lblWin.Click += lblWin_Click;
            // 
            // tabPageFriend
            // 
            tabPageFriend.Controls.Add(flpFriend);
            tabPageFriend.Location = new Point(4, 34);
            tabPageFriend.Name = "tabPageFriend";
            tabPageFriend.Padding = new Padding(3);
            tabPageFriend.Size = new Size(297, 214);
            tabPageFriend.TabIndex = 1;
            tabPageFriend.Text = "Friend";
            tabPageFriend.UseVisualStyleBackColor = true;
            tabPageFriend.Click += tabPageFriend_Click;
            // 
            // flpFriend
            // 
            flpFriend.Controls.Add(pnlFriend);
            flpFriend.Location = new Point(0, 0);
            flpFriend.Name = "flpFriend";
            flpFriend.Size = new Size(300, 218);
            flpFriend.TabIndex = 0;
            // 
            // pnlFriend
            // 
            pnlFriend.AutoScroll = true;
            pnlFriend.Dock = DockStyle.Fill;
            pnlFriend.Location = new Point(3, 3);
            pnlFriend.Name = "pnlFriend";
            pnlFriend.Size = new Size(284, 0);
            pnlFriend.TabIndex = 0;
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
            // Profile
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(btnBack);
            Controls.Add(tabProfile);
            Name = "Profile";
            Size = new Size(281, 293);
            tabProfile.ResumeLayout(false);
            tabPageProfile.ResumeLayout(false);
            tabPageProfile.PerformLayout();
            tabPageFriend.ResumeLayout(false);
            flpFriend.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabProfile;
        private TabPage tabPageProfile;
        private TabPage tabPageFriend;
        private Button btnBack;
        private Label lblID;
        private Label lblUsername;
        private Label lblWin;
        private Label lblLose;
        private Label label1;
        private FlowLayoutPanel flpFriend;
        private FlowLayoutPanel pnlFriend;
    }
}
