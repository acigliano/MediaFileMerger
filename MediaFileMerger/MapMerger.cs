using System;
using System.Collections.Generic;
using System.Data;

namespace MediaFileMerger
{
    class MapMerger : Merger
    {
        public MapMerger(ConfigurationVO cvo)
            : base(cvo)
        {
            MapStreams();
        }

        public MapMerger(ConfigurationVO cvo, List<MergedFile> clipData)
            : base(cvo, clipData)
        {
            MapStreams();
        }

        public void MapStreams()
        {
            List<MergedFile> audioMaster = getAudioList();

            MergedFile totalAudio = JoinAndMergeAudio(audioMaster);
            audioMaster = null;

            MergedFile result = MapVideoAndAudioMaster(totalAudio);

            ConvertToTransportStream(result, commands.CONVERTTOTRANSPORTSTREAM);
            ConvertToTransportStream(totalAudio, commands.CONVERTTOTRANSPORTSTREAMAUDIO);

            Utility.DeleteFile(new List<string> { result.GetFullFileName(), totalAudio.GetFullFileName() });
        }

        // Used by MapStreams. Returns a list of the audio tracks extracted from source files
        private List<MergedFile> getAudioList()
        {
            String args;
            MergedFile mf;
            List<MergedFile> audioMaster = new List<MergedFile>();

            for (int i = 0; i < clipData.Count; i++)
            {
                if (isStreamAudio(clipData[i].CueXml) &&
                    Utility.getXmlElement(clipData[i].CueXml, "_TargetView") != "ScreenBroadcastView")
                {
                    Int32 duration = clipData[i].Duration;
                    if (duration > ConfigurationVO.MINCLIPMILLISECONDS)
                    {
                        // add to audio list
                        mf = new MergedFile((i + 1), cvo.ProcessingPath, FileTypes.Audio);
                        Int32 start = clipData[i].Start;
                        Int32 end = clipData[i].End;
                        String streamName = String.Concat(cvo.DemoPath, Utility.getStreamNameElement(clipData[i].CueXml), ".flv");

                        mf.Start = start;
                        mf.End = end;
                        args = String.Format(commands.EXTRACTAUDIO, streamName, mf.GetFullFileName());
                        ExecuteFFmpeg(args, mf.GetFullFileName());

                        // Trim the audio file to what the db duration
                        string tempFile = String.Concat(mf.FilePath, "tempFile.wav");
                        args = String.Format(commands.GETAUDIOPART, mf.GetFullFileName(), tempFile, 0, TimeSpan.FromMilliseconds(duration));
                        ExecuteSox(args);
                        Utility.MoveFile(tempFile, mf.GetFullFileName());

                        audioMaster.Add(mf);
                    }
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
                        audioMasterPart1, 
                        String.Concat(totalAudio.FilePath, "audioMasterPart2.wav"), 
                        String.Concat(totalAudio.FilePath, "audioMasterPart3.wav"),
                        file.GetFullFileName()});
                    if (end > totalAudio.End)
                        totalAudio.End = end;
                }
            }
            return totalAudio;
        }

        // Used by MapStreams to join the total audio file with the video from the source files
        private MergedFile MapVideoAndAudioMaster(MergedFile totalAudio)
        {
            String args;
            String streamName;

            MergedFile result = new MergedFile(0, cvo.ProcessingPath, FileTypes.Video);
            result.FileName = "RESULT";

            DataTable dtSource = getSourceData();
            Boolean bResultCreated = false;

            for (int i = 0; i < dtSource.Rows.Count; i++)
            {
                Int32 duration = Int32.Parse(dtSource.Rows[i]["Duration"].ToString());

                if (isStreamVideo(dtSource.Rows[i]["CueXml"].ToString()) &&
                    duration > ConfigurationVO.MINCLIPMILLISECONDS &&
                    Utility.getXmlElement(dtSource.Rows[i]["CueXml"].ToString(), "_TargetView") != "ScreenBroadcastView")
                {
                    streamName = String.Concat(cvo.DemoPath, Utility.getStreamNameElement(dtSource.Rows[i]["CueXml"].ToString()), ".flv");
                    TimeSpan streamStart = TimeSpan.FromMilliseconds(Int32.Parse(dtSource.Rows[i]["Time"].ToString()));

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
                        args = String.Format(commands.MAPSTREAMS, result.GetFullFileName(), String.Concat("-r ", getFrameRate(result.GetFullFileName())), firstPart, firstPartMap);
                        ExecuteFFmpeg(args, firstPartMap);

                        // run flvmdi on the original
                        String streamNameMdi = String.Concat(cvo.ProcessingPath, "flvmdi.flv");
                        args = String.Format(commands.RUNFLVMDI, streamName, streamNameMdi);
                        ExecuteFlvMdi(args);

                        // map video with second part
                        String secondPartMap = String.Concat(totalAudio.FilePath, "secondPartMap.flv");
                        args = String.Format(commands.MAPSTREAMS, streamName, String.Concat("-r ", getFrameRate(streamNameMdi)), secondPart, secondPartMap);
                        ExecuteFFmpeg(args, secondPartMap);

                        ExecuteFlvMerge(String.Format(commands.FLVMERGE, firstPartMap, secondPartMap), result.GetFullFileName());

                        Utility.DeleteFile(new List<String> { 
                            firstPart, 
                            secondPart, 
                            firstPartMap, 
                            secondPartMap, 
                            streamNameMdi});
                    }
                    else
                    {
                        // run flvmdi on the original
                        String streamNameMdi = String.Concat(cvo.ProcessingPath, "flvmdi.flv");
                        args = String.Format(commands.RUNFLVMDI, streamName, streamNameMdi);
                        ExecuteFlvMdi(args);

                        // Assumes the video is starting at zero
                        args = String.Format(commands.MAPSTREAMS, streamNameMdi, String.Concat("-r ", getFrameRate(streamNameMdi)), totalAudio.GetFullFileName(), result.GetFullFileName());
                        ExecuteFFmpeg(args, result.GetFullFileName());
                        bResultCreated = true;
                    }
                }
            }

            return result;
        }
    }
}
