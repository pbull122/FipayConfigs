using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace FipayConfigs
{
    class Program
    {
        static void Main(string[] args)
        {
            DeleteLog();
            Fipay();
            
        }

        static string logFile = @"C:\Batch\Fipay\FipayConfig.log";
        static string pathtoexe = @"C:\FP\FipayEpsx.exe";
        static string log = @"C:\Batch\Fipay\Test.log";

        static void DeleteLog()
        {
            try
            {
                if(File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Fipay()
        {
            try
            {

                
                ServiceController sc = new ServiceController("FIPAY");
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped);
                        //ExecuteCommand(@"Del C:\Fipay\fipayeps_config.xml");
                        //ExecuteCommand(@"Del C:\Fipay\secureAL.dll");
                       
                        ExecuteCommand(@"Robocopy C:\FP C:\FiPay /z /r:5");
                        System.Threading.Thread.Sleep(2000);
                        sc.Start();
                        sc.WaitForStatus(ServiceControllerStatus.Running);
                        


                    }


                
                System.Threading.Thread.Sleep(2000);
                GetVersionInfo();
                //sw.WriteLine(DateTime.Now.ToString() +" did you make it here?");
                //sw.Dispose();
                System.Threading.Thread.Sleep(2000);

                SendEmail();



            }
            catch(Exception ex)
            {
                using(StreamWriter sw = new StreamWriter(logFile, true))
                {
                    sw.WriteLine(DateTime.Now.ToString() + " There has been a problem: " + ex.Message);
                    sw.Dispose();
                } 
            }
        }

       
        //gets fipayepsx.exe version number
        public static void GetVersionInfo()
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(pathtoexe);
            string version = versionInfo.ProductVersion;
            using (StreamWriter sw3 = new StreamWriter(logFile, true))
            {
                sw3.WriteLine(DateTime.Now.ToString() + " The Fipay exe version at " + Environment.MachineName + " " + version);
                sw3.Dispose();
            }

        }

        //can take command line arguments and logs the output
        static void ExecuteCommand(string command)
        {
            
            string Ctime = DateTime.Now.ToString();
            using (StreamWriter sw2 = new StreamWriter(logFile, true))
            {
                var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                processInfo.CreateNoWindow = false;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;

                var process = Process.Start(processInfo);

                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    sw2.WriteLine(Ctime + " Output>>" + e.Data);

                process.BeginOutputReadLine();

                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    sw2.WriteLine(Ctime + " Error>>" + e.Data);
                process.BeginErrorReadLine();

                process.WaitForExit();

                sw2.WriteLine(Ctime + " ExitCode: {0}", process.ExitCode);
                process.Close();
                sw2.Dispose();
            }
        }


        static void SendEmail()
        {
            string name = System.Environment.MachineName;
            string from = "noreply@bosselman.com";
            MailAddress add = new MailAddress(from);
            MailMessage mail = new MailMessage();
            mail.From = add;
            mail.To.Add("paul.bulling@bosselman.com, itpos.team@bosselman.com");
            
            SmtpClient smtp = new SmtpClient("BCPCISMTP01P.bosselman.local", 25);
            mail.IsBodyHtml = true;
            mail.Subject = @"Fipay Upgrade log from " + name;
            smtp.UseDefaultCredentials = false;
            smtp.EnableSsl = false;
            mail.Body = "<h1 style=Color:Red>Check email attachment for log file from " + name +"</h1>";
            System.Net.Mail.Attachment attach;
            attach = new System.Net.Mail.Attachment(logFile);
            mail.Attachments.Add(attach);
            smtp.Send(mail);
        }
    }
}
