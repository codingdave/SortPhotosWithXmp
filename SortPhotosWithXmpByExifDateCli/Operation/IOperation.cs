namespace SortPhotosWithXmpByExifDateCli.Operation
{
    public interface IOperation
    {
        public bool IsChanging { get; }
        public bool IsSimulating => !IsChanging;
    }
}