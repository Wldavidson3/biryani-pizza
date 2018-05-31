using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        int numObj = 0;
        string fileDirectory;
        string filePath;
        string newFileName;
        //String[] comArr = new String[] { "AGEMG", "DNBW", "MDNSTY", "PAGEMG", "STNUMB"};
        //String[] comLb = new String[5];
        ArrayList varName = new ArrayList();
        ArrayList lowBound = new ArrayList();
        ArrayList upBound = new ArrayList();

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

                StreamWriter writer = new StreamWriter(newFileName);

                BoilerPlate(writer);
                BnsVars(writer);
                fillDataArr(writer);
                FillDefaultBounds();

                StreamReader reader =
                new StreamReader(filePath);

            //regex for numbering each line
            Regex regex1 = new Regex(@"\d{3}\)\s\s");
            Regex regex2 = new Regex(@"\d{3}\)\s[a-zA-Z]");
            while ((line = reader.ReadLine()) != null)
            {
                //takes in a single line
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
                        writer.WriteLine("ENDINIT");
                    sb.Replace("DIVERT t_t4xxx.dat", "");

                }

                Match match1 = regex1.Match(sb.ToString());
                Match match2 = regex2.Match(sb.ToString());
                if (match1.Success || match2.Success)
                {
                    int NumInd = (sb.ToString().IndexOf(")"));
                    sb.Replace(")", "]", NumInd, 1);
                    sb.Insert(0, "[R_");

                }

                //if (sb.ToString().Contains("STDNECK "))
                //{
                //    sb.Insert(0, "!");
                //}

                if(sb.ToString().Contains("! set bounds "))
                {
                        sb.Replace("! set bounds and guess on ingredients", "INIT:");
                }

                    if (sb.ToString().Contains("END"))
                    {
                        sb.Replace("END", "");
                    }

                    if (sb.ToString().Contains("- obj" + (numObj)))
                    {
                        sb.Append(";");
                    }
                    if(sb.ToString().Contains("GUESS "))
                    {
                        sb.Remove(0, 6);
                        if(sb.ToString().Contains("  ")){
                            sb.Replace("  ", " ");
                        }
                        sb.Replace(" "," = ");
                        sb.Append(" ;");
                    }

                    if (!(sb.ToString().Contains("SLB ")|| sb.ToString().Contains("SUB ") || sb.ToString().Contains("SETP ")))
                    {
                        writer.WriteLine(sb);
                    }
           
                    if (sb.ToString().Contains("- obj" + (numObj)))
                    {
                      
                        writer.WriteLine();
                        writer.WriteLine("!Bounds;");
                        //for (int i = 0; i < comArr.Length; i++)
                        //{
                        //    String s;
                        //    s = "1.E10);";
                        //    if (comArr[i].Equals("STNUMB"))
                        //    {
                        //        writer.WriteLine("@BND( " + comLb[i] + ", " + comArr[i] + ", " + upBound[0] + ");");
                        //    }
                        //    else
                        //    {
                        //        writer.WriteLine("@BND( " + comLb[i] + ", " + comArr[i] + ", " + s);
                        //    }


                        //}

                        for (int i = 0; i < varName.Count; i++)
                        {
                            String s;
                            s = upBound[i] + ");";

                            writer.WriteLine("@BND( " + lowBound[i] + ", " + varName[i] + ", " + s);

                        }
                    }
                }
                writer.Write("END");
                writer.Close();

                DialogResult dialogResult = MessageBox.Show(
                    "The new input file has been created in the folder of the selected file. Would you like to open the folder now?", 
                    "File Has Been Created",
                    MessageBoxButtons.YesNo, MessageBoxIcon.None);
                if(dialogResult == DialogResult.Yes)
                {
                    Process.Start("explorer.exe", "/select, " + newFileName);
                }
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

        private void BnsVars(StreamWriter writer)
        {
            String sVar = "";
            String line1;
            StreamReader reader1 =
            new StreamReader(filePath);
            StringBuilder sb1;

            while ((line1 = reader1.ReadLine()) != null)
            {
                sb1 = new StringBuilder(line1);

                //Variable array fill
                if (sb1.ToString().Contains("objective:"))
                {
                    numObj++;
                }
                if (sb1.ToString().Contains("SLB ") || sb1.ToString().Contains("SUB "))
                {
                    // Add to the Variable array
                    int i = 4;
                    char c = sb1[i];

                    do
                    {       
                        if (!c.Equals(" "))
                        {
                            sVar += c;
                        }

                        i++;
                        c = sb1[i];
                    } while (!c.Equals(' '));

                    if (varName.Count > 0)
                    {
                        for (i = 0; i < varName.Count; i++)
                        {
                            if (!varName.Contains(sVar))
                            {
                                varName.Add(sVar);
                            }
                        }
                    }
                    else
                    {
                        varName.Add(sVar);
                    }
                    
                    sVar = "";
                }
            }
        }

        private void FillDefaultBounds()
        {
            for (int i = 0; i < varName.Count; i++)
            {
                if (lowBound[i].ToString() == "")
                {
                    lowBound[i] = "-1.E10";
                }

                if (upBound[i].ToString() == "")
                {
                    upBound[i] = "1.E10";
                }
            }
        }

        private void fillDataArr(StreamWriter writer)
        {
            String line2;
            StreamReader reader2 =
            new StreamReader(filePath);

            for (int i = 0; i < varName.Count; i++)
            {
                lowBound.Add("");
                upBound.Add("");
            }

            while ((line2 = reader2.ReadLine()) != null)
            {
                StringBuilder sb2 = new StringBuilder(line2);

                //for (int i = 0; i < comArr.Length; i++)
                //{
                //    if (sb2.ToString().Contains("SLB " + comArr[i]))
                //    {
                //        int indAss = (6 + comArr[i].ToString().Length);
                //        String teset = sb2.ToString().Substring(indAss);
                //        comLb[i]=teset;
                //    }
                //}  

                for (int i = 0; i < varName.Count; i++)
                {
                    //lowerbound array only

                    if (sb2.ToString().Contains("SLB " + varName[i] + " "))
                    {
                        String teset = sb2.ToString().Replace("SLB " + varName[i] + " ", "").Trim();
                        lowBound[i] = teset;
                    }

                    if (sb2.ToString().Contains("SUB " + varName[i] + " "))
                    {
                        String test = sb2.ToString().Replace("SUB " + varName[i] + " ", "").Trim();
                        upBound[i] = test;
                    }
                }
            }
        }



        private void button2_Click(object sender, EventArgs e)
        {
            int size = -1;
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                filePath = openFileDialog1.FileName;
                fileDirectory = filePath.Replace(openFileDialog1.SafeFileName, "");
                newFileName = fileDirectory + (openFileDialog1.SafeFileName).Replace(".dat", "") + " - Reformatted.lng";

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
