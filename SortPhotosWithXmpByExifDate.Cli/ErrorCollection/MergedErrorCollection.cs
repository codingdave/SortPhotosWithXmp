namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    internal class MergedErrorCollection : IReadOnlyErrorCollection
    {
        private readonly IReadOnlyErrorCollection _errorCollection1;
        private readonly IReadOnlyErrorCollection _errorCollection2;

        public MergedErrorCollection(IReadOnlyErrorCollection errorCollection1, IReadOnlyErrorCollection errorCollection2)
        {
            _errorCollection1 = errorCollection1;
            _errorCollection2 = errorCollection2;
        }

        public IReadOnlyList<IError> Errors => _errorCollection1.Errors.Concat(_errorCollection2.Errors).ToList();
    }
}