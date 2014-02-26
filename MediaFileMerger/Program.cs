using System;

namespace MediaFileMerger
{
    class Program
    {
        //public static String USAGE = "\n     USAGE: $> MediaFileMerger presentationID";
        public static string USAGE = "\n    USAGE: $> MediaFileMerger presentationID inputPath outputPath";
        //Int32 presentationID = 25612;
        public static Int32 presentationID;
        public static string demoPath;
        public static string outputPath;

        static void Main(string[] args)
        {
            string errorMessage;

            ConfigurationVO cvo;

            // Check if PresentationID was passed
            if (args.Length != 3)
            {
                Console.Write(USAGE);
                Console.ReadLine();
                return;
            }

            // Check if command line parms are proper
            if (!checkCmdLineVars(args, out errorMessage))
            {
                Console.Write(errorMessage);
                Console.ReadLine();
                return;
            }

            // Check if config file is proper
            if (!tryConfiguration(out cvo))
            {
                Console.WriteLine("Some important configuration values are missing.");
                return;
            }

            cvo.PresentationID = presentationID;
            cvo.DemoPath = demoPath;
            cvo.ExportPath = outputPath;

            Utility.Log(cvo.LogFile, String.Concat("args: [", String.Join("], [", args), "]"));

            Boolean bSuccess = true;
            MediaFileMerger sm = new MediaFileMerger(cvo);
            try
            {
                sm.Process();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in Program.cs: [{0}] {1}", e.GetType(), e.Message);
                bSuccess = false;
            }
            finally
            {
                UpdaterService us = new UpdaterService(cvo.UpdateServiceUrl);
                us.Update("MediaFileMerger", cvo.PresentationID.ToString(), bSuccess);
            }
        }

        private static Boolean checkCmdLineVars(string[] args, out string errMsg)
        {
            Boolean cmdLineVarsOk = true;
            string strErrMsg = "";

            if (!Int32.TryParse(args[0], out presentationID))
            {
                cmdLineVarsOk = false;
                strErrMsg = String.Concat(strErrMsg, "PresentationID must be numeric\n");
            }
            if (!Utility.DirectoryExists(args[1]))
            {
                cmdLineVarsOk = false;
                strErrMsg = String.Concat(strErrMsg, "InputPath is not found\n");
            }
            else
            {
                demoPath = args[1];
            }

            outputPath = args[2].Trim();
            if (outputPath.Substring(outputPath.Length-1) != "\\")
            {
                outputPath = String.Concat(outputPath, "\\");
            }
            Utility.CreateDirectory(args[2]);

            errMsg = strErrMsg;
            return cmdLineVarsOk;
        }

        public static Boolean tryConfiguration(out ConfigurationVO cvo)
        {
            cvo = new ConfigurationVO();
            // Verify properties and return false if any are missing
            if (cvo.FfmpegPath == null ||
                cvo.FlvmergePath == null ||
                cvo.SoxPath == null ||
                cvo.Segmenter == null ||
                cvo.SegmenterAudio == null ||
                cvo.ConnectionString == null ||
                cvo.UpdateServiceUrl == null ||
                cvo.ProcessingPath == null ||
                cvo.FfmpegPath.Length == 0 ||
                cvo.FlvmergePath.Length == 0 ||
                cvo.SoxPath.Length == 0 ||
                cvo.Segmenter.Length == 0 ||
                cvo.SegmenterAudio.Length == 0 ||
                cvo.ConnectionString.Length == 0 ||
                cvo.UpdateServiceUrl.Length == 0 ||
                cvo.ProcessingPath.Length == 0 ||
                cvo.SegmentLength == 0)
                    return false;
            return true;
        }
    }
}