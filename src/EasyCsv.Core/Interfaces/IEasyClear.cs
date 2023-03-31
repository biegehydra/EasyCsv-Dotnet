namespace EasyCsv.Core
{
    public interface IEasyClear<T>
    {
        /// <summary>
        /// Sets <code>CsvContent</code> to null. 
        /// </summary>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        T Clear();
    }
}