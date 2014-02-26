using System;
using System.Collections.Generic;
using System.IO;

namespace MediaFileMerger
{
    class Utility
    {
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static void CopyFile(String source, String dest)
        {
            File.Copy(source, dest, true);
        }

        public static void DeleteFileIfExists(String fileLocation)
        {
            if (FileExists(fileLocation))
                File.Delete(fileLocation);
        }

        public static void DeleteFile(String fileLocation)
        {
            File.Delete(fileLocation);
        }

        public static void DeleteFile(List<String> list)
        {
            foreach (String file in list)
            {
                DeleteFile(file);
            }
        }

        public static Boolean DirectoryExists(String path)
        {
            return Directory.Exists(path);
        }

        public static Boolean FileExists(String fileLocation)
        {
            return System.IO.File.Exists(fileLocation);
        }

        public static void MoveFile(String source, String dest)
        {
            if (FileExists(dest))
            {
                DeleteFile(dest);
            }
            File.Move(source, dest);
        }

        /*
        public static void MoveFiles(List<string> extensions, string sourcePath, string destPath)
        {
            foreach (string sFile in Directory.GetFiles(sourcePath))
            {
                if (extensions.Contains(new FileInfo(sFile).Extension))
                {
                    MoveFile(sFile, String.Concat(destPath, Path.GetFileName(sFile)));
                }
            }
        }
        */

        public static void MoveFiles(List<string> extensions, string sourcePath, string destPath)
        {
            foreach (string sFile in Directory.GetFiles(sourcePath))
            {
                if (extensions.Contains(new FileInfo(sFile).Extension))
                {
                    CopyFile(sFile, String.Concat(destPath, Path.GetFileName(sFile)));
                    DeleteFile(sFile);
                }
            }
        }

        public static void Log(string file, string content)
        {
            if (file.Length > 0)
            {
                CreateDirectory(file.Substring(0, file.LastIndexOf("\\")));
                Append(file, String.Concat("[", DateTime.Now, "] ", content));
            }
        }

        public static void Append(string file, string content)
        {
            File.AppendAllText(file, String.Concat(content, Environment.NewLine));
        }

        public static String getStreamNameElement(string sXml)
        {
            string streamname = getXmlElement(sXml, "_StreamName");
            if (streamname.IndexOf("/") > -1)
            {
                streamname = streamname.Substring(streamname.LastIndexOf("/") + 1, streamname.Length - streamname.LastIndexOf("/") - 1);
            }
            return streamname;
        }

        public static String getXmlElement(string sXml, string element)
        {
            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            xml.LoadXml(sXml);
            if (xml.FirstChild[element] != null)
            {
                return xml.FirstChild[element].InnerXml;
            }
            else
            {
                return "";
            }
        }

        public static Boolean isStreamAudio(string cueXml)
        {
            return Boolean.Parse(Utility.getXmlElement(cueXml, "_AudioStream"));
        }

        public static Boolean isStreamVideo(string cueXml)
        {
            return Boolean.Parse(Utility.getXmlElement(cueXml, "_VideoStream"));
        }
    }
}
