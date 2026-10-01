namespace Battleship.Client
{
    partial class PlayRoom
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
            tableGame = new TableLayoutPanel();
            pnlPlayer = new Panel();
            LblPlayerHit = new Label();
            lblPlayerTime = new Label();
            lblPlayerUsername = new Label();
            lblPlayerTxt = new Label();
            pnlOpp = new Panel();
            lblOppTime = new Label();
            lblOppHit = new Label();
            lblOppUsername = new Label();
            lblOppTxt = new Label();
            pnlPlayer.SuspendLayout();
            pnlOpp.SuspendLayout();
            SuspendLayout();
            // 
            // tableGame
            // 
            tableGame.Anchor = AnchorStyles.Top;
            tableGame.ColumnCount = 10;
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableGame.Location = new Point(167, 0);
            tableGame.Name = "tableGame";
            tableGame.RowCount = 10;
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableGame.Size = new Size(644, 644);
            tableGame.TabIndex = 0;
            tableGame.Paint += tableGame_Paint;
            // 
            // pnlPlayer
            // 
            pnlPlayer.Controls.Add(LblPlayerHit);
            pnlPlayer.Controls.Add(lblPlayerTime);
            pnlPlayer.Controls.Add(lblPlayerUsername);
            pnlPlayer.Controls.Add(lblPlayerTxt);
            pnlPlayer.Location = new Point(0, 0);
            pnlPlayer.Name = "pnlPlayer";
            pnlPlayer.Size = new Size(167, 644);
            pnlPlayer.TabIndex = 1;
            // 
            // LblPlayerHit
            // 
            LblPlayerHit.Anchor = AnchorStyles.None;
            LblPlayerHit.Font = new Font("Segoe UI", 20F);
            LblPlayerHit.Location = new Point(37, 284);
            LblPlayerHit.Name = "LblPlayerHit";
            LblPlayerHit.Size = new Size(88, 65);
            LblPlayerHit.TabIndex = 4;
            LblPlayerHit.Text = "60";
            LblPlayerHit.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPlayerTime
            // 
            lblPlayerTime.AutoSize = true;
            lblPlayerTime.Location = new Point(43, 458);
            lblPlayerTime.Name = "lblPlayerTime";
            lblPlayerTime.Size = new Size(44, 25);
            lblPlayerTime.TabIndex = 2;
            lblPlayerTime.Text = "Hit: ";
            // 
            // lblPlayerUsername
            // 
            lblPlayerUsername.AutoSize = true;
            lblPlayerUsername.Location = new Point(39, 105);
            lblPlayerUsername.Name = "lblPlayerUsername";
            lblPlayerUsername.Size = new Size(91, 25);
            lblPlayerUsername.TabIndex = 1;
            lblPlayerUsername.Text = "Username";
            // 
            // lblPlayerTxt
            // 
            lblPlayerTxt.AutoSize = true;
            lblPlayerTxt.Location = new Point(39, 57);
            lblPlayerTxt.Name = "lblPlayerTxt";
            lblPlayerTxt.Size = new Size(63, 25);
            lblPlayerTxt.TabIndex = 0;
            lblPlayerTxt.Text = "Player:";
            // 
            // pnlOpp
            // 
            pnlOpp.Controls.Add(lblOppTime);
            pnlOpp.Controls.Add(lblOppHit);
            pnlOpp.Controls.Add(lblOppUsername);
            pnlOpp.Controls.Add(lblOppTxt);
            pnlOpp.Location = new Point(811, 0);
            pnlOpp.Name = "pnlOpp";
            pnlOpp.Size = new Size(167, 644);
            pnlOpp.TabIndex = 2;
            // 
            // lblOppTime
            // 
            lblOppTime.Anchor = AnchorStyles.None;
            lblOppTime.Font = new Font("Segoe UI", 20F);
            lblOppTime.Location = new Point(36, 284);
            lblOppTime.Name = "lblOppTime";
            lblOppTime.Size = new Size(88, 65);
            lblOppTime.TabIndex = 8;
            lblOppTime.Text = "60";
            lblOppTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblOppHit
            // 
            lblOppHit.AutoSize = true;
            lblOppHit.Location = new Point(67, 458);
            lblOppHit.Name = "lblOppHit";
            lblOppHit.Size = new Size(44, 25);
            lblOppHit.TabIndex = 7;
            lblOppHit.Text = "Hit: ";
            // 
            // lblOppUsername
            // 
            lblOppUsername.AutoSize = true;
            lblOppUsername.Location = new Point(38, 105);
            lblOppUsername.Name = "lblOppUsername";
            lblOppUsername.Size = new Size(91, 25);
            lblOppUsername.TabIndex = 6;
            lblOppUsername.Text = "Username";
            // 
            // lblOppTxt
            // 
            lblOppTxt.AutoSize = true;
            lblOppTxt.Location = new Point(38, 57);
            lblOppTxt.Name = "lblOppTxt";
            lblOppTxt.Size = new Size(98, 25);
            lblOppTxt.TabIndex = 5;
            lblOppTxt.Text = "Opponent:";
            // 
            // PlayRoom
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(pnlOpp);
            Controls.Add(pnlPlayer);
            Controls.Add(tableGame);
            Name = "PlayRoom";
            Size = new Size(978, 644);
            pnlPlayer.ResumeLayout(false);
            pnlPlayer.PerformLayout();
            pnlOpp.ResumeLayout(false);
            pnlOpp.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableGame;
        private Panel pnlPlayer;
        private Panel pnlOpp;
        private Label lblPlayerTxt;
        private Label lblPlayerTime;
        private Label lblPlayerUsername;
        private Label LblPlayerHit;
        private Label lblOppTime;
        private Label lblOppHit;
        private Label lblOppUsername;
        private Label lblOppTxt;
    }
}
