using neeksdk.Scripts.Properties;

namespace neeksdk.Scripts.LevelCreator.Lines.Mono
{
    public interface IBezierLine
    {
        void AddPoint(IBezierLinePart newLinePart);
        void DeletePoint(IBezierLinePart bezierLinePart);
        void DeletePoint(int index);
        void UpdateLineWithFingerPoint();
        void ChangeDotColors(ColorTypes colorType);
    }
}