using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace SingLife.ULTracker.WebAPI.Infrastructure.BsonFormatters
{
    internal static class NewtonsoftBsonMvcBuilderExtensions
    {
        public static IMvcBuilder AddPolicySystemBsonSerializerFormatters(this IMvcBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, NewtonsoftBsonSerializerOptionsSetup>());

            return builder;
        }
    }
}