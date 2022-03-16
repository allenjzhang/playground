using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cadl.ProviderHubController.Common.Additions
{
    public abstract class VersioningContractResolverBase : DefaultContractResolver
    {
        public const string DefaultVersion = "v3";
        Func<string, string> _libraryVersionMap;
        VersionComparer _libraryVersionComparer = new VersionComparer(new[] { "v2", "v3" });
        VersionComparer _serviceVersionComparer;
        string _serviceVersion;

        // think about the version mapper if the service is using the canonical version, it should also be canonical
        public VersioningContractResolverBase() : this(new VersionComparer(new[] { VersionComparer.CanonicalVersion }), v => VersionComparer.CanonicalVersion, VersionComparer.CanonicalVersion) { }
        public VersioningContractResolverBase(VersionComparer serviceVersionComparer, Func<string, string> libraryVersionMap, string serviceVersion)
        {
            _serviceVersion = serviceVersion;
            _libraryVersionMap = libraryVersionMap;
            _serviceVersionComparer = serviceVersionComparer;
        }

        protected abstract bool ShouldCreateServiceProperties(Type type);

        protected virtual JsonProperty CreateServiceProperty(MemberInfo member, MemberSerialization memberSerialization, JsonProperty property)
        {
            return CreateVersionedProperty(_serviceVersionComparer, _serviceVersion, member, memberSerialization, property);
        }

        JsonProperty CreateLibraryProperty(MemberInfo member, MemberSerialization memberSerialization, JsonProperty property)
        {
            return CreateVersionedProperty(_libraryVersionComparer, _libraryVersionMap(_serviceVersion), member, memberSerialization, property);
        }

        bool ShouldCreateLibraryProperties(Type type)
        {
            return string.Equals(type?.Namespace, "Cadl.ProviderHubController.Common", StringComparison.InvariantCultureIgnoreCase);
        }

        public static JsonProperty CreateVersionedProperty(VersionComparer comparer, string version, MemberInfo member, MemberSerialization memberSerialization, JsonProperty property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (version == VersionComparer.CanonicalVersion)
                return property;
            if (Attribute.IsDefined(member, typeof(AddedAttribute)))
            {
                var added = AddedAttribute.GetCustomAttribute(member, typeof(AddedAttribute)) as AddedAttribute;
                if (comparer.Compare(version, added.OnVersion) < 0)
                {
                    property.ShouldSerialize = t => false;
                    property.ShouldDeserialize = t => false;
                }
            }
            if (Attribute.IsDefined(member, typeof(RemovedAttribute)))
            {
                var removed = RemovedAttribute.GetCustomAttribute(member, typeof(RemovedAttribute)) as RemovedAttribute;
                if (comparer.Compare(version, removed.OnVersion) >= 0)
                {
                    property.ShouldSerialize = t => false;
                    property.ShouldDeserialize = t => false;
                }
            }

            return property;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (ShouldCreateServiceProperties(member.DeclaringType))
                return CreateServiceProperty(member, memberSerialization, property);
            if (ShouldCreateLibraryProperties(member.DeclaringType))
                return CreateLibraryProperty(member, memberSerialization, property);
            return property;
        }
    }
}
