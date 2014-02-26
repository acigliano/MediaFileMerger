using System;
using System.Collections.Generic;
using System.Data;

namespace MediaFileMerger
{
    class ResizeMerger : Merger
    {
        public ResizeMerger(ConfigurationVO cvo)
            : base(cvo)
        {
            MapResizedStreams();
        }

        public ResizeMerger(ConfigurationVO cvo, List<MergedFile> clipData)
            : base(cvo, clipData)
        {
            MapResizedStreams();
        }

        private void MapResizedStreams()
        {
            List<MergedFile> resizedFiles = resizeFiles();
            List<MergedFile> audioMaster = getAudioList(resizedFiles);

            MergedFile totalAudio = JoinAndMergeAudio(audioMaster);
            audioMaster = null;
            MergedFile result = MapVideoAndAudioMaster(totalAudio, resizedFiles);
            resizedFiles = null;

            ConvertToTransportStream(result, commands.CONVERTTOTRANSPORTSTREAM);
            ConvertToTransportStream(totalAudio, commands.CONVERTTOTRANSPORTSTREAMAUDIO);

            Utility.DeleteFile(new List<string> { result.GetFullFileName(), totalAudio.GetFullFileName() });
        }

        private List<MergedFile> resizeFiles()
        {
            List<MergedFile> resizedList = new List<MergedFile>();
            DataTable dtSource = getSourceData();
            string args, streamName;

            if (dtSource.Rows.Count == 0)
            {
                throw new Exception("No records found for this presentation");
            }
            for (int i = 0; i < dtSource.Rows.Count; i++)
            {
                if (Utility.getXmlElement(dtSource.Rows[i]["CueXml"].ToString(), "_TargetView") != "ScreenBroadcastView" &&
                    Int32.Parse(dtSource.Rows[i]["Duration"].ToString()) > ConfigurationVO.MINCLIPMILLISECONDS)
                {
                    MergedFile file = new MergedFile(i, cvo.ProcessingPath, FileTypes.Avi);

                    streamName = String.Concat(cvo.DemoPath, Utility.getStreamNameElement(dtSource.Rows[i]["CueXml"].ToString()), ".flv");
                    file.StreamName = streamName;

                    Int32 start = Int32.Parse(dtSource.Rows[i]["Time"].ToString());
                    Int32 end = Int32.Parse(dtSource.Rows[i]["End"].ToString());
                    Int32 clipStart = 0;
                    Int32 altStart;
                    Int32 duration = Int32.Parse(dtSource.Rows[i]["Duration"].ToString());

                    if (Int32.TryParse(Utility.getXmlElement(dtSource.Rows[i]["CueXml"].ToString(), "_StartTime"), out altStart) && altStart > 0)
                    {
                        clipStart = altStart;
                        file.AltStart = altStart;
                    }

                    file.Start = start;
                    file.End = end;
                    file.bAudio = isStreamAudio(dtSource.Rows[i]["CueXml"].ToString());
                    file.bVideo = isStreamVideo(dtSource.Rows[i]["CueXml"].ToString());

                    if (file.bVideo)
                    {
                        // Run flvmdi to get fps
                        String flvFileTemp = String.Concat(file.FilePath, "temp.flv");
                        args = String.Format(commands.RUNFLVMDI, streamName, flvFileTemp);
                        ExecuteFlvMdi(args);

                        // Get fps
                        file.FrameRate = getFrameRate(flvFileTemp);

                        String aviFileTemp = String.Concat(file.FilePath, "temp.avi");

                        args = String.Format(commands.CONVERTCLIP, streamName, file.FrameRate, aviFileTemp);
                        ExecuteFFmpeg(args, aviFileTemp);

                        if (clipStart > 0)
                        {
                            // See how long the file is and compare the length minus the adjustment to the duration
                            // If the length less adjustment falls short of duration, decrease the adjustment
                            // This file length truncation is due to missing video frames on the conversion to raw
                            Int32 fileLength = getVideoFileLength(aviFileTemp);
                            Int32 fileLengthSource = getVideoFileLength(streamName);
                            if (fileLength > 0 && fileLengthSource > 0 && (fileLength < fileLengthSource))
                            {
                                clipStart -= (fileLengthSource - fileLength);
                            }


                            args = String.Format(commands.RESIZECLIP, TimeSpan.FromMilliseconds(clipStart), TimeSpan.FromMilliseconds(duration), aviFileTemp, file.GetFullFileName());
                            ExecuteFFmpeg(args, file.GetFullFileName());
                        }
                        else
                        {
                            Utility.CopyFile(aviFileTemp, file.GetFullFileName());
                        }
                        Utility.DeleteFile(new List<String> { aviFileTemp, flvFileTemp });
                    }

                    resizedList.Add(file);
                }
            }
            return resizedList;
        }

