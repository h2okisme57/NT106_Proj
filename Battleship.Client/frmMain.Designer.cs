namespace Battleship.Client;

partial class frmMain
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        pnlContainer = new Panel();
        SuspendLayout();
        // 
        // pnlContainer
        // 
        pnlContainer.Dock = DockStyle.Fill;
        pnlContainer.Location = new Point(0, 0);
        pnlContainer.Name = "pnlContainer";
        pnlContainer.Size = new Size(978, 644);
        pnlContainer.TabIndex = 0;
        pnlContainer.Paint += pnlContainer_Paint;
        // 
        // frmMain
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(978, 644);
        Controls.Add(pnlContainer);
        Name = "frmMain";
        Text = "Battleship";
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlContainer;
}
