using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using neeksdk.Scripts.Extensions;
using neeksdk.Scripts.LevelCreator.Lines.Mono;
using neeksdk.Scripts.StaticData.LinesData;
using UnityEditor;
using UnityEngine;

namespace neeksdk.Scripts.Infrastructure.SaveLoad
{
    public static class SaveLoadDataClass
    {
        public static void SaveFigureWithDialog(List<BezierLine> bezierLines)
        {
            BezierFigureData bezierFigureData = new BezierFigureData();
            for (int i = 0; i < bezierLines.Count; i++)
            {
                BezierLine bezierLine = bezierLines[i];
                if (bezierLine != null)
                {
                    int dotsCount = bezierLine.Dots.Count;
                    BezierDotsData dotsData = new BezierDotsData();
                    dotsData.LineDots = new SerializedVectorData[dotsCount];
                    dotsData.BezierControlDots = new SerializedVectorData[dotsCount];
                    dotsData.FirstDot = bezierLine.StartPointTransform.position.ToSerializedVector();
                    for (int j = 0; j < dotsCount; j++)
                    {
                        IBezierLinePart linePart = bezierLine.Dots[j];
                        dotsData.LineDots[j] = linePart.GetLineDotPosition.ToSerializedVector();
                        dotsData.BezierControlDots[j] = linePart.GetBezierControlDotPosition.ToSerializedVector();
                    }
                    bezierFigureData.BezierLinesData.Add(dotsData);
                }
            }

            string destination = EditorUtility.OpenFilePanel("Select file to save", Path.Combine(Application.streamingAssetsPath, "FigureAssets"), "dat");
            FileStream file;

            if (File.Exists(destination)) {
                file = File.OpenWrite(destination);
            } else {
                file = File.Create(destination);
            }
 
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, bezierFigureData);
            file.Close();
        }
        
        public static bool TryLoadFigureWithDialog(out BezierFigureData bezierFigureData)
        {
            bezierFigureData = null;
            string loadPath = EditorUtility.OpenFilePanel("Select file to load from", Path.Combine(Application.streamingAssetsPath, "FigureAssets"), "dat");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(loadPath, FileMode.Open);
            if (bf.Deserialize(file) is BezierFigureData figureData)
            {
                bezierFigureData = figureData;
            }
            
            file.Close();

            return bezierFigureData != null;
        }

        public static bool TryLoadFigureByFile(string loadPath, out BezierFigureData bezierFigureData)
        {
            bezierFigureData = null;
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(loadPath, FileMode.Open);
            if (bf.Deserialize(file) is BezierFigureData figureData)
            {
                bezierFigureData = figureData;
            }
            
            file.Close();

            return bezierFigureData != null;
        }
    }
}