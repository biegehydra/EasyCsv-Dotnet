# EasyCsv [Nuget](https://www.nuget.org/packages/EasyCsv)

EasyCsv is a simple and efficient .NET library for handling CSV files in your projects. With a **fluent** user-friendly API, it allows you to easily read, write, and manipulate CSV files with a minimal amount of code.

## Features

* Read and write CSV files
* Support for larger number of types: byte[], string, Stream, TextReader, Objects<T>, IBrowserFile, IFormFile
* Fluent API for method chaining
* Perform basic operations on CSV files, such as adding or removing columns, filtering rows, sorting data, and replacing values in a column
* Support for dependency injection

## Installation

Install the EasyCsv package via NuGet:

`NuGet\Install-Package EasyCsv`

## Usage

## Creating an IEasyCsv and reading a CSV file

The reading of the csv is automatically done when you use the `EasyCsvFactory` or `EasyCsvFileFactory` to create your `IEasyCsv`. To recalculate `ContentBytes` and `ContentStr`, call `easyCsv.CalculateContent()`. WARNING. All `From(Type)` methods will return `null` if something goes wrong or the csv contains 0 rows.

```csharp
    IBrowserFile file = files[0];
    var strCsv = "header1,header2\nheader1value,header2value";
    var easyCsv = await EasyCsvFileFactory.FromIBrowserFileAsync(file);
    var easyCsv2 = await EasyCsvFactory.FromStringAsync();
    // You can access the ContentStr, ContentBytes, and create C# Objects using GetRecords<T> at this point
```
## Manipulate CSV data

EasyCsv provides a set of methods for manipulating CSV data. Some examples include:

**No matter what you manipulate, don't forget to call** `easyCsv.CalculateContent()` **after you are done doing your manipulations.** `easyCsv.CsvContent` **is the only content that is always up to date.** `ContentStr` **and** `ContentBytes` **are calculated when you create an** `IEasyCsv`, after that they are only recalculated when `easyCsv.CalculateContent()` is called.

## Remove a column:
```csharp
    var easyCsv = await EasyCsvFactory.FromStreamAsync(fileStream)
    easyCsv.RemoveColumn("header2");
```
## Add column with default value
```csharp
    easyCsv.AddColumn("column name", "value given to all rows in column/header field", upsert: true);
```
## Replace header row

You can replice all the headers in the header row of this CSV. ***The number of headers in the new row must match the number of headers current CsvContent or no operation will be performed***
```csharp
    List<string> newHeaderRow = new () { "header1", "header2", "header3" }
    easyCsv.ReplaceHeaderRow(newHeaderRow);
```
## Filter rows:

This would remove any row where the value of header1 is less than 10. Would throw an error if any value couldn't be converted to an int.
```csharp
    easyCsv.FilterRows(row => (int)row["header1"] > 10);
```
## Map values in a column:
```csharp
    // before "header1,header2\nOldValue,OldValue";
    var valueMapping = new Dictionary<object, object>
    {
        { "OldValue", "NewValue" }
    };
    easyCsv.MapValuesInColumn("header1", valueMapping);
    // after "header1,header2\nNewValue,OldValue";
```
## Sort data by column:

The easiest way is to sort by a column to give them in alphabetical order based on that column, but you can also provide a Func<IDictionary<string, object>, TKey>. to sort like this `csvService.SortCsv(row => row["FieldName"].ToString().Length, ascending: false);`. This would sort rows by the lengths of fields in column "FieldName"
```csharp
    easyCsv.SortCsv("header1", ascending: true);
```
## Read directly to objects

Do whatever operations you need to the csv, then read it directly into objects
```csharp
    List<Person> people = easyCsv.GetRecordsAsync<Person>();
```
## Chain Method Calls Fluently
```csharp
    // before  header1,header2
    //          value1,value2"
    var newRow2Columns = new Dictionary<string, object>()
    {
        {"header1", "other header1 value"},
        {"header2", "header2 value"}
    };
    var newRow3Columns = new Dictionary<string, object>()
    {
        {"header1", "header1 value"},
        {"header2", "header2 value"},
        {"header3", "header3 value"}
    };
    easyCsv.InsertRow(newRow)
           .AddColumn("header3", "value given to all rows herein header3 column")
           .InsertRow(newRow3Columns)
           .FilterRows(row => (string)row["header1"] != "other header1 value")
           .CalculateFileContent();
           
    // after header1,header2,header3
             value1,value2,value given to all rows here in header3 column
             header1 value, header2 value, header3 value
            // Not the data in newRow2Columns because it got filtered
```
For more methods and usage examples, please refer to the EasyCsv documentation and source code.

## Contributing
I gladly welcome contributions to EasyCsv! If you find a bug or have a feature request, please open an issue on the project's GitHub repository. If you would like to contribute code, please submit a pull request.

## Acknowledgements

This library makes use of the following third-party dependencies:

### CsvHelper

EasyCsv uses [CsvHelper](https://joshclose.github.io/CsvHelper/) to read and write CSV files. CsvHelper is licensed under the [Microsoft Public License (Ms-PL)](https://opensource.org/licenses/MS-PL). We would like to thank the authors and contributors of CsvHelper for their work on this excellent library.

## Known Issues and Limitations

License
EasyCsv is licensed under the MIT License.
