namespace Battleship.Client
{
    partial class JoinRoom
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
            lblJoinTxt = new Label();
            lblIDEnter = new Label();
            boxJoinEnter = new TextBox();
            btnCancel = new Button();
            btnJoin = new Button();
            SuspendLayout();
            // 
            // lblJoinTxt
            // 
            lblJoinTxt.AutoSize = true;
            lblJoinTxt.Location = new Point(13, 26);
            lblJoinTxt.Name = "lblJoinTxt";
            lblJoinTxt.Size = new Size(96, 25);
            lblJoinTxt.TabIndex = 0;
            lblJoinTxt.Text = "Join Room";
            // 
            // lblIDEnter
            // 
            lblIDEnter.AutoSize = true;
            lblIDEnter.Location = new Point(23, 74);
            lblIDEnter.Name = "lblIDEnter";
            lblIDEnter.Size = new Size(79, 25);
            lblIDEnter.TabIndex = 1;
            lblIDEnter.Text = "Enter ID:";
            // 
            // boxJoinEnter
            // 
            boxJoinEnter.Location = new Point(78, 122);
            boxJoinEnter.Name = "boxJoinEnter";
            boxJoinEnter.Size = new Size(150, 31);
            boxJoinEnter.TabIndex = 2;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(180, 172);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(112, 34);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += button1_Click;
            // 
            // btnJoin
            // 
            btnJoin.Location = new Point(23, 172);
            btnJoin.Name = "btnJoin";
            btnJoin.Size = new Size(112, 34);
            btnJoin.TabIndex = 4;
            btnJoin.Text = "Join";
            btnJoin.UseVisualStyleBackColor = true;
            btnJoin.Click += button1_Click_1;
            // 
            // JoinRoom
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            Controls.Add(btnJoin);
            Controls.Add(btnCancel);
            Controls.Add(boxJoinEnter);
            Controls.Add(lblIDEnter);
            Controls.Add(lblJoinTxt);
            Name = "JoinRoom";
            Size = new Size(340, 221);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblJoinTxt;
        private Label lblIDEnter;
        private TextBox boxJoinEnter;
        private Button btnCancel;
        private Button btnJoin;
    }
}
