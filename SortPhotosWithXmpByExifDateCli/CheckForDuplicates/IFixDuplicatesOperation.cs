namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    internal interface IFixDuplicatesOperation : IOperation
    {
        void HandleDuplicates(string imagePath1, string imagePath2, double similarity);
        void HandleDuplicates(IEnumerable<string> enumerable);
    }
}