using Autofac;
using MediatR;
using MediatR.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SingLife.PolicySystem.Shared.MediatRRegistration
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Registers all necessary MediatR classes and interfaces to the specified Autofac container.
        /// This method should be called only once at the application startup.
        /// </summary>
        /// <param name="builder">An Autofac container to add the registrations to.</param>
        /// <param name="typesFromAssembliesContainingMediatROpenTypes">Types from assemblies containing
        /// MediatR open types, e.g. command and query handler classes.</param>
        /// <param name="customBehaviorTypes">Types that implement the <see cref=" IPipelineBehavior{TRequest,TResponse}"/>
        /// interface.</param>
        /// <returns>The Autofac container.</returns>
        public static ContainerBuilder RegisterMediatR(
            this ContainerBuilder builder,
            IEnumerable<Type> typesFromAssembliesContainingMediatROpenTypes,
            IEnumerable<Type> customBehaviorTypes = null)
        {
            var assemblies = GetAssemblies(typesFromAssembliesContainingMediatROpenTypes);
            var customBehaviors = GetCustomBehaviors(customBehaviorTypes);

            builder.RegisterModule(new MediatRModule(assemblies, customBehaviors));

            return builder;
        }

        private static Assembly[] GetAssemblies(IEnumerable<Type> types)
        {
            return types
                ?.Select(t => t.Assembly)
                .Distinct()
                .ToArray();
        }

        private static Type[] GetCustomBehaviors(IEnumerable<Type> customBehaviorTypes) =>
            customBehaviorTypes == null ? Array.Empty<Type>() : customBehaviorTypes.ToArray();
    }

    internal class MediatRModule : Autofac.Module
    {
        private readonly Assembly[] assemblies;
        private readonly Type[] customBehaviorTypes;

        public MediatRModule(Assembly[] assemblies, Type[] customBehaviorTypes)
        {
            this.assemblies = assemblies?.Length > 0
                ? assemblies
                : throw new ArgumentException("Please specify at least one assembly for MediatR registration.", nameof(assemblies));

            this.customBehaviorTypes = customBehaviorTypes ?? throw new ArgumentNullException(nameof(customBehaviorTypes));
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Sample registrations
            // - https://github.com/jbogard/MediatR/wiki#autofac
            // - https://github.com/jbogard/MediatR/blob/master/samples/MediatR.Examples.Autofac/Program.cs
            RegisterMediator(builder);
            RegisterHandlers(builder);
            RegisterMediatRBehaviors(builder);
            RegisterCustomBehaviors(builder);
            RegisterServiceFactory(builder);
        }

        private static void RegisterMediator(ContainerBuilder builder)
        {
            // Register as IMediator for backward compatibility.
            // Do not register as IPublisher because we don't need it yet.
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .As<ISender>()
                .InstancePerLifetimeScope();
        }

        private void RegisterHandlers(ContainerBuilder builder)
        {
            var mediatrOpenTypes = new[]
            {
                // Do not register INotificationHandler because we don't need it yet.
                typeof(IRequestHandler<,>),
                typeof(IRequestExceptionHandler<,,>),
                typeof(IRequestExceptionAction<,>)
            };

            foreach (var mediatrOpenType in mediatrOpenTypes)
            {
                builder.RegisterAssemblyTypes(this.assemblies).AsClosedTypesOf(mediatrOpenType);
            }
        }

        private static void RegisterMediatRBehaviors(ContainerBuilder builder)
        {
            // It appears Autofac returns the last registered types first
            builder.RegisterGeneric(typeof(RequestPostProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(RequestPreProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(RequestExceptionActionProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>)).InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(RequestExceptionProcessorBehavior<,>)).As(typeof(IPipelineBehavior<,>)).InstancePerLifetimeScope();
        }

        private void RegisterCustomBehaviors(ContainerBuilder builder)
        {
            foreach (var customBehaviorType in this.customBehaviorTypes)
            {
                builder.RegisterGeneric(customBehaviorType).As(typeof(IPipelineBehavior<,>));
            }
        }

        private static void RegisterServiceFactory(ContainerBuilder builder) =>
            builder.Register<ServiceFactory>(outerContext =>
            {
                var innerContext = outerContext.Resolve<IComponentContext>();

                return serviceType => innerContext.Resolve(serviceType);
            });
    }
}