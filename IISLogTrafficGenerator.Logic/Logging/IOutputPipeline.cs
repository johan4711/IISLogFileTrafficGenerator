namespace IISLogTrafficGenerator.Logic.Logging
{
    public interface IOutputPipeline
    {
        void Write(string message);
    }
}