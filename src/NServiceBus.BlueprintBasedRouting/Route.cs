using System;
using System.Linq;

class Route
{
    string site;

    public Route(string immediateDestination, string nextHop, string site)
    {
        ImmediateDestination = immediateDestination ?? throw new ArgumentNullException(nameof(immediateDestination));
        NextHop = nextHop;
        this.site = site;
    }
    public string NextHop { get; }
    public string ImmediateDestination { get; }

    public bool Match(string[] destinationSites)
    {
        return site == null || destinationSites.Contains(site);
    }
}