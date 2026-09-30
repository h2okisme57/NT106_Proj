namespace Battleship.Client
{
    partial class FriendFightInv
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
            lblInviteTxt = new Label();
            lblInviteMess = new Label();
            btnAccept = new Button();
            btnDecline = new Button();
            SuspendLayout();
            // 
            // lblInviteTxt
            // 
            lblInviteTxt.AutoSize = true;
            lblInviteTxt.Location = new Point(13, 11);
            lblInviteTxt.Name = "lblInviteTxt";
            lblInviteTxt.Size = new Size(60, 25);
            lblInviteTxt.TabIndex = 0;
            lblInviteTxt.Text = "Invite ";
            // 
            // lblInviteMess
            // 
            lblInviteMess.Location = new Point(14, 68);
            lblInviteMess.Name = "lblInviteMess";
            lblInviteMess.Size = new Size(287, 77);
            lblInviteMess.TabIndex = 1;
            lblInviteMess.Text = "[challengerName] has challenged you to a fight!";
            // 
            // btnAccept
            // 
            btnAccept.Location = new Point(14, 172);
            btnAccept.Name = "btnAccept";
            btnAccept.Size = new Size(112, 34);
            btnAccept.TabIndex = 2;
            btnAccept.Text = "Accept";
            btnAccept.UseVisualStyleBackColor = true;
            // 
            // btnDecline
            // 
            btnDecline.Location = new Point(189, 172);
            btnDecline.Name = "btnDecline";
            btnDecline.Size = new Size(112, 34);
            btnDecline.TabIndex = 3;
            btnDecline.Text = "Decline";
            btnDecline.UseVisualStyleBackColor = true;
            // 
            // FriendFightInv
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnDecline);
            Controls.Add(btnAccept);
            Controls.Add(lblInviteMess);
            Controls.Add(lblInviteTxt);
            Name = "FriendFightInv";
            Size = new Size(340, 221);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblInviteTxt;
        private Label lblInviteMess;
        private Button btnAccept;
        private Button btnDecline;
    }
}
