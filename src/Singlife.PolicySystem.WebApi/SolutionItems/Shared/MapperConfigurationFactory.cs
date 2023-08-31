using AutoMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SingLife.ULTracker
{
    public static class MapperConfigurationFactory
    {
        /// <summary>
        /// Create <see cref="MapperConfiguration"/> with creating missing TypeMaps automatically.
        /// </summary>
        /// <param name="configure">Mapping configuration options</param>
        /// <returns><see cref="MapperConfiguration"/></returns>
        public static MapperConfiguration CreateV7CompatibleMapperConfiguration(Action<IMapperConfigurationExpression> configure)
        {
            var existingMaps = new HashSet<TypeMap>();
            var missingMaps = new HashSet<TypePair>();
            var shouldIgnoreProperties = new HashSet<IgnoreProperty>();
            var propertyName = string.Empty;

            GetExistingMaps();

            GetMissingMaps();

            var configuration = CreateMapperConfigurationWithMissingMaps();

            return configuration;

            void GetExistingMaps()
            {
                var configuration = new MapperConfiguration(config =>
                {
                    configure?.Invoke(config);

                    config.ForAllMaps((map, _) => {
                        existingMaps.Add(map);
                    });
                });
            }

            void GetMissingMaps()
            {
                var configuration = new MapperConfiguration(config =>
                {
                    configure?.Invoke(config);

                    config.ForAllMaps((map, mapExpression) =>
                    {
                        propertyName = string.Empty;
                        FillMissingMaps(map, map.SourceType, map.DestinationType);
                    });
                });
            }

            void FillMissingMaps(TypeMap map, Type sourceType, Type destinationType)
            {
                TypeMap newMap = null;

                if (sourceType == destinationType || destinationType.IsNullablePrimitive())
                    return;

                if (destinationType.DoesNotHaveParameterlessConstructor())
                {
                    shouldIgnoreProperties.Add(new IgnoreProperty(map, propertyName));
                    return;
                }

                if (DoesNotExistMap(sourceType, destinationType))
                {
                    newMap = TypeMapFactory.CreateTypeMap(sourceType, destinationType, map.Profile);
                    missingMaps.Add(new TypePair(newMap.SourceType, newMap.DestinationType));
                }

                foreach (var destinationProperty in destinationType.GetProperties())
                {
                    propertyName = destinationProperty.Name;
                    var source = GetCorrespondingSourceType(propertyName);

                    if (source != null)
                    {
                        Type destination = destinationProperty.PropertyType;
                        if (source.IsEnumerable() && destination.IsEnumerable())
                        {
                            source = source.GetEnumeratedType();
                            destination = destination.GetEnumeratedType();
                        }

                        if (destination.IsGenericType && source.IsGenericType)
                        {
                            FillMissingMaps(
                                newMap != null ? newMap : map,
                                source.GetGenericArguments().First(),
                                destination.GetGenericArguments().First());
                        }
                        else if (destination.IsClass && source.IsClass)
                        {
                            FillMissingMaps(
                                newMap != null ? newMap : map,
                                source,
                                destination);
                        }
                    }
                }

                bool DoesNotExistMap(Type sourceType, Type destinationType) =>
                    !existingMaps.Any(x => x.SourceType == sourceType && x.DestinationType == destinationType) &&
                    !missingMaps.Any(x => x.SourceType == sourceType && x.DestinationType == destinationType);

                Type GetCorrespondingSourceType(string destinationPropertyName) => newMap != null
                        ? sourceType.GetProperties().FirstOrDefault(p => p.Name == destinationPropertyName)?.PropertyType
                        : map.MemberMaps.FirstOrDefault(x => x.DestinationName == destinationPropertyName && !x.Ignored)?.SourceType;
            }

            MapperConfiguration CreateMapperConfigurationWithMissingMaps()
            {
                return new MapperConfiguration(config =>
                {
                    configure?.Invoke(config);

                    foreach (var map in missingMaps)
                    {
                        config.CreateMap(map.SourceType, map.DestinationType);
                    }

                    config.ForAllMaps((map, expression) =>
                    {
                        IgnoreMismatchedDestinationProperty(map, expression);
                    });
                });

                void IgnoreMismatchedDestinationProperty(TypeMap map, IMappingExpression expression)
                {
                    var flags = BindingFlags.Public | BindingFlags.Instance;
                    var mappedProperties = map.MemberMaps.Select(x => x.DestinationName);
                    var mismatchedProperties = map.DestinationType
                        .GetProperties(flags)
                        .Select(x => x.Name)
                        .Except(mappedProperties);

                    foreach (var property in mismatchedProperties)
                    {
                        expression.ForMember(property, opt => opt.Ignore());
                    }

                    var ignoreProperties = shouldIgnoreProperties
                        .Where(x => x.Map.SourceType == map.SourceType && x.Map.DestinationType == map.DestinationType && x.PropertyName != string.Empty)
                        .Select(x => x.PropertyName);

                    foreach (var property in ignoreProperties)
                    {
                        expression.ForMember(property, opt => opt.Ignore());
                    }
                }
            }
        }

        private static bool IsNullablePrimitive(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return underlyingType != null && underlyingType.IsPrimitive;
        }

        private static bool DoesNotHaveParameterlessConstructor(this Type type) =>
            !type.IsArray && type.IsClass &&
            !type.GetConstructors().Any(x => x.GetParameters().Length == 0);

        private static Type GetEnumeratedType(this Type type) =>
           type?.GetElementType() ?? (type.IsEnumerable()
            ? type.GenericTypeArguments.FirstOrDefault()
            : null);

        private static bool IsEnumerable(this Type type) => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);

        private class IgnoreProperty
        {
            public IgnoreProperty(TypeMap map, string propertyName)
            {
                Map = map;
                PropertyName = propertyName;
            }
            public TypeMap Map { get; }

            public string PropertyName { get; }
        }
    }
}