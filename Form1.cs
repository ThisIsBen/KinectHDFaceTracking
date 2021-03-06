﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Microsoft.Samples.Kinect.HDFaceBasics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            rele();
            //show nonverbal chart 
            nonverbal_webBrowser.Navigate("http://54.191.185.244/linechart.html");//
            ///////////////





            ///////////////
        }

        public void rele()
        {
            StreamReader sr = new StreamReader("allQA.txt");
            string[] qa = sr.ReadToEnd().Split('\n');
            int result = 0;

            Label label1 = new Label();
            label1.Text = "Relevant Score";
            label1.AutoSize = true;
            label1.Location = new Point(130, 0);
            label1.Font = new Font("Arial", 13);

            for(int i = 0; i < qa.Length-1; i++)
            {
                string[] temp = qa[i].Split('#');
                
                ProgressBar progressBar1 = new ProgressBar();
                Label label = new Label();
                Label label2 = new Label();
                Label label3 = new Label();

                progressBar1.Maximum = 200;                
                progressBar1.Value = Convert.ToInt16((Convert.ToDouble(temp[0])+1) * 100);
                progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
                result += Convert.ToInt16((Convert.ToDouble(temp[0]) + 1) * 100);
                if(i > 0)
                {
                    progressBar1.Location = new Point(20, 80 * i + 60);
                    label2.Location = new Point(5, 80 * i + 60);
                    label3.Location = new Point(120, 80 * i + 65);
                }
                else
                {
                    progressBar1.Location = new Point(20,60);
                    label2.Location = new Point(5, 60);
                    label3.Location = new Point(120, 65);
                }
                label.Text = "Qusetion:" + temp[1] + "\n" + "Answer:" + temp[2];
                label.AutoSize = true;
                label.Location = new Point(20, 80 * i + 30);
                label2.Text = "-1";
                label2.AutoSize = true;               
                label3.Text = "1";
                label3.AutoSize = true;            
                Controls.Add(progressBar1);
                Controls.Add(label);
                Controls.Add(label2);
                Controls.Add(label3);
            }

            Controls.Add(label1);

            Label label4 = new Label();
            Label label5 = new Label();
            Label label6 = new Label();
            ProgressBar progressBar2 = new ProgressBar();
            progressBar2.ForeColor = Color.Red;
            progressBar2.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            progressBar2.Maximum = 200;
            progressBar2.Location = new Point(20, (qa.Length-1) * 80 + 45);
            progressBar2.Value = result / (qa.Length - 1);
            label4.Location = new Point(5, 80 * (qa.Length - 1) + 50);
            label5.Location = new Point(120, 80 * (qa.Length - 1) + 50);
            label4.Text = "-1";
            label4.AutoSize = true;
            label5.Text = "1";
            label5.AutoSize = true;
            label6.Text = "Average";
            label6.AutoSize = true;
            label6.Location = new Point(20, 80 * (qa.Length - 1) + 30);
            Controls.Add(progressBar2);
            Controls.Add(label4);
            Controls.Add(label5);
            Controls.Add(label6);
            // Creates a new record in the dataset.
            // NOTE: The code below will not compile, it merely
            // illustrates how the progress bar would be used.
          /*  CustomerRow anyRow = DatasetName.ExistingTable.NewRow();
            anyRow.FirstName = "Stephen";
            anyRow.LastName = "James";
            ExistingTable.Rows.Add(anyRow);*/

            // Increases the value displayed by the progress bar.
           
            // Updates the label to show that a record was read.
           // label1.Text = "Records Read = " + progressBar1.Value.ToString();
            // Create a Button object 
        
        }
    }
}
