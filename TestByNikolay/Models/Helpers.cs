using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WpfUserControlTest
{
    class Helpers
    {
        internal static string[] GetLinesFromFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document";
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt";
            if (dlg.ShowDialog() != true) return null; // nullable
            string fileName = dlg.FileName;
            try
            {
                string[] input = File.ReadAllLines(fileName);
                return input;
            }
            catch 
            {
                return null;
            }
        }

        internal static void SaveTextToFile(string text)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text documents (.txt)|*.txt";
            dialog.FileName = "coordinates.txt";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, text.Trim());
                }
                catch { }
            }
        }
    }
}
