using System;
using System.Windows.Forms;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Cryptography;

using Vault;

namespace git_credential_cert
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length >0)
            {
                // https://mirrors.edge.kernel.org/pub/software/scm/git/docs/git-credential.html
                if (args[0].ToLower() == "get")
                {
                    if (args.Length > 1) {
                        throw new Exception("get command does not support any arguments");
                    }
                    

                    UriBuilder uriBuilder = new UriBuilder();
                    CultureInfo culture = new CultureInfo("en-US");

                    string line;

                    while ((line = Console.ReadLine()) != null)
                    {
                        if (line.StartsWith("protocol=", true, culture))
                        {
                            uriBuilder.Scheme = Regex.Replace(line, "protocol=", "", RegexOptions.IgnoreCase);
                            continue;
                        }

                        if (line.StartsWith("host=", true, culture))
                        {
                            uriBuilder.Host = Regex.Replace(line, "host=", "", RegexOptions.IgnoreCase);
                            continue;
                        }

                        if (line.StartsWith("path=", true, culture))
                        {
                            uriBuilder.Path = Regex.Replace(line, "path=", "", RegexOptions.IgnoreCase);
                            continue;
                        }

                        if (line == "")
                        {
                            break;
                        }

                        throw new Exception(string.Format("cannot parse input parameter: {1}{0}please, check specification {2}",Environment.NewLine, line, "https://mirrors.edge.kernel.org/pub/software/scm/git/docs/git-credential.html"));
                    }



                    MessageBoxButtons buttons1 = MessageBoxButtons.YesNo;
                    MessageBox.Show(uriBuilder.ToString(), "", buttons1);

                }

                if (args[0].ToLower() == "add") {
                    if (args.Length != 2) {
                        throw new Exception("add command supports only ine parameter, it is URL");
                    }

                    UriBuilder uriBuilder = new UriBuilder(args[1]);
                    CultureInfo culture = new CultureInfo("en-US");

                    Console.Write("Enter username: ");
                    string username = Console.ReadLine();
                    string password = "";
                    Console.Write("Enter password: ");
                    ConsoleKeyInfo keyInfo;

                    do
                    {
                        keyInfo = Console.ReadKey(true);
                        // Skip if Backspace or Enter is Pressed
                        if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
                        {
                            password += keyInfo.KeyChar;
                            Console.Write("*");
                        }
                        else
                        {
                            if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                            {
                                // Remove last charcter if Backspace is Pressed
                                password = password.Substring(0, (password.Length - 1));
                                Console.Write("\b \b");
                            }
                        }
                    } // Stops Getting Password Once Enter is Pressed
                    while (keyInfo.Key != ConsoleKey.Enter);
                    Console.WriteLine("");

                    SignedContainer container = new SignedContainer(uriBuilder.ToString(), username,password);

                    Store store = Store.Open();

                    store.Add(container);

                    store.Save();
                }


                string message = string.Join(",", args);
                string caption = "1";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                result = MessageBox.Show(message, caption, buttons);
            }
        }
    }
}
