namespace Battleship.Client
{
    partial class RoomWait
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
            lblPlayer1txt = new Label();
            lblPlayer2Txt = new Label();
            lblPlayer2Conddition = new Label();
            lblPlayer1Condition = new Label();
            lblPlayer2Username = new Label();
            lblPlayer1Username = new Label();
            lblLogo = new Label();
            btnReady = new Button();
            btnOut = new Button();
            SuspendLayout();
            // 
            // lblPlayer1txt
            // 
            lblPlayer1txt.AutoSize = true;
            lblPlayer1txt.Location = new Point(71, 81);
            lblPlayer1txt.Name = "lblPlayer1txt";
            lblPlayer1txt.Size = new Size(78, 25);
            lblPlayer1txt.TabIndex = 0;
            lblPlayer1txt.Text = "Player 1:";
            // 
            // lblPlayer2Txt
            // 
            lblPlayer2Txt.AutoSize = true;
            lblPlayer2Txt.Location = new Point(302, 81);
            lblPlayer2Txt.Name = "lblPlayer2Txt";
            lblPlayer2Txt.Size = new Size(78, 25);
            lblPlayer2Txt.TabIndex = 1;
            lblPlayer2Txt.Text = "Player 2:";
            // 
            // lblPlayer2Conddition
            // 
            lblPlayer2Conddition.AutoSize = true;
            lblPlayer2Conddition.ForeColor = Color.Red;
            lblPlayer2Conddition.Location = new Point(302, 166);
            lblPlayer2Conddition.Name = "lblPlayer2Conddition";
            lblPlayer2Conddition.Size = new Size(84, 25);
            lblPlayer2Conddition.TabIndex = 2;
            lblPlayer2Conddition.Text = "Waiting...";
            lblPlayer2Conddition.Click += lblPlayer2Conddition_Click;
            // 
            // lblPlayer1Condition
            // 
            lblPlayer1Condition.AutoSize = true;
            lblPlayer1Condition.ForeColor = Color.Red;
            lblPlayer1Condition.Location = new Point(71, 166);
            lblPlayer1Condition.Name = "lblPlayer1Condition";
            lblPlayer1Condition.Size = new Size(84, 25);
            lblPlayer1Condition.TabIndex = 3;
            lblPlayer1Condition.Text = "Waiting...";
            lblPlayer1Condition.Click += lblPlayer1Condition_Click;
            // 
            // lblPlayer2Username
            // 
            lblPlayer2Username.AutoSize = true;
            lblPlayer2Username.Location = new Point(302, 124);
            lblPlayer2Username.Name = "lblPlayer2Username";
            lblPlayer2Username.Size = new Size(123, 25);
            lblPlayer2Username.TabIndex = 4;
            lblPlayer2Username.Text = "Player 2 name";
            lblPlayer2Username.Click += lblPlayer2Username_Click;
            // 
            // lblPlayer1Username
            // 
            lblPlayer1Username.AutoSize = true;
            lblPlayer1Username.Location = new Point(71, 124);
            lblPlayer1Username.Name = "lblPlayer1Username";
            lblPlayer1Username.Size = new Size(128, 25);
            lblPlayer1Username.TabIndex = 5;
            lblPlayer1Username.Text = "Player 1 name ";
            lblPlayer1Username.Click += lblPlayer1Username_Click;
            // 
            // lblLogo
            // 
            lblLogo.AutoSize = true;
            lblLogo.Location = new Point(23, 22);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(125, 25);
            lblLogo.TabIndex = 6;
            lblLogo.Text = "Waiting Room";
            // 
            // btnReady
            // 
            btnReady.BackColor = SystemColors.ControlDarkDark;
            btnReady.FlatStyle = FlatStyle.Flat;
            btnReady.Location = new Point(169, 291);
            btnReady.Name = "btnReady";
            btnReady.Size = new Size(151, 52);
            btnReady.TabIndex = 7;
            btnReady.Text = "READY";
            btnReady.UseVisualStyleBackColor = false;
            btnReady.Click += btnReady_Click;
            // 
            // btnOut
            // 
            btnOut.BackColor = Color.IndianRed;
            btnOut.FlatStyle = FlatStyle.Flat;
            btnOut.Location = new Point(429, 13);
            btnOut.Name = "btnOut";
            btnOut.Size = new Size(81, 34);
            btnOut.TabIndex = 9;
            btnOut.Text = "OUT";
            btnOut.UseVisualStyleBackColor = false;
            btnOut.Click += btnOut_Click;
            // 
            // RoomWait
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.AppWorkspace;
            Controls.Add(btnOut);
            Controls.Add(btnReady);
            Controls.Add(lblLogo);
            Controls.Add(lblPlayer1Username);
            Controls.Add(lblPlayer2Username);
            Controls.Add(lblPlayer1Condition);
            Controls.Add(lblPlayer2Conddition);
            Controls.Add(lblPlayer2Txt);
            Controls.Add(lblPlayer1txt);
            Name = "RoomWait";
            Size = new Size(525, 382);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblPlayer1txt;
        private Label lblPlayer2Txt;
        private Label lblPlayer2Conddition;
        private Label lblPlayer1Condition;
        private Label lblPlayer2Username;
        private Label lblPlayer1Username;
        private Label lblLogo;
        private Button btnReady;
        private Button btnOut;
    }
}
