using System;
using System.Diagnostics;

namespace MediaFileMerger
{
    class CommandLineProcessor
    {
        public static void ExecuteCommand(String application, String arguments)
        {
            ProcessStartInfo process = new ProcessStartInfo(application, arguments);
            process.CreateNoWindow = false;
            process.UseShellExecute = false;
            //process.FileName = application;
            process.WindowStyle = ProcessWindowStyle.Hidden;
            //process.Arguments = arguments;

            try
            {
                using (Process exeProcess = Process.Start(process))
                {
                    exeProcess.WaitForExit();
                    int exitCode = exeProcess.ExitCode;
                    if (exitCode != 0)
                    {
                        throw new Exception(String.Format("ExitCode {0} for [{1}]({2})", exitCode, application, arguments));
                    }
                }
            }
            catch (Exception e)
            {
                string errMsg = String.Format("Exception in CommandLineProcessor: [{0}] {1}", e.GetType(), e.Message);
                Utility.Log("CommandLineProcessor", errMsg);
                throw new Exception(errMsg);
            }
        }
    }
}
