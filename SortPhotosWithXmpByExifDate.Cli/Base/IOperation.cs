namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public interface IOperation
    {
        public bool IsChanging { get; }
        public bool IsSimulating => !IsChanging;
    }
}