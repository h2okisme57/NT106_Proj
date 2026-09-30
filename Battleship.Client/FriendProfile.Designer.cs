namespace Battleship.Client
{
    partial class FriendProfile
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
            lblFriendName = new Label();
            lblFriendCondition = new Label();
            btnFight = new Button();
            SuspendLayout();
            // 
            // lblFriendName
            // 
            lblFriendName.AutoSize = true;
            lblFriendName.Location = new Point(13, 11);
            lblFriendName.Name = "lblFriendName";
            lblFriendName.Size = new Size(61, 25);
            lblFriendName.TabIndex = 0;
            lblFriendName.Text = "Friend";
            // 
            // lblFriendCondition
            // 
            lblFriendCondition.AutoSize = true;
            lblFriendCondition.Location = new Point(13, 36);
            lblFriendCondition.Name = "lblFriendCondition";
            lblFriendCondition.Size = new Size(60, 25);
            lblFriendCondition.TabIndex = 1;
            lblFriendCondition.Text = "online";
            // 
            // btnFight
            // 
            btnFight.Location = new Point(292, 11);
            btnFight.Name = "btnFight";
            btnFight.Size = new Size(74, 34);
            btnFight.TabIndex = 2;
            btnFight.Text = "Fight";
            btnFight.UseVisualStyleBackColor = true;
            btnFight.Click += btnFight_Click;
            // 
            // FriendProfile
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnFight);
            Controls.Add(lblFriendCondition);
            Controls.Add(lblFriendName);
            Name = "FriendProfile";
            Size = new Size(394, 65);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public Label lblFriendName;
        public Label lblFriendCondition;
        public Button btnFight;
    }
}
