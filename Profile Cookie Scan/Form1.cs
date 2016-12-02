using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Profile_Cookie_Scan
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;
        }

        public int numdirs;

        public void CookieSearchWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Directory.Exists(textBox1.Text))
            {
                try
                {
                    toolStripStatusLabel1.Text = ("Getting Directory: " + textBox1.Text);
                    DirectoryInfo di = new DirectoryInfo(textBox1.Text);
                    numdirs = di.GetDirectories("*.v2").Length;
                    int i = 0;
                    if (listView1.InvokeRequired)
                    {
                        listView1.Invoke(new MethodInvoker(delegate
                        {
                            listView1.Items.Clear();
                        }));
                    }
                    else
                    {
                        listView1.Items.Clear();
                    }

                    double percentdone = ((double)i / numdirs) * 100.0;

                    foreach (var fold in di.GetDirectories("*.v2"))
                    {
                        if (CookieSearchWorker1.CancellationPending == true)
                        {
                            toolStripStatusLabel1.Text = "Cancelled!";
                            break;
                        }

                        int numcookies = 0;
                        i++;
                        percentdone = ((double)i / numdirs) * 100.0;

                        toolStripStatusLabel1.Text = ("Scanning " + i + ": " + fold.Name);


                        if (Directory.Exists(fold.FullName + @"\AppData\Roaming\Microsoft\Windows\Cookies\Low"))
                        {
                            DirectoryInfo cookiedir = new DirectoryInfo((fold.FullName + @"\AppData\Roaming\Microsoft\Windows\Cookies\Low"));
                            numcookies = cookiedir.GetFiles("*.txt").Length;

                            string[] arr = new string[4];

                            ListViewItem itm;

                            arr[0] = fold.Name;
                            arr[1] = numcookies.ToString();
                            arr[2] = cookiedir.FullName;

                            itm = new ListViewItem(arr);

                            if (listView1.InvokeRequired)
                            {
                                listView1.Invoke(new MethodInvoker(delegate
                                {
                                    listView1.Items.Add(itm);
                                }));
                            }
                            else
                            {
                                listView1.Items.Add(itm);
                            }
                        }

                        CookieSearchWorker1.ReportProgress((int)percentdone, fold.Name);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(("Error occurred: " + ex.Message));
                    toolStripStatusLabel1.Text = ("Error: " + ex.Message);
                    button3.Enabled = true;
                    button4.Enabled = false;
                    throw;
                }

            }
            else
            {
                MessageBox.Show(("Directory " + textBox1.Text + " not found"));
            }
            button3.Enabled = true;
            button4.Enabled = false;
        }

        public void CookieSearchWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                progressBar1.Value = e.ProgressPercentage;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void CookieSearchWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripStatusLabel1.Text = ("Done");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            button4.Enabled = true;
            CookieSearchWorker1.RunWorkerAsync();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            CookieSearchWorker1.CancelAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem listItem in listView1.Items)
            {
                listItem.Checked = true;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem listItem in listView1.Items)
            {
                listItem.Checked = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            button5.Enabled = false;
            progressBar1.Value = 1;
            toolStripStatusLabel1.Text = ("Cleaning");
            deleteWorker.RunWorkerAsync();
        }

        private void deleteWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                int numitems = listView1.Items.Count;
                int i = 0;

                foreach (ListViewItem listItem in listView1.Items)
                {
                    i++;
                    progressBar1.Value = (int)(((double)i / numitems) * 100.0);

                    if (listItem.Checked == true)
                    {
                        toolStripStatusLabel1.Text = ("Cleaning: " + listItem.Text);

                        DirectoryInfo di = new DirectoryInfo(listItem.SubItems[2].Text);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            if (file.Exists)
                            {
                                //MessageBox.Show("Would delete: " + file.Name);
                                try
                                {
                                    file.Delete();
                                }
                                finally { }
                                
                            }

                        }
                    }
                }
                toolStripStatusLabel1.Text = ("Done");
                button5.Enabled = true;
                MessageBox.Show("Profiles cleaned");

            }
            catch (Exception ex)
            {
                button5.Enabled = true;
                MessageBox.Show(("Error occurred: " + ex.Message));
                toolStripStatusLabel1.Text = ("Error: " + ex.Message);
                throw;
            }

        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //listView1.Sorting = SortOrder.Ascending;
        }
    }
}
