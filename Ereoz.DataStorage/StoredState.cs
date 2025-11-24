using Ereoz.Abstractions.Logging;
using Ereoz.Abstractions.Serialization;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Ereoz.DataStorage
{
    /// <summary>
    /// Represents a base class for states that can be stored and loaded from a file.
    /// This class provides the fundamental functionality for managing the state of an object
    /// by serializing and deserializing it to and from a file.
    /// </summary>
    [Serializable]
    public abstract class StoredState : INotifyPropertyChanged
    {
        [NonSerialized]
        private readonly string _fileName;
        [NonSerialized]
        private readonly FileStorage _storage;
        [NonSerialized]
        private readonly Type _targetType;

        protected StoredState() : this(new Ereoz.Serialization.Json.SimpleJson(), null, null) { }

        protected StoredState(string fileName) : this(new Ereoz.Serialization.Json.SimpleJson(), fileName, null) { }

        protected StoredState(string fileName, ILogger logger) : this(new Ereoz.Serialization.Json.SimpleJson(), fileName, logger) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredState"/> class with a specified string serializer.
        /// The file name will be the type name of derived class with the extension:
        /// .json - if the serializer type name contains the substring "json";
        /// .xml - if the serializer type name contains the substring "xml";
        /// .soap - if the serializer type name contains the substring "soap";
        /// .yml - if the serializer type name contains the substring "yaml";
        /// otherwise .txt.
        /// </summary>
        /// <param name="serializer">The serializer to use for state operations.</param>
        protected StoredState(IStringSerializer serializer) : this(serializer, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredState"/> class with a specified binary serializer.
        /// The file name will be the type name of derived class with the extension:
        /// .bin - if the serializer type name contains the substring "binary";
        /// .ptb - if the serializer type name contains the substring "protobuf";
        /// .msp - if the serializer type name contains the substring "messagepack";
        /// otherwise .dat.
        /// </summary>
        /// <param name="serializer">The serializer to use for state operations.</param>
        protected StoredState(IBinarySerializer serializer) : this(serializer, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredState"/> class with a specified string serializer and file name.
        /// </summary>
        /// <param name="serializer">The serializer to use for state operations.</param>
        /// <param name="fileName">The name of the file to store or load the state from.</param>
        protected StoredState(IStringSerializer serializer, string fileName) : this(serializer, fileName, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredState"/> class with a specified binary serializer and file name.
        /// </summary>
        /// <param name="serializer">The serializer to use for state operations.</param>
        /// <param name="fileName">The name of the file to store or load the state from.</param>
        protected StoredState(IBinarySerializer serializer, string fileName) : this(serializer, fileName, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredState"/> class with a specified string serializer and logger.
        /// The file name will be the type name of derived class with the extension:
        /// .json - if the serializer type name contains the substring "json";
        /// .xml - if the serializer type name contains the substring "xml";
        /// .soap - if the serializer type name contains the substring "soap";
        /// .yml - if the serializer type name contains the substring "yaml";
        /// otherwise .txt.
        /// </summary>
        /// <param name="serializer">The serializer to use for state operations.</param>
        /// <param name="logger">The logger to use for logging events and errors.</param>
        protected StoredState(IStringSerializer serializer, ILogger logger) : this(serializer, null, logger) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredState"/> class with a specified binary serializer and logger.
        /// The file name will be the type name of derived class with the extension:
        /// .bin - if the serializer type name contains the substring "binary";
        /// .ptb - if the serializer type name contains the substring "protobuf";
        /// .msp - if the serializer type name contains the substring "messagepack";
        /// otherwise .dat.
        /// </summary>
        /// <param name="serializer">The serializer to use for state operations.</param>
        /// <param name="logger">The logger to use for logging events and errors.</param>
        protected StoredState(IBinarySerializer serializer, ILogger logger) : this(serializer, null, logger) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredState"/> class with a specified string serializer, file name, and logger.
        /// </summary>
        /// <param name="serializer">The serializer to use for state operations.</param>
        /// <param name="fileName">The name of the file to store or load the state from.</param>
        /// <param name="logger">The logger to use for logging events and errors.</param>
        protected StoredState(IStringSerializer serializer, string fileName, ILogger logger)
        {
            _storage = new FileStorage(serializer, logger);
            _targetType = this.GetType();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                _fileName = _targetType.Name;

                if (_fileName.EndsWith("`1"))
                    _fileName = _fileName.Substring(0, _fileName.Length - 2);

                string serializerName = serializer.GetType().Name.ToLower();

                if (serializerName.Contains("json"))
                    _fileName += ".json";
                else if (serializerName.Contains("xml"))
                    _fileName += ".xml";
                else if (serializerName.Contains("soap"))
                    _fileName += ".soap";
                else if (serializerName.Contains("yaml"))
                    _fileName += ".yml";
                else
                    _fileName += ".txt";
            }
            else
            {
                _fileName = fileName;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoredState"/> class with a specified binary serializer, file name, and logger.
        /// </summary>
        /// <param name="serializer">The serializer to use for state operations.</param>
        /// <param name="fileName">The name of the file to store or load the state from.</param>
        /// <param name="logger">The logger to use for logging events and errors.</param>
        protected StoredState(IBinarySerializer serializer, string fileName, ILogger logger)
        {
            _storage = new FileStorage(serializer, logger);
            _targetType = this.GetType();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                _fileName = _targetType.Name;

                if (_fileName.EndsWith("`1"))
                    _fileName = _fileName.Substring(0, _fileName.Length - 2);

                string serializerName = serializer.GetType().Name.ToLower();

                if (serializerName.Contains("binary"))
                    _fileName += ".bin";
                else if (serializerName.Contains("protobuf"))
                    _fileName += ".ptb";
                else if (serializerName.Contains("messagepack"))
                    _fileName += ".msp";
                else
                    _fileName += ".dat";
            }
            else
            {
                _fileName = fileName;
            }
        }

        /// <summary>
        /// Saves the current state of the object to the associated file.
        /// </summary>
        /// <returns><see langword="true"/> if the state was saved successfully; otherwise, <see langword="false"/>.</returns>
        public bool SaveState() =>
            _storage.Save(_fileName, this);

        /// <summary>
        /// Loads the state of the object from the associated file.
        /// </summary>
        /// <returns><see langword="true"/> if the state was loaded successfully; otherwise, <see langword="false"/>.</returns>
        public bool LoadState()
        {
            object loadedData = _storage.Load(_fileName, _targetType);

            if (loadedData != null)
            {
                var loadedProps = loadedData.GetType().GetProperties();

                foreach (var prop in _targetType.GetProperties())
                {
                    PropertyInfo loadedProp = loadedProps.Where(p => p.Name == prop.Name).SingleOrDefault();

                    if (loadedProp != null)
                    {
                        prop.SetValue(this, loadedProp.GetValue(loadedData, null), null);
                    }
                }

                LoadComplete();
                return true;
            }
            else
            {
                LoadFail();
                return false;
            }
        }

        /// <summary>
        /// This method is called when an attempt to load the state fails.
        /// Derived classes can override this method to provide custom failure handling.
        /// </summary>
        protected virtual void LoadFail() { }

        /// <summary>
        /// This method is called after the state has been successfully loaded.
        /// Derived classes can override this method to perform actions after loading,
        /// such as updating UI elements or initializing other components.
        /// </summary>
        protected virtual void LoadComplete() { }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
