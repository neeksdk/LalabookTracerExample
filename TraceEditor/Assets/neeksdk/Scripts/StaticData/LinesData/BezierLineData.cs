using System;

namespace neeksdk.Scripts.StaticData.LinesData
{
    [Serializable]
    public class BezierLineData
    {
        public SerializedVectorData FirstLineDot;
        public SerializedVectorData[] LineDots;
        public SerializedVectorData[] BezierControlDots;
    }
}