
namespace NServiceBus.Blueprint
{
    using System;
    using System.Linq;

    /// <summary>
    /// Message type.
    /// </summary>
    public class MessageType
    {
        /// <summary>
        /// Creates a new instance based on the assembly-qualified type name.
        /// </summary>
        public MessageType(string type)
        {
            var parts = type.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim()).ToArray();

            AssemblyName = parts[1];
            var nameAndNamespaceParts = parts[0].Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            Namespace = nameAndNamespaceParts.Length > 1
                ? string.Join(".", nameAndNamespaceParts.Take(nameAndNamespaceParts.Length - 1))
                : null;
            Name = nameAndNamespaceParts.Last();
        }

        /// <summary>
        /// Creates new instance based on the Type object.
        /// </summary>
        /// <param name="type"></param>
        public MessageType(Type type)
            : this(type.AssemblyQualifiedName)
        {
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the namespace of the type.
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// Gets the name of the assembly in which the type has been defined.
        /// </summary>
        public string AssemblyName { get; }

        bool Equals(MessageType other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Namespace, other.Namespace) && string.Equals(AssemblyName, other.AssemblyName);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MessageType)obj);
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AssemblyName != null ? AssemblyName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}