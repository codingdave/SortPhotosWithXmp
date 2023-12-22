namespace SortPhotosWithXmp.Operation;

public interface IOperation
{
    public bool IsForce { get; }
    public bool IsSimulating => !IsForce;
}