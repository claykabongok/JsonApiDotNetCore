using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Used to locate types and facilitate resource auto-discovery.
    /// </summary>
    internal static class TypeLocator
    {
        /// <summary>
        /// Determine whether or not this is a json:api resource by checking if it implements <see cref="IIdentifiable{TId}"/>.
        /// </summary>
        public static Type GetIdType(Type resourceType)
        {
            var identifiableInterface = resourceType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIdentifiable<>));
            return identifiableInterface?.GetGenericArguments()[0];
        }

        /// <summary>
        /// Attempts to get a descriptor for the resource type.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the type is a valid json:api type (must implement <see cref="IIdentifiable"/>); <c>false</c>, otherwise.
        /// </returns>
        internal static bool TryGetResourceDescriptor(Type type, out ResourceDescriptor descriptor)
        {
            if (TypeHelper.IsOrImplementsInterface(type, typeof(IIdentifiable)))
            {
                descriptor = new ResourceDescriptor(type, GetIdType(type));
                return true;
            }
            descriptor = ResourceDescriptor.Empty;
            return false;
        }
        /// <summary>
        /// Gets all implementations of the generic interface.
        /// </summary>
        /// <param name="assembly">The assembly to search.</param>
        /// <param name="openGenericInterfaceType">The open generic type, e.g. `typeof(IResourceService&lt;&gt;)`.</param>
        /// <param name="genericInterfaceArguments">Parameters to the generic type.</param>
        /// <example>
        /// <code><![CDATA[
        /// GetGenericInterfaceImplementation(assembly, typeof(IResourceService<>), typeof(Article), typeof(Guid));
        /// ]]></code>
        /// </example>
        public static (Type implementation, Type registrationInterface) GetGenericInterfaceImplementation(Assembly assembly, Type openGenericInterfaceType, params Type[] genericInterfaceArguments)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            if (openGenericInterfaceType == null) throw new ArgumentNullException(nameof(openGenericInterfaceType));
            if (genericInterfaceArguments == null) throw new ArgumentNullException(nameof(genericInterfaceArguments));
            if (genericInterfaceArguments.Length == 0) throw new ArgumentException("No arguments supplied for the generic interface.", nameof(genericInterfaceArguments));
            if (!openGenericInterfaceType.IsGenericType) throw new ArgumentException("Requested type is not a generic type.", nameof(openGenericInterfaceType));

            foreach (var type in assembly.GetTypes())
            {
                var interfaces = type.GetInterfaces();
                foreach (var interfaceType in interfaces)
                {
                    if (interfaceType.IsGenericType)
                    {
                        var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();
                        if (interfaceType.GetGenericArguments().First() == genericInterfaceArguments.First() &&genericTypeDefinition == openGenericInterfaceType.GetGenericTypeDefinition())
                        {
                            return (
                                type,
                                genericTypeDefinition.MakeGenericType(genericInterfaceArguments)
                            );
                        }
                    }
                }
            }

            return (null, null);
        }

        /// <summary>
        /// Gets all derivatives of the concrete, generic type.
        /// </summary>
        /// <param name="assembly">The assembly to search.</param>
        /// <param name="openGenericType">The open generic type, e.g. `typeof(ResourceDefinition&lt;&gt;)`.</param>
        /// <param name="genericArguments">Parameters to the generic type.</param>
        /// <example>
        /// <code><![CDATA[
        /// GetDerivedGenericTypes(assembly, typeof(ResourceDefinition<>), typeof(Article))
        /// ]]></code>
        /// </example>
        public static IReadOnlyCollection<Type> GetDerivedGenericTypes(Assembly assembly, Type openGenericType, params Type[] genericArguments)
        {
            var genericType = openGenericType.MakeGenericType(genericArguments);
            return GetDerivedTypes(assembly, genericType).ToArray();
        }

        /// <summary>
        /// Gets all derivatives of the specified type.
        /// </summary>
        /// <param name="assembly">The assembly to search.</param>
        /// <param name="inheritedType">The inherited type.</param>
        /// <example>
        /// <code>
        /// GetDerivedGenericTypes(assembly, typeof(DbContext))
        /// </code>
        /// </example>
        public static IEnumerable<Type> GetDerivedTypes(Assembly assembly, Type inheritedType)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (inheritedType.IsAssignableFrom(type))
                    yield return type;
            }
        }
    }
}
