namespace Tencent.FormControls {
    partial class VideoCtrl {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoCtrl));
            this.axNvrCtrl1 = new AxNvrViewerLib.AxNvrCtrl();
            ((System.ComponentModel.ISupportInitialize)(this.axNvrCtrl1)).BeginInit();
            this.SuspendLayout();
            // 
            // axNvrCtrl1
            // 
            this.axNvrCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axNvrCtrl1.Enabled = true;
            this.axNvrCtrl1.Location = new System.Drawing.Point(0, 0);
            this.axNvrCtrl1.Name = "axNvrCtrl1";
            this.axNvrCtrl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axNvrCtrl1.OcxState")));
            this.axNvrCtrl1.Size = new System.Drawing.Size(300, 227);
            this.axNvrCtrl1.TabIndex = 0;
            // 
            // VideoCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.axNvrCtrl1);
            this.Name = "VideoCtrl";
            this.Size = new System.Drawing.Size(300, 227);
            ((System.ComponentModel.ISupportInitialize)(this.axNvrCtrl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxNvrViewerLib.AxNvrCtrl axNvrCtrl1;
    }
}
