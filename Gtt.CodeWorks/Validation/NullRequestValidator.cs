﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.CodeWorks.Validation
{
    public class NullRequestValidator : IRequestValidator
    {
        private NullRequestValidator()
        {
            
        }

        public ValidationAttempt Validate<T>(T request, string prefix = null)
        {
            return ValidationAttempt.Success;
        }

        public static NullRequestValidator Instance => new NullRequestValidator();
    }
}
