namespace SortPhotosWithXmp.ErrorHandlers;

public sealed class NoTimeFoundError : ErrorBase
{
    public NoTimeFoundError(string file, List<string> messages)
    : base(file, messages.Prepend(nameof(NoTimeFoundError)))
    {
    }

    public override string Name => nameof(NoTimeFoundError);
}
