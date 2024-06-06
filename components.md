# EasyCsv.Components

EasyCsv.Components provides you with 3 components. They are available to test at this [example website](https://d143idkvxttaq3.cloudfront.net/processing)                                                                                                         
## Links
- [CsvProcessingStepper](#csvprocessingstepper)
  - [Create Processsor/Evaluator](#create-processsorevaluator)
  - [Create Strategy Options Component](#create-strategy-options-component)
  - [Add Options Components To CsvProcessingStepper](#add-options-components-to-csvprocessingstepper)
  - [Reversible Edits](#reversible-edits)
- [CsvFileInput](#csvfileinput)
- [CsvTableHeaderMatcher](#csvtableheadermatcher)
  - [Expected Headers](#expected-headers)
  - [Expected Header Config](#expected-header-config)
  - [Additional CsvTableHeaderMatcher Options](#additional-csvtableheadermatcher-options)
  - [Multiple Types](#multiple-types)

## Installation (CsvProcessingStepper is in beta package)

[![NuGet](https://img.shields.io/nuget/dt/EasyCsv.Components?label=NuGet%20Downloads&style=plastic)](https://www.nuget.org/packages/EasyCsv.Components/)

`NuGet\Install-Package EasyCsv.Components -Version=2.0.0-beta8.2`

## CsvProcessingStepper

The csv processing stepper is like a miniature version of excel/google sheets that is fully customizable to you. It's features include:
- Editing/Deleting/Sorting rows
- Performing complex operations with versioning
- Adding additional csvs to working csv
- Performing dedupe operations
- Specifying an operation to only operate on filtered rows

### Create Processsor/Evaluator
To create your own strategies, you must first implement one of these [Interfaces](https://github.com/biegehydra/EasyCsv-Dotnet/blob/master/src/EasyCsv.Processing/Interfaces.cs). 

Take for example this `TagRowsStrategy`
```csharp
public class TagRowsStrategy : IFullCsvProcessor
{
    public bool OperatesOnlyOnFilteredRows => true;
    private readonly Action<CsvRow, IList<string>> _addTagsFunc;
    public TagRowsStrategy(Action<CsvRow, IList<string>> addTagsFunc)
    {
        _addTagsFunc = addTagsFunc;
    }

    public async ValueTask<OperationResult> ProcessCsv(IEasyCsv csv, ICollection<int>? filteredRowIds)
    {
        await csv.MutateAsync(x =>
        {
            x.AddColumn(InternalColumnNames.Tags, null, ExistingColumnHandling.Keep);
            foreach (var row in x.CsvContent.FilterByIndexes(filteredRowIds))
            {
                var existingTags = row.ProcessingTags()?.ToList() ?? [];
                _addTagsFunc(row, existingTags);
                row[InternalColumnNames.Tags] = string.Join(",", existingTags.Distinct());
            }
        }, saveChanges: false); // Note saveChanges shouldn't be called when performing operation to increase speed
        return new OperationResult(true);
    }
}
```
This strategy implements `IFullCsvProcessor` which operates on an entire csv. The reason this strategy must use the IFullCsvProcessor, is because it potentially alters the shape/structure of the csv - when it adds the `InternalColumnNames.Tags`. Note how this strategy handles changing the structure of the csv, the column is added to every row at the very beginning. This is the safest way to do it because it is up to the Processor to ensure all rows have the same structure.

More examples can be found [here](https://github.com/biegehydra/EasyCsv-Dotnet/tree/master/src/EasyCsv.Processing/Strategies).

### Create Strategy Options Component
Once you have written your strategy, to integrate it with the CsvProcessingStepper, it is recommended to write a `StrategyItem` wrapper to execute the strategy and manage its options. Take for example the wrapper for the `DivideAndReplicate` strategy.

```html
@inherits StrategyItemBase
<StrategyItem DisplayName="@DisplayName" OnlyOperatesOnFilteredRows="OnlyOperatesOnFilteredRows" DescriptionStr="@DescriptionStr" Description="Description" BeforeCsvExample="BeforeExample" AfterCsvExample="AfterExample" ExampleOptions="ExampleOptions" AllowRun="AllowRun" StrategyPicked="RunDivideAndReplicate">
    <Options>
        <MudListItem>
            <MudTextField Disabled="context" Label="Delimiter To Divide On" Variant="Variant.Outlined" @bind-Value="_delimiter"></MudTextField>
            @* <ColumnSelect/> *@
            @* <MultiColumnSelect/> *@
            @* <TagSelect/> *@
            @* <MultiTagSelect/> *@
            @* <ReferenceCsvSelect/> *@
            @* <ReferenceColumnSelect/> *@
            @* <MultiReferenceColumnSelect/> *@
        </MudListItem>
    </Options>
</StrategyItem>

@code
{
    [Parameter] public override string? DisplayName { get; set; } = "Divide And Replicate";
    [Parameter] public override string? DescriptionStr { get; set; } = "Will split the values in $column_name, on a specified delimiter, into parts and then create a copy of the row for each part";

    private static readonly Dictionary<string, string> ExampleOptions = new Dictionary<string, string>()
    {
        {"Delimiter", "-"}
    };
    private static readonly Dictionary<string, string>[] BeforeExample =
    [
        new Dictionary<string, string>()
        {
            {"Column1", "value1"},
            {"ColumnToDivideAndReplicate", "value2-value3"},
        },
    ];
    private static readonly Dictionary<string, string>[] AfterExample =
    [
        new Dictionary<string, string>()
        {
            {"Column1", "value1"},
            {"ColumnToDivideAndReplicate", "value2"},
        },
        new Dictionary<string, string>()
        {
            {"Column1", "value1"},
            {"ColumnToDivideAndReplicate", "value3"},
        }
    ];

    private bool AllowRun => !string.IsNullOrWhiteSpace(_delimiter);
    private string? _delimiter;
    private async Task RunDivideAndReplicate(string columnName)
    {
        if (!AllowRun) return;
        var divideAndReplicateStrategy = new DivideAndReplicateStrategy(columnName, y => y?.ToString()?.Split(_delimiter).Cast<object?>().ToArray());
        _ = await CsvProcessor.PerformCsvStrategy(divideAndReplicateStrategy);
    }
}
```
The options inherits from `StrategyItemBase` which gives you access to the `StrategyBucket` and `CsvProcessingStepper` (CsvProcessor) that the component is being rendered in. `StrategyBucket` just holds the context for the popup you see here:

![2024-05-12_13-54](https://github.com/biegehydra/EasyCsv-Dotnet/assets/84036995/ce563585-f299-4aa4-8234-c6fcd70f9938)

All you need to do in your component is give your StrategyItem a `DisplayName`, optionally define an `<Options>` section, and subscribe a callback to `StrategyPicked` that will create your strategy/reversible edit and use the CsvProcessor to perform it. The `CsvProcessingStepper` has a function to perform each operation you in the [interfaces](https://github.com/biegehydra/EasyCsv-Dotnet/blob/master/src/EasyCsv.Processing/Interfaces.cs) file. `Description`, `DescriptionStr` `BeforeCsvExample`, `AfterCsvExample`, and `Example Options` are optional parameters for the UI.

`AllowRun` controls whether the `RunOperation` button is disabled or not. When the "Run Operation" button is clicked, the `StrategyPicked` callback is called (calling `RunDivideAndReplicate` here) with the column name of the StrategyBucket this component is rendered in.

There are 7 input components integrated with the stepper that you can use in your options components. `<ColumnSelect/>`, `<MultiColumnSelect/>`, `<TagSelect/>`, `<MultiTagSelect/>`, `<ReferenceCsvSelect/>`, `<ReferenceColumnSelect/>`, `<MultiReferenceColumnSelect/>`. All of these are used in the example website.

### Add Options Components To CsvProcessingStepper
Once you're done write your `StrategyItem` wrappers, just put them in the `<ColumnStrategies>` or `<FullCsvStrategies>` section of the CsvProcessingStepper. Note, when a **full csv** strategy is picked, the column name will be `InternalColumnNames.FullCsvOperations` or "_FullCsvOperations" in the `StrategyPicked` callback

 ```html
<CsvProcessingStepper @ref="_csvProcessor" EasyCsv="_easyCsv" EasyCsvFileName="Example.csv">
    <ColumnStrategies>
        <FindDedupesExactMatchColumn MustSelectRow="false" />
        <StringSplitColumn />
        <DivideAndReplicate />
        <TagAndReferenceMatches />
        <DeleteOnEmptyColumn />
    </ColumnStrategies>
    <FullCsvStrategies>
        <AddCsv />
        <CombineColumns />
    </FullCsvStrategies>
</CsvProcessingStepper>
```


### Reversible Edits
In addition to operations, which should be used for complex operations because they require **cloning the entire csv**, `IReversibleEdit`'s can be used to alter the working csv in place. Some operations, such as the `ICsvColumnProcessor`, `ICsvColumnDeleteEvaluator`, `ICsvRowDeleteEvaluator`, and `IFindDupesOperation` are automatically treated as reversible edits so those can be used in some scenarios instead of writing your own `IReversibleEdit`.

```csharp
public interface IReversibleEdit
{
    public bool MakeBusy { get; } // Just controls whether the UI will make everything disabled while this operation runs
    void DoEdit(IEasyCsv csv, StrategyRunner runner);
    void UndoEdit(IEasyCsv csv, StrategyRunner runner);
}
```
For example, this is the ModifyRowEdit class. The `DoEdit` and `UndoEdit` methods provide the working csv, but do not require you to use them
```csharp
public class ModifyRowEdit : IReversibleEdit
{
    public bool MakeBusy { get; } = false;
    public ModifyRowEdit(CsvRow row, CsvRow rowClone, CsvRow rowAfterOperation)
    {
        Row = row;
        RowClone = rowClone;
        RowAfterOperation = rowAfterOperation;
    }

    public CsvRow Row { get; }
    public CsvRow RowClone { get; }
    public CsvRow RowAfterOperation { get; }
    public void DoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        RowAfterOperation.MapValuesTo(Row);
    }
    public void UndoEdit(IEasyCsv csv, StrategyRunner runner)
    {
        RowClone.MapValuesTo(Row);
    }
}
```
Note how I provide a reference to the original row. **Within a step, CsvRow references should be maintainted**, the same is not true across steps. The reason to provide the row instead of the row index, is because at any given point it time, you don't know how the csv will be sorted. 

After calling `CsvProcessingStepper.AddReversibleEdit(IReversibleEdit reversibleEdit)`, the current column sort will be applied with `CsvProcessingTable.ApplyCurrentColumnSort()` to ensure row order is maintained. Row ordering is able to be maintained because the `StrategyRunner` stores a `Dictionary<CsvRow, int>` holding the original index of each row.

## CsvFileInput


The first is just a simple Csv input component that will automatically convert the selected `IBrowserFile` into an `IEasyCsv` and fire off an event. It can be `FileInputVariant.Paper` as shown below or a regular mudblazor button with `FileInputVariant.Button`.

![Screenshot_78](https://github.com/biegehydra/EasyCsv-Dotnet/assets/84036995/818dcdbc-9ea0-4893-b070-b2933a42b795)

## CsvTableHeaderMatcher

This component takes the `IEasyCsv` that you get from the `CsvFileInput` component (or elsewhere) and allows users to map the csv headers to expected headers or provide default values.

![Screenshot_80](https://github.com/biegehydra/EasyCsv-Dotnet/assets/84036995/106a64d5-9937-4193-bc04-75350577c14a)


### Expected Headers


All mapping from the csv to your C# class works through the `ExpectedHeaders` parameter. It takes an `ICollection<ExpectedHeader>`. 

For example, if trying to map to `Foo`, there should be one `ExpectedHeader` for each property on `Foo` that you want data for. Alternatively, you can can provide a `Type` to `AutoGenerateExpectedHeadersType` and a default `ExpectedHeader` will be generated for each public instance variable on `Foo` with a setter.

#### Constructor Parameters
EH - Expected Header

These parameters essentially defines the property/field on `Foo` that you are mapping to, how it will be displayed, and what values to automatch to.

**CSharpPropertyName (string)**: This is defines which property on the class you will be getting records for that data for this EH will go to. For robust code, assign it with `nameof`

**Display Name (string)**: This defines what to display for this EH in the "Expected Header" column.

**ValuesToMatch (ICollection<string>)**: When a Csv is imported, the matcher will attempt to figure out which of the csv headers match to your EHs. It does this by performing matching on each of the values in this ICollection. For example, if you have an EH for a `Zip` property. You might want `ValuesToMatch` to look like `new string[] { "Zip", "Zip Code", "Postal Code" }`. How the matching is done is explained in the AutoMatching section.

### Expected Header Config

The rest of the options are either provided through the `ExpectedHeaderConfig` or through an `Action<ExpectedHeaderConfigurator>`.

Ex: 
```
ExpectedHeader throughConfig = new ExpectedHeader(nameof(Person.DateOfBirth), new ExpectedHeaderConfig(DefaultValueType.DateTime, required: true));
// or
ExpectedHeader throughConfigurator = new ExpectedHeader(nameof(Person.DateOfBirth), x => { x.Required = true; x.DefaultValueType = DefaultValueType.DateTime; } );
```

**Required (bool)**: All EHs that are marked as required must either have a default value provided or csv header mapped to be marked as valid. If any EH is invalid, it will show up as red in the table and the whole table will have a red border (configurable).

**Initial Default Value (object?)**: The initial default value of this expected header. Will show up in whatever input type is used in the "Default Value" column.

**Default Value Type (DefaultValueType)**: Options are `None`, `Text`, `DateTime`, `CheckBox`, and `TriStateCheckBox`. These will control what MudBlazor input component is used in the default value column. This value is ignored if a value is provided for `DefaultValueRenderFragment`
**DefaultValueRenderFragment (RenderFragment\<DefaultValueRenderFragmentsArgs\>)**: If you would like a custom input element in "Default Value" column, you can define that here. An example of how to do this can be seen [here](https://github.com/biegehydra/EasyCsv-Dotnet/blob/76fac05fbb2476839aab7f8fa7b805211e4e9e94/src/ExampleBlazorApp/Pages/Index.razor#L118).

**AutoMatching**: Lets you select an automatching level for a specific property in case you need more granular control. By default, expected headers have the same automatching level of the table.

There are a few static configs on `ExpectedHeaderConfig` that you can use: `ExpectedHeaderConfig.Default`, `ExpectedHeaderConfig.Required`, `ExpectedHeaderConfig.TextDefaultValue` and `ExpectedHeaderConfig.RequiredTextDefaultValue` since these are commonly used.

### Additional CsvTableHeaderMatcher Options

**HideDefaultValueColumn**: When true, the default value column is hidden.

**HidePreviewInformationColumn**: When true, the preview information column is hidden.

**Initial OrderBy**: Let's you control how expected headers should be ordered in the table after initial matching completes.

**Frozen**: When true, no changes can be made to the matcher. Default value input fields will be disabled and csv header mappings can't be changed.

**AutoMatch**: Controls how `ValuesToMatch` on `ExpectedHeader` will be compared to the csv headers during matching. For example, if `ValuesToMatch` contains a single value "FirstName", this is what it would match to with the different options:
- Exact: "FirstName"
- Strict: "FirstNme"
- Lenient: "First"

Auto matching just uses a simple Levenshtein Distance algorithm, the dependancy on `FuzzySharp` has been removed.

**DisplayMatchState (RenderFragment\<MatchState\>)**: Lets you control what gets rendered in the "Matched" column.

### Multiple Types

If for whatever reason you need the `CsvTableHeaderMatcher` to work for multiple types, you are able to.
To do this, all you need to do is provide a new value for `ExpectedHeaders`. This **HAS** to be a new collection,
don't just add items to your existing existing collection. Assigning a new collection will trigger the component to be reset
and use the new expected headers. You have to keep track of what type you want to read from when you call
`GetRecords<T>`. 
For example,

```
<CsvTableHeaderMatcher @ref="_tableHeaderMatcher" Csv="_easyCsv" AllHeadersValidChanged="StateHasChanged" ExpectedHeaders="_expectedHeaders" AutoMatch="AutoMatching.Lenient"></CsvTableHeaderMatcher>

@code {
	private List<ExpectedHeaders> _classOneExpectedHeaders;
	private List<ExpectedHeaders> _classTwoExpectedHeaders;
	private List<ExpectedHeaders> _expectedHeaders;
	private Type CurrentType { get; set; }
	private void SetType(Type type){
		if (type == typeof(ClassOne)){
			_expectedHeaders = _classOneExpectedHeaders;
			CurrentType = type;
		}
		else if (type == typeof(ClassTwo)){
			_expectedHeaders = _classTwoExpectedHeaders;
			CurrentType = type;
		}
	}
	private List<T> CustomGetRecords<T>(){
		if (typeof(T) == typeof(ClassOne) || typeof(T) == typeof(ClassTwo)) {
			return _tableHeaderMatcher.GetRecords<T>();
		}
		throw new Expection("Type not supported");
	}
}

```

