namespace Battleship.Client
{
    partial class Lobby
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
            components = new System.ComponentModel.Container();
            btnFindRoom = new Button();
            btnCreateRoom = new Button();
            btnJoinRoom = new Button();
            btnQuit = new Button();
            btnProfile = new Button();
            lblBattleshiptxt = new Label();
            contextMenuStrip1 = new ContextMenuStrip(components);
            boxChat = new TextBox();
            btnLeaderBoard = new Button();
            SuspendLayout();
            // 
            // btnFindRoom
            // 
            btnFindRoom.Location = new Point(0, 321);
            btnFindRoom.Name = "btnFindRoom";
            btnFindRoom.Size = new Size(344, 34);
            btnFindRoom.TabIndex = 0;
            btnFindRoom.Text = "Find Opponent                 ";
            btnFindRoom.UseVisualStyleBackColor = true;
            btnFindRoom.Click += btnFindRoom_Click;
            // 
            // btnCreateRoom
            // 
            btnCreateRoom.Location = new Point(0, 361);
            btnCreateRoom.Name = "btnCreateRoom";
            btnCreateRoom.Size = new Size(344, 34);
            btnCreateRoom.TabIndex = 1;
            btnCreateRoom.Text = "Create Room                     ";
            btnCreateRoom.UseVisualStyleBackColor = true;
            btnCreateRoom.Click += btnCreateRoom_Click;
            // 
            // btnJoinRoom
            // 
            btnJoinRoom.Location = new Point(0, 401);
            btnJoinRoom.Name = "btnJoinRoom";
            btnJoinRoom.Size = new Size(344, 34);
            btnJoinRoom.TabIndex = 2;
            btnJoinRoom.Text = "Join Room                        ";
            btnJoinRoom.UseVisualStyleBackColor = true;
            btnJoinRoom.Click += btnJoinRoom_Click;
            // 
            // btnQuit
            // 
            btnQuit.Location = new Point(0, 480);
            btnQuit.Name = "btnQuit";
            btnQuit.Size = new Size(344, 33);
            btnQuit.TabIndex = 3;
            btnQuit.Text = "Quit                                  ";
            btnQuit.UseVisualStyleBackColor = true;
            btnQuit.Click += btnQuit_Click;
            // 
            // btnProfile
            // 
            btnProfile.Location = new Point(3, 594);
            btnProfile.Name = "btnProfile";
            btnProfile.Size = new Size(192, 44);
            btnProfile.TabIndex = 4;
            btnProfile.Text = "Profile";
            btnProfile.UseVisualStyleBackColor = true;
            btnProfile.Click += btnProfile_Click;
            // 
            // lblBattleshiptxt
            // 
            lblBattleshiptxt.BackColor = Color.Transparent;
            lblBattleshiptxt.Font = new Font("Segoe UI", 30F);
            lblBattleshiptxt.ForeColor = Color.Navy;
            lblBattleshiptxt.Location = new Point(287, 24);
            lblBattleshiptxt.Name = "lblBattleshiptxt";
            lblBattleshiptxt.Size = new Size(422, 155);
            lblBattleshiptxt.TabIndex = 5;
            lblBattleshiptxt.Text = "BATTLESHIP";
            lblBattleshiptxt.TextAlign = ContentAlignment.MiddleCenter;
            lblBattleshiptxt.Click += lblBattleshiptxt_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(24, 24);
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(61, 4);
            // 
            // boxChat
            // 
            boxChat.BackColor = SystemColors.InactiveCaptionText;
            boxChat.ForeColor = SystemColors.InactiveBorder;
            boxChat.Location = new Point(566, 613);
            boxChat.Name = "boxChat";
            boxChat.Size = new Size(412, 31);
            boxChat.TabIndex = 8;
            boxChat.TextChanged += textBox1_TextChanged;
            // 
            // btnLeaderBoard
            // 
            btnLeaderBoard.Location = new Point(0, 441);
            btnLeaderBoard.Name = "btnLeaderBoard";
            btnLeaderBoard.Size = new Size(344, 33);
            btnLeaderBoard.TabIndex = 10;
            btnLeaderBoard.Text = "Leader Board                    ";
            btnLeaderBoard.UseVisualStyleBackColor = true;
            btnLeaderBoard.Click += btnLeaderBoard_Click_1;
            // 
            // Lobby
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.sonia_wisniewska_background_4k;
            BackgroundImageLayout = ImageLayout.Stretch;
            Controls.Add(btnLeaderBoard);
            Controls.Add(boxChat);
            Controls.Add(lblBattleshiptxt);
            Controls.Add(btnProfile);
            Controls.Add(btnQuit);
            Controls.Add(btnJoinRoom);
            Controls.Add(btnCreateRoom);
            Controls.Add(btnFindRoom);
            Name = "Lobby";
            Size = new Size(978, 641);
            Load += Lobby_Load_1;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnFindRoom;
        private Button btnCreateRoom;
        private Button btnJoinRoom;
        private Button btnQuit;
        private Button btnProfile;
        private Label lblBattleshiptxt;
        private ContextMenuStrip contextMenuStrip1;
        private TextBox boxChat;
        private Button btnLeaderBoard;
    }
}
