namespace NetCoreMQTTExampleCluster.Models.Exceptions;

public class ConfigurationException : Exception
{
    public ConfigurationException()
    {
    }

    public ConfigurationException(string name) : base(name)
    {
    }
}
