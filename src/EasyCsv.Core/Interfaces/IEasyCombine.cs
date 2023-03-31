using System.Collections.Generic;

namespace EasyCsv.Core
{
    public interface IEasyCombine
    {
        /// <summary>
        /// If all the headers match, adds all the data from other csv to this csv.
        /// </summary>
        /// <param name="otherCsv">The csv with the data you would like to be added to this one</param>
        /// <returns></returns>
        IEasyCsv Combine(IEasyCsv? otherCsv);


        /// <summary>
        /// Performs combine on multiple csvs. See <see cref="Combine(IEasyCsv?)"/>
        /// </summary>
        /// <param name="otherCsv">The csvs with the data you would like to be added to this one</param>
        /// <returns></returns>
        IEasyCsv Combine(List<IEasyCsv?> otherCsv);
    }
}