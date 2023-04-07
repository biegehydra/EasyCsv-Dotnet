# EasyCsv [Nuget](https://www.nuget.org/packages/EasyCsv)

EasyCsv is a simple and efficient .NET library for handling CSV files in your projects. With a **fluent** user-friendly API, it allows you to easily read, write, and manipulate CSV files with a minimal amount of code.

## Features

* Read and write CSV files
* Support for larger number of types: byte[], string, Stream, TextReader, Objects<T>, IBrowserFile, IFormFile
* Fluent API for method chaining
* Mutations context to limit error proneability
* Perform basic operations on CSV files, such as adding or removing columns, filtering rows, sorting data, and replacing values in a column
* Support for dependency injection

## [Breaking Changes](breaking-changes.md)

## Installation

Install the EasyCsv package via NuGet:

`NuGet\Install-Package EasyCsv`

## Usage

## Creating an IEasyCsv and reading a CSV file

The reading of the csv is automatically done when you use the `EasyCsvFactory` or `EasyCsvFileFactory` to create your `IEasyCsv`. It is also automatically done when you call `easyCsv.Mutate()` or `easyCsv.MutateAsync()` unless you set the `saveChanges` flag to false. WARNING. All `From(Type)` methods will return `null` if something goes wrong or the csv contains 0 rows.

```csharp
    IBrowserFile file = files[0];
    var strCsv = "header1,header2\nheader1value,header2value";
    var easyCsv = await EasyCsvFileFactory.FromIBrowserFileAsync(file);
    var easyCsv2 = await EasyCsvFactory.FromStringAsync();
    // You can access the ContentStr, ContentBytes, and create C# Objects using GetRecords<T> at this point
```
## Manipulate CSV data

EasyCsv provides an assortment of methods for manipulating CSV data. All calls that manipulate the CSV are done through `easyCsv.Manipulate(Action<CSVMuationScope> scope)` or `easyCsv.ManipulateAsync(Action<CSVMuationScope> scope)`. The scope will ensure that the `ContentStr` and `ContentBytes` are up to date after you do manipulations. 

## Add column with default value
```csharp
        easyCsv.Mutate(mutation => mutation.AddColumn("column name", "value given to all rows in column/header field", upsert: true));
```
## Remove a column:
```csharp
        var easyCsv = await EasyCsvFactory.FromStreamAsync(fileStream);
        easyCsv.Mutate(mutation => mutation.RemoveColumn("header2"));
```
## Replace Columns
Removes the column of the old header field and upserts all it's values to all the rows of the new header field. CSV
```csharp
    easyCsv.Mutate(mutation => mutation.ReplaceColumn(string oldHeaderField, string newHeaderField));
```
## Replace header row
You can replace all the headers in the header row of this CSV. ***The number of headers in the new row must match the number of headers current CsvContent or no operation will be performed***
```csharp
        List<string> newHeaderRow = new () { "newHeader1", "newHeader2", "newHeader3" }
        easyCsv.Mutate(mutation => mutation.ReplaceHeaderRow(newHeaderRow));
```
## Remove unused data
Removes any header that does match a public property on the type param T.
```csharp
    // Removes all fields that don't match public property on Person
    await easyCsv.MutateAsync(mutation => await mutation.RemoveUnusedHeadersAsync<Person>(caseInsensitive:true));
```
## Filter rows:
This would remove any row where the value of header1 column is less than 10. Would throw an error if any value couldn't be converted to an int.
```csharp
        easyCsv.Mutate(mutation => mutation.FilterRows(row => (int)row["header1"] > 10));
```
## Map values in a column:
```csharp
        // before "header1,header2\nOldValue,OldValue";
        var valueMapping = new Dictionary<object, object>
        {
            { "OldValue", "NewValue" }
        };
        easyCsv.Mutate(mutation => mutation.MapValuesInColumn("header1", valueMapping));
        // after "header1,header2\nNewValue,OldValue";
```
## Sort data by column:
The easiest way is to sort by a column to give them in alphabetical order based on that column, but you can also provide a Func<IDictionary<string, object>, TKey>. to sort like this `csvService.SortCsv(row => row["FieldName"].ToString().Length, ascending: false);`. This would sort rows by the lengths of fields in column "FieldName"
```csharp
        easyCsv.Mutate(mutation => mutation.SortCsv("header1", ascending: true));
```
## Combine CSVs
Some care will need to be taken with this since the headers must match exactly, however it is perfect to use when you know you have two csvs that were read an object of type T.
```csharp
    var easyCsv1 = EasyCsvFactory.FromObjects<Person>(people1);
    var easyCsv2 = EasyCsvFactory.FromObjects<Person>(people2);
    var combinedCsv = easyCsv1.Combine(easyCsv2); 
    // The configurations from easyCsv1 will used in the combinedCsv
    // This can also be done in the mutation context
```
## Read directly to objects
Do whatever operations you need to the csv, then read it directly into objects
```csharp
        List<Person> people = easyCsv.GetRecordsAsync<Person>();
```
## Crud Operations

I also have some CRUD operations for directly working with rows. UpdateRow, UpsertRow, DeleteRow, AddRow, etc

## Convenience methods

I include plenty of convenience methods on EasyCsv such as `Clone()`, `GetHeaders()`, `GetRowCount()`, `ContainsHeader()`, `Clear()`

## Chain Method Calls Fluently

```csharp
        // before  header1,header2, header3
        //          value1,value2, value3
        //          value1,value2, value3
        easyCsv.Mutate(mutations => mutations.RemoveColumn("header2")
                                             .AddColumn("header4", "value4")
                                             .ReplaceColumn("header1", "newHeader1"));
               
        // after  newHeader1, header3, header4
        //          value1, value3, value4
        //          value1, value3, value4
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
