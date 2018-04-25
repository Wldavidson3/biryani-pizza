using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace camaraTrans
{
    public partial class Form1 : Form
    {
        string fileDirectory;
        string filePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (filePath == null)
            {
                MessageBox.Show("Please select the old input file.", "File Not Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string line;
                

                StreamWriter writer = new StreamWriter(fileDirectory + "New Input.lng");

                BoilerPlate(writer);

                StreamReader reader =
                    new StreamReader(filePath);

                Regex regex1 = new Regex(@"\d{3}\)\s\s");
                Regex regex2 = new Regex(@"\d{3}\)\s[a-zA-Z]");

                while ((line = reader.ReadLine()) != null)
                {

                    StringBuilder sb = new StringBuilder(line);

                    if (sb.ToString().Contains("MODEL"))
                    {
                        sb.Remove(0, 6);
                    }
                    if (sb.ToString().Contains("Intake:") || sb.ToString().Contains("constraints") || sb.ToString().Contains("objective:") || sb.ToString().Contains("equations") || sb.ToString().Contains("objective"))
                    {
                        sb.Append(";");
                    }
                    if (sb.ToString().Contains("ABS"))
                    {
                        int ABSind = (sb.ToString().IndexOf("A"));
                        sb.Insert(ABSind, "@");
                    }
                    if (sb.ToString().Contains("LOG"))
                    {
                        int ABSind = (sb.ToString().IndexOf("L"));
                        sb.Insert(ABSind, "@");
                    }
                    if (sb.ToString().Contains("- obj4"))
                    {
                        sb.Append(";");
                    }
                    if (sb.ToString().Contains("GO"))
                    {
                        sb.Remove(0, 2);
                    }
                    if (sb.ToString().Contains("QUIT"))
                    {
                        sb.Remove(0, 4);
                    }
                    if (sb.ToString().Contains("DIVERT t_t4xxx.dat"))
                    {
                        sb.Replace("DIVERT t_t4xxx.dat", "END");
                    }

                    Match match1 = regex1.Match(sb.ToString());
                    Match match2 = regex2.Match(sb.ToString());

                    if (match1.Success || match2.Success)
                    {
                        int NumInd = (sb.ToString().IndexOf(")"));
                        sb.Replace(")", "]", NumInd, 1);
                        sb.Insert(0, "[R_");
                    }

                    writer.WriteLine(sb);

                }
                writer.Close();
                MessageBox.Show("The new input file has been created in the directory of the selected file.", "File Has Been Created",
                    MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            
        }

        private void BoilerPlate(StreamWriter writer)
        {
            writer.WriteLine("MODEL:");
            writer.WriteLine("");
            writer.WriteLine("CALC:");
            writer.WriteLine("   !Enable global solver;");
            writer.WriteLine("   @SET( 'GLOBAL', 1);");
            writer.WriteLine("   !Allow all variables to go negative;");
            writer.WriteLine("   @SET( 'NONNEG', 0);");
            writer.WriteLine("   !No dual solution required;");
            writer.WriteLine("   @SET( 'DUALCO', 0);");
            writer.WriteLine("   !Set the global solver preprocessing level");
            writer.WriteLine("    to also substitute out intermediate variables;");
            writer.WriteLine("   @APISET( 6410, 'INT', 1023);");
            writer.Write("ENDCALC");
        }

        private StringBuilder Pat1(StringBuilder s)
        {
            StringBuilder currentSt = new StringBuilder();

            return currentSt;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int size = -1;
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                filePath = openFileDialog1.FileName;
                fileDirectory = filePath.Replace(openFileDialog1.SafeFileName, "");

                try
                {
                    string text = File.ReadAllText(filePath);
                    size = text.Length;
                }
                catch (IOException)
                {
                }

                textBox1.Text = filePath;
            }
        }
    }
}
