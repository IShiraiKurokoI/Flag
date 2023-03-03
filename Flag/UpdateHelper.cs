using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using WinUICommunity.Common.Helpers;

namespace Flag
{
    public static class UpdateHelper
    {
        private const string GITHUB_API = "http://api.github.com/repos/{0}/{1}/releases/latest";

        public static async Task<UpdateInfo> CheckUpdateAsync(string username, string repository, Version currentVersion = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }

            if (string.IsNullOrEmpty(repository))
            {
                throw new ArgumentNullException("repository");
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient obj = new HttpClient
            {
                DefaultRequestHeaders = { { "User-Agent", username } }
            };
            string requestUri = $"http://api.github.com/repos/{username}/{repository}/releases/latest";
            HttpResponseMessage obj2 = await obj.GetAsync(requestUri);
            obj2.EnsureSuccessStatusCode();
            UpdateInfo updateInfo = JsonSerializer.Deserialize<UpdateInfo>(await obj2.Content.ReadAsStringAsync());
            if (updateInfo != null)
            {
                if (currentVersion == null)
                {
                    string version = typeof(Application).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version;
                    currentVersion = new Version(version);
                }

                SystemVersionInfo asVersionInfo = GetAsVersionInfo(updateInfo.TagName);
                int major = ((currentVersion.Major != -1) ? currentVersion.Major : 0);
                int minor = ((currentVersion.Minor != -1) ? currentVersion.Minor : 0);
                int build = ((currentVersion.Build != -1) ? currentVersion.Build : 0);
                int revision = ((currentVersion.Revision != -1) ? currentVersion.Revision : 0);
                SystemVersionInfo systemVersionInfo = new SystemVersionInfo(major, minor, build, revision);
                return new UpdateInfo
                {
                    Changelog = updateInfo?.Changelog,
                    CreatedAt = Convert.ToDateTime(updateInfo?.CreatedAt),
                    Assets = updateInfo?.Assets,
                    IsPreRelease = updateInfo.IsPreRelease,
                    PublishedAt = Convert.ToDateTime(updateInfo?.PublishedAt),
                    TagName = updateInfo?.TagName,
                    AssetsUrl = updateInfo?.AssetsUrl,
                    Author = updateInfo?.Author,
                    HtmlUrl = updateInfo?.HtmlUrl,
                    Name = updateInfo?.Name,
                    TarballUrl = updateInfo?.TarballUrl,
                    TargetCommitish = updateInfo?.TargetCommitish,
                    UploadUrl = updateInfo?.UploadUrl,
                    Url = updateInfo?.Url,
                    ZipballUrl = updateInfo?.ZipballUrl,
                    IsExistNewVersion = (asVersionInfo > systemVersionInfo)
                };
            }

            return null;
        }

        private static SystemVersionInfo GetAsVersionInfo(string version)
        {
            List<int> list = GetVersionNumbers(version).Split('.').Select(new Func<string, int>(int.Parse)).ToList();
            if (list.Count <= 1)
            {
                return new SystemVersionInfo(list[0], 0, 0);
            }

            if (list.Count <= 2)
            {
                return new SystemVersionInfo(list[0], list[1], 0);
            }

            if (list.Count <= 3)
            {
                return new SystemVersionInfo(list[0], list[1], list[2]);
            }

            return new SystemVersionInfo(list[0], list[1], list[2], list[3]);
        }

        private static string GetVersionNumbers(string version)
        {
            string allowedChars = "01234567890.";
            return new string(version.Where((char c) => allowedChars.Contains(c)).ToArray());
        }
    }
}
