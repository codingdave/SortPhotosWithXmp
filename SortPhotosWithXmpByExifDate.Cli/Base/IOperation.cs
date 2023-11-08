namespace SortPhotosWithXmpByExifDate.Cli.Operations
{
    public interface IOperation
    {
        public bool IsForce { get; }
        public bool IsSimulating => !IsForce;
    }
}