using CustomAutoAdapterMapper.Exceptions;

namespace CustomAutoAdapterMapper.Tests;

public class ExceptionTests
{
    [Test]
    public void ItemKeyOptionNullExceptionDefaultConstructor()
    {
        var ex = new ItemKeyOptionNullException();
        Assert.That(ex, Is.InstanceOf<Exception>());
        Assert.That(ex.InnerException, Is.Null);
    }

    [Test]
    public void ItemKeyOptionNullExceptionWithMessageAndInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new ItemKeyOptionNullException("outer", inner);
        Assert.That(ex.Message, Is.EqualTo("outer"));
        Assert.That(ex.InnerException, Is.SameAs(inner));
    }

    [Test]
    public void JsonContentExceptionDefaultConstructor()
    {
        var ex = new JsonContentException();
        Assert.That(ex, Is.InstanceOf<Exception>());
        Assert.That(ex.InnerException, Is.Null);
    }

    [Test]
    public void JsonContentExceptionWithMessageAndInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new JsonContentException("outer", inner);
        Assert.That(ex.Message, Is.EqualTo("outer"));
        Assert.That(ex.InnerException, Is.SameAs(inner));
    }

    [Test]
    public void RootKeyOptionNullExceptionDefaultConstructor()
    {
        var ex = new RootKeyOptionNullException();
        Assert.That(ex, Is.InstanceOf<Exception>());
        Assert.That(ex.InnerException, Is.Null);
    }

    [Test]
    public void RootKeyOptionNullExceptionWithMessageAndInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new RootKeyOptionNullException("outer", inner);
        Assert.That(ex.Message, Is.EqualTo("outer"));
        Assert.That(ex.InnerException, Is.SameAs(inner));
    }

    [Test]
    public void RootKeyPropertyNullExceptionDefaultConstructor()
    {
        var ex = new RootKeyPropertyNullException();
        Assert.That(ex, Is.InstanceOf<Exception>());
        Assert.That(ex.InnerException, Is.Null);
    }

    [Test]
    public void RootKeyPropertyNullExceptionWithMessageAndInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new RootKeyPropertyNullException("outer", inner);
        Assert.That(ex.Message, Is.EqualTo("outer"));
        Assert.That(ex.InnerException, Is.SameAs(inner));
    }
}
