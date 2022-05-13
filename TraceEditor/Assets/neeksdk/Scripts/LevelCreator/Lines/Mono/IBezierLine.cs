namespace neeksdk.Scripts.LevelCreator.Lines.Mono
{
    public interface IBezierLine
    {
        void AddPoint(BezierLinePart newLinePart);
        void DeletePoint(BezierLinePart bezierLinePart);
        void DeletePoint(int index);
        void UpdateLineWithFingerPoint();
    }
}