namespace SortPhotosWithXmp.ErrorHandlers
{
    public interface IError
    {
        string FileName { get; }
        string ErrorMessage { get; }
        string Name { get; }

        void AddMessage(string errorMessage);
    }
}