namespace Server
{
    partial class ServerAreaSelector
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
            this.VideoPictureBox = new System.Windows.Forms.PictureBox();
            this.TopTrackBar = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BottomTrackBar = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.LeftTrackBar = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.RightTrackBar = new System.Windows.Forms.TrackBar();
            this.OkButton = new System.Windows.Forms.Button();
            this.CancelAreaButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.VideoPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LeftTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RightTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // VideoPictureBox
            // 
            this.VideoPictureBox.Location = new System.Drawing.Point(12, 12);
            this.VideoPictureBox.Name = "VideoPictureBox";
            this.VideoPictureBox.Size = new System.Drawing.Size(368, 220);
            this.VideoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.VideoPictureBox.TabIndex = 1;
            this.VideoPictureBox.TabStop = false;
            // 
            // TopTrackBar
            // 
            this.TopTrackBar.Location = new System.Drawing.Point(83, 255);
            this.TopTrackBar.Name = "TopTrackBar";
            this.TopTrackBar.Size = new System.Drawing.Size(104, 45);
            this.TopTrackBar.TabIndex = 3;
            this.TopTrackBar.Scroll += new System.EventHandler(this.TopTrackBar_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 255);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Top";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 306);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Bottom";
            // 
            // BottomTrackBar
            // 
            this.BottomTrackBar.Location = new System.Drawing.Point(83, 306);
            this.BottomTrackBar.Name = "BottomTrackBar";
            this.BottomTrackBar.Size = new System.Drawing.Size(104, 45);
            this.BottomTrackBar.TabIndex = 5;
            this.BottomTrackBar.Scroll += new System.EventHandler(this.BottomTrackBar_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(205, 255);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Left";
            // 
            // LeftTrackBar
            // 
            this.LeftTrackBar.Location = new System.Drawing.Point(258, 255);
            this.LeftTrackBar.Name = "LeftTrackBar";
            this.LeftTrackBar.Size = new System.Drawing.Size(104, 45);
            this.LeftTrackBar.TabIndex = 7;
            this.LeftTrackBar.Scroll += new System.EventHandler(this.LeftTrackBar_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(205, 306);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Right";
            // 
            // RightTrackBar
            // 
            this.RightTrackBar.Location = new System.Drawing.Point(258, 306);
            this.RightTrackBar.Name = "RightTrackBar";
            this.RightTrackBar.Size = new System.Drawing.Size(104, 45);
            this.RightTrackBar.TabIndex = 9;
            this.RightTrackBar.Scroll += new System.EventHandler(this.RightTrackBar_Scroll);
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(208, 362);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(75, 23);
            this.OkButton.TabIndex = 11;
            this.OkButton.Text = "Ok";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // CancelAreaButton
            // 
            this.CancelAreaButton.Location = new System.Drawing.Point(305, 362);
            this.CancelAreaButton.Name = "CancelAreaButton";
            this.CancelAreaButton.Size = new System.Drawing.Size(75, 23);
            this.CancelAreaButton.TabIndex = 12;
            this.CancelAreaButton.Text = "Cancel";
            this.CancelAreaButton.UseVisualStyleBackColor = true;
            this.CancelAreaButton.Click += new System.EventHandler(this.CancelAreaButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(112, 362);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(75, 23);
            this.ResetButton.TabIndex = 13;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // ServerAreaSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 397);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.CancelAreaButton);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.RightTrackBar);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.LeftTrackBar);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BottomTrackBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TopTrackBar);
            this.Controls.Add(this.VideoPictureBox);
            this.Name = "ServerAreaSelector";
            this.Text = "Select area to stream";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ServerAreaSelector_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.VideoPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TopTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BottomTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LeftTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RightTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox VideoPictureBox;
        private System.Windows.Forms.TrackBar TopTrackBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar BottomTrackBar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar LeftTrackBar;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar RightTrackBar;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelAreaButton;
        private System.Windows.Forms.Button ResetButton;
    }
}