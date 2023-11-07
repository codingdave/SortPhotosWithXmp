namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public interface IOperation
    {
        public bool Force { get; }
        public bool IsSimulating => !Force;
    }
}