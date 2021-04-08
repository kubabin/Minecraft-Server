using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace MinecraftServer
{
    public partial class Form1 : Form
    {
        public enum wlrd
        {
            seed = 0,
            gensettings = 1
        }
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Read settings save file
            string MemAlloc = File.ReadAllLines("./AppSettings.txt")[4];
            Debug.WriteLine(MemAlloc);
            
            // Set variables
            string[] worldsettings = new string[5];
            worldsettings[0] = null;
            if ( File.Exists("./server/server.exe") )
            {
                Debug.WriteLine("bedrock server detected, not downloading");
            }
            else if (File.Exists("./server/server.jar"))
            {
                Debug.WriteLine("java server detected, not downloading");
            }
            else 
            {
                Debug.WriteLine("No server SW detected. Asking user...");
            }
            MessageBox.Show("Select server?", "No server!", MessageBoxButtons.YesNo,MessageBoxIcon.Warning);
        }
        private object syncGate = new object();
        private Process process;
        private StringBuilder output = new StringBuilder();
        private bool outputChanged;

        private void button1_Click(object sender, EventArgs e)
        {
            lock (syncGate)
            {
                if (process != null) return;
            }

            output.Clear();
            outputChanged = false;
            richTextBox1.Text = "";

            
            process = new Process();
            process.StartInfo.FileName = "C:\\Program Files (x86)\\Common Files\\Oracle\\Java\\javapath\\java.exe" + "-Xmx" + " -jar ./server.jar ";
            //process.StartInfo.FileName = "";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            new Thread(ReadData) { IsBackground = true }.Start();
        }


        private void ReadData()
        {
            var input = process.StandardOutput;
            int nextChar;
            while ((nextChar = input.Read()) >= 0)
            {
                lock (syncGate)
                {
                    output.Append((char)nextChar);
                    if (!outputChanged)
                    {
                        outputChanged = true;
                        BeginInvoke(new Action(OnOutputChanged));
                    }
                }
            }
            lock (syncGate)
            {
                process.Dispose();
                process = null;
            }
        }

        private void OnOutputChanged()
        {
            lock (syncGate)
            {
                richTextBox1.Text = output.ToString();
                outputChanged = false;
            }
        }
    }
}
    

