namespace CustomAutoAdapterMapper.Tests;

public class TestObjectWithReadOnly
{
    public string Title { get; set; }
    public string ReadOnlyField { get; } = "fixed";
}
