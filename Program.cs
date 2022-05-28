using System;
using System.Globalization;
using System.Text.RegularExpressions;

using Vault;

namespace git_credential_cert
{


    class Program
    {
        const string SPECIFICATION = "https://mirrors.edge.kernel.org/pub/software/scm/git/docs/git-credential.html";

        const string HELP = "usage: git-credential-cert.exe [get|store|list|erase|delete]";

        enum ExitCodes : int
        {
            Success = 0,
            GitInputError = 1,
            PassError = 2,
            PassEntryNotFound = 3,
            CertNotFound = 4,
            CredentialsNotFound = 5,
            ArgumentNotFound = 6,
            UnknownCommand = 97,
            ShowHelp = 98,
            UnknownError = 99
        }

        static void Exit(ExitCodes code)
        {
            Environment.Exit((int)code);
        }

        static void ThrowPanic(string msg, ExitCodes code)
        {
            Console.Error.WriteLine(msg);
            Exit(code);
        }

        static void Debug(string output)
        {
            //Console.Error.WriteLine(string.Format("DEBUG:{0}", output));
        }

        static void ConsoleOutLine(string output)
        {
            Debug(output);
            Console.Out.Write(output + "\n");
        }

        static (string url, string username, string password) ReadFromInput()
        {
            string username = "", password = "";

            UriBuilder uriBuilder = new UriBuilder();

            CultureInfo culture = new CultureInfo("en-US");

            string line;

            while ((line = Console.ReadLine()) != null)
            {
                if (line == "")
                {
                    break;
                }
                else
                {
                    var sp = line.Split('=');
                    if (sp.Length < 2)
                    {
                        ThrowPanic(string.Format("incorrect parameter in string : {1}{0}please, check specification {2}", Environment.NewLine, line, SPECIFICATION), ExitCodes.GitInputError);
                    }
                    else
                    {
                        var key = sp[0].ToLower();
                        var val = Regex.Replace(line, string.Format("{0}=", key), "", RegexOptions.IgnoreCase);

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
                                ThrowPanic(string.Format("incorrect input parameter: {1}{0}please, check specification {2}", Environment.NewLine, key, SPECIFICATION), ExitCodes.GitInputError);
                                break;
                        }
                    }
                }
            }

            return (uriBuilder.ToString(), username, password);
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleOutLine(string.Format("Git-Credential-Cert {0}", typeof(Program).Assembly.GetName().Version));

                ConsoleOutLine(HELP);
                Exit(ExitCodes.ShowHelp);
            }
            else
            {
                Debug(string.Format("method:{0}", args[0]));

                switch (args[0].ToLower())
                {
                    case "get":
                        {
                            if (args.Length > 1)
                            {
                                ThrowPanic("get command does not support any additional arguments. Console input should be used.", ExitCodes.GitInputError);
                            }

                            {
                                (string url, string _, string _) = ReadFromInput();

                                Store store = Store.Open();

                                try
                                {
                                    (string protocol, string host, string path, string username, string password) = store.GetCredentialsFor(url);
                                    ConsoleOutLine(string.Format("username={0}", username));
                                    ConsoleOutLine(string.Format("password={0}", password));

                                    Exit(ExitCodes.Success);
                                }
                                catch (Exception e)
                                { // credentials for url not found
                                    Console.Error.WriteLine(e.Message);
                                }
                            }
                        }
                        break;
                    case "store":
                        {
                            if (args.Length > 1)
                            {
                                ThrowPanic("store command does not support any additional arguments. Console input should be used.", ExitCodes.GitInputError);
                            }
                            (string url, string username, string password) = ReadFromInput();

                            Store store = Store.Open();

                            try
                            {
                                store.Add(url, username, password);
                            }
                            catch (NoCertWithPrivateKeyException e)
                            {
                                ThrowPanic(e.Message, ExitCodes.CertNotFound);
                            }
                            catch (CredentialsAlreadyExistsException)
                            {
                            }

                            store.Save();

                            Exit(ExitCodes.Success);
                        }
                        break;
                    case "list":
                        {
                            if (args.Length > 1)
                            {
                                ThrowPanic("list command does not support any additional arguments", ExitCodes.UnknownCommand);
                            }

                            Store store = Store.Open();
                            store.GetList().ForEach(signedContainer =>
                            {
                                Console.Out.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}",
                                    signedContainer.GetURL(),
                                    signedContainer.GetUserName(),
                                    signedContainer.GetCertificateSubject(),
                                    signedContainer.GetCertificateThumbprint())
                                );
                            });
                            break;
                        }
                    case "erase":
                        {
                            try
                            {
                                (string url, string _, string _) = ReadFromInput();
                                Store store = Store.Open();
                                store.Remove(url);
                                store.Save();
                                Exit(ExitCodes.Success);
                            }
                            catch (CouldNotFoundCredentialsException e)
                            {
                                ThrowPanic(e.Message, ExitCodes.CredentialsNotFound);
                            }
                            catch (Exception e)
                            {
                                ThrowPanic(e.Message, ExitCodes.UnknownError);
                            }
                        }
                        break;

                    case "delete":
                        {
                            if (args.Length != 2)
                            {
                                ThrowPanic("delete command should have url argument", ExitCodes.ArgumentNotFound);
                            }

                            try
                            {
                                Store store = Store.Open();
                                store.Remove(args[1]);
                                store.Save();
                                Exit(ExitCodes.Success);
                            }
                            catch (CouldNotFoundCredentialsException e)
                            {
                                ThrowPanic(e.Message, ExitCodes.CredentialsNotFound);
                            }
                            catch (Exception e)
                            {
                                ThrowPanic(e.Message, ExitCodes.UnknownError);
                            }
                        }
                        break;
                    default:
                        ThrowPanic(string.Format("unknown argument '{0}'", args[0]), ExitCodes.UnknownCommand);
                        break;
                }

            }
        }
    }
}
