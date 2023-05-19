using System.Linq;

namespace SortPhotosWithXmpByExifDateCli.Statistics
{
    internal class MergedFileError : IReadOnlyFileError
    {
        private readonly IReadOnlyFileError _errorCollection1;
        private readonly IReadOnlyFileError _errorCollection2;

        public MergedFileError(IReadOnlyFileError errorCollection1, IReadOnlyFileError errorCollection2)
        {
            _errorCollection1 = errorCollection1;
            _errorCollection2 = errorCollection2;
        }

        public IReadOnlyList<IError> Errors => _errorCollection1.Errors.Concat(_errorCollection2.Errors).ToList();
    }
}