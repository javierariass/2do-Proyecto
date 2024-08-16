using System;
using System.Windows.Forms;

namespace CardCompiler
{
    public partial class IDE : Form
    {

        public IDE()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SintaxBack process = new SintaxBack();
            richTextBox2.Text = "";
            process.CleanSintax();
            
            process.VerifcateSintax(richTextBox1.Text);
            if (process.error.Count <= 0)
            {
                foreach(string s in process.TextImpress)
                {
                    richTextBox2.Text += s + "\n";
                }
                richTextBox2.Text += "Build Succed";
            }

            else
            {
                foreach (ErrorBack lines in process.error)
                {
                    richTextBox2.Text += lines.typeError + "   Line: " + lines.Line +"\n";
                }
            }
        }
    }
}
