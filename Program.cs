using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace git_credential_cert
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("arguments:");
            

            string message = string.Join(",", args);
            string caption = "Error Detected in Input";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;

            result = MessageBox.Show(message, caption, buttons);

        }
    }
}
