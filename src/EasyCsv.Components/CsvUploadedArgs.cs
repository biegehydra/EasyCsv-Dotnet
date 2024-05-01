using EasyCsv.Core;

namespace EasyCsv.Components;

public readonly record struct CsvUploadedArgs(IEasyCsv Csv, string FileName)
{
    public void Deconstruct(out IEasyCsv csv, out string fileName)
    {
        csv = Csv;
        fileName = FileName;
    }
};