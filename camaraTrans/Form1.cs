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
        String[] VarArr = new String[] { "AGEMG", "DNBW", "MDNSTY", "MKTWT", "PAGEMG", "STDNO1", "STDNO2", "STDNO3", "STDNO4", "STNUMB" };
        String[] lowBound = new String[10];
        String[] upBound = new String[10];

        public Form1()
        {
            InitializeComponent();
        }



        private void button1_Click(object sender, EventArgs e)
        {


            string line;
            Boolean isObj = false;

            StreamWriter writer = new StreamWriter("C:\\camera\\Outputs.txt");

            BoilerPlate(writer);
            BnsVars(writer);

            StreamReader reader =
                new StreamReader("C:\\Users\\Amahn\\Desktop\\Files for the phone conference\\Input Files\\t_t3dat for old optimizer.dat");

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
                if (sb.ToString().Contains("- obj4"))
                {
                    isObj = true;
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

                if (sb.ToString().Contains("STDNECK "))
                {
                    sb.Insert(0, "!");
                }
                //Console.WriteLine("SLB " + VarArr[0]);
                //Console.Read();
                if (sb.ToString().Contains("END"))
                {
                    //remove end
                }
                //write a single line
                writer.WriteLine(sb);

                if (isObj == true)
                {
                    upBound[9] = upBound[8];
                    writer.WriteLine();
                    for (int i = 0; i < 10; i++)
                    {
                        String s;
                        if (VarArr[i].Equals("AGEMG") || VarArr[i].Equals("DNBW") || VarArr[i].Equals("MDNSTY") || VarArr[i].Equals("PAGEMG"))
                        {
                            s = "1.E10);";
                        }

                        else
                        {
                            s = upBound[i] + ");";
                        }
                        writer.WriteLine("@BND( " + lowBound[i] + ", " + VarArr[i] + ", " + s);

                    }
                    isObj = false;
                }

            }

            writer.Close();

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

            String line1;
            StreamReader reader1 =
            new StreamReader("C:\\Users\\Amahn\\Desktop\\Files for the phone conference\\Input Files\\t_t3dat for old optimizer.dat");


            while ((line1 = reader1.ReadLine()) != null)
            {
                StringBuilder sb1 = new StringBuilder(line1);


                for (int i = 0; i < 10; i++)
                {
                    //lowerbound array only
                    if (sb1.ToString().Contains("SLB " + VarArr[i]))
                    {
                        int indAss = (6 + VarArr[i].Length);
                        String teset = sb1.ToString().Substring(indAss);
                        lowBound[i] = teset;
                    }
                    //upperb array fill
                    if (sb1.ToString().Contains("SUB " + VarArr[i]))
                    {
                        int indAss = (6 + VarArr[i].Length);
                        String teset = sb1.ToString().Substring(indAss);
                        upBound[i] = teset;
                    }



                }


            }


        }




    }
}