        private List<MergedFile> getAudioList(List<MergedFile> resizedFile)
        {
            String args;
            MergedFile mf;
            List<MergedFile> audioMaster = new List<MergedFile>();

            for (int i = 0; i < resizedFile.Count; i++)
            {
                if (resizedFile[i].bAudio)
                {
                    // add to audio list
                    mf = new MergedFile((i + 1), cvo.ProcessingPath, FileTypes.Audio);
                    Int32 start = resizedFile[i].Start;
                    Int32 end = resizedFile[i].End;

                    mf.Start = start;
                    mf.End = end;
                    mf.AltStart = resizedFile[i].AltStart;
                    mf.StreamName = resizedFile[i].StreamName;
                    args = String.Format(commands.EXTRACTAUDIO, mf.StreamName, mf.GetFullFileName());
                    ExecuteFFmpeg(args, mf.GetFullFileName());

                    if (mf.AltStart != 0)
                    {
                        String tempfile = String.Concat(mf.FilePath, "tempaudio.wav");
                        args = String.Format(commands.GETAUDIOPART, mf.GetFullFileName(), tempfile, TimeSpan.FromMilliseconds(mf.AltStart), TimeSpan.FromMilliseconds(mf.End));
                        ExecuteSox(args);
                        Utility.MoveFile(tempfile, mf.GetFullFileName());
                    }

                    audioMaster.Add(mf);
                }
            }
            return audioMaster;
        }

        // Results in one audio file, with contiguous files joined together and overlapping file segments merged
        private MergedFile JoinAndMergeAudio(List<MergedFile> audioMaster)
        {
            String args;
            List<MergedFile> audioMasterNew = new List<MergedFile>();
            MergedFile totalAudio = new MergedFile(1, cvo.ProcessingPath, FileTypes.Audio);
            totalAudio.FileName = "total";
            totalAudio.Start = audioMaster[0].Start;
            totalAudio.End = audioMaster[0].End;
            Utility.CopyFile(audioMaster[0].GetFullFileName(), totalAudio.GetFullFileName());
            Utility.DeleteFile(audioMaster[0].GetFullFileName());

            // Check if total audio doesn't start at Zero
            if (totalAudio.Start != 0)
            {
                string bridgeFile = getBridgeFile(totalAudio.Start);
                string tempFile = String.Concat(cvo.ProcessingPath, "temp.wav");
                args = String.Format(commands.JOINAUDIOFILES, bridgeFile, totalAudio.GetFullFileName(), tempFile);
                ExecuteSox(args);
                Utility.MoveFile(tempFile, totalAudio.GetFullFileName());
                Utility.DeleteFile(new List<String> { 
                        tempFile, 
                        bridgeFile});
            }

            for (Int32 i = 1; i < audioMaster.Count; i++)
            {
                MergedFile file = audioMaster[i];
                Int32 start = file.Start;
                Int32 end = file.End;
                TimeSpan timeFormat = TimeSpan.FromMilliseconds(start);
                Int32 duration = end - start;

                String streamName = file.GetFullFileName();

                String audioMasterPart1 = String.Concat(totalAudio.FilePath, "audioMasterPart1.wav");
                if (start == totalAudio.End)
                {
                    joinAudioFiles(totalAudio, file, audioMasterPart1);
                }
                else if (start == totalAudio.Start)
                {
                    args = String.Format(commands.MERGEAUDIOFILES, totalAudio.GetFullFileName(), file.GetFullFileName(), audioMasterPart1);
                    ExecuteSox(args);

                    if (end > totalAudio.End)
                    {
                        totalAudio.End = end;
                    }
                    Utility.MoveFile(audioMasterPart1, totalAudio.GetFullFileName());
                    Utility.DeleteFile(file.GetFullFileName());
                }
                else if (start > totalAudio.End)
                {
                    MergedFile bridgeFile = new MergedFile(0, totalAudio.FilePath, FileTypes.Audio);
                    bridgeFile.Start = start;
                    bridgeFile.End = end;
                    bridgeFile.FileName = getBridgeFile((start - totalAudio.End));

                    string silentMinute = string.Concat(ConfigurationVO.ExecutableDirectory, "silentminute.wav");

                    // Create the bridgefile
                    args = String.Format(commands.GETAUDIOPART, silentMinute, bridgeFile.GetFullFileName(), 0, TimeSpan.FromMilliseconds(start - totalAudio.End));
                    ExecuteSox(args);

                    // Join the total with the bridge
                    joinAudioFiles(totalAudio, bridgeFile, audioMasterPart1);

                    // Join the total with the curent file
                    joinAudioFiles(totalAudio, file, audioMasterPart1);
                }
                else
                {
                    // starts aren't the same, doesn't matter which is longer
                    // 1) split joinedFile at new's start
                    // 2) merge 2nd joinedFile and new (ASSUMING THAT THE MERGES DO NOT HAVE TO BE THE SAME LENGTH)
                    // 3) join the 1st part of joinedFile, and the new and 2nd part merge

                    args = String.Format(commands.GETAUDIOPART, totalAudio.GetFullFileName(), audioMasterPart1, 0, timeFormat);
                    ExecuteSox(args);

                    args = String.Format(commands.GETAUDIOPART, totalAudio.GetFullFileName(), String.Concat(totalAudio.FilePath, "audioMasterPart2.wav"), timeFormat, TimeSpan.FromMilliseconds(totalAudio.End));
                    ExecuteSox(args);

                    // Use SOX to merge audio
                    args = String.Format(commands.MERGEAUDIOFILES, file.GetFullFileName(), String.Concat(totalAudio.FilePath, "audioMasterPart2.wav"), String.Concat(totalAudio.FilePath, "audioMasterPart3.wav"));
                    ExecuteSox(args);

                    // Increase volume on the merge
                    IncreaseVolume(String.Concat(totalAudio.FilePath, "audioMasterPart3.wav"));

                    args = String.Format(commands.JOINAUDIOFILES, audioMasterPart1, String.Concat(totalAudio.FilePath, "audioMasterPart3.wav"), totalAudio.GetFullFileName());
                    ExecuteSox(args);

                    // Clean up
                    Utility.DeleteFile(new List<String> { 
                        (audioMasterPart1), 
                        (String.Concat(totalAudio.FilePath, "audioMasterPart2.wav")), 
                        (String.Concat(totalAudio.FilePath, "audioMasterPart3.wav")),
                        (file.GetFullFileName())});
                    if (end > totalAudio.End)
                        totalAudio.End = end;
                }
            }
            return totalAudio;
        }

