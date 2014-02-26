using System;
using System.Collections.Generic;
using System.Text;

namespace MediaFileMerger
{
    class ConfigurationVO
    {
        private String ffmpegPath;
        private String flvmergePath;
        private String soxPath;
        private String demoPath;
        private String flvmdiPath;
        private String segmenter;
        private String segmenterAudio;
        private String logFile;
        private String exportPath;
        private Int32 presentationID;
        private String defaultFrameRate = "15";
        private String updateServiceUrl;
        private string processingPath;
        private string tsPath = "/content/streams/{0}/";
        private int segmentLength;

        private String connectionString;
        private Boolean loggingEnabled = false;

        public static String ExecutableDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location.Substring(0, System.Reflection.Assembly.GetExecutingAssembly().Location.LastIndexOf("\\") + 1);
        public static string m3u8Ending = "#EXT-X-ENDLIST";
        public const Int32 MINCLIPMILLISECONDS = 1000;
        public const String MODULEIDENTIFIER = "s57.net.StreamManager";

        public ConfigurationVO()
        {
            try
            {
                ffmpegPath = System.Configuration.ConfigurationManager.AppSettings["ffmpegPath"].ToString();
                flvmergePath = System.Configuration.ConfigurationManager.AppSettings["flvmergePath"].ToString();
                soxPath = System.Configuration.ConfigurationManager.AppSettings["soxPath"].ToString();
                flvmdiPath = System.Configuration.ConfigurationManager.AppSettings["flvmdiPath"].ToString();
                segmenter = System.Configuration.ConfigurationManager.AppSettings["segmenter"].ToString();
                segmenterAudio = System.Configuration.ConfigurationManager.AppSettings["segmenterAudio"].ToString();
                connectionString = System.Configuration.ConfigurationManager.AppSettings["connStr"].ToString();
                exportPath = String.Concat(demoPath, "export\\");
                updateServiceUrl = System.Configuration.ConfigurationManager.AppSettings["updateServiceUrl"].ToString();
                processingPath = System.Configuration.ConfigurationManager.AppSettings["processingPath"].ToString();
                segmentLength = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["segmentLength"].ToString());

                // Logging is optional
                if (System.Configuration.ConfigurationManager.AppSettings["logfile"].Length > 0)
                {
                    logFile = System.Configuration.ConfigurationManager.AppSettings["logfile"];
                    logFile = String.Concat(logFile, "_", DateTime.Now.ToString().Replace(@"/", "-").Replace(" ", "_").Replace(":", "."), ".txt");
                    loggingEnabled = true;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Exception in Configuration object: [{0}] {1}", e.GetType(), e.Message);
            }
        }

        public String FfmpegPath
        {
            get
            {
                return ffmpegPath;
            }
        }

        public String FlvmergePath
        {
            get
            {
                return flvmergePath;
            }
        }

        public String FlvmdiPath
        {
            get
            {
                return flvmdiPath;
            }
        }

        public String Segmenter
        {
            get
            {
                return segmenter;
            }
        }

        public String SegmenterAudio
        {
            get
            {
                return segmenterAudio;
            }
        }

        public String SoxPath
        {
            get
            {
                return soxPath;
            }
        }
        
        public String DemoPath
        {
            get
            {
                return demoPath;
            }
            set
            {
                demoPath = value;
            }
        }

        public String ExportPath
        {
            get
            {
                return exportPath;
            }
            set
            {
                exportPath = value;
            }
        }

        public String DefaultFrameRate
        {
            get
            {
                return defaultFrameRate;
            }
        }

        public Int32 PresentationID
        {
            get
            {
                return presentationID;
            }
            set
            {
                presentationID = value;
                tsPath = String.Format(tsPath, PresentationID);
            }
        }

        public String ConnectionString
        {
            get
            {
                return connectionString;
            }
        }

        public Boolean LoggingEnabled {
            get
            {
                return loggingEnabled;
            }
        }

        public String LogFile
        {
            get
            {
                return logFile;
            }
        }

        public String UpdateServiceUrl
        {
            get
            {
                return updateServiceUrl;
            }
        }

        public string ProcessingPath
        {
            get
            {
                return processingPath;
            }
        }

        public string TsPath
        {
            get
            {
                return tsPath;
            }
        }

        public Int32 SegmentLength
        {
            get
            {
                return segmentLength;
            }
        }
    }
}
