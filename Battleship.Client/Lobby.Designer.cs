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
            btnJoinRoom = new Button();
            btnProfile = new Button();
            pnlProfile = new TableLayoutPanel();
            pnlQuit = new TableLayoutPanel();
            btnQuit = new Button();
            pnlJoinGame = new TableLayoutPanel();
            btnMakeRoom = new Button();
            btnPlay = new Button();
            lblLogoTxt = new Label();
            pnlProfile.SuspendLayout();
            pnlQuit.SuspendLayout();
            pnlJoinGame.SuspendLayout();
            SuspendLayout();
            // 
            // btnJoinRoom
            // 
            btnJoinRoom.Anchor = AnchorStyles.None;
            btnJoinRoom.Location = new Point(352, 6);
            btnJoinRoom.Name = "btnJoinRoom";
            btnJoinRoom.Size = new Size(161, 52);
            btnJoinRoom.TabIndex = 2;
            btnJoinRoom.Text = "JOIN ROOM";
            btnJoinRoom.UseVisualStyleBackColor = true;
            btnJoinRoom.Click += btnJoinRoom_Click;
            // 
            // btnProfile
            // 
            btnProfile.Anchor = AnchorStyles.Bottom;
            btnProfile.Location = new Point(4, 8);
            btnProfile.Name = "btnProfile";
            btnProfile.Size = new Size(84, 34);
            btnProfile.TabIndex = 3;
            btnProfile.Text = "Profile";
            btnProfile.UseVisualStyleBackColor = true;
            btnProfile.Click += btnProfile_Click;
            // 
            // pnlProfile
            // 
            pnlProfile.Anchor = AnchorStyles.Bottom;
            pnlProfile.ColumnCount = 1;
            pnlProfile.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlProfile.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            pnlProfile.Controls.Add(btnProfile, 0, 0);
            pnlProfile.Location = new Point(59, 544);
            pnlProfile.Name = "pnlProfile";
            pnlProfile.RowCount = 1;
            pnlProfile.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlProfile.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            pnlProfile.Size = new Size(92, 45);
            pnlProfile.TabIndex = 8;
            // 
            // pnlQuit
            // 
            pnlQuit.Anchor = AnchorStyles.Bottom;
            pnlQuit.ColumnCount = 1;
            pnlQuit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlQuit.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            pnlQuit.Controls.Add(btnQuit, 0, 0);
            pnlQuit.Location = new Point(798, 541);
            pnlQuit.Name = "pnlQuit";
            pnlQuit.RowCount = 1;
            pnlQuit.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlQuit.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            pnlQuit.Size = new Size(94, 45);
            pnlQuit.TabIndex = 9;
            // 
            // btnQuit
            // 
            btnQuit.Anchor = AnchorStyles.Bottom;
            btnQuit.Location = new Point(5, 8);
            btnQuit.Name = "btnQuit";
            btnQuit.Size = new Size(84, 34);
            btnQuit.TabIndex = 3;
            btnQuit.Text = "Quit";
            btnQuit.UseVisualStyleBackColor = true;
            btnQuit.Click += btnQuit_Click;
            // 
            // pnlJoinGame
            // 
            pnlJoinGame.Anchor = AnchorStyles.None;
            pnlJoinGame.ColumnCount = 3;
            pnlJoinGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            pnlJoinGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            pnlJoinGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            pnlJoinGame.Controls.Add(btnMakeRoom, 1, 0);
            pnlJoinGame.Controls.Add(btnPlay, 0, 0);
            pnlJoinGame.Controls.Add(btnJoinRoom, 2, 0);
            pnlJoinGame.Location = new Point(219, 522);
            pnlJoinGame.Name = "pnlJoinGame";
            pnlJoinGame.RowCount = 1;
            pnlJoinGame.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlJoinGame.Size = new Size(520, 64);
            pnlJoinGame.TabIndex = 10;
            // 
            // btnMakeRoom
            // 
            btnMakeRoom.Anchor = AnchorStyles.None;
            btnMakeRoom.Location = new Point(179, 6);
            btnMakeRoom.Name = "btnMakeRoom";
            btnMakeRoom.Size = new Size(161, 52);
            btnMakeRoom.TabIndex = 4;
            btnMakeRoom.Text = "MAKE ROOM";
            btnMakeRoom.UseVisualStyleBackColor = true;
            btnMakeRoom.Click += btnMakeRoom_Click;
            // 
            // btnPlay
            // 
            btnPlay.Anchor = AnchorStyles.None;
            btnPlay.Location = new Point(6, 6);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(161, 52);
            btnPlay.TabIndex = 3;
            btnPlay.Text = "PLAY";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // lblLogoTxt
            // 
            lblLogoTxt.Location = new Point(219, 45);
            lblLogoTxt.Margin = new Padding(0);
            lblLogoTxt.Name = "lblLogoTxt";
            lblLogoTxt.Size = new Size(520, 258);
            lblLogoTxt.TabIndex = 11;
            lblLogoTxt.Text = "BATTLESHIP";
            lblLogoTxt.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Lobby
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblLogoTxt);
            Controls.Add(pnlJoinGame);
            Controls.Add(pnlQuit);
            Controls.Add(pnlProfile);
            Name = "Lobby";
            Size = new Size(978, 644);
            Load += Lobby_Load;
            pnlProfile.ResumeLayout(false);
            pnlQuit.ResumeLayout(false);
            pnlJoinGame.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button btnJoinRoom;
        private Button btnProfile;
        private TableLayoutPanel pnlProfile;
        private TableLayoutPanel pnlQuit;
        private Button btnQuit;
        private TableLayoutPanel pnlJoinGame;
        private Button btnMakeRoom;
        private Button btnPlay;
        private Label lblLogoTxt;
    }
}
