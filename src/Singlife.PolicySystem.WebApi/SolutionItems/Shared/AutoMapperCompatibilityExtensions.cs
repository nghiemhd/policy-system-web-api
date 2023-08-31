using AutoMapper;
using System;

namespace SingLife.ULTracker
{
    public static class AutoMapperCompatibilityExtensions
    {
        /// <summary>
        /// Resolve destination member using a custom value resolver callback. 
        /// Used instead of MapFrom when not simply redirecting a source member. 
        /// This method cannot be used in conjunction with LINQ query projection.
        /// </summary>
        /// <param name="member"><see cref="IMemberConfigurationExpression"/>, the member configuration options.</param>
        /// <param name="resolver">Callback function to resolve against source type.</param>
        public static void ResolveUsing<TSource, TDestination, TMember, TResult>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> member, 
            Func<TSource, TResult> resolver) => member.MapFrom((src, dest) => resolver(src));

        /// <summary>
        /// Resolve destination member using a custom value resolver callback. 
        /// Used instead of MapFrom when not simply redirecting a source member. 
        /// This method cannot be used in conjunction with LINQ query projection.
        /// </summary>
        /// <param name="member"><see cref="IMemberConfigurationExpression"/>, the member configuration options.</param>
        /// <param name="resolver">Callback function to resolve against source type.</param>
        public static void ResolveUsing<TSource, TDestination, TMember, TResult>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> member,
            Func<TSource, TDestination, TMember, ResolutionContext, TResult> resolver) => member.MapFrom((src, dest, member, context) => resolver(src, dest, member, context));

        /// <summary>
        /// Use a custom value to map the destination member.
        /// </summary>
        /// <param name="member"><see cref="IMemberConfigurationExpression"/>, the member configuration options.</param>
        /// <param name="value">The value of destination member.</param>
        public static void UseValue<TSource, TDestination, TMember, TValue>(
            this IMemberConfigurationExpression<TSource, TDestination, TMember> member,
            TValue value) => member.MapFrom(src => value);
    }
}