
namespace NServiceBus.Blueprint
{
    using System;
    using System.Linq;

    public class MessageType
    {
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

        public MessageType(Type type)
            : this(type.AssemblyQualifiedName)
        {
        }

        MessageType(string name, string ns, string assemblyName)
        {
            Name = name;
            Namespace = ns;
            AssemblyName = assemblyName;
        }

        public string Name { get; }
        public string Namespace { get; }
        public string AssemblyName { get; }

        protected bool Equals(MessageType other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Namespace, other.Namespace) && string.Equals(AssemblyName, other.AssemblyName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MessageType)obj);
        }

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