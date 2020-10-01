using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gtt.CodeWorks.Tests.Core
{
    [TestClass]
    public class ServiceResultsExpanderTests
    {
        [TestMethod]
        public void GetSuccessMetaData()
        {
            var r = ServiceResult.Successful;
            Assert.AreEqual(ResultOutcome.Successful, r.Outcome());
            Assert.AreEqual(ResultCategory.Successful, r.Category());
            Assert.AreEqual(200, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetCreatedMetaData()
        {
            var r = ServiceResult.Created;
            Assert.AreEqual(ResultOutcome.Successful, r.Outcome());
            Assert.AreEqual(ResultCategory.Successful, r.Category());
            Assert.AreEqual(201, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetFulfilledByExistingMetaData()
        {
            var r = ServiceResult.FulfilledByExistingResource;
            Assert.AreEqual(ResultOutcome.Successful, r.Outcome());
            Assert.AreEqual(ResultCategory.Successful, r.Category());
            Assert.AreEqual(200, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetQueuedMetaData()
        {
            var r = ServiceResult.Queued;
            Assert.AreEqual(ResultOutcome.Successful, r.Outcome());
            Assert.AreEqual(ResultCategory.Successful, r.Category());
            Assert.AreEqual(202, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetResourceNotFoundMetaData()
        {
            var r = ServiceResult.ResourceNotFound;
            Assert.AreEqual(ResultOutcome.Failed, r.Outcome());
            Assert.AreEqual(ResultCategory.BusinessLogicError, r.Category());
            Assert.AreEqual(404, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetValidationErrorMetaData()
        {
            var r = ServiceResult.ValidationError;
            Assert.AreEqual(ResultOutcome.Failed, r.Outcome());
            Assert.AreEqual(ResultCategory.BusinessLogicError, r.Category());
            Assert.AreEqual(400, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetConflictingRequestMetaData()
        {
            var r = ServiceResult.ConflictingRequest;
            Assert.AreEqual(ResultOutcome.Failed, r.Outcome());
            Assert.AreEqual(ResultCategory.BusinessLogicError, r.Category());
            Assert.AreEqual(409, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetTransientErrorMetaData()
        {
            var r = ServiceResult.TransientError;
            Assert.AreEqual(ResultOutcome.Failed, r.Outcome());
            Assert.AreEqual(ResultCategory.SystemError, r.Category());
            Assert.AreEqual(503, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetPermanentErrorMetaData()
        {
            var r = ServiceResult.PermanentError;
            Assert.AreEqual(ResultOutcome.Failed, r.Outcome());
            Assert.AreEqual(ResultCategory.SystemError, r.Category());
            Assert.AreEqual(500, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetRateLimitedMetaData()
        {
            var r = ServiceResult.RateLimited;
            Assert.AreEqual(ResultOutcome.Failed, r.Outcome());
            Assert.AreEqual(ResultCategory.SystemError, r.Category());
            Assert.AreEqual(429, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetUpstreamErrorMetaData()
        {
            var r = ServiceResult.UpstreamError;
            Assert.AreEqual(ResultOutcome.Failed, r.Outcome());
            Assert.AreEqual(ResultCategory.DependencyError, r.Category());
            Assert.AreEqual(502, r.HttpStatusCode());
        }

        [TestMethod]
        public void GetUpstreamTimeoutMetaData()
        {
            var r = ServiceResult.UpstreamTimeout;
            Assert.AreEqual(ResultOutcome.Failed, r.Outcome());
            Assert.AreEqual(ResultCategory.DependencyError, r.Category());
            Assert.AreEqual(504, r.HttpStatusCode());
        }



    }
}
