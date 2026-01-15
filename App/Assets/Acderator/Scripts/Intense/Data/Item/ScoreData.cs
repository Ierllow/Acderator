namespace Intense.Data
{
    [System.Serializable]
    public class ScoreData
    {
        public int Sid { get; }
        public int ScoreNum { get; set; }

        public ScoreData(int sid, int scoreNum)
        {
            Sid = sid;
            ScoreNum = scoreNum;
        }
    }
}