﻿/* https://github.com/uon-nuget/ExpressiveAnnotations
 * Original work Copyright (c) 2014 Jarosław Waliszko
 * Modified work Copyright (c) 2018 The University of Nottingham
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Caching.Memory;
using UoN.ExpressiveAnnotations.NetCore.Attributes;
using UoN.ExpressiveAnnotations.NetCore.Caching;

namespace UoN.ExpressiveAnnotations.NetCore.Validators
{
    /// <summary>
    ///     Model validator for <see cref="RequiredIfAttribute" />.
    /// </summary>
    public class RequiredIfValidator : ExpressiveValidator<RequiredIfAttribute>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RequiredIfValidator" /> class.
        /// </summary>
        /// <param name="metadata">The model metadata.</param>
        /// <param name="attribute">The expressive assertion attribute instance.</param>
        /// <param name="memoryCache">An IMemoryCache instance, scoped to the process.</param>
        /// <param name="requestCache">A RequestCache instance, scoped to the request.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"></exception>
        public RequiredIfValidator(ModelMetadata metadata, string attributeId, RequiredIfAttribute attribute, IMemoryCache memoryCache, IMemoryCache requestCache)
            : base(metadata, attributeId, attribute, memoryCache, requestCache)
        {
            AllowEmpty = attribute.AllowEmptyStrings;

            try
            {
                var propType = metadata.ModelType;
                if (propType.IsNonNullableValueType())
                    throw new InvalidOperationException(
                        $"{nameof(RequiredIfAttribute)} has no effect when applied to a field of non-nullable value type '{propType.FullName}'. Use nullable '{propType.FullName}?' version instead, or switch to {nameof(AssertThatAttribute)} otherwise.");
            }
            catch (Exception e)
            {
                throw new ValidationException(
                    $"{this.GetType().Name}: validation applied to {metadata.PropertyName} field failed.", e);
            }

        }

        private bool AllowEmpty { get; set; }

        /// <summary>
        ///     Attaches the validation rules to the context.
        /// </summary>
        /// <returns>
        ///     void
        /// </returns>
        public void AttachValidationRules(ClientModelValidationContext context, string defaultErrorMessage)
        {
            AttachValidationRules(context, "requiredif", defaultErrorMessage);
            MergeAttribute(context.Attributes, "data-val-allowempty", AllowEmpty.ToJson());
        }
    }
}
