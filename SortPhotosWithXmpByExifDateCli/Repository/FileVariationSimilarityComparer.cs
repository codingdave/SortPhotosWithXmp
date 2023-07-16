namespace SortPhotosWithXmpByExifDateCli.Repository
{
    public class FileVariationSimilarityComparer : IComparer<(double similarity, FileVariations first, FileVariations second)>
    {
        public int Compare((double similarity, FileVariations first, FileVariations second) x,
                           (double similarity, FileVariations first, FileVariations second) y)
        {
            return -x.similarity.CompareTo(y.similarity);
        }
    }
}