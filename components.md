# EasyCsv.Components

EasyCsv.Components provides you with 2 components. They are available to test at this [example website](https://easycsv-components-exampleapp.s3.amazonaws.com/index.html).

## Installation

`NuGet\Install-Package EasyCsv.Components`

## CsvFileInput

The first is just a simple Csv input component that will automatically convert the selected `IBrowserFile` into an `IEasyCsv` and fire off an event.

![Screenshot_78](https://github.com/biegehydra/EasyCsv-Dotnet/assets/84036995/818dcdbc-9ea0-4893-b070-b2933a42b795)

## CsvTableHeaderMatcher

This component takes the `IEasyCsv` that you got from the `CsvFileInput` (or elsewhere) and allows users to map the csv headers to expected headers or provide default values.

![Screenshot_80](https://github.com/biegehydra/EasyCsv-Dotnet/assets/84036995/106a64d5-9937-4193-bc04-75350577c14a)


### Expected Headers


All of this works through the `ExpectedHeaders` parameter which takes a `List<ExpectedHeader>`. There should be one `ExpectedHeader` for each C# property on your class that you want data for. Alternatively, you can can provide a value for `AutoGenerateExpectedHeadersType` and a default `ExpectedHeader` will be generated for each public instance variable with a setter.

### Expected Header Options
EH - Expected Header

The options for the EH essentially lets you control how users will be able to provide a value for your C# property and how it will be displayed.

**CSharpPropertyName (string)**: This is defines which property on the class you will be getting records for that data for this EH will go to. For robust code, assign it with `nameof`

**Display Name (string)**: This defines what to display for this EH in the "Expected Header" column.

**ValuesToMatch (List<string>)**: When a Csv is imported, the matcher will attempt to figure out which of the csv headers match to your EHs. It does this by performing matching on each of the values in this list. For example, if you have an EH for a `Zip` property. You might want `ValuesToMatch` to look like `new List<string>() { "Zip", "Zip Code", "Postal Code" }`. How the matching is done is explained in the AutoMatching section.


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

**AutoMatching** Lets you select an automatching level for a specific property in case you need more granular control.

There are a few static `ExpectedHeaderConfig`s on `ExpectedHeaderConfig` that you can use such as `Default`, `Required`, `TextDefaultValue` and `RequiredTextDefaultValue` since these are commonly used.

### Additional CsvTableHeaderMatcher Options

**HideDefaultValueColumn**: When true, the default value column is hidden.

**Frozen**: When true, no changes can be made to the matcher. Default value input fields will be disabled and csv header mappings can't be changed.

**AutoMatch**: Controls how `ValuesToMatch` on `ExpectedHeader`s will be compared to csv headers during matching. If the `ValuesToMatch` contains a single value "FirstName", this is how what it would match to with the different options
- Exact: "FirstName"
- Strict: "FirstNme"
- Lenient: "First"

**DisplayMatchState (RenderFragment\<MatchState\>)**: Lets you control what gets rendered in the "Matched" column.

### Dynamic Headers

If for whatever reason you need the `CsvTableHeaderMatcher` to work for multiple types, you are able to.
To do this, all you need to do is provide a new value for `ExpectedHeaders`. This **HAS** to be a new list,
don't just add items to your existing expected headers list. This will trigger the component to be reset
and use the new expected headers. You will have to keep track of what type you want to read from when you call
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

