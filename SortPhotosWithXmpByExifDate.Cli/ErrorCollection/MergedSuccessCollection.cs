using SortPhotosWithXmpByExifDate.Cli.Result;

namespace SortPhotosWithXmpByExifDate.Cli.ErrorCollection
{
    internal class MergedSuccessCollection : IReadOnlySuccessCollection
    {
        private readonly IReadOnlySuccessCollection _successCollection1;
        private readonly IReadOnlySuccessCollection _successCollection2;

        public MergedSuccessCollection(IReadOnlySuccessCollection successCollection1, IReadOnlySuccessCollection successCollection2)
        {
            _successCollection1 = successCollection1;
            _successCollection2 = successCollection2;
        }

        public IReadOnlyList<ISuccess> Successes => _successCollection1.Successes.Concat(_successCollection2.Successes).ToList();
    }
}