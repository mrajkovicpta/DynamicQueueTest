namespace Lib.Entities;

public class MessageConfiguration
{
    public int Id {get;set;}
    public string MessageName {get;set;}
    public string ServiceQueue {get;set;}
    public bool Enabled {get;set;}
    public bool ShouldStore {get;set;}
}
