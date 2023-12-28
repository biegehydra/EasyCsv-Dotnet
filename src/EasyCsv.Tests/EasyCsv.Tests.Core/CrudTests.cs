using EasyCsv.Core;
using EasyCsv.Core.Configuration;

namespace EasyCsv.Tests.Core;

public class CrudTests
{
    private static EasyCsvConfiguration DefaultConfig => EasyCsvConfiguration.Instance;
    [Fact]
    public void AddRecordTest()
    {
        IEasyCsv easyCsv = CreateCsvWithSampleData();
        easyCsv.Mutate(x => x.InsertRecord(new List<object> { "Value3", "Value4" }));

        Assert.Equal(3, easyCsv.CsvContent!.Count);
        Assert.Equal("Value3", easyCsv.CsvContent[2]["Header1"]);
        Assert.Equal("Value4", easyCsv.CsvContent[2]["Header2"]);
    }

    [Fact]
    public void InsertRecordTest()
    {
        IEasyCsv easyCsv = CreateCsvWithSampleData();
        easyCsv.InsertRecord(["Value3", "Value4"], 1);

        Assert.Equal(3, easyCsv.CsvContent!.Count);
        Assert.Equal("Value3", easyCsv.CsvContent[1]["Header1"]);
        Assert.Equal("Value4", easyCsv.CsvContent[1]["Header2"]);
    }

    [Fact]
    public void UpsertRecordTest_AddsRecord()
    {
        IEasyCsv easyCsv = CreateCsvWithSampleData();
        var newRow = new CsvRow(new Dictionary<string, object>() { { "Header1", "Value1" }, { "Header2", "UpdatedValue" } });

        easyCsv.UpsertRecord(newRow);

        Assert.Equal(3, easyCsv.CsvContent!.Count);
        Assert.Equal("Value1", easyCsv.CsvContent[2]["Header1"]);
        Assert.Equal("UpdatedValue", easyCsv.CsvContent[2]["Header2"]);
    }

    [Fact]
    public void UpsertRecordTest_UpdatesRecord()
    {
        IEasyCsv easyCsv = CreateCsvWithSampleData();
        var newRow = new CsvRow(new Dictionary<string, object>() { { "Header1", "Value1" }, { "Header2", "UpdatedValue" } });

        easyCsv.UpsertRecord(newRow, 1);

        Assert.Equal(2, easyCsv.CsvContent!.Count);
        Assert.Equal("Value1", easyCsv.CsvContent[1]["Header1"]);
        Assert.Equal("UpdatedValue", easyCsv.CsvContent[1]["Header2"]);
    }

    [Fact]
    public void GetRecordTest()
    {
        IEasyCsv easyCsv = CreateCsvWithSampleData();
        var row = easyCsv.GetRecord(0);

        Assert.NotNull(row);
        Assert.Equal("Value1", row["Header1"]);
        Assert.Equal("Value2", row["Header2"]);
    }

    [Fact]
    public void UpdateRecordTest()
    {
        IEasyCsv easyCsv = CreateCsvWithSampleData();
        var newRow = new CsvRow(new Dictionary<string, object>() { { "Header1", "UpdatedValue1" }, { "Header2", "UpdatedValue2" } });

        easyCsv.UpdateRecord(0, newRow);

        Assert.Equal(2, easyCsv.CsvContent!.Count);
        Assert.Equal("UpdatedValue1", easyCsv.CsvContent[0]["Header1"]);
        Assert.Equal("UpdatedValue2", easyCsv.CsvContent[0]["Header2"]);
    }

    [Fact]
    public void DeleteRecordTest()
    {
        IEasyCsv easyCsv = CreateCsvWithSampleData();
        easyCsv.DeleteRecord(0);

        Assert.Single(easyCsv.CsvContent!);
    }

    private static IEasyCsv CreateCsvWithSampleData()
    {
        var data = new List<CsvRow>
        {
            new (new Dictionary<string, object> { { "Header1", "Value1" }, { "Header2", "Value2" } }),
            new (new Dictionary < string, object > { { "Header1", "Value3" }, { "Header2", "Value4" } })
        };

        return new EasyCsv.Core.EasyCsv(data, DefaultConfig);
    }
}