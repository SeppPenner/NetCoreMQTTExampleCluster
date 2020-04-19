// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicChecker.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A class to test the topics validity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.TopicCheck
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     A class to test the topics validity.
    /// </summary>
    public static class TopicChecker
    {
        /// <summary>
        ///     Does a regex check on the topics.
        /// </summary>
        /// <param name="allowedTopic">The allowed topic.</param>
        /// <param name="topic">The topic.</param>
        /// <returns><c>true</c> if the topic is valid, <c>false</c> if not.</returns>
        public static bool Regex(string allowedTopic, string topic)
        {
            // Check if the topics match directly
            if (allowedTopic == topic)
            {
                return true;
            }

            // Check if there is more than one cross in the topic
            var crossCountTopic = topic.Count(c => c == '#');
            if (crossCountTopic > 1)
            {
                return false;
            }

            // If the cross count is 1 in the topic
            if (crossCountTopic == 1)
            {
                // Check if the cross is the last char in the topic
                var index = topic.IndexOf("#", StringComparison.Ordinal);

                if (index != topic.Length - 1)
                {
                    return false;
                }
            }

            // Else do a regex replace
            var realTopicRegex = allowedTopic.Replace(@"/", @"\/").Replace(
                    "+",
                    @"[!""$%&'()*,\-.0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ\[\\\]\^_`abcdefghijklmnopqrstuvwxyz{\|}~¡¢£¤¥¦§¨©ª«¬®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ]*")
                .Replace(
                    "#",
                    @"[!""$%&'()*+#,\-.\/0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ\[\\\]\^_`abcdefghijklmnopqrstuvwxyz{\|}~¡¢£¤¥¦§¨©ª«¬®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ]*");

            var regex = new Regex(realTopicRegex);
            var matches = regex.Matches(topic);

            return matches.ToList().Any(match => match.Value == topic);
        }
    }
}