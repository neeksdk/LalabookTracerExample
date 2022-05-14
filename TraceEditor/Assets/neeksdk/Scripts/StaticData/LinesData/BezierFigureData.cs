using System;
using System.Collections.Generic;

namespace neeksdk.Scripts.StaticData.LinesData
{
    [Serializable]
    public class BezierFigureData
    {
        public List<BezierDotsData> BezierLinesData = new List<BezierDotsData>();
    }
}