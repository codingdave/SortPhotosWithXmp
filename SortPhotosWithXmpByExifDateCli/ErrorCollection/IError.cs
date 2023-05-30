namespace SortPhotosWithXmpByExifDateCli.ErrorCollection
{
    public interface IError
    {
        string File { get; }
        string ErrorMessage { get; }
        string Name { get; }

        void AddMessage(string errorMessage);
    }
}