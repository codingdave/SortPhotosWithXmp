using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    public interface IReadOnlyErrorCollection 
    {
        public IReadOnlyList<IError> Errors { get; }
    }
}