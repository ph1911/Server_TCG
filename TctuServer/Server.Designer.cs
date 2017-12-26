namespace FctServer
{
    partial class Server
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
            if (disposing && (components != null)) {
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server));
            this.OutputTB = new System.Windows.Forms.TextBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.ConnectionTimer = new System.Windows.Forms.Timer(this.components);
            this.ClientListBox = new System.Windows.Forms.ListBox();
            this.IncomingDataListBox = new System.Windows.Forms.ListBox();
            this.SendListBox = new System.Windows.Forms.ListBox();
            this.Fighter1HPTB = new System.Windows.Forms.TextBox();
            this.Fighter2HPTB = new System.Windows.Forms.TextBox();
            this.Fighter2CardsTB = new System.Windows.Forms.TextBox();
            this.Fighter1CardsTB = new System.Windows.Forms.TextBox();
            this.BattleTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // OutputTB
            // 
            this.OutputTB.Location = new System.Drawing.Point(305, 12);
            this.OutputTB.Name = "OutputTB";
            this.OutputTB.Size = new System.Drawing.Size(373, 20);
            this.OutputTB.TabIndex = 0;
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(7, 6);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(143, 73);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(156, 6);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(143, 73);
            this.StopButton.TabIndex = 2;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // ConnectionTimer
            // 
            this.ConnectionTimer.Enabled = true;
            this.ConnectionTimer.Interval = 1000;
            this.ConnectionTimer.Tick += new System.EventHandler(this.ConnectionTimer_Tick);
            // 
            // ClientListBox
            // 
            this.ClientListBox.FormattingEnabled = true;
            this.ClientListBox.Location = new System.Drawing.Point(305, 38);
            this.ClientListBox.Name = "ClientListBox";
            this.ClientListBox.Size = new System.Drawing.Size(373, 121);
            this.ClientListBox.TabIndex = 4;
            // 
            // IncomingDataListBox
            // 
            this.IncomingDataListBox.FormattingEnabled = true;
            this.IncomingDataListBox.Location = new System.Drawing.Point(12, 179);
            this.IncomingDataListBox.Name = "IncomingDataListBox";
            this.IncomingDataListBox.Size = new System.Drawing.Size(311, 277);
            this.IncomingDataListBox.TabIndex = 5;
            // 
            // SendListBox
            // 
            this.SendListBox.FormattingEnabled = true;
            this.SendListBox.Location = new System.Drawing.Point(343, 179);
            this.SendListBox.Name = "SendListBox";
            this.SendListBox.Size = new System.Drawing.Size(335, 277);
            this.SendListBox.TabIndex = 6;
            // 
            // Fighter1HPTB
            // 
            this.Fighter1HPTB.Location = new System.Drawing.Point(12, 85);
            this.Fighter1HPTB.Name = "Fighter1HPTB";
            this.Fighter1HPTB.Size = new System.Drawing.Size(100, 20);
            this.Fighter1HPTB.TabIndex = 7;
            // 
            // Fighter2HPTB
            // 
            this.Fighter2HPTB.Location = new System.Drawing.Point(199, 85);
            this.Fighter2HPTB.Name = "Fighter2HPTB";
            this.Fighter2HPTB.Size = new System.Drawing.Size(100, 20);
            this.Fighter2HPTB.TabIndex = 8;
            // 
            // Fighter2CardsTB
            // 
            this.Fighter2CardsTB.Location = new System.Drawing.Point(199, 111);
            this.Fighter2CardsTB.Name = "Fighter2CardsTB";
            this.Fighter2CardsTB.Size = new System.Drawing.Size(100, 20);
            this.Fighter2CardsTB.TabIndex = 9;
            // 
            // Fighter1CardsTB
            // 
            this.Fighter1CardsTB.Location = new System.Drawing.Point(12, 111);
            this.Fighter1CardsTB.Name = "Fighter1CardsTB";
            this.Fighter1CardsTB.Size = new System.Drawing.Size(100, 20);
            this.Fighter1CardsTB.TabIndex = 10;
            // 
            // BattleTimer
            // 
            this.BattleTimer.Interval = 1000;
            this.BattleTimer.Tick += new System.EventHandler(this.BattleTimer_Tick);
            // 
            // Server
            // 
            this.AcceptButton = this.StartButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.StopButton;
            this.ClientSize = new System.Drawing.Size(690, 468);
            this.Controls.Add(this.Fighter1CardsTB);
            this.Controls.Add(this.Fighter2CardsTB);
            this.Controls.Add(this.Fighter2HPTB);
            this.Controls.Add(this.Fighter1HPTB);
            this.Controls.Add(this.SendListBox);
            this.Controls.Add(this.IncomingDataListBox);
            this.Controls.Add(this.ClientListBox);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.OutputTB);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Server";
            this.Text = "FCT Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox OutputTB;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Timer ConnectionTimer;
        private System.Windows.Forms.ListBox ClientListBox;
        public System.Windows.Forms.TextBox Fighter1HPTB;
        public System.Windows.Forms.TextBox Fighter2HPTB;
        public System.Windows.Forms.TextBox Fighter2CardsTB;
        public System.Windows.Forms.TextBox Fighter1CardsTB;
        public System.Windows.Forms.ListBox IncomingDataListBox;
        public System.Windows.Forms.ListBox SendListBox;
        private System.Windows.Forms.Timer BattleTimer;
    }
}

