namespace Battleship.Client
{
    partial class joinRoom
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
            lblJoinlgo = new Label();
            btnBack = new Button();
            pnlJoin = new FlowLayoutPanel();
            SuspendLayout();
            // 
            // lblJoinlgo
            // 
            lblJoinlgo.AutoSize = true;
            lblJoinlgo.Location = new Point(3, 11);
            lblJoinlgo.Name = "lblJoinlgo";
            lblJoinlgo.Size = new Size(110, 25);
            lblJoinlgo.TabIndex = 0;
            lblJoinlgo.Text = "JOIN ROOM";
            // 
            // btnBack
            // 
            btnBack.BackColor = SystemColors.ControlDarkDark;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Location = new Point(3, 452);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(435, 34);
            btnBack.TabIndex = 1;
            btnBack.Text = "BACK";
            btnBack.UseVisualStyleBackColor = false;
            btnBack.Click += button1_Click;
            // 
            // pnlJoin
            // 
            pnlJoin.Location = new Point(3, 46);
            pnlJoin.Name = "pnlJoin";
            pnlJoin.Size = new Size(435, 400);
            pnlJoin.TabIndex = 3;
            pnlJoin.Paint += flowLayoutPanel1_Paint_1;
            // 
            // joinRoom
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.AppWorkspace;
            Controls.Add(pnlJoin);
            Controls.Add(btnBack);
            Controls.Add(lblJoinlgo);
            Name = "joinRoom";
            Size = new Size(441, 489);
            Load += joinRoom_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblJoinlgo;
        private Button btnBack;
        private FlowLayoutPanel pnlJoin;
    }
}
