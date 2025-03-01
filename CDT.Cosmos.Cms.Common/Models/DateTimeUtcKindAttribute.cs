﻿using System;
using System.ComponentModel.DataAnnotations;

namespace CDT.Cosmos.Cms.Common.Models
{
    /// <summary>
    ///     Ensures that a DateTime object is of kind UTC
    /// </summary>
    public class DateTimeUtcKindAttribute : ValidationAttribute
    {
        /// <summary>
        ///     Determines if value is valid.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dateTime = (DateTime?)value;

            if (dateTime.HasValue && dateTime.Value.Kind != DateTimeKind.Utc)
                return new ValidationResult($"Must be DateTimeKind.Utc, not {dateTime.Value.Kind}.");

            return ValidationResult.Success;
        }
    }
}