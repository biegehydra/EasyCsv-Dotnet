# Breaking Changes

## 1.0.13

Do not use 1.0.13, or 1.0.14. 1.0.15 is the stable release. However, starting in 1.0.13 all mutations are done through a manipulation context.
```csharp
// 1.0.12
easyCsv.RemoveColumn("Column1")
       .CalculateContent();
// 1.0.13+
easyCsv.Mutate(x => x.RemoveColumn("Column1"));

// CalculateContent is no longer public, in the example before, it will automatically be called for you
// You can call as many mutations as you would like in the mutation context fluently (or not) like before
// To forgo CalculateContent after the mutation, I added a flag `saveChanges`.
```
