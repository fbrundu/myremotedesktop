namespace Client
{
    partial class ClientChatClipboard
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
            this.HistoryTextBox = new System.Windows.Forms.TextBox();
            this.ChatTextBox = new System.Windows.Forms.TextBox();
            this.ChatSendButton = new System.Windows.Forms.Button();
            this.ClipboardPasteButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // HistoryTextBox
            // 
            this.HistoryTextBox.Location = new System.Drawing.Point(12, 12);
            this.HistoryTextBox.Multiline = true;
            this.HistoryTextBox.Name = "HistoryTextBox";
            this.HistoryTextBox.ReadOnly = true;
            this.HistoryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.HistoryTextBox.Size = new System.Drawing.Size(268, 94);
            this.HistoryTextBox.TabIndex = 10;
            // 
            // ChatTextBox
            // 
            this.ChatTextBox.Location = new System.Drawing.Point(12, 112);
            this.ChatTextBox.MaxLength = 160;
            this.ChatTextBox.Multiline = true;
            this.ChatTextBox.Name = "ChatTextBox";
            this.ChatTextBox.Size = new System.Drawing.Size(268, 46);
            this.ChatTextBox.TabIndex = 0;
            // 
            // ChatSendButton
            // 
            this.ChatSendButton.Location = new System.Drawing.Point(205, 165);
            this.ChatSendButton.Name = "ChatSendButton";
            this.ChatSendButton.Size = new System.Drawing.Size(75, 23);
            this.ChatSendButton.TabIndex = 1;
            this.ChatSendButton.Text = "Send";
            this.ChatSendButton.UseVisualStyleBackColor = true;
            this.ChatSendButton.Click += new System.EventHandler(this.ChatSendButton_Click);
            // 
            // ClipboardPasteButton
            // 
            this.ClipboardPasteButton.Location = new System.Drawing.Point(12, 165);
            this.ClipboardPasteButton.Name = "ClipboardPasteButton";
            this.ClipboardPasteButton.Size = new System.Drawing.Size(75, 23);
            this.ClipboardPasteButton.TabIndex = 11;
            this.ClipboardPasteButton.Text = "Paste";
            this.ClipboardPasteButton.UseVisualStyleBackColor = true;
            this.ClipboardPasteButton.Click += new System.EventHandler(this.ClipboardPasteButton_Click);
            // 
            // ClientChatClipboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.ClipboardPasteButton);
            this.Controls.Add(this.ChatSendButton);
            this.Controls.Add(this.ChatTextBox);
            this.Controls.Add(this.HistoryTextBox);
            this.Name = "ClientChatClipboard";
            this.Text = "Client Chat/Clipboard Window";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ClientChatClipboard_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox HistoryTextBox;
        private System.Windows.Forms.TextBox ChatTextBox;
        private System.Windows.Forms.Button ChatSendButton;
        private System.Windows.Forms.Button ClipboardPasteButton;
    }
}