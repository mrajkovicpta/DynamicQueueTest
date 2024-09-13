namespace Lib.Messages;

public class StringMessage
{
    public    string StringValue {get;set;}

    public StringMessage(){}

    public StringMessage(string stringValue)
    {
        StringValue = stringValue;
    }
}
