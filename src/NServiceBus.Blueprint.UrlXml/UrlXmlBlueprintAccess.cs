using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NServiceBus.Blueprint;

class UrlXmlBlueprintAccess : IBlueprintAccess
{
    Uri routingFileUri;
    TimeSpan updateInterval;
    FileParser parser;
    AsyncTimer updateTimer = new AsyncTimer();

    public UrlXmlBlueprintAccess(Uri routingFileUri, TimeSpan updateInterval)
    {
        this.routingFileUri = routingFileUri;
        this.updateInterval = updateInterval;
        parser = new FileParser();
    }

    public async Task Start(Action<Blueprint> onChanged, Action<Exception> onError)
    {
        await Refresh(onChanged).ConfigureAwait(false);
        if (updateInterval == TimeSpan.Zero)
        {
            return;
        }

        updateTimer.Start(() => Refresh(onChanged), updateInterval, ex => { onError(ex); });
    }

    public Task Stop()
    {
        return updateTimer.Stop();
    }

    Task Refresh(Action<Blueprint> onChanged)
    {
        try
        {
            var doc = XDocument.Load(routingFileUri.ToString());
            var map = parser.Parse(doc);

            onChanged(map);
            return Task.CompletedTask;
        }
        catch (XmlException e)
        {
            throw new Exception("The configured routing file is no valid XML file.", e);
        }
    }
}