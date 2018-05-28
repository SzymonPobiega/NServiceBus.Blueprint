using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using NServiceBus.Blueprint;

class FileParser
{
    public Blueprint Parse(XDocument document)
    {
        var groups = document.Root.Descendants("endpoint")
            .Select(e =>
            {
                var endpointName = e.Attribute("name")?.Value;

                var commandSelector = GetTypeSelector(e, "command", "commands");
                var eventSelector = GetTypeSelector(e, "event", "events");
                var messageSelector = GetTypeSelector(e, "message", "messages");

                bool TypeSelector(MessageType type) => commandSelector(type) || eventSelector(type) || messageSelector(type);

                var group = new MessageDestinations(TypeSelector, new List<string> { endpointName });
                return group;
            })
            .ToList();
        
        var sites = document.Root.Descendants("site")
            .Select(s =>
            {
                var siteName = s.Attribute("name")?.Value;

                var endpointNames = s.Elements("endpoint")
                    .Select(e => e.Attribute("name")?.Value)
                    .Where(e => e != null)
                    .ToList();

                var routerObjects = s.Elements("router")
                    .Select(r =>
                    {
                        var name = r.Attribute("name")?.Value;
                        var iface = r.Attribute("interface")?.Value;
                        return new Router(name, iface);
                    }).ToList();

                var site = new Site(siteName, endpointNames, routerObjects);
                return site;
            })
            .ToList();
        
        return new Blueprint(groups, sites);
    }

    static Func<MessageType, bool> GetTypeSelector(XElement endpointElement, string singular, string plural)
    {
        var handles = endpointElement.Element("handles");
        if (handles == null)
        {
            return type => false;
        }

        var typeBasedMessageSelector = SelectMessagesByType(singular, handles);
        var assemblyBasedMessageSelector = SelectMessagesByAssemblyAndNamespace(plural, handles);

        return type => typeBasedMessageSelector(type) || assemblyBasedMessageSelector(type);
    }

    static Func<MessageType, bool> SelectMessagesByAssemblyAndNamespace(string plural, XElement handles)
    {
        Func<MessageType, bool> noMatch = type => false;

        var assemblyBasedMessageSelector = handles.Elements(plural).Select(SelectMessages)
            .Aggregate(noMatch, (acc, el) => (type => acc(type) || el(type)));
        return assemblyBasedMessageSelector;
    }

    static Func<MessageType, bool> SelectMessagesByType(string singular, XElement handles)
    {
        var messageTypes = handles
                               .Elements(singular)
                               .Select(e => new MessageType(e.Attribute("type").Value))
                               .ToArray();

        return type => messageTypes.Contains(type);
    }

    static Func<MessageType, bool> SelectMessages(XElement commandsElement)
    {
        var assemblyName = commandsElement.Attribute("assembly").Value;
        

        var @namespace = commandsElement.Attribute("namespace");
        if (@namespace == null)
        {
            return type => type.AssemblyName == assemblyName;
        }

        // the namespace attribute exists, but it's empty
        if (string.IsNullOrEmpty(@namespace.Value))
        {
            // return only types with no namespace at all
            return type => type.AssemblyName == assemblyName && type.Namespace == null;
        }

        return type => type.AssemblyName == assemblyName && type.Namespace.StartsWith(@namespace.Value);
    }
}
