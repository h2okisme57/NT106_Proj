namespace Battleship.Client
{
    partial class ProfileControl
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
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            lblCondition = new Label();
            btnLogOut = new Button();
            lblLose = new Label();
            lblWin = new Label();
            lblUsername = new Label();
            lblUsenameTxt = new Label();
            tabPage2 = new TabPage();
            LayoutFriend = new FlowLayoutPanel();
            flowLayoutPanel2 = new FlowLayoutPanel();
            btnBack = new Button();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            LayoutFriend.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Top;
            tabControl1.ItemSize = new Size(203, 30);
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(408, 239);
            tabControl1.SizeMode = TabSizeMode.Fixed;
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(lblCondition);
            tabPage1.Controls.Add(btnLogOut);
            tabPage1.Controls.Add(lblLose);
            tabPage1.Controls.Add(lblWin);
            tabPage1.Controls.Add(lblUsername);
            tabPage1.Controls.Add(lblUsenameTxt);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(400, 201);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            tabPage1.Click += this.tabPage1_Click;
            // 
            // lblCondition
            // 
            lblCondition.AutoSize = true;
            lblCondition.ForeColor = Color.Green;
            lblCondition.Location = new Point(6, 123);
            lblCondition.Name = "lblCondition";
            lblCondition.Size = new Size(63, 25);
            lblCondition.TabIndex = 5;
            lblCondition.Text = "Online";
            // 
            // btnLogOut
            // 
            btnLogOut.Location = new Point(285, 22);
            btnLogOut.Name = "btnLogOut";
            btnLogOut.Size = new Size(112, 34);
            btnLogOut.TabIndex = 4;
            btnLogOut.Text = "Log out";
            btnLogOut.UseVisualStyleBackColor = true;
            btnLogOut.Click += btnLogOut_Click;
            // 
            // lblLose
            // 
            lblLose.Location = new Point(204, 71);
            lblLose.Name = "lblLose";
            lblLose.Size = new Size(190, 25);
            lblLose.TabIndex = 3;
            lblLose.Text = "Lose";
            // 
            // lblWin
            // 
            lblWin.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblWin.Location = new Point(6, 71);
            lblWin.Margin = new Padding(0);
            lblWin.Name = "lblWin";
            lblWin.RightToLeft = RightToLeft.Yes;
            lblWin.Size = new Size(192, 25);
            lblWin.TabIndex = 2;
            lblWin.Text = "Win";
            // 
            // lblUsername
            // 
            lblUsername.AutoSize = true;
            lblUsername.Location = new Point(107, 31);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(56, 25);
            lblUsername.TabIndex = 1;
            lblUsername.Text = "name";
            // 
            // lblUsenameTxt
            // 
            lblUsenameTxt.AutoSize = true;
            lblUsenameTxt.Location = new Point(6, 31);
            lblUsenameTxt.Name = "lblUsenameTxt";
            lblUsenameTxt.Size = new Size(95, 25);
            lblUsenameTxt.TabIndex = 0;
            lblUsenameTxt.Text = "Username:";
            lblUsenameTxt.Click += label1_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(LayoutFriend);
            tabPage2.Location = new Point(4, 34);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(400, 201);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // LayoutFriend
            // 
            LayoutFriend.AutoScroll = true;
            LayoutFriend.Controls.Add(flowLayoutPanel2);
            LayoutFriend.Dock = DockStyle.Fill;
            LayoutFriend.Location = new Point(3, 3);
            LayoutFriend.Name = "LayoutFriend";
            LayoutFriend.Size = new Size(394, 195);
            LayoutFriend.TabIndex = 0;
            LayoutFriend.Paint += flowLayoutPanel1_Paint;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Dock = DockStyle.Fill;
            flowLayoutPanel2.Location = new Point(3, 3);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(300, 0);
            flowLayoutPanel2.TabIndex = 0;
            // 
            // btnBack
            // 
            btnBack.Location = new Point(10, 241);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(388, 33);
            btnBack.TabIndex = 1;
            btnBack.Text = "Back";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += button1_Click;
            // 
            // ProfileControl
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnBack);
            Controls.Add(tabControl1);
            Name = "ProfileControl";
            Size = new Size(408, 279);
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            LayoutFriend.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label lblUsenameTxt;
        private Label lblLose;
        private Label lblWin;
        private Label lblUsername;
        private Button btnLogOut;
        private Label lblCondition;
        private Button btnBack;
        private FlowLayoutPanel LayoutFriend;
        private FlowLayoutPanel flowLayoutPanel2;
    }
}
