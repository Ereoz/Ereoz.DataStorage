using Ereoz.Abstractions.Logging;
using Ereoz.Abstractions.Serialization;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace Ereoz.DataStorage
{
    /// <summary>
    /// Provides a thread-safe wrapper for serializing and deserializing objects to and from files (only properties).
    /// This class allows storing serialized representations of objects, typically as strings or byte arrays, in the file system.
    /// </summary>
    public sealed class FileStorage
    {
        private static readonly object _locker = new object();

        private readonly ILogger _logger;
        private readonly IStringSerializer _stringSerializer;
        private readonly IBinarySerializer _binarySerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStorage"/> with a specified string serializer.
        /// </summary>
        /// <param name="serializer">The serializer to use for converting objects to and from their serialized form.</param>
        public FileStorage(IStringSerializer serializer) : this(serializer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStorage"/> with a specified binary serializer.
        /// </summary>
        /// <param name="serializer">The serializer to use for converting objects to and from their serialized form.</param>
        public FileStorage(IBinarySerializer serializer) : this(serializer, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStorage"/> with a specified string serializer and logger.
        /// </summary>
        /// <param name="serializer">The serializer to use for converting objects to and from their serialized form.</param>
        /// <param name="logger">An optional logger for recording events and errors. If null, a <see cref="NullLogger"/> will be used.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FileStorage(IStringSerializer serializer, ILogger logger)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer), "Serializer cannot be null.");

            _stringSerializer = serializer;
            _logger = logger ?? new NullLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStorage"/> with a specified binary serializer and logger.
        /// </summary>
        /// <param name="serializer">The serializer to use for converting objects to and from their serialized form.</param>
        /// <param name="logger">An optional logger for recording events and errors. If null, a <see cref="NullLogger"/> will be used.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FileStorage(IBinarySerializer serializer, ILogger logger)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer), "Serializer cannot be null.");

            _binarySerializer = serializer;
            _logger = logger ?? new NullLogger();
        }

        /// <summary>
        /// Serializes an object and saves it to a specified file. This operation is thread-safe.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="fileName">The name of the file to save the serialized object to.</param>
        /// <param name="value">The object to serialize and save.</param>
        /// <returns><see langword="true"/> if the object was successfully serialized and saved; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileName"/> is null or empty.</exception>
        public bool Save<T>(string fileName, T value)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName), "File name cannot be null or empty.");

            var valueType = typeof(T);

            try
            {
                lock (_locker)
                {
                    if (_stringSerializer != null)
                    {
                        object cloneObject = FormatterServices.GetUninitializedObject(value.GetType());
                        foreach (var prop in value.GetType().GetProperties())
                            prop.SetValue(cloneObject, prop.GetValue(value, null), null);

                        var serializedObject = _stringSerializer.Serialize(cloneObject, true);
                        File.WriteAllText(fileName, serializedObject);
                    }
                    else
                    {
                        var serializedObject = _binarySerializer.Serialize(value);
                        File.WriteAllBytes(fileName, serializedObject);
                    }

                    _logger.Info("Successfully saved object of type {0} to file: {1}", typeof(T).Name, fileName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save object of type {0} to file: {1}", typeof(T).Name, fileName);
                return false;
            }
        }

        /// <summary>
        /// Loads and deserializes an object from a specified file. This operation is thread-safe.
        /// </summary>
        /// <param name="fileName">The name of the file to load the serialized object from.</param>
        /// <param name="targetType">The type of the object to load and deserialize.</param>
        /// <returns>The deserialized object, or null if the file does not exist or an error occurs during loading/deserialization.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="targetType"/> or <paramref name="fileName"/> is null or empty.</exception>
        public object Load(string fileName, Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType), "Target type cannot be null.");

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName), "File name cannot be null or empty.");

            try
            {
                lock (_locker)
                {
                    if (_stringSerializer != null)
                    {
                        var serializedObject = File.ReadAllText(fileName);
                        object originalObject = _stringSerializer.Deserialize(serializedObject, targetType);

                        _logger.Info("Successfully loaded object of type {0} from file: {1}", targetType.Name, fileName);
                        return originalObject;
                    }
                    else
                    {
                        var serializedObject = File.ReadAllBytes(fileName);
                        object originalObject = _binarySerializer.Deserialize(serializedObject, targetType);

                        _logger.Info("Successfully loaded object of type {0} from file: {1}", targetType.Name, fileName);
                        return originalObject;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load object of type {0} from file: {1}", targetType.Name, fileName);
                return null;
            }
        }

        /// <summary>
        /// Loads and deserializes an object of a specific type from a specified file. This operation is thread-safe.
        /// </summary>
        /// <typeparam name="T">The type of the object to load and deserialize.</typeparam>
        /// <param name="fileName">The name of the file to load the serialized object from.</param>
        /// <returns>The deserialized object, or null if the file does not exist or an error occurs during loading/deserialization.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="fileName"/> is null or empty.</exception>
        public T Load<T>(string fileName)
        {
            object result = Load(fileName, typeof(T));

            if (result != null)
                return (T)result;
            else
                return default;
        }
    }
}
