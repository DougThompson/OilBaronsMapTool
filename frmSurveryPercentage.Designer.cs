namespace OilBaronsMapTool
{
	partial class frmSurveryPercentage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSurveryPercentage));
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.txtSurveyPercentage = new System.Windows.Forms.TextBox();
            this.picCurTerrain = new System.Windows.Forms.PictureBox();
            this.lblCurrentLocation = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picCurTerrain)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Enter Survey Percentage";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(159, 76);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(78, 76);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtSurveyPercentage
            // 
            this.txtSurveyPercentage.AcceptsReturn = true;
            this.txtSurveyPercentage.Location = new System.Drawing.Point(159, 40);
            this.txtSurveyPercentage.Name = "txtSurveyPercentage";
            this.txtSurveyPercentage.Size = new System.Drawing.Size(35, 20);
            this.txtSurveyPercentage.TabIndex = 4;
            // 
            // picCurTerrain
            // 
            this.picCurTerrain.Image = ((System.Drawing.Image)(resources.GetObject("picCurTerrain.Image")));
            this.picCurTerrain.Location = new System.Drawing.Point(6, 9);
            this.picCurTerrain.Name = "picCurTerrain";
            this.picCurTerrain.Size = new System.Drawing.Size(16, 16);
            this.picCurTerrain.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picCurTerrain.TabIndex = 5;
            this.picCurTerrain.TabStop = false;
            // 
            // lblCurrentLocation
            // 
            this.lblCurrentLocation.AutoSize = true;
            this.lblCurrentLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentLocation.Location = new System.Drawing.Point(28, 10);
            this.lblCurrentLocation.Name = "lblCurrentLocation";
            this.lblCurrentLocation.Size = new System.Drawing.Size(43, 15);
            this.lblCurrentLocation.TabIndex = 28;
            this.lblCurrentLocation.Text = "( -, - )";
            // 
            // frmSurveryPercentage
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(246, 111);
            this.Controls.Add(this.lblCurrentLocation);
            this.Controls.Add(this.picCurTerrain);
            this.Controls.Add(this.txtSurveyPercentage);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSurveryPercentage";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enter Survey Percentage";
            this.Load += new System.EventHandler(this.frmSurveryPercentage_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmSurveryPercentage_KeyUp);
            ((System.ComponentModel.ISupportInitialize)(this.picCurTerrain)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.TextBox txtSurveyPercentage;
        private System.Windows.Forms.PictureBox picCurTerrain;
        private System.Windows.Forms.Label lblCurrentLocation;
	}
}