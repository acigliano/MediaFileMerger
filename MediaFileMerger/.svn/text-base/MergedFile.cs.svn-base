using System;

namespace MediaFileMerger
{
    class MergedFile
    {
        public Int32 AltStart = 0;
        public Int32 Start = 1;
        public Int32 End = -1000;
        public Int32 Duration = 0;
        public Int32 iteration;
        
        public string FilePath = "";
        private string frameRate = "";
        public string StreamName;
        private string fileName = "joined{0}{1}.{2}";
        public string CueXml;

        public FileTypes type;
        
        public Boolean bAudio = false;
        public Boolean bVideo = false;
        
        public MergedFile() { }
        public MergedFile(Int32 iteration, String filePath, FileTypes fileType)
        {
            this.iteration = iteration;
            FilePath = filePath;
            type = fileType;
        }

        public String FileName
        {
            get
            {
                return string.Format(fileName, type, iteration, getFileExtension());
            }
            set
            {
                fileName = string.Concat(value, "{0}{1}.{2}");
            }
        }

        public String FrameRate
        {
            get
            {
                if (frameRate.Length == 0)
                    return frameRate;
                else
                    return String.Concat("-r ", frameRate);
            }
            set
            {
                frameRate = value.Replace("-r ", "");
            }
        }

        public String GetFullFileName()
        {
            return String.Concat(FilePath, FileName);
        }

        private String getFileExtension()
        {
            String fileType = "";
            switch (this.type)
            {
                case FileTypes.Video:
                    fileType = "flv";
                    break;
                case FileTypes.Audio:
                    fileType = "wav";
                    break;
                case FileTypes.Avi:
                    fileType = "avi";
                    break;
            }
            return fileType;
        }
    }

    #region FileTypes for MergedFile
    enum FileTypes
    {
        Video,
        Audio,
        Avi
    }
    #endregion
}
