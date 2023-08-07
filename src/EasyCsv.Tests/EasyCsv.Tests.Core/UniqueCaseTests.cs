using EasyCsv.Core;

namespace EasyCsv.Tests.Core;
public class UniqueCaseTests
{

    [Fact]
    public async Task SingleColumnCreateObjects()
    {
        var dir = Directory.GetCurrentDirectory();
        var path = Path.Combine(dir, "Csvs/SingleColumn.csv");
        var easyCsv = await EasyCsvFactory.FromFileAsync(path, int.MaxValue);
        var records = await easyCsv!.GetRecordsAsync<SingleColumnExample>();

        Assert.NotNull(records);
        Assert.NotEmpty(records);
    }
}
public class SingleColumnExample
{
    public string FullSiteAddress { get; set; }
};
