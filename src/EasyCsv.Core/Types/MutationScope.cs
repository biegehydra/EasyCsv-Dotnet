using System;

namespace EasyCsv.Core;

public class CSVMutationScope
{
    private readonly IEasyCsv _csv;

    public CSVMutationScope(IEasyCsv csv)
    {
        _csv = csv;
    }

}