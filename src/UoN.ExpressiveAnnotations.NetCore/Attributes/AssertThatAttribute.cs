﻿/* https://github.com/uon-nuget/ExpressiveAnnotations
 * Original work Copyright (c) 2014 Jarosław Waliszko
 * Modified work Copyright (c) 2018 The University of Nottingham
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using UoN.ExpressiveAnnotations.NetCore.Caching;
using UoN.ExpressiveAnnotations.NetCore.Validators;

namespace UoN.ExpressiveAnnotations.NetCore.Attributes
{
    /// <summary>
    ///     Validation attribute, executed for non-null annotated field, which indicates that assertion given 
    ///     in logical expression has to be satisfied, for such a field to be considered as valid.
    /// </summary>
    public sealed class AssertThatAttribute : ExpressiveAttribute, IClientModelValidator
    {
        private static string _defaultErrorMessage = "Assertion for {0} field is not satisfied by the following logic: {1}";

        /// <summary>
        ///     Gets or sets the default error message.
        /// </summary>
        public static string DefaultErrorMessage
        {
            get { return _defaultErrorMessage; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Default error message cannot be null.");
                _defaultErrorMessage = value;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssertThatAttribute" /> class.
        /// </summary>
        /// <param name="expression">The logical expression based on which assertion condition is computed.</param>
        public AssertThatAttribute(string expression)
            : base(expression, DefaultErrorMessage)
        {
        }

        /// <summary>
        ///     Validates a specified value with respect to the associated validation attribute.
        ///     Internally used by the <see cref="ExpressiveAttribute.IsValid(object,System.ComponentModel.DataAnnotations.ValidationContext)" /> method.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>
        ///     An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.
        /// </returns>
        protected override ValidationResult IsValidInternal(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                Compile(validationContext.ObjectType);
                if (!CachedValidationFuncs[validationContext.ObjectType](validationContext.ObjectInstance)) // check if the assertion condition is not satisfied
                    return new ValidationResult( // assertion not satisfied => notify
                        FormatErrorMessage(validationContext.DisplayName, Expression, validationContext.ObjectInstance),
                        new[] {validationContext.MemberName});
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Use the HttpContext to inject the MemoryCache into the validator, since we can't inject it in the constructor of 
            // ValidationAttribute...see https://andrewlock.net/injecting-services-into-validationattributes-in-asp-net-core/

            var processCache = context.ActionContext.HttpContext.RequestServices.GetService<IMemoryCache>();
            var requestCache = context.ActionContext.HttpContext.RequestServices.GetService<RequestCache>();

            var validator = new AssertThatValidator(context.ModelMetadata, (context.Attributes.ContainsKey("id") ? context.Attributes["id"] : null), this, processCache, requestCache);
            validator.AttachValidationRules(context, DefaultErrorMessage);
        }
    }
}
