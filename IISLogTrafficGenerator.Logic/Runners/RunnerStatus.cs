namespace IISLogTrafficGenerator.Logic.Runners
{
    public enum RunnerStatus
    {
        NotStarted = 0,
        Started,
        Running,
        StoppedWithErrors,
        Done
    }
}