using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MediaFileMerger
{
    class MediaFileMerger
    {
        private ConfigurationVO cvo;
        private Boolean bClipsNeedTrimming = false;

        public MediaFileMerger(ConfigurationVO cvo)
        {
            this.cvo = cvo;
            Utility.CreateDirectory(this.cvo.ProcessingPath);
        }

        public void Process()
        {
            // get data
            DataTable dtSource = getSourceData();

            List<MergedFile> clipData = new List<MergedFile>();

            // check for alt starts
            foreach (DataRow dr in dtSource.Rows)
            {
                Int32 altStart;
                MergedFile clip = new MergedFile(clipData.Count, cvo.ProcessingPath, FileTypes.Video);
                clip.bAudio = Utility.isStreamAudio(dr["CueXml"].ToString());
                clip.bVideo = Utility.isStreamVideo(dr["CueXml"].ToString());
                clip.End = Int32.Parse(dr["End"].ToString());
                clip.Start = Int32.Parse(dr["Time"].ToString());
                clip.StreamName = String.Concat(cvo.DemoPath, Utility.getStreamNameElement(dr["CueXml"].ToString()), ".flv");
                clip.Duration = Int32.Parse(dr["Duration"].ToString());
                clip.CueXml = dr["CueXml"].ToString();

                if (Int32.TryParse(Utility.getXmlElement(dr["CueXml"].ToString(), "_StartTime"), out altStart) && altStart > 0)
                {
                    clip.AltStart = altStart;
                    bClipsNeedTrimming = true;
                }
                clipData.Add(clip);
            }

            if (bClipsNeedTrimming)
            {
                //MapResizedStreams();
                //ResizeMerger rm = new ResizeMerger(cvo);
                ResizeMerger rm = new ResizeMerger(cvo, clipData);
            }
            else 
            {
                //MapStreams();
                MapMerger mm = new MapMerger(cvo, clipData);
            }
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
    }
}
