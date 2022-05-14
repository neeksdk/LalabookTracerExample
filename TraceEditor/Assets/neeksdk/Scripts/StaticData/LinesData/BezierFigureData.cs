using System;
using System.Collections.Generic;

namespace neeksdk.Scripts.StaticData.LinesData
{
    [Serializable]
    public class BezierFigureData
    {
        public SerializedVectorData FirstDot;
        public List<BezierDotsData> BezierLinesData = new List<BezierDotsData>();
    }
}