namespace Battleship.Client
{
    partial class CreateRoom
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
            btnCancel = new Button();
            lblWait = new Label();
            lblID = new Label();
            lblCreatetxt = new Label();
            timeWait = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(89, 166);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(112, 34);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click_1;
            // 
            // lblWait
            // 
            lblWait.AutoSize = true;
            lblWait.Location = new Point(108, 117);
            lblWait.Name = "lblWait";
            lblWait.Size = new Size(72, 25);
            lblWait.TabIndex = 7;
            lblWait.Text = "Waiting";
            // 
            // lblID
            // 
            lblID.AutoSize = true;
            lblID.Location = new Point(15, 53);
            lblID.Name = "lblID";
            lblID.Size = new Size(83, 25);
            lblID.TabIndex = 6;
            lblID.Text = "ID room:";
            // 
            // lblCreatetxt
            // 
            lblCreatetxt.AutoSize = true;
            lblCreatetxt.Location = new Point(15, 16);
            lblCreatetxt.Name = "lblCreatetxt";
            lblCreatetxt.Size = new Size(115, 25);
            lblCreatetxt.TabIndex = 5;
            lblCreatetxt.Text = "Create Room";
            // 
            // timeWait
            // 
            timeWait.Interval = 500;
            // 
            // CreateRoom
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveBorder;
            Controls.Add(btnCancel);
            Controls.Add(lblWait);
            Controls.Add(lblID);
            Controls.Add(lblCreatetxt);
            Name = "CreateRoom";
            Size = new Size(340, 221);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCancel;
        private Label lblWait;
        private Label lblID;
        private Label lblCreatetxt;
        private System.Windows.Forms.Timer timeWait;
    }
}
