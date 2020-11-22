using System.Linq;
using AnitomySharp;
using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.Anime.Providers
{
    public static class AnitomyAdapter
    {
        public static string ParseSeriesName(SeriesInfo info)
        {
            return AnitomySharp.AnitomySharp
                .Parse(info.Name, new Options(episode: false, extension: false))
                .FirstOrDefault(x => x.Category == Element.ElementCategory.ElementAnimeTitle)
                ?.Value ?? info.Name;
        }
    }
}