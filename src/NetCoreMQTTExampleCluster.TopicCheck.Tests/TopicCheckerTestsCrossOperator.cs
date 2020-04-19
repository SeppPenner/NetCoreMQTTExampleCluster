// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicCheckerTestsCrossOperator.cs" company="Haemmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   A test class to test the <see cref="TopicChecker" /> with the # operator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleCluster.TopicCheck.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     A test class to test the <see cref="TopicChecker" /> with the # operator.
    /// </summary>
    [TestClass]
    public class TopicCheckerTestsCrossOperator
    {
        /// <summary>
        ///     Checks the tester with a valid topic for the # operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValueCrossMatch()
        {
            var result = TopicChecker.Regex("a/#", "a/b");
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Checks the tester with another valid topic for the # operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValueCrossMatch2()
        {
            var result = TopicChecker.Regex("a/#", "a/b/c");
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Checks the tester with a valid topic with a # for the # operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValueCrossMatchWithCross()
        {
            var result = TopicChecker.Regex("a/#", "a/#");
            Assert.IsTrue(result);
        }

        /// <summary>
        ///     Checks the tester with a valid topic with a + for the # operator.
        /// </summary>
        [TestMethod]
        public void CheckSingleValueCrossMatchWithPlus()
        {
            var result = TopicChecker.Regex("a/#", "a/+");
            Assert.IsTrue(result);
        }
    }
}