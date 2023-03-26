# EasyCsv [Nuget](https://www.nuget.org/packages/EasyCsv)
EasyCsv is a simple and efficient .NET library for handling CSV files in your projects. With a user-friendly API, it allows you to easily read, write, and manipulate CSV files with a minimal amount of code.

## Features

- Read and write CSV files
- Perform basic operations on CSV files, such as adding or removing columns, filtering rows, sorting data, and replacing values in a column
- Use EasyCsv as a service in your .NET projects
- Support for dependency injection

## Installation

Install the EasyCsv package via NuGet:

`NuGet\Install-Package EasyCsv`

or

`NuGet\Install-Package EasyCsv.Core`

or

`NuGet\Install-Package EasyCsv.Files`

## Usage

### Add EasyCsv services to your .NET project

In your `Startup.cs` or in the class where you configure your services, add the following code to register EasyCsv services:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddEasyCsvServices();
    // services.AddEasyCsvServiceFactory();
    // services.AddEasyFileCsvServiceFactory();
}
```

### Read a CSV file

The is automatically done when you use the 'IEasyCsvServiceFactory' or 'IEasyFileCsvServiceFactory' factory to create your 'ICsvService'.

```csharp
IBrowserFile file = files[0];
var easyCsv = await EasyFileCsvServiceFactory.CreateFromIBrowserFileAsync(file)
// You can access the FileContentStr, FileContentBytes, and create C# Objects using GetRecords<T> at this point
```
### Manipulate CSV data

EasyCsv provides a set of methods for manipulating CSV data. Some examples include:

**No matter what you manipulate, don't forget to call `csvService.CalculateFileContent()` after you are done doing your manipulations. `csvService.CsvContent` is the only content that is always up to date. `FileContentStr` and `FileContentBytes` are calculated when you create an `ICsvService`, after that they are only calculated when `csvService.CalculateFileContent()` is called.**


#### Remove a column:

```csharp
var easyCsv = csvServiceFactory.CreateFromBytes(fileContent)
easyCsv.RemoveColumn("header2");
```
#### Filter rows:
```csharp
easyCsv.FilterRows(row => (int)row["header1"] > 10);
```
#### Replace values in a column:
```csharp
var valueMapping = new Dictionary<object, object>
{
    { "OldValue", "NewValue" }
};
easyCsv.ReplaceValuesInColumn("header1", valueMapping);
```
#### Sort data by column:
```csharp
easyCsv.SortCsvByColumnData("header1", ascending: true);
```
For more methods and usage examples, please refer to the EasyCsv documentation and source code.

Contributing
I gladly welcome contributions to EasyCsv! If you find a bug or have a feature request, please open an issue on the project's GitHub repository. If you would like to contribute code, please submit a pull request.

License
EasyCsv is licensed under the MIT License.
