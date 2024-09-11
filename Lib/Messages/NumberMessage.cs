namespace Lib.Messages;

public record NumberMessage(string consumerIdentifier, int Number) : IConfigurableMessage
{
    public static string MessageId => nameof(NumberMessage);
}
