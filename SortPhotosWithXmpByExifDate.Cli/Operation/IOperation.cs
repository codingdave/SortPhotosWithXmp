namespace SortPhotosWithXmpByExifDate.Operation;

public interface IOperation
{
    public bool IsForce { get; }
    public bool IsSimulating => !IsForce;
}