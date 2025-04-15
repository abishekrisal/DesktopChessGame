namespace WindowsFormsApp1
{
    partial class ChessBoard
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chessboardpanel = new System.Windows.Forms.Panel();
            this.txtPlayerTurn = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnRestart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chessboardpanel
            // 
            this.chessboardpanel.Location = new System.Drawing.Point(8, 63);
            this.chessboardpanel.Name = "chessboardpanel";
            this.chessboardpanel.Size = new System.Drawing.Size(1196, 852);
            this.chessboardpanel.TabIndex = 0;
            // 
            // txtPlayerTurn
            // 
            this.txtPlayerTurn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPlayerTurn.Location = new System.Drawing.Point(177, 17);
            this.txtPlayerTurn.Name = "txtPlayerTurn";
            this.txtPlayerTurn.ReadOnly = true;
            this.txtPlayerTurn.Size = new System.Drawing.Size(232, 30);
            this.txtPlayerTurn.TabIndex = 4;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(44, 17);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(128, 30);
            this.textBox1.TabIndex = 5;
            this.textBox1.Text = "PlayerTurn:";
            // 
            // btnRestart
            // 
            this.btnRestart.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRestart.Location = new System.Drawing.Point(510, 11);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(225, 40);
            this.btnRestart.TabIndex = 6;
            this.btnRestart.Text = "RestartGame";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // ChessBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1226, 926);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.txtPlayerTurn);
            this.Controls.Add(this.chessboardpanel);
            this.Name = "ChessBoard";
            this.Text = "ChessGame";
            this.Load += new System.EventHandler(this.Mainform_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel chessboardpanel;
        private System.Windows.Forms.TextBox txtPlayerTurn;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnRestart;
    }
}