        private MergedFile MapVideoAndAudioMaster(MergedFile totalAudio, List<MergedFile> resizedFiles)
        {
            String args;

            MergedFile result = new MergedFile(0, cvo.ProcessingPath, FileTypes.Video);
            result.FileName = "RESULT";

            Boolean bResultCreated = false;

            for (int i = 0; i < resizedFiles.Count; i++)
            {
                if (resizedFiles[i].bVideo)
                {
                    TimeSpan streamStart = TimeSpan.FromMilliseconds(resizedFiles[i].Start);

                    // merge if result hasn't been created yet
                    if (bResultCreated)
                    {
                        // split totalaudio into 2 parts
                        String firstPart = String.Concat(totalAudio.FilePath, "audio1st.wav");
                        String secondPart = String.Concat(totalAudio.FilePath, "audio2nd.wav");

                        args = String.Format(commands.GETAUDIOPART, totalAudio.GetFullFileName(), firstPart, TimeSpan.FromMilliseconds(0), streamStart);
                        ExecuteSox(args);

                        args = String.Format(commands.GETAUDIOPART, totalAudio.GetFullFileName(), secondPart, streamStart, TimeSpan.FromMilliseconds(totalAudio.End));
                        ExecuteSox(args);

                        String firstPartMap = String.Concat(totalAudio.FilePath, "firstPartMap.flv");
                        args = String.Format(commands.MAPSTREAMS, result.GetFullFileName(), result.FrameRate, firstPart, firstPartMap);
                        ExecuteFFmpeg(args, firstPartMap);

                        // map video with second part
                        String secondPartMap = String.Concat(totalAudio.FilePath, "secondPartMap.flv");
                        args = String.Format(commands.MAPSTREAMS, resizedFiles[i].GetFullFileName(), resizedFiles[i].FrameRate, secondPart, secondPartMap);
                        ExecuteFFmpeg(args, secondPartMap);

                        ExecuteFlvMerge(String.Format(commands.FLVMERGE, firstPartMap, secondPartMap), result.GetFullFileName());

                        Utility.DeleteFile(new List<String> { 
                            (firstPart), 
                            (secondPart), 
                            (firstPartMap), 
                            (secondPartMap)});
                    }
                    else
                    {
                        // Assumes the video is starting at zero
                        args = String.Format(commands.MAPSTREAMS, resizedFiles[i].GetFullFileName(), resizedFiles[i].FrameRate, totalAudio.GetFullFileName(), result.GetFullFileName());
                        ExecuteFFmpeg(args, result.GetFullFileName());
                        result.FrameRate = resizedFiles[i].FrameRate;
                        bResultCreated = true;
                    }
                }
                Utility.DeleteFile(resizedFiles[i].GetFullFileName());
            }
            return result;
        }
    }
}
