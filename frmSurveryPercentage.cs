/*
Copyright (c) 2012 Doug Thompson

Permission is hereby granted, free of charge, to any person obtaining a
copy of this software and associated documentation files (the
"Software"),to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OilBaronsMapTool
{
	public partial class frmSurveryPercentage : Form
	{
		public int surveyPercentage = 0;
        public int x;
        public int y;
        public Bitmap curTerrain;
        public string terrainName;
        public string playerName;

		public frmSurveryPercentage()
		{
			InitializeComponent();
		}

		private void frmSurveryPercentage_Load(object sender, EventArgs e)
		{
			txtSurveyPercentage.Text = surveyPercentage.ToString();
            picCurTerrain.Image = curTerrain;
            lblCurrentLocation.Text = String.Format("{2} ({0}, {1}), {3}", x, y, terrainName, playerName);
            this.Show();
            Application.DoEvents();

			txtSurveyPercentage.Focus();
			//txtSurveyPercentage.SelectAll();
			txtSurveyPercentage.SelectionStart = 0;
			txtSurveyPercentage.SelectionLength = txtSurveyPercentage.Text.Length;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
            updateSurveyPercentage();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			surveyPercentage = int.Parse(txtSurveyPercentage.Text);
		}

        private void frmSurveryPercentage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOK_Click(btnOK as object, null);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                btnCancel_Click(btnCancel as object, null);
            }
        }

        private void updateSurveyPercentage()
        {
            int result = 0;
            if (int.TryParse(txtSurveyPercentage.Text, out result))
            {
                surveyPercentage = int.Parse(txtSurveyPercentage.Text);
            }
            else
            {
                surveyPercentage = 0;
            }
        }
	}
}
