﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Anime.Providers.AniSearch
{
    public class AniSearchSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
    {
        private readonly IHttpClient _httpClient;
        private readonly IApplicationPaths _paths;
        private readonly ILogger<AniSearchSeriesProvider> _log;
        public int Order => -3;
        public string Name => "AniSearch";

        public AniSearchSeriesProvider(IApplicationPaths appPaths, IHttpClient httpClient, ILogger<AniSearchSeriesProvider> logger)
        {
            _log = logger;
            _httpClient = httpClient;
            _paths = appPaths;
        }

        public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<Series>();

            var aid = info.ProviderIds.GetOrDefault(ProviderNames.AniSearch);
            if (string.IsNullOrEmpty(aid))
            {
                var anitomyName = AnitomyAdapter.ParseSeriesName(info);
                
                _log.LogInformation("Start AniSearch... Searching({Name})", anitomyName);

                aid = await AniSearchApi.FindSeries(anitomyName, cancellationToken);
            }

            if (!string.IsNullOrEmpty(aid))
            {
                string WebContent = await AniSearchApi.WebRequestAPI(AniSearchApi.AniSearch_anime_link + aid);
                result.Item = new Series();
                result.HasMetadata = true;

                result.Item.ProviderIds.Add(ProviderNames.AniSearch, aid);
                result.Item.Overview = await AniSearchApi.Get_Overview(WebContent);
                try
                {
                    //AniSearch has a max rating of 5
                    result.Item.CommunityRating = (float.Parse(await AniSearchApi.Get_Rating(WebContent), System.Globalization.CultureInfo.InvariantCulture) * 2);
                }
                catch (Exception) { }
                foreach (var genre in await AniSearchApi.Get_Genre(WebContent))
                    result.Item.AddGenre(genre);
                GenreHelper.CleanupGenres(result.Item);
                StoreImageUrl(aid, await AniSearchApi.Get_ImageUrl(WebContent), "image");
            }
            return result;
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        {
            var results = new Dictionary<string, RemoteSearchResult>();

            var aid = searchInfo.ProviderIds.GetOrDefault(ProviderNames.AniSearch);
            if (!string.IsNullOrEmpty(aid))
            {
                if (!results.ContainsKey(aid))
                    results.Add(aid, await AniSearchApi.GetAnime(aid));
            }

            if (!string.IsNullOrEmpty(searchInfo.Name))
            {
                var anitomyName = AnitomyAdapter.ParseSeriesName(searchInfo);
                
                List<string> ids = await AniSearchApi.Search_GetSeries_list(anitomyName, cancellationToken);
                foreach (string a in ids)
                {
                    results.Add(a, await AniSearchApi.GetAnime(a));
                }
            }

            return results.Values;
        }

        private void StoreImageUrl(string series, string url, string type)
        {
            var path = Path.Combine(_paths.CachePath, "anisearch", type, series + ".txt");
            var directory = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);

            File.WriteAllText(path, url);
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                UserAgent = Constants.UserAgent,
                CancellationToken = cancellationToken,
                Url = url
            });
        }
    }

    public class AniSearchSeriesImageProvider : IRemoteImageProvider
    {
        private readonly IHttpClient _httpClient;

        public AniSearchSeriesImageProvider(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string Name => "AniSearch";

        public bool Supports(BaseItem item) => item is Series || item is Season;

        public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
        {
            return new[] { ImageType.Primary };
        }

        public Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
        {
            var seriesId = item.GetProviderId(ProviderNames.AniSearch);
            return GetImages(seriesId, cancellationToken);
        }

        public async Task<IEnumerable<RemoteImageInfo>> GetImages(string aid, CancellationToken cancellationToken)
        {
            var list = new List<RemoteImageInfo>();

            if (!string.IsNullOrEmpty(aid))
            {
                var primary = await AniSearchApi.Get_ImageUrl(await AniSearchApi.WebRequestAPI(AniSearchApi.AniSearch_anime_link + aid));
                list.Add(new RemoteImageInfo
                {
                    ProviderName = Name,
                    Type = ImageType.Primary,
                    Url = primary
                });
            }
            return list;
        }

        public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClient.GetResponse(new HttpRequestOptions
            {
                UserAgent = Constants.UserAgent,
                CancellationToken = cancellationToken,
                Url = url
            });
        }
    }
}
