namespace Battleship.Client
{
    partial class BoardGame
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
            tableGame.Location = new Point(0, 0);
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
            // BoardRoom
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableGame);
            Name = "BoardRoom";
            Size = new Size(644, 644);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableGame;
    }
}
