using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a json:api relationship (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    public abstract class RelationshipAttribute : ResourceFieldAttribute
    {
        private LinkTypes _links;

        public string InverseNavigation { get; set; }

        /// <summary>
        /// The internal navigation property path to the related resource.
        /// </summary>
        /// <remarks>
        /// In all cases except for <see cref="HasManyThroughAttribute"/> relationships, this equals the property name.
        /// </remarks>
        public virtual string RelationshipPath => Property.Name;

        /// <summary>
        /// The child resource type. This does not necessarily match the navigation property type.
        /// In the case of a <see cref="HasManyAttribute"/> relationship, this value will be the collection argument type.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// public List<Tag> Tags { get; set; } // Type => Tag
        /// ]]></code>
        /// </example>
        public Type RightType { get; internal set; }

        /// <summary>
        /// The parent resource type. This is the type of the class in which this attribute was used.
        /// </summary>
        public Type LeftType { get; internal set; }

        /// <summary>
        /// Configures which links to show in the <see cref="Links"/> object for this relationship.
        /// When not explicitly assigned, the default value depends on the relationship type (see remarks).
        /// </summary>
        /// <remarks>
        /// This defaults to <see cref="LinkTypes.All"/> for <see cref="HasManyAttribute"/> and <see cref="HasManyThroughAttribute"/> relationships.
        /// This defaults to <see cref="LinkTypes.NotConfigured"/> for <see cref="HasOneAttribute"/> relationships, which means that
        /// the configuration in <see cref="IJsonApiOptions"/> or <see cref="ResourceContext"/> is used.
        /// </remarks>
        public LinkTypes Links
        {
            get => _links;
            set
            {
                if (value == LinkTypes.Paging)
                {
                    throw new InvalidConfigurationException($"{LinkTypes.Paging:g} not allowed for argument {nameof(value)}");
                }

                _links = value;
            }
        }

        /// <summary>
        /// Whether or not this relationship can be included using the <c>?include=publicName</c> query string parameter.
        /// This is <c>true</c> by default.
        /// </summary>
        public bool CanInclude { get; set; } = true;

        /// <summary>
        /// Gets the value of the resource property this attributes was declared on.
        /// </summary>
        public virtual object GetValue(object resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            return Property.GetValue(resource);
        }

        /// <summary>
        /// Sets the value of the resource property this attributes was declared on.
        /// </summary>
        public virtual void SetValue(object resource, object newValue, IResourceFactory resourceFactory)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (resourceFactory == null) throw new ArgumentNullException(nameof(resourceFactory));

            Property.SetValue(resource, newValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (RelationshipAttribute) obj;

            return LeftType == other.LeftType && RightType == other.RightType && Links == other.Links && 
                CanInclude == other.CanInclude && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LeftType, RightType, Links, CanInclude, base.GetHashCode());
        }
    }
}
