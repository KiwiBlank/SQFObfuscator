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
            // Set the info for the folder browser popup.
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.Description = "Select Mission Folder";
            fbd.ShowNewFolderButton = false;
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                // When a folder has been selected, set the directory text to a textbox.
                textBox1.Text = fbd.SelectedPath;
                // Count how many .sqf files exist to determine the maximum value for the progressbar.
                int fCount = Directory.GetFiles(fbd.SelectedPath, "*.sqf", SearchOption.AllDirectories).Length;
                pBar1.Maximum = fCount;

                // Setup progressbar.
                pBar1.Visible = true;
                pBar1.Minimum = 0;
                pBar1.Value = 0;
                pBar1.Step = 1;
            }

        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        { 
            // Create a temporary directory, with a random file name. Appears in the appdata\local\temp directory on win10
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Console.WriteLine(tempDirectory);
            Directory.CreateDirectory(tempDirectory);

            // Create all the folders in the temporary directory from the user's selected folder.
            foreach (string dirPath in Directory.GetDirectories(fbd.SelectedPath, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(fbd.SelectedPath, tempDirectory));

            // Copy all files from the user's selected directory to the temporary directory.
            foreach (string newPath in Directory.GetFiles(fbd.SelectedPath, "*.*", SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(fbd.SelectedPath, tempDirectory), true);



            // Get all .sqf files in the temporary directory.
            foreach (string newPath in Directory.GetFiles(tempDirectory, "*.sqf", SearchOption.AllDirectories))
            {

                // Use regex to remove both types of comments in the files.
                string contents = File.ReadAllText(newPath);
                Regex comments = new Regex (@"(\/\/.*?(\r?\n|$))|(\/\*(?:[\s\S]*?)\*\/)|(""(?:\\[^\n]|[^""\n])*"")|(@(?:""[^""]*"")+)");

                // Remove comments from file contents.
                string afterRemoval = comments.Replace(contents, "");

                // Take file text and turn it into a sequence of bytes.
                byte[] utfBytes = Encoding.UTF8.GetBytes(afterRemoval);

                // Empty the current file in the temporary directory to allow the new obfuscated text to be written.
                File.WriteAllText(newPath, String.Empty);

                // Add the starting code to "deobfuscate" inside ARMA.
                using (StreamWriter sw = File.AppendText(newPath))
                {
                    sw.WriteLine("call compile tostring[");
                }

                // Write the bytes into a line and end it with a comma.
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

                // Removes unnecessary whitespace and makes sure that the final item in the array does not have a comma at the end.
                string contentsRemoval = File.ReadAllText(newPath);
                contentsRemoval = contentsRemoval.Trim();
                contentsRemoval = contentsRemoval.TrimEnd(',');

                // Remove text.
                File.WriteAllText(newPath, contentsRemoval);

                // Append to close the array with bracket.
                using (StreamWriter sw = File.AppendText(newPath))
                {
                    sw.WriteLine("];");
                    string result = Path.GetFileName(newPath);

                    // Increment the progressbar to show progress.
                    pBar1.Increment(1);
                }

            }
            // Check if the value of the progressbar is equals or greater than the maximum integer.
            if (pBar1.Value >= pBar1.Maximum)
            {
                // Set the progressbar to complete.
                pBar1.Maximum = 100;
                pBar1.Value = pBar1.Maximum;
                string message = "Obfuscation has been completed, please pack the new folder with a PBO Manager";
                string caption = "Obfuscation Complete";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result;

                // Create a notification for when obfuscation is complete.
                result = MessageBox.Show(message, caption, buttons);
                if (result == System.Windows.Forms.DialogResult.Yes) 
                {
                    this.Close();
                }
            }

            // Create a new directory for the obfuscated files.
            string newFinalPath = fbd.SelectedPath + "_Obfuscated";
            System.IO.Directory.CreateDirectory(newFinalPath);

            // Create folders inside this new directory.
            foreach (string dirPath in Directory.GetDirectories(tempDirectory, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(tempDirectory, newFinalPath));

            // Copy files in to the directory from the temporary folder.
            foreach (string newPath in Directory.GetFiles(tempDirectory, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(tempDirectory, newFinalPath), true);

            // Delete the temporary directory to not leave unused files on the user's system.
            Directory.Delete(tempDirectory, true);

            // Restart the app when done to allow user to reobfuscate. 
            Application.Restart(); 

        }
        private void progressBar1_Click(object sender, EventArgs e) { 
        
        }

    }

}

