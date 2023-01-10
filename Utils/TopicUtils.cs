using System;

namespace CConn
{
    internal static class TopicUtils
    {
        public static string ToFullTopic(string appTopic, Method method)
        {
            return string.Format("{0}/{1}", appTopic, method.ToString().ToLower());
        }

        public static Tuple<string, Method> ToAppTopic(string fullTopic)
        {
            int lastSlashIndex = fullTopic.LastIndexOf('/');
            if (lastSlashIndex < 0)
            {
                throw new Exception("fullTopic does not have slash");
            }

            string appTopic = fullTopic.Substring(0, lastSlashIndex);
            string strMethod = fullTopic.Substring(lastSlashIndex + 1);

            Method method = Method.REPORT;
            if (!Enum.TryParse<Method>(strMethod, true, out method))
            {
                throw new Exception(string.Format("Parse method {0} failed", strMethod));
            }

            return Tuple.Create(appTopic, method);
        }

        public static bool IsTopicMatch(string topicFilter, string topic)
        {
            var topicFilterTokens = topicFilter.Split('/');
            var topicTokens = topic.Split('/');

            if (topicFilterTokens.Length > topicTokens.Length)
            {
                return false;
            }

            for (int i = 0; i < topicFilterTokens.Length; i++)
            {
                var topicFilterToken = topicFilterTokens[i];
                var topicToken = topicTokens[i];

                if (topicFilterToken == "#")
                {
                    var filterLastToken = topicFilterTokens[topicFilterTokens.Length - 1];
                    var lastToken = topicTokens[topicTokens.Length - 1];

                    return filterLastToken == "#" ? true : filterLastToken == lastToken;
                }

                if (topicFilterToken != "+" && topicFilterToken != topicToken)
                {
                    return false;
                }
            }

            return topicFilterTokens.Length == topicTokens.Length;
        }
    }
}
