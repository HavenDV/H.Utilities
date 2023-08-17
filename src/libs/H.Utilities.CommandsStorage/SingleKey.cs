namespace H.Utilities;

public class SingleKey : TextObject
{
    #region Constructors

    public SingleKey()
    {
    }

    public SingleKey(string text) : base(text)
    {
    }

    #endregion

    #region ICloneable

    public object Clone() => new SingleKey(Text);

    #endregion
}