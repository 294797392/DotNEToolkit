
namespace DotNEToolkitDemo.Forms
{
    partial class libVLCForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.ButtonEnumChildWindow = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonSetMarquueText = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panel1.Location = new System.Drawing.Point(41, 63);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(719, 462);
            this.panel1.TabIndex = 0;
            this.panel1.Click += new System.EventHandler(this.panel1_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(125, 557);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "放大";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(219, 557);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "缩小";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(116, 610);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(84, 26);
            this.ButtonStart.TabIndex = 3;
            this.ButtonStart.Text = "开始播放";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // ButtonStop
            // 
            this.ButtonStop.Location = new System.Drawing.Point(219, 610);
            this.ButtonStop.Name = "ButtonStop";
            this.ButtonStop.Size = new System.Drawing.Size(84, 26);
            this.ButtonStop.TabIndex = 4;
            this.ButtonStop.Text = "结束播放";
            this.ButtonStop.UseVisualStyleBackColor = true;
            this.ButtonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(331, 610);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "设置logo";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // ButtonEnumChildWindow
            // 
            this.ButtonEnumChildWindow.Location = new System.Drawing.Point(430, 610);
            this.ButtonEnumChildWindow.Name = "ButtonEnumChildWindow";
            this.ButtonEnumChildWindow.Size = new System.Drawing.Size(75, 23);
            this.ButtonEnumChildWindow.TabIndex = 6;
            this.ButtonEnumChildWindow.Text = "枚举子窗口";
            this.ButtonEnumChildWindow.UseVisualStyleBackColor = true;
            this.ButtonEnumChildWindow.Click += new System.EventHandler(this.ButtonEnumChildWindow_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(548, 532);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(212, 48);
            this.label1.TabIndex = 8;
            this.label1.Text = "qweqweqw";
            // 
            // buttonSetMarquueText
            // 
            this.buttonSetMarquueText.Location = new System.Drawing.Point(331, 653);
            this.buttonSetMarquueText.Name = "buttonSetMarquueText";
            this.buttonSetMarquueText.Size = new System.Drawing.Size(117, 23);
            this.buttonSetMarquueText.TabIndex = 9;
            this.buttonSetMarquueText.Text = "设置MarquueText";
            this.buttonSetMarquueText.UseVisualStyleBackColor = true;
            this.buttonSetMarquueText.Click += new System.EventHandler(this.buttonSetMarquueText_Click);
            // 
            // libVLCForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 716);
            this.Controls.Add(this.buttonSetMarquueText);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ButtonEnumChildWindow);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.ButtonStop);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "libVLCForm";
            this.Text = "libVLCForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.Button ButtonStop;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button ButtonEnumChildWindow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonSetMarquueText;
    }
}