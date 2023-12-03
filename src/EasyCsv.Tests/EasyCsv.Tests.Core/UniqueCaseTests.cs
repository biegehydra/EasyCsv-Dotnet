using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;
using EasyCsv.Core;
using System.Globalization;
using EasyCsv.Core.Configuration;

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


    public class Person
    {
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Num { get; set; }
    }

    public class PersonMap : ClassMap<Person>
    {
        public PersonMap()
        {
            Map(m => m.Name).Name("SomeOtherName");
            Map(m => m.DateOfBirth).Name("SomeOtherDateOfBirth");
            Map(x => x.Num).Name("Num");
        }
    }

    public class EasyDefaultTypeConverter<T> : DefaultTypeConverter
    {
        private static readonly DefaultConverter<T> Converter = new () {Culture = CultureInfo.InvariantCulture};
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return Converter.Get(text);
        }
    }

    [Fact]
    public async Task ClassMapTest()
    {
        var service = await EasyCsvFactory.FromStringAsync("SomeOtherName,SomeOtherDateOfBirth,Num\nJohn,01-01-2000,1");

        var csvContextProfile = new CsvContextProfile()
        {
            ClassMaps = new List<ClassMap>()
            {
                new PersonMap()
            }
        };
        var records = await service!.GetRecordsAsync<Person>(csvContextProfile: csvContextProfile);
        Assert.Equal("John", records[0].Name);
        Assert.Equal(new DateTime(2000, 1, 1), records[0].DateOfBirth);
        Assert.Equal(1, records[0].Num);
    }

    [Fact]
    public async Task TypeConverterTest()
    {
        var service = await EasyCsvFactory.FromStringAsync("Name,DateOfBirth,Num\nJohn,2000-01-01,1");
        var csvContextProfile = new CsvContextProfile()
        {
            TypeConverters = new Dictionary<Type, ITypeConverter>()
            {
                {typeof(DateTime), new EasyDefaultTypeConverter<DateTime>()}
            }
        };

        var records = await service!.GetRecordsAsync<Person>(csvContextProfile: csvContextProfile);
        Assert.Equal(new DateTime(2000, 1, 1), records[0].DateOfBirth);
    }

    [Fact]
    public async Task TypeConverterOptionsTest()
    {
        var service = await EasyCsvFactory.FromStringAsync("Name,DateOfBirth,Num\nJohn,01-01-2000,$1");
        var csvContextProfile = new CsvContextProfile()
        {
            TypeConvertersOptionsDict = new Dictionary<Type, TypeConverterOptions>()
            {
                {typeof(int), new TypeConverterOptions {NumberStyles = NumberStyles.AllowCurrencySymbol}}
            }
        };
        var records = await service.GetRecordsAsync<Person>(csvContextProfile: csvContextProfile);
        Assert.Equal(new DateTime(2000, 1, 1), records[0].DateOfBirth);
    }

    public class IncorrectPersonMap : ClassMap<Person>
    {
        public IncorrectPersonMap()
        {
            // Intentionally mapping to wrong column names
            Map(m => m.Name).Name("IncorrectName");
            Map(m => m.DateOfBirth).Name("IncorrectDateOfBirth");
            Map(m => m.Num).Name("IncorrectNum");
        }
    }

    [Fact]
    public async Task FailingClassMapTest()
    {
        var service = await EasyCsvFactory.FromStringAsync("SomeOtherName,SomeOtherDateOfBirth,SomeOtherNum\nJohn,01-01-2000,1");
        var csvContextProfile = new CsvContextProfile()
        {
            ClassMaps = new List<ClassMap>()
            {
                new IncorrectPersonMap()
            }
        };
        // Using incorrect mapping
        await Assert.ThrowsAsync<HeaderValidationException>(() => service!.GetRecordsAsync<Person>(csvContextProfile:  csvContextProfile));
    }


    [Fact]
    public async Task FailingTypeConverterOptionsTest()
    {
        var service = await EasyCsvFactory.FromStringAsync("Name,DateOfBirth,Num\nJohn,01-01-2000,$1");
        var options = new TypeConverterOptions { Formats = new[] { "yyyy-MM-dd" } }; // Incorrect format
        // Assertion will fail because the options format doesn't match the actual format
        await Assert.ThrowsAsync<TypeConverterException>(() => service!.GetRecordsAsync<Person>());
    }

}
public class SingleColumnExample
{
    public string FullSiteAddress { get; set; }
};
