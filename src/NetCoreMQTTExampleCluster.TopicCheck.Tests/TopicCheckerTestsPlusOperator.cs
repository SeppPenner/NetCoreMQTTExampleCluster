// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicCheckerTestsPlusOperator.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A test class to test the <see cref="TopicChecker" /> with the + operator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.TopicCheck.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     A test class to test the <see cref="TopicChecker" /> with the + operator.
    /// </summary>
    [TestClass]
    public class TopicCheckerTestsPlusOperator
    {
        /// <summary>
        ///     Checks the tester with an invalid topic for the + operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValuePlusDontMatch()
        {
            var result = TopicChecker.Regex("a/+", "a/b/c");
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Checks the tester with an invalid topic with a # for the + operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValuePlusDontMatchWithCross()
        {
            var result = TopicChecker.Regex("a/+", "a/#");
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Checks the tester with a valid topic for the + operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValuePlusMatch()
        {
            var result = TopicChecker.Regex("a/+", "a/b");
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Checks the tester with a valid topic with a + for the + operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValuePlusMatchWithPlus()
        {
            var result = TopicChecker.Regex("a/+", "a/+");
            Assert.IsTrue(result);
        }
    }
}