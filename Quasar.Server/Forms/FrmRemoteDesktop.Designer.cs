using Quasar.Server.Controls;

namespace Quasar.Server.Forms
{
    partial class FrmRemoteDesktop
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRemoteDesktop));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.barQuality = new System.Windows.Forms.TrackBar();
            this.lblQuality = new System.Windows.Forms.Label();
            this.lblQualityShow = new System.Windows.Forms.Label();
            this.btnMouse = new System.Windows.Forms.Button();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblKernelDriverState = new System.Windows.Forms.Label();
            this.chkIncludeCursor = new System.Windows.Forms.CheckBox();
            this.chkForceAffinity = new System.Windows.Forms.CheckBox();
            this.chkRequireDriver = new System.Windows.Forms.CheckBox();
            this.lblKernelTarget = new System.Windows.Forms.Label();
            this.cbKernelTargets = new System.Windows.Forms.ComboBox();
            this.btnKernelUnblock = new System.Windows.Forms.Button();
            this.btnRefreshDriverStatus = new System.Windows.Forms.Button();
            this.btnKeyboard = new System.Windows.Forms.Button();
            this.cbMonitors = new System.Windows.Forms.ComboBox();
            this.btnHide = new System.Windows.Forms.Button();
            this.btnShow = new System.Windows.Forms.Button();
            this.toolTipButtons = new System.Windows.Forms.ToolTip(this.components);
            this.picDesktop = new RapidPictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).BeginInit();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDesktop)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(15, 5);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(68, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.TabStop = false;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(96, 5);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(68, 23);
            this.btnStop.TabIndex = 2;
            this.btnStop.TabStop = false;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // barQuality
            // 
            this.barQuality.Location = new System.Drawing.Point(206, -1);
            this.barQuality.Maximum = 100;
            this.barQuality.Minimum = 1;
            this.barQuality.Name = "barQuality";
            this.barQuality.Size = new System.Drawing.Size(76, 45);
            this.barQuality.TabIndex = 3;
            this.barQuality.TabStop = false;
            this.barQuality.Value = 75;
            this.barQuality.Scroll += new System.EventHandler(this.barQuality_Scroll);
            // 
            // lblQuality
            // 
            this.lblQuality.AutoSize = true;
            this.lblQuality.Location = new System.Drawing.Point(167, 5);
            this.lblQuality.Name = "lblQuality";
            this.lblQuality.Size = new System.Drawing.Size(46, 13);
            this.lblQuality.TabIndex = 4;
            this.lblQuality.Text = "Quality:";
            // 
            // lblQualityShow
            // 
            this.lblQualityShow.AutoSize = true;
            this.lblQualityShow.Location = new System.Drawing.Point(220, 26);
            this.lblQualityShow.Name = "lblQualityShow";
            this.lblQualityShow.Size = new System.Drawing.Size(52, 13);
            this.lblQualityShow.TabIndex = 5;
            this.lblQualityShow.Text = "75 (high)";
            // 
            // btnMouse
            // 
            this.btnMouse.Image = global::Quasar.Server.Properties.Resources.mouse_delete;
            this.btnMouse.Location = new System.Drawing.Point(470, 32);
            this.btnMouse.Name = "btnMouse";
            this.btnMouse.Size = new System.Drawing.Size(28, 28);
            this.btnMouse.TabIndex = 6;
            this.btnMouse.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnMouse, "Enable mouse input.");
            this.btnMouse.UseVisualStyleBackColor = true;
            this.btnMouse.Click += new System.EventHandler(this.btnMouse_Click);
            // 
            // panelTop
            // 
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.lblKernelDriverState);
            this.panelTop.Controls.Add(this.lblInputStatus);
            this.panelTop.Controls.Add(this.lblKernelTarget);
            this.panelTop.Controls.Add(this.cbKernelTargets);
            this.panelTop.Controls.Add(this.btnInputUnblock);
            this.panelTop.Controls.Add(this.chkInputKeyboard);
            this.panelTop.Controls.Add(this.chkInputMouse);
            this.panelTop.Controls.Add(this.btnKernelUnblock);
            this.panelTop.Controls.Add(this.btnRefreshDriverStatus);
            this.panelTop.Controls.Add(this.btnKeyboard);
            this.panelTop.Controls.Add(this.cbMonitors);
            this.panelTop.Controls.Add(this.btnHide);
            this.panelTop.Controls.Add(this.chkRequireDriver);
            this.panelTop.Controls.Add(this.chkForceAffinity);
            this.panelTop.Controls.Add(this.chkIncludeCursor);
            this.panelTop.Controls.Add(this.lblQualityShow);
            this.panelTop.Controls.Add(this.btnMouse);
            this.panelTop.Controls.Add(this.btnStart);
            this.panelTop.Controls.Add(this.btnStop);
            this.panelTop.Controls.Add(this.lblQuality);
            this.panelTop.Controls.Add(this.barQuality);
            this.panelTop.Location = new System.Drawing.Point(82, -1);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(620, 132);
            this.panelTop.TabIndex = 7;
            // 
            // lblKernelDriverState
            // 
            this.lblKernelDriverState.AutoSize = true;
            this.lblKernelDriverState.Location = new System.Drawing.Point(302, 9);
            this.lblKernelDriverState.Name = "lblKernelDriverState";
            this.lblKernelDriverState.Size = new System.Drawing.Size(92, 13);
            this.lblKernelDriverState.TabIndex = 12;
            this.lblKernelDriverState.Text = "Driver: Unknown";
            // 
            // lblInputStatus
            // 
            this.lblInputStatus.AutoSize = true;
            this.lblInputStatus.Location = new System.Drawing.Point(302, 110);
            this.lblInputStatus.Name = "lblInputStatus";
            this.lblInputStatus.Size = new System.Drawing.Size(122, 13);
            this.lblInputStatus.TabIndex = 18;
            this.lblInputStatus.Text = "Input status: Unknown";
            // 
            // lblKernelTarget
            // 
            this.lblKernelTarget.AutoSize = true;
            this.lblKernelTarget.Location = new System.Drawing.Point(170, 37);
            this.lblKernelTarget.Name = "lblKernelTarget";
            this.lblKernelTarget.Size = new System.Drawing.Size(82, 13);
            this.lblKernelTarget.TabIndex = 13;
            this.lblKernelTarget.Text = "Kernel target:";
            // 
            // cbKernelTargets
            // 
            this.cbKernelTargets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbKernelTargets.FormattingEnabled = true;
            this.cbKernelTargets.Location = new System.Drawing.Point(262, 34);
            this.cbKernelTargets.Name = "cbKernelTargets";
            this.cbKernelTargets.Size = new System.Drawing.Size(120, 21);
            this.cbKernelTargets.TabIndex = 14;
            this.cbKernelTargets.TabStop = false;
            // 
            // btnInputUnblock
            // 
            this.btnInputUnblock.Location = new System.Drawing.Point(503, 87);
            this.btnInputUnblock.Name = "btnInputUnblock";
            this.btnInputUnblock.Size = new System.Drawing.Size(84, 23);
            this.btnInputUnblock.TabIndex = 21;
            this.btnInputUnblock.TabStop = false;
            this.btnInputUnblock.Text = "Unblock Input";
            this.btnInputUnblock.UseVisualStyleBackColor = true;
            this.btnInputUnblock.Click += new System.EventHandler(this.btnInputUnblock_Click);
            // 
            // chkInputKeyboard
            // 
            this.chkInputKeyboard.AutoSize = true;
            this.chkInputKeyboard.Checked = true;
            this.chkInputKeyboard.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInputKeyboard.Location = new System.Drawing.Point(242, 108);
            this.chkInputKeyboard.Name = "chkInputKeyboard";
            this.chkInputKeyboard.Size = new System.Drawing.Size(69, 17);
            this.chkInputKeyboard.TabIndex = 20;
            this.chkInputKeyboard.TabStop = false;
            this.chkInputKeyboard.Text = "Keyboard";
            this.chkInputKeyboard.UseVisualStyleBackColor = true;
            // 
            // chkInputMouse
            // 
            this.chkInputMouse.AutoSize = true;
            this.chkInputMouse.Checked = true;
            this.chkInputMouse.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInputMouse.Location = new System.Drawing.Point(175, 108);
            this.chkInputMouse.Name = "chkInputMouse";
            this.chkInputMouse.Size = new System.Drawing.Size(61, 17);
            this.chkInputMouse.TabIndex = 19;
            this.chkInputMouse.TabStop = false;
            this.chkInputMouse.Text = "Mouse";
            this.chkInputMouse.UseVisualStyleBackColor = true;
            // 
            // btnKernelUnblock
            // 
            this.btnKernelUnblock.Location = new System.Drawing.Point(438, 32);
            this.btnKernelUnblock.Name = "btnKernelUnblock";
            this.btnKernelUnblock.Size = new System.Drawing.Size(76, 23);
            this.btnKernelUnblock.TabIndex = 11;
            this.btnKernelUnblock.TabStop = false;
            this.btnKernelUnblock.Text = "Unblock";
            this.btnKernelUnblock.UseVisualStyleBackColor = true;
            this.toolTipButtons.SetToolTip(this.btnKernelUnblock, "Reset SetWindowDisplayAffinity for the selected process.");
            this.btnKernelUnblock.Click += new System.EventHandler(this.btnKernelUnblock_Click);
            // 
            // btnRefreshDriverStatus
            // 
            this.btnRefreshDriverStatus.Location = new System.Drawing.Point(460, 5);
            this.btnRefreshDriverStatus.Name = "btnRefreshDriverStatus";
            this.btnRefreshDriverStatus.Size = new System.Drawing.Size(85, 23);
            this.btnRefreshDriverStatus.TabIndex = 10;
            this.btnRefreshDriverStatus.TabStop = false;
            this.btnRefreshDriverStatus.Text = "Refresh";
            this.btnRefreshDriverStatus.UseVisualStyleBackColor = true;
            this.btnRefreshDriverStatus.Click += new System.EventHandler(this.btnRefreshDriverStatus_Click);
            // 
            // btnKeyboard
            // 
            this.btnKeyboard.Image = global::Quasar.Server.Properties.Resources.keyboard_delete;
            this.btnKeyboard.Location = new System.Drawing.Point(504, 32);
            this.btnKeyboard.Name = "btnKeyboard";
            this.btnKeyboard.Size = new System.Drawing.Size(28, 28);
            this.btnKeyboard.TabIndex = 9;
            this.btnKeyboard.TabStop = false;
            this.toolTipButtons.SetToolTip(this.btnKeyboard, "Enable keyboard input.");
            this.btnKeyboard.UseVisualStyleBackColor = true;
            this.btnKeyboard.Click += new System.EventHandler(this.btnKeyboard_Click);
            // 
            // cbMonitors
            // 
            this.cbMonitors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMonitors.FormattingEnabled = true;
            this.cbMonitors.Location = new System.Drawing.Point(15, 33);
            this.cbMonitors.Name = "cbMonitors";
            this.cbMonitors.Size = new System.Drawing.Size(149, 21);
            this.cbMonitors.TabIndex = 8;
            this.cbMonitors.TabStop = false;
            // 
            // btnHide
            // 
            this.btnHide.Location = new System.Drawing.Point(15, 80);
            this.btnHide.Name = "btnHide";
            this.btnHide.Size = new System.Drawing.Size(54, 19);
            this.btnHide.TabIndex = 7;
            this.btnHide.TabStop = false;
            this.btnHide.Text = "Hide";
            this.btnHide.UseVisualStyleBackColor = true;
            this.btnHide.Click += new System.EventHandler(this.btnHide_Click);
            // 
            // chkIncludeCursor
            // 
            this.chkIncludeCursor.AutoSize = true;
            this.chkIncludeCursor.Checked = true;
            this.chkIncludeCursor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeCursor.Location = new System.Drawing.Point(175, 61);
            this.chkIncludeCursor.Name = "chkIncludeCursor";
            this.chkIncludeCursor.Size = new System.Drawing.Size(103, 17);
            this.chkIncludeCursor.TabIndex = 15;
            this.chkIncludeCursor.TabStop = false;
            this.chkIncludeCursor.Text = "Show cursor";
            this.chkIncludeCursor.UseVisualStyleBackColor = true;
            this.chkIncludeCursor.CheckedChanged += new System.EventHandler(this.chkIncludeCursor_CheckedChanged);
            // 
            // chkForceAffinity
            // 
            this.chkForceAffinity.AutoSize = true;
            this.chkForceAffinity.Location = new System.Drawing.Point(300, 61);
            this.chkForceAffinity.Name = "chkForceAffinity";
            this.chkForceAffinity.Size = new System.Drawing.Size(127, 17);
            this.chkForceAffinity.TabIndex = 16;
            this.chkForceAffinity.TabStop = false;
            this.chkForceAffinity.Text = "Force unblock pass";
            this.chkForceAffinity.UseVisualStyleBackColor = true;
            this.chkForceAffinity.CheckedChanged += new System.EventHandler(this.chkForceAffinity_CheckedChanged);
            // 
            // chkRequireDriver
            // 
            this.chkRequireDriver.AutoSize = true;
            this.chkRequireDriver.Checked = true;
            this.chkRequireDriver.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRequireDriver.Location = new System.Drawing.Point(446, 61);
            this.chkRequireDriver.Name = "chkRequireDriver";
            this.chkRequireDriver.Size = new System.Drawing.Size(99, 17);
            this.chkRequireDriver.TabIndex = 17;
            this.chkRequireDriver.TabStop = false;
            this.chkRequireDriver.Text = "Require driver";
            this.chkRequireDriver.UseVisualStyleBackColor = true;
            this.chkRequireDriver.CheckedChanged += new System.EventHandler(this.chkRequireDriver_CheckedChanged);
            // 
            // btnShow
            // 
            this.btnShow.Location = new System.Drawing.Point(0, 0);
            this.btnShow.Name = "btnShow";
            this.btnShow.Size = new System.Drawing.Size(54, 19);
            this.btnShow.TabIndex = 8;
            this.btnShow.TabStop = false;
            this.btnShow.Text = "Show";
            this.btnShow.UseVisualStyleBackColor = true;
            this.btnShow.Visible = false;
            this.btnShow.Click += new System.EventHandler(this.btnShow_Click);
            // 
            // picDesktop
            // 
            this.picDesktop.BackColor = System.Drawing.Color.Black;
            this.picDesktop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picDesktop.Cursor = System.Windows.Forms.Cursors.Default;
            this.picDesktop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picDesktop.GetImageSafe = null;
            this.picDesktop.Location = new System.Drawing.Point(0, 0);
            this.picDesktop.Name = "picDesktop";
            this.picDesktop.Running = false;
            this.picDesktop.Size = new System.Drawing.Size(784, 562);
            this.picDesktop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDesktop.TabIndex = 0;
            this.picDesktop.TabStop = false;
            this.picDesktop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picDesktop_MouseDown);
            this.picDesktop.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picDesktop_MouseMove);
            this.picDesktop.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picDesktop_MouseUp);
            // 
            // FrmRemoteDesktop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.btnShow);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.picDesktop);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "FrmRemoteDesktop";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Remote Desktop []";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmRemoteDesktop_FormClosing);
            this.Load += new System.EventHandler(this.FrmRemoteDesktop_Load);
            this.Resize += new System.EventHandler(this.FrmRemoteDesktop_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.barQuality)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picDesktop)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.TrackBar barQuality;
        private System.Windows.Forms.Label lblQuality;
        private System.Windows.Forms.Label lblQualityShow;
        private System.Windows.Forms.Button btnMouse;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblKernelDriverState;
        private System.Windows.Forms.Label lblInputStatus;
        private System.Windows.Forms.Label lblKernelTarget;
        private System.Windows.Forms.ComboBox cbKernelTargets;
        private System.Windows.Forms.Button btnInputUnblock;
        private System.Windows.Forms.CheckBox chkInputKeyboard;
        private System.Windows.Forms.CheckBox chkInputMouse;
        private System.Windows.Forms.Button btnKernelUnblock;
        private System.Windows.Forms.Button btnRefreshDriverStatus;
        private System.Windows.Forms.Button btnHide;
        private System.Windows.Forms.Button btnShow;
        private System.Windows.Forms.ComboBox cbMonitors;
        private System.Windows.Forms.Button btnKeyboard;
        private System.Windows.Forms.CheckBox chkIncludeCursor;
        private System.Windows.Forms.CheckBox chkForceAffinity;
        private System.Windows.Forms.CheckBox chkRequireDriver;
        private System.Windows.Forms.ToolTip toolTipButtons;
        private Controls.RapidPictureBox picDesktop;
    }
}
