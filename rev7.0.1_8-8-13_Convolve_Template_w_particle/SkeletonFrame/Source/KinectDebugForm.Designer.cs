namespace KinectHand
{
    partial class KinectDebugForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KinectDebugForm));
            this.MessageBox = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.VersionInfo = new System.Windows.Forms.TextBox();
            this.FPSInfo = new System.Windows.Forms.TextBox();
            this.HandDepth = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // MessageBox
            // 
            this.MessageBox.AutoEllipsis = true;
            this.MessageBox.BackColor = System.Drawing.Color.Transparent;
            this.MessageBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MessageBox.ForeColor = System.Drawing.Color.White;
            this.MessageBox.Location = new System.Drawing.Point(12, 840);
            this.MessageBox.Name = "MessageBox";
            this.MessageBox.Size = new System.Drawing.Size(1240, 73);
            this.MessageBox.TabIndex = 4;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // VersionInfo
            // 
            this.VersionInfo.BackColor = System.Drawing.Color.Black;
            this.VersionInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.VersionInfo.ForeColor = System.Drawing.Color.White;
            this.VersionInfo.Location = new System.Drawing.Point(644, 174);
            this.VersionInfo.Multiline = true;
            this.VersionInfo.Name = "VersionInfo";
            this.VersionInfo.Size = new System.Drawing.Size(144, 40);
            this.VersionInfo.TabIndex = 6;
            this.VersionInfo.Text = "Version:";
            // 
            // FPSInfo
            // 
            this.FPSInfo.BackColor = System.Drawing.Color.Black;
            this.FPSInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FPSInfo.ForeColor = System.Drawing.Color.White;
            this.FPSInfo.Location = new System.Drawing.Point(644, 219);
            this.FPSInfo.Name = "FPSInfo";
            this.FPSInfo.Size = new System.Drawing.Size(144, 13);
            this.FPSInfo.TabIndex = 7;
            this.FPSInfo.Text = "FPS: ";
            // 
            // HandDepth
            // 
            this.HandDepth.BackColor = System.Drawing.Color.Black;
            this.HandDepth.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.HandDepth.ForeColor = System.Drawing.Color.White;
            this.HandDepth.Location = new System.Drawing.Point(644, 238);
            this.HandDepth.Name = "HandDepth";
            this.HandDepth.Size = new System.Drawing.Size(144, 13);
            this.HandDepth.TabIndex = 8;
            this.HandDepth.Text = "Hand Depth:";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.ForeColor = System.Drawing.Color.White;
            this.checkBox1.Location = new System.Drawing.Point(644, 258);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(83, 17);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Show Angle";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.ForeColor = System.Drawing.Color.White;
            this.checkBox2.Location = new System.Drawing.Point(644, 281);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(97, 17);
            this.checkBox2.TabIndex = 10;
            this.checkBox2.Text = "Left Hand Only";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // KinectDebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(800, 480);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.HandDepth);
            this.Controls.Add(this.FPSInfo);
            this.Controls.Add(this.VersionInfo);
            this.Controls.Add(this.MessageBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "KinectDebugForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KinectDebug";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label MessageBox;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox VersionInfo;
        private System.Windows.Forms.TextBox FPSInfo;
        private System.Windows.Forms.TextBox HandDepth;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
    }


}

