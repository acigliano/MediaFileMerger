using System;
using System.Collections.Generic;
using System.Text;

namespace MediaFileMerger
{
    class ExeCommands
    {
        private Dictionary<string, string> commands = new Dictionary<string, string>();

        public ExeCommands()
        {
            commands.Add("CONVERTCLIP", "-i {0} -f avi -vcodec rawvideo -an {1} {2}");
            commands.Add("CONVERTTOTRANSPORTSTREAM", @"-i {0} -f mpegts -acodec libmp3lame -ar 48000 -ab 64k -vcodec libx264 -b 256k -subq 5 -trellis 1 -refs 1 -coder 0 -me_range 16 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -bt 200k -maxrate 96k -bufsize 96k -rc_eq 'blurCplx^(1-qComp)' -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -level 30 -aspect 320:240 -g 30 -async 2 {1}");
            commands.Add("CONVERTTOTRANSPORTSTREAMAUDIO", @"-i {0} -f mpegts -acodec libmp3lame -ar 48000 -ab 64k -subq 5 -trellis 1 -refs 1 -coder 0 -me_range 16 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -bt 200k -maxrate 96k -bufsize 96k -rc_eq 'blurCplx^(1-qComp)' -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -level 30 -aspect 320:240 -g 30 -async 2 {1}");
            commands.Add("EXPORTTOIOS", "-i {0} -b 256k {1} -acodec aac -strict experimental -ar 22050 -ab 56000 {2}");
            commands.Add("EXPORTTOIOSAUDIO", "-i {0} -b 64000 -ar 22050 -acodec aac -strict experimental {1}");
            commands.Add("EXTRACTAUDIO", "-i {0} -vn -ar 44100 -ab 64000 -async 1 {1}");
            commands.Add("FLVMERGE", "{0} {1}");
            commands.Add("GETAUDIOPART", "\"{0}\" \"{1}\" trim {2} {3}");
            commands.Add("INCREASESOUNDVOLUME", "-v 3.0 {0} {1}");
            commands.Add("JOINAUDIOFILES", "\"{0}\" \"{1}\" \"{2}\" splice");
            commands.Add("MAPSTREAMS", "-i {0} -vcodec vp6f {1} -b 256k -i {2} -acodec copy -map 0.0 -map 1.0 {3}");
            commands.Add("MERGEAUDIOFILES", "-m {0} {1} {2}");
            commands.Add("RESIZECLIP", "-ss {0} -t {1} -i {2} {3}");
            commands.Add("RUNFLVMDI", "{0} {1}");
            commands.Add("SEGMENTER", "{0} {1} {2} {3}.m3u8 {4}"); 
            /*
             *  0 inputFile
             *  1 segmentDurationInSeconds 
             *  2 outputTSFilePrefix
             *  3 output_m3u8File
             *  4 httpPrefix
             */
        }

        public string CONVERTCLIP { get { return commands["CONVERTCLIP"]; } }
        public string CONVERTTOTRANSPORTSTREAM { get { return commands["CONVERTTOTRANSPORTSTREAM"]; } }
        public string CONVERTTOTRANSPORTSTREAMAUDIO { get { return commands["CONVERTTOTRANSPORTSTREAMAUDIO"]; } }
        public string EXPORTTOIOS { get { return commands["EXPORTTOIOS"]; } }
        public string EXPORTTOIOSAUDIO { get { return commands["EXPORTTOIOSAUDIO"]; } }
        public string EXTRACTAUDIO { get { return commands["EXTRACTAUDIO"]; } }
        public string FLVMERGE { get { return commands["FLVMERGE"]; } }
        public string GETAUDIOPART { get { return commands["GETAUDIOPART"]; } }
        public string INCREASESOUNDVOLUME { get { return commands["INCREASESOUNDVOLUME"]; } }
        public string JOINAUDIOFILES { get { return commands["JOINAUDIOFILES"]; } }
        public string MAPSTREAMS { get { return commands["MAPSTREAMS"]; } }
        public string MERGEAUDIOFILES { get { return commands["MERGEAUDIOFILES"]; } }
        public string RESIZECLIP { get { return commands["RESIZECLIP"]; } }
        public string RUNFLVMDI { get { return commands["RUNFLVMDI"]; } }
        public string SEGMENTER { get { return commands["SEGMENTER"]; } }
    }
}
