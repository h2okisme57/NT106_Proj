namespace Battleship.Client
{
    partial class FindRandomRoom
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
            lblFindtxt = new Label();
            btnCancel = new Button();
            timeFind = new System.Windows.Forms.Timer(components);
            lblFindStatus = new Label();
            SuspendLayout();
            // 
            // lblFindtxt
            // 
            lblFindtxt.AutoSize = true;
            lblFindtxt.Location = new Point(3, 19);
            lblFindtxt.Name = "lblFindtxt";
            lblFindtxt.Size = new Size(133, 25);
            lblFindtxt.TabIndex = 0;
            lblFindtxt.Text = "Find Opponent";
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(84, 130);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(112, 34);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // timeFind
            // 
            timeFind.Interval = 500;
            // 
            // lblFindStatus
            // 
            lblFindStatus.AutoSize = true;
            lblFindStatus.Location = new Point(84, 78);
            lblFindStatus.Name = "lblFindStatus";
            lblFindStatus.Size = new Size(71, 25);
            lblFindStatus.TabIndex = 3;
            lblFindStatus.Text = "Finding";
            // 
            // FindRandomRoom
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblFindStatus);
            Controls.Add(btnCancel);
            Controls.Add(lblFindtxt);
            Name = "FindRandomRoom";
            Size = new Size(309, 180);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblFindtxt;
        private Button btnCancel;
        private System.Windows.Forms.Timer timeFind;
        private Label lblFindStatus;
    }
}
