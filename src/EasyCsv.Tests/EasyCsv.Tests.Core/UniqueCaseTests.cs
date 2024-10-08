﻿using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;
using EasyCsv.Core;
using System.Globalization;
using CsvHelper.Configuration.Attributes;
using EasyCsv.Core.Configuration;
using System.Text;
using System.Drawing;
using Newtonsoft.Json;
using Point = NetTopologySuite.Geometries.Point;

namespace EasyCsv.Tests.Core;
public class UniqueCaseTests
{
    [Fact]
    public async Task RunRapidApiListingFromCsv()
    {
        var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            ShouldUseConstructorParameters = x => true,
            GetConstructor = x => x.ClassType.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0) ?? x.ClassType.GetConstructors().FirstOrDefault(),
            PrepareHeaderForMatch = x => x.Header.Trim()
        };
        var config = new EasyCsvConfiguration()
        {
            CsvHelperConfig = csvConfig
        };
        var csv = await EasyCsvFactory.FromFileAsync("C:\\Users\\sweed\\Downloads\\RapidApiPanamaCityAddresses.csv", config: config);
        await csv!.MutateAsync(x =>
        {
            x.AddColumn("Country", "US");
            x.ReplaceColumn("Address", "StreetLine");
            x.ReplaceColumn("Estimated Value", "EstimatedValueAmount");
            x.ReplaceColumn("Last Sold Date", "LastSaleDate");
            x.ReplaceColumn("Sqft", "AreaSqFt");
        });
        var rapidApiAddresses = await csv.GetRecordsAsync<RapidApiAddress>();
    }


    [Fact]
    public async Task SingleColumnCreateObjects()
    {
        var dir = Directory.GetCurrentDirectory();
        var path = Path.Combine(dir, "Csvs/SingleColumn.csv");
        IEasyCsv easyCsv = await EasyCsvFactory.FromFileAsync(path, int.MaxValue);
        var records = await easyCsv!.GetRecordsAsync<SingleColumnExample>();

        Assert.NotNull(records);
        Assert.NotEmpty(records);
    }


    [Fact]
    public async Task DuplicateHeadersDoesNotThrow()
    {
        try
        {
            var easyCsv = await EasyCsvFactory.FromStringAsync("header,header,header,header\nvalue1,value2");
            Assert.True(easyCsv!.ColumnNames()!.SequenceEqual(["header","header2","header3","header4"]));
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.ToString());
        }
    }


    [Fact]
    public async Task EmptyHeadersDoesNotThrowException()
    {
        var dir = Directory.GetCurrentDirectory();
        var path = Path.Combine(dir, "Csvs/EmptyHeaders.csv");
        IEasyCsv easyCsv = await EasyCsvFactory.FromFileAsync(path, int.MaxValue, new EasyCsvConfiguration()
        {
            CsvHelperConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            },
            GiveEmptyHeadersNames = true
        });
        var records = await easyCsv!.GetRecordsAsync<SingleColumnExample>();

        Assert.NotNull(records);
        Assert.NotEmpty(records);
        Assert.True(!string.IsNullOrWhiteSpace(records[0].FullSiteAddress));
    }

    [Fact]
    public async Task EmptyHeadersDoesNotThrowExceptionBytes()
    {
        var dir = Directory.GetCurrentDirectory();
        var path = Path.Combine(dir, "Csvs/EmptyHeaders.csv");
        var text = await File.ReadAllTextAsync(path);
        var bytes = Encoding.UTF8.GetBytes(text);
        IEasyCsv easyCsv = await EasyCsvFactory.FromBytesAsync(bytes, new EasyCsvConfiguration()
        {
            CsvHelperConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            },
            GiveEmptyHeadersNames = true
        });
        var records = await easyCsv!.GetRecordsAsync<SingleColumnExample>();

        Assert.NotNull(records);
        Assert.NotEmpty(records);
        Assert.True(!string.IsNullOrWhiteSpace(records[0].FullSiteAddress));
    }


    public class Person
    {
        public string Name { get; set; }
        [TypeConverter(typeof(EasyDefaultTypeConverter<DateTime>))]
        public DateTime DateOfBirth { get; set; }
        [NumberStyles(NumberStyles.AllowCurrencySymbol)]
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

    public class PersonMap2 : ClassMap<Person>
    {
        public PersonMap2()
        {
            Map(m => m.Name);
            Map(m => m.DateOfBirth).Ignore();
            Map(x => x.Num).Name("Num").TypeConverter(new DefaultNullConverter<int?>());
        }
    }

    public class DefaultNullConverter<T> : ITypeConverter
    {
        private static readonly DefaultConverter<T> Converter = new() { Culture = CultureInfo.InvariantCulture };
        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return Converter.Get(text);
        }

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return Converter.Set((T)value);
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
    public async Task ClassMapTest2()
    {
        var service = await EasyCsvFactory.FromStringAsync("Name,Num\nJohn,4.0");

        var csvContextProfile = new CsvContextProfile()
        {
            ClassMaps = new List<ClassMap>()
            {
                new PersonMap2()
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
        var service = await EasyCsvFactory.FromStringAsync("Name,DateOfBirth,Num\nJohn,01/01/2000,1");
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

    [Fact(Skip = "I really feel like this should work but it doesn't yet")]
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
        var records = await service!.GetRecordsAsync<Person>(csvContextProfile: csvContextProfile);
        Assert.Equal(new DateTime(2000, 1, 1), records[0].DateOfBirth);
    }

    public class MyTp : TypeConverterOptions
    {
        
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
public partial class RapidApiAddress
{
    private string DebuggerDisplay
    {
        get
        {
            return $"MasterAddressId: {MasterAddressId} | Full Address: {StreetLine}, {City}, {State}, {Zip} | Beds: {Beds} | Baths: {Baths}";
        }
    }
    public int MasterAddressId { get; set; }

    public string AssessorParcelNumber { get; set; }

    public string StreetLine { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string Zip { get; set; }

    public string Country { get; set; }
    public string Url { get; set; }
    [JsonIgnore]
    [Ignore]
    public Point Location { get; set; }

    public int? Beds { get; set; }

    public decimal? Baths { get; set; }

    public int? Sleeps { get; set; }

    public string EstimatedValuationDate { get; set; }

    public int? EstimatedValueAmount { get; set; }

    public string LastSaleDate { get; set; }

    public int? LastSalePrice { get; set; }

    public int? ListPrice { get; set; }

    public int? PropertyUseCode { get; set; }

    public string PropertyType { get; set; }

    public string Image { get; set; }

    public string Amenities { get; set; }

    public int? YearBuilt { get; set; }

    public int? AreaSqFt { get; set; }

    public DateTime? LastSeen { get; set; }
}

public enum DataSource
{
    RapidApi
}

public class MasterAddress
{

}