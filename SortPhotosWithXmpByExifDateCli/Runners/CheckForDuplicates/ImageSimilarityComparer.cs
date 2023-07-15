namespace SortPhotosWithXmpByExifDateCli.CheckForDuplicates
{
    public class ImageSimilarityComparer : IComparer<(double similarity, string imagePath1, string imagePath2)>
    {
        public int Compare((double similarity, string imagePath1, string imagePath2) x,
                           (double similarity, string imagePath1, string imagePath2) y)
        {
            return -x.similarity.CompareTo(y.similarity);
        }
    }
}