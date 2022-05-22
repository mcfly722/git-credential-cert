using System;
using System.Windows.Forms;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;


using Vault;

namespace git_credential_cert
{


    class Program
    {
        const string SPECIFICATION = "https://mirrors.edge.kernel.org/pub/software/scm/git/docs/git-credential.html";

        static void Debug(string output) {
            Console.Error.WriteLine(string.Format("DEBUG:{0}", output));
        }

        static void ConsoleOutLine(string output)
        {
            Debug(output);
            //Console.WriteLine(output);
            Console.Out.Write(output+ "\n");
        }

        static (string url, string username, string password) readFromInput()
        {
            string username = "", password = "";

            UriBuilder uriBuilder = new UriBuilder();

            CultureInfo culture = new CultureInfo("en-US");

            string line;

            while ((line = Console.ReadLine()) != null)
            {
                //Console.WriteLine(line);
                if (line == "")
                {
                    break;
                }
                else
                {
                    var sp = line.Split('=');
                    if (sp.Length < 2)
                    {
                        throw new Exception(string.Format("incorrect parameter in string : {1}{0}please, check specification {2}", Environment.NewLine, line, SPECIFICATION));
                    }
                    else
                    {
                        var key = sp[0].ToLower();
                        var val = Regex.Replace(line, string.Format("{0}=", key), "", RegexOptions.IgnoreCase);

                        //Console.Error.WriteLine(string.Format("DEBUG: {0}={1}", key, val));

                        ConsoleOutLine(line);

                        switch (key)
                        {
                            case "protocol":
                                uriBuilder.Scheme = val;
                                break;
                            case "host":
                                uriBuilder.Host = val;
                                break;
                            case "path":
                                uriBuilder.Path = val;
                                break;
                            case "username":
                                username = val;
                                break;
                            case "password":
                                password = val;
                                break;
                            default:
                                throw new Exception(string.Format("cannot parse input parameter: {1}{0}please, check specification {2}", Environment.NewLine, line, SPECIFICATION));
                        }
                    }
                }
            }

            return (uriBuilder.ToString(), username, password);
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Debug(string.Format("method:{0}",args[0]));

                switch (args[0].ToLower())
                {
                    case "get":
                        if (args.Length > 1)
                        {
                            throw new Exception("get command does not support any additional arguments. Console input should be used.");
                        }

                        {
                            (string url, string _, string _) = readFromInput();

                            Store store = Store.Open();

                            try
                            {
                                (string protocol, string host, string path, string username, string password) = store.GetCredentialsFor(url);
                                ConsoleOutLine(string.Format("username={0}", username));
                                ConsoleOutLine(string.Format("password={0}", password));

                                Environment.ExitCode = 0;
                                return;
                            }
                            catch (Exception e)
                            { // credentials for url not found
                                Console.Error.WriteLine(e.Message);
                            }
                        }

                        break;

                    case "store":
                        
                        if (args.Length > 1)
                        {
                            throw new Exception("store command does not support any additional arguments. Console input should be used.");
                        }

                        {
                            (string url, string username, string password) = readFromInput();

                            Store store = Store.Open();

                            store.Add(url, username, password);

                            store.Save();
                        }
                        
                        break;
                    case "erase":
                        /*
                        if (args.Length > 1)
                        {
                            throw new Exception("store command does not support any additional arguments. Console input should be used.");
                        }

                        {
                            (string url, string _, string _) = readFromInput();

                            Store store = Store.Open();

                            store.Remove(url);

                            store.Save();
                        }
                        */

                        break;
                    default:
                        throw new Exception(string.Format("unknown parameter {0}", args[0]));
                }

            }
        }
    }
}
