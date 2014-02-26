using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using VideoInfoLib.Metadata;

namespace MediaFileMerger
{
    abstract class Merger
    {
        protected ExeCommands commands;
        protected ConfigurationVO cvo;
        protected List<MergedFile> clipData;

        public Merger(ConfigurationVO cvo)
        {
            this.cvo = cvo;
            commands = new ExeCommands();
        }

        public Merger(ConfigurationVO cvo, List<MergedFile> clipData)
        {
            this.cvo = cvo;
            commands = new ExeCommands();
            this.clipData = clipData;
        }

        // Creates a new file by performing some sort FFMPEG operation
        // Always delete the output file because the overwrite parameter isn't working
        public void ExecuteFFmpeg(String args, String destFile)
        {
            if (destFile.Length != 0)
            {
                Utility.DeleteFileIfExists(destFile);
            }
            LogMessage(String.Concat("ffmpeg ", args));
            CommandLineProcessor.ExecuteCommand(cvo.FfmpegPath, args);
        }

        public void ExecuteFlvMerge(String args, String destFile)
        {
            LogMessage(String.Concat("flvmerge ", args));
            CommandLineProcessor.ExecuteCommand(cvo.FlvmergePath, args);
            Utility.MoveFile(String.Concat(ConfigurationVO.ExecutableDirectory, "merge.flv"), destFile);
        }

        public void ExecuteSox(String args)
        {
            LogMessage(String.Concat("sox ", args));
            CommandLineProcessor.ExecuteCommand(cvo.SoxPath, args);
        }

        public void ExecuteFlvMdi(String args)
        {
            CommandLineProcessor.ExecuteCommand(cvo.FlvmdiPath, args);
        }

        public void ExecuteSegmenter(String args)
        {
            LogMessage(String.Concat("segmenter ", args));
            CommandLineProcessor.ExecuteCommand(cvo.Segmenter, args);
        }

        public void ExecuteSegmenter(string segmenterType, String args)
        {
            LogMessage(String.Concat("segmenter ", args));
            CommandLineProcessor.ExecuteCommand(segmenterType, args);
        }

        public void LogMessage(string message)
        {
            if (cvo.LoggingEnabled)
                Utility.Log(cvo.LogFile, message);
        }

        protected void ConvertToTransportStream(MergedFile result, string command)
        {
            string resultFileName = String.Concat(cvo.ProcessingPath, cvo.PresentationID, ".ts");
            string args = String.Format(command, result.GetFullFileName(), resultFileName);
            string indexFile = String.Concat(cvo.PresentationID.ToString(), result.type);

            ExecuteFFmpeg(args, resultFileName);

            args = String.Format(commands.SEGMENTER, resultFileName, cvo.SegmentLength, String.Concat(cvo.PresentationID.ToString(), result.type), String.Concat(cvo.PresentationID.ToString(), result.type), cvo.TsPath);
            ExecuteSegmenter(result.type==FileTypes.Audio?cvo.SegmenterAudio:cvo.Segmenter, args);
            PrepareIndexFile(String.Concat(ConfigurationVO.ExecutableDirectory, indexFile, ".m3u8"));
            Utility.MoveFiles(new List<string> {".ts", ".m3u8"}, ConfigurationVO.ExecutableDirectory, cvo.ExportPath);
            Utility.DeleteFileIfExists(resultFileName);
        }

        private void PrepareIndexFile(string indexFile)
        {
            Utility.Append(indexFile, ConfigurationVO.m3u8Ending);
        }

        public Boolean isStreamAudio(string cueXml)
        {
            return Boolean.Parse(Utility.getXmlElement(cueXml, "_AudioStream"));
        }

        public Boolean isStreamVideo(string cueXml)
        {
            return Boolean.Parse(Utility.getXmlElement(cueXml, "_VideoStream"));
        }

        private MetadataReadResult getMetadataReadResult(string filename)
        {
            return (new MetaMetadataReader()).Read(filename);
        }

        public String getFrameRate(String filename)
        {
            if (filename != null)
            {
                Double frameRate;
                MetadataReadResult res = getMetadataReadResult(filename);
                if (res != null && res.VideoMetadata != null && Double.TryParse(res.VideoMetadata.Framerate.ToString(), out frameRate) && frameRate > 0)
                    return frameRate.ToString();
            }
            return "";
        }

        public Int32 getVideoFileLength(string filename)
        {
            if (filename != null)
            {
                Int32 FileLength;
                MetadataReadResult res = getMetadataReadResult(filename);
                if (res != null && res.VideoMetadata != null && Int32.TryParse(res.VideoMetadata.Duration.TotalMilliseconds.ToString(), out FileLength) && FileLength > 0)
                    return FileLength;
            }
            return 0;
        }

        public DataTable getSourceData()
        {
            String query = @"SELECT [CueXml], [Time], [Duration], [Time] + [Duration] AS [End] 
                FROM dbo.scoCues WHERE PresentationID = {0} 
                AND ModuleIdentifier = '{1}'
                ORDER BY [Time];";

            SqlConnection conn = new SqlConnection(cvo.ConnectionString);
            conn.Open();

            SqlDataAdapter da = new SqlDataAdapter(String.Format(query, cvo.PresentationID, ConfigurationVO.MODULEIDENTIFIER), conn);

            DataTable dtSource = new DataTable();
            da.Fill(dtSource);
            conn.Close();
            return dtSource.Copy();
        }

        internal string getBridgeFile(int duration)
        {
            string returnFileName = "longSilencePart";

            // The 1 minute+ silent file
            string silentMinute = string.Concat(ConfigurationVO.ExecutableDirectory, "silentminute.wav");
            // This is the file that will be greater than the bridge needed
            string silenceFile = String.Concat(cvo.ProcessingPath, "longSilence.wav");
            // This is the silence file trimmed to duration
            string silenceFilePart = String.Concat(cvo.ProcessingPath, returnFileName, "Audio0.wav");

            int silentMinutes = (duration / 60000) + 1;
            string args = String.Format(commands.JOINAUDIOFILES, silentMinute, silentMinute, silenceFile);
            // Build the silenceFile
            for (int i = 0; i < silentMinutes; i++)
            {
                ExecuteSox(args);
                args = String.Format(commands.JOINAUDIOFILES, silentMinute, silenceFile, silenceFile);
            }
            // Trim to duration
            args = String.Format(commands.GETAUDIOPART, silenceFile, silenceFilePart, 0, TimeSpan.FromMilliseconds(duration));
            ExecuteSox(args);

            Utility.DeleteFile(silenceFile);
            return returnFileName;
        }

        internal void joinAudioFiles(MergedFile total, MergedFile file, String tempFile)
        {
            String args = String.Format(commands.JOINAUDIOFILES, total.GetFullFileName(), file.GetFullFileName(), tempFile);
            ExecuteSox(args);
            total.End = file.End;
            Utility.MoveFile(tempFile, total.GetFullFileName());
            Utility.DeleteFile(file.GetFullFileName());
        }

        internal void IncreaseVolume(String audioFile)
        {
            String tempFile = String.Concat(audioFile.Substring(0, audioFile.LastIndexOf("\\") + 1), "tempaudio.wav");
            String args = String.Format(commands.INCREASESOUNDVOLUME, audioFile, tempFile);
            ExecuteSox(args);
            Utility.MoveFile(tempFile, audioFile);
        }
    }
}
