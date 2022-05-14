using System;

namespace neeksdk.Scripts.StaticData.LinesData
{
    [Serializable]
    public class BezierDotsData
    {
        public SerializedVectorData[] LineDots;
        public SerializedVectorData[] BezierControlDots;
    }
}