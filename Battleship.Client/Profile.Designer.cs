namespace Battleship.Client
{
    partial class Profile
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
            comboBox1 = new ComboBox();
            tabProfile = new TabControl();
            tabPageProfile = new TabPage();
            tabPageFriend = new TabPage();
            tabProfile.SuspendLayout();
            SuspendLayout();
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(28, 66);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(182, 33);
            comboBox1.TabIndex = 0;
            // 
            // tabProfile
            // 
            tabProfile.Controls.Add(tabPageProfile);
            tabProfile.Controls.Add(tabPageFriend);
            tabProfile.Location = new Point(0, 0);
            tabProfile.Name = "tabProfile";
            tabProfile.SelectedIndex = 0;
            tabProfile.Size = new Size(290, 300);
            tabProfile.SizeMode = TabSizeMode.Fixed;
            tabProfile.TabIndex = 1;
            // 
            // tabPageProfile
            // 
            tabPageProfile.Location = new Point(4, 34);
            tabPageProfile.Name = "tabPageProfile";
            tabPageProfile.Padding = new Padding(3);
            tabPageProfile.Size = new Size(282, 262);
            tabPageProfile.TabIndex = 0;
            tabPageProfile.Text = "Profile";
            tabPageProfile.UseVisualStyleBackColor = true;
            tabPageProfile.Click += tabPageProfile_Click;
            // 
            // tabPageFriend
            // 
            tabPageFriend.Location = new Point(4, 34);
            tabPageFriend.Name = "tabPageFriend";
            tabPageFriend.Padding = new Padding(3);
            tabPageFriend.Size = new Size(282, 262);
            tabPageFriend.TabIndex = 1;
            tabPageFriend.Text = "Friend";
            tabPageFriend.UseVisualStyleBackColor = true;
            // 
            // Profile
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabProfile);
            Controls.Add(comboBox1);
            Name = "Profile";
            Size = new Size(288, 317);
            tabProfile.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ComboBox comboBox1;
        private TabControl tabProfile;
        private TabPage tabPageProfile;
        private TabPage tabPageFriend;
    }
}
