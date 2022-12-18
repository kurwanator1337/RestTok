using RestSharp;
using RestTok.Models;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RestTok
{
    public static class TikTok
    {
        /// <summary>
        /// Collect data from tiktok video url
        /// </summary>
        /// <param name="videoUrl">example: https://www.tiktok.com/@csgo.loot/video/7177406186021752069</param>
        /// <returns></returns>
        public async static Task<Report> GetTikTokData(string videoUrl)
        {
            RestClient client = new RestClient(videoUrl);
            RestRequest request = new RestRequest();

            var result = await client.ExecuteGetAsync(request);

            string content = result.Content;
            content = content[(content.IndexOf("SIGI_STATE") + 36)..];
            content = content[..content.IndexOf("</script>")];

            string accountUrl = videoUrl[..videoUrl.IndexOf("/video")];

            return GetReport(content, accountUrl);
        }

        private static Report GetReport(string inputString, string accountUrl)
        {
            return new Report()
            {
                Author = GetAuthorInfo(inputString),
                AuthorStats = GetAuthorStats(accountUrl),
                Common = GetCommonData(inputString),
                VideoData = GetVideoData(inputString),
                VideoStats = GetVideoStats(inputString)
            };
        }

        private static AuthorStats GetAuthorStats(string accountUrl)
        {
            AuthorStats authorStats = new AuthorStats();

            RestClient client = new RestClient("https://tiktok.com");
            CookieCollection CookieCollection = client.ExecuteGet(new RestRequest()).Cookies;

            client = new RestClient(accountUrl);
            client.Options.FollowRedirects = true;
            client.CookieContainer.Add(CookieCollection);
            client.Options.CookieContainer = new CookieContainer();
            client.Options.CookieContainer.Add(CookieCollection);
            for (int i = 0; i < CookieCollection.Count; i++)
            {
                RestRequest request = new RestRequest();
                request.AddOrUpdateHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 YaBrowser/22.11.3.815 Yowser/2.5 Safari/537.36");
                request.AddOrUpdateHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                request.AddOrUpdateHeader("Accept-Encoding", "gzip, deflate, br");
                request.AddOrUpdateHeader("cache-control", "no-cache");
                request.AddOrUpdateHeader("Content-Type", "application/json;charset=UTF-8");
                request.AddOrUpdateHeader("Origin", "https://www.tiktok.com/");
                request.AddOrUpdateHeader("Referer", "https://www.tiktok.com/");
                request.AddOrUpdateHeader("Path", accountUrl.Replace("https://www.tiktok.com", ""));
                request.AddOrUpdateHeader("Scheme", "https");
                request.AddOrUpdateHeader("Cookie", CookieCollection[i].Value);

                request.Method = Method.Get;

                var result = client.ExecuteGet(request);

                string content = result.Content;
                if (!content.Contains("SIGI_STATE"))
                    continue;
                content = content[(content.IndexOf("SIGI_STATE") + 36)..];
                content = content[..content.IndexOf("</script>")];

                Regex regexObj = new Regex(@"authorStats"":{""followerCount"":(?<followers>\d*),""followingCount"":(?<following>\d*),""heart"":(?<likes>\d*),""heartCount"":(?<likesEx>\d*),""videoCount"":(?<videos>\d*),""diggCount"":(?<diggs>\d*)");
                Match matchResult = regexObj.Match(content);
                while (matchResult.Success)
                {
                    authorStats.FollowersCount = int.Parse(matchResult.Groups["followers"].Value);
                    authorStats.FollowingCount = int.Parse(matchResult.Groups["following"].Value);
                    authorStats.HeartCount = int.Parse(matchResult.Groups["likesEx"].Value);
                    authorStats.VideosCount = int.Parse(matchResult.Groups["videos"].Value);
                    authorStats.DiggsCount = int.Parse(matchResult.Groups["diggs"].Value);
                    return authorStats;
                }
            }

            return authorStats;
        }

        private static VideoStats GetVideoStats(string inputString)
        {
            VideoStats videoStats = new VideoStats();

            Regex regexObj = new Regex(@"stats"":{""diggCount"":(?<diggs>\d*),""shareCount"":(?<shares>\d*),""commentCount"":(?<comments>\d*),""playCount"":(?<views>\d*)");
            Match matchResult = regexObj.Match(inputString);
            while (matchResult.Success)
            {
                videoStats.ViewsCount = int.Parse(matchResult.Groups["views"].Value);
                videoStats.SharesCount = int.Parse(matchResult.Groups["shares"].Value);
                videoStats.CommentsCount = int.Parse(matchResult.Groups["comments"].Value);
                videoStats.DiggsCount = int.Parse(matchResult.Groups["diggs"].Value);
                matchResult = matchResult.NextMatch();
            }

            return videoStats;
        }

        private static Author GetAuthorInfo(string inputString)
        {
            Author author = new Author();

            Regex regexObj = new Regex(@"users"":{""(?<authorName>.*?)"":{""id"":""(?<authorId>\d*)""");
            Match matchResult = regexObj.Match(inputString);
            while (matchResult.Success)
            {
                author.Name = matchResult.Groups["authorName"].Value;
                author.Id = long.Parse(matchResult.Groups["authorId"].Value);
                matchResult = matchResult.NextMatch();
            }

            return author;
        }

        private static VideoData GetVideoData(string inputString)
        {
            VideoData video = new VideoData();

            Regex regexObj = new Regex(@"""duration"":(?<duration>\d*),""ratio");
            Match matchResult = regexObj.Match(inputString);
            while (matchResult.Success)
            {
                video.Duration = int.Parse(matchResult.Groups["duration"].Value);
                matchResult = matchResult.NextMatch();
            }

            regexObj = new Regex("\"title\":\"(?<title>.*?)\",\"playUrl\":\"(?<url>.*?)\",");
            matchResult = regexObj.Match(inputString);
            while (matchResult.Success)
            {
                string title = matchResult.Groups["title"].Value;
                video.MusicTitle = title.Length < 100 ? title : "too large string";
                string unicodeUrl = matchResult.Groups["url"].Value;
                video.MusicURL = unicodeUrl.Replace("\\u002F", "/");
                matchResult = matchResult.NextMatch();
            }

            return video;
        }

        private static Common GetCommonData(string inputString)
        {
            Common data = new Common();

            Regex regexObj = new Regex(@"id"":""(?<id>\d*)"",""desc"":""(?<desc>.*?)"",""createTime"":""(?<createTime>\d*)""");
            Match matchResult = regexObj.Match(inputString);
            while (matchResult.Success)
            {
                data.CreateTime = int.Parse(matchResult.Groups["createTime"].Value);
                data.Id = long.Parse(matchResult.Groups["id"].Value);
                data.Description = matchResult.Groups["desc"].Value;
                matchResult = matchResult.NextMatch();
            }

            return data;
        }
    }
}
