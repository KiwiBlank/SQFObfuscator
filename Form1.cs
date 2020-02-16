using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;


namespace FileObfus
{
    public partial class ObfusForm : Form
    {
        FolderBrowserDialog fbd = new FolderBrowserDialog();

        public ObfusForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public void button1_Click(object sender, EventArgs e)
        {
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Select Mission Folder";
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fbd.SelectedPath;
                Console.WriteLine(fbd.SelectedPath);
                int fCount = Directory.GetFiles(fbd.SelectedPath, "*.sqf", SearchOption.AllDirectories).Length;
                pBar1.Maximum = fCount;
                Console.WriteLine(fCount);
                Console.WriteLine(pBar1.Maximum);
                pBar1.Visible = true;
                pBar1.Minimum = 0;
                pBar1.Value = 1;
                pBar1.Step = 1;
            }

        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        { 
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);


            foreach (string dirPath in Directory.GetDirectories(fbd.SelectedPath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(fbd.SelectedPath, tempDirectory));

            foreach (string newPath in Directory.GetFiles(fbd.SelectedPath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(fbd.SelectedPath, tempDirectory), true);




            foreach (string newPath in Directory.GetFiles(tempDirectory, "*.sqf", SearchOption.AllDirectories))
            {

                string contents = File.ReadAllText(newPath);

                Regex comments = new Regex (@"(\/\/.*?(\r?\n|$))|(\/\*(?:[\s\S]*?)\*\/)|(""(?:\\[^\n]|[^""\n])*"")|(@(?:""[^""]*"")+)");

                string afterRemoval = comments.Replace(contents, "");


                byte[] utfBytes = Encoding.UTF8.GetBytes(afterRemoval);

                File.WriteAllText(newPath, String.Empty);

                using (StreamWriter sw = File.AppendText(newPath))
                {
                    sw.WriteLine("call compile tostring[");
                }


                using (StreamWriter sw = File.AppendText(newPath))
                {
                    Parallel.ForEach(utfBytes, b =>
                {
                    lock (sw)
                    {
                        sw.WriteLine(b + ",");
                    }
                });
                    sw.Flush();
                }

                string contentsRemoval = File.ReadAllText(newPath);
                contentsRemoval = contentsRemoval.Trim();
                contentsRemoval = contentsRemoval.TrimEnd(',');
                File.WriteAllText(newPath, contentsRemoval);

                using (StreamWriter sw = File.AppendText(newPath))
                {
                    sw.WriteLine("];");
                    string result = Path.GetFileName(newPath);
                    pBar1.Increment(1);
                }

            }
            if (pBar1.Value >= pBar1.Maximum)
            {
                pBar1.Maximum = 100;
                pBar1.Value = 0;
                Console.WriteLine("Complete");
                string message = "Obfuscation has been completed, please pack the new folder with a PBO Manager";
                string caption = "Obfuscation Complete";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;

                result = MessageBox.Show(message, caption, buttons);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Close();
                }
            }


            string newFinalPath = fbd.SelectedPath + "_Obfuscated";
            System.IO.Directory.CreateDirectory(newFinalPath);

            foreach (string dirPath in Directory.GetDirectories(tempDirectory, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(tempDirectory, newFinalPath));


            foreach (string newPath in Directory.GetFiles(tempDirectory, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(tempDirectory, newFinalPath), true);

            Directory.Delete(tempDirectory, true);

        }
        private void progressBar1_Click(object sender, EventArgs e) { 
        
        }

    }

}

