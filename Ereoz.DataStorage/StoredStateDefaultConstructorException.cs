using System;

namespace Ereoz.DataStorage
{
    /// <summary>
    /// Exception that is thrown when an instance of a type based on <see cref="StoredState" /> has been created with a default constructor.
    /// </summary>
    [Serializable]
    public sealed class StoredStateDefaultConstructorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoredStateDefaultConstructorException"/> class.
        /// </summary>
        public StoredStateDefaultConstructorException(Type type)
            : base($"When creating an instance of \"{type}\", you cannot use the constructor without parameters, since it has a service purpose (it is needed for some serializers to work). You must pass at least an instance of the serializer to the constructor.")
        {
        }
    }
}
