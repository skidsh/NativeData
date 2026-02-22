using NativeData.Abstractions;

Console.WriteLine("Analyzer smoke sample");

var knownType = typeof(Widget);
Console.WriteLine(knownType.Name);

[NativeDataEntity("Widgets", "Id")]
public sealed class Widget
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
