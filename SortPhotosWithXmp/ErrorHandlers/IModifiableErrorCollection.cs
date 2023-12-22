namespace SortPhotosWithXmp.ErrorHandlers
{
    public interface IModifiableErrorCollection<T> where T : IError
    {
        public void AddError(T error);
    }
}