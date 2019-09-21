using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoldierB.TrueType;
using System.IO;

namespace FontTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            if (FontDialog.ShowDialog() == DialogResult.OK)
            {
                Stopwatch timer = new Stopwatch();
                foreach (string fontFile in FontDialog.FileNames)
                {
                    string fontFileName = Path.GetFileName(fontFile);
                    FontInformation info = null;
                    Exception fontException = null;

                    timer.Restart();

                    try
                    {
                        info = FontInformation.Read(fontFile);
                    }
                    catch (Exception ex)
                    {
                        fontException = ex;
                    }

                    timer.Stop();

                    if (info != null)
                        OutputList.Items.Add($"{timer.Elapsed.TotalMilliseconds}ms {fontFileName} > {info.FamilyName} {info.Style}");
                    else
                        OutputList.Items.Add($"{timer.Elapsed.TotalMilliseconds}ms {fontFileName} > {fontException.Message}");
                }
            }
        }
    }
}
