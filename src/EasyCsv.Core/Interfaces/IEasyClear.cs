namespace EasyCsv.Core
{
    public interface IEasyClear
    {
        /// <summary>
        /// Sets <code>CsvContent</code> to null. 
        /// </summary>
        /// <remarks>WARNING: Writes and reads all records. Can be an expensive call</remarks>
        IEasyCsv Clear();
    }
}