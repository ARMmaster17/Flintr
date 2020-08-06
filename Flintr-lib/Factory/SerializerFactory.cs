using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Flintr_lib.Factory
{
    /// <summary>
    /// Responsible for the creation and caching of serializers for network transmission.
    /// </summary>
    public class SerializerFactory
    {
        private Dictionary<Type, XmlSerializer> serializerCache;

        /// <summary>
        /// Default constructor for SerializerFactory. Initializes a serializer cache.
        /// </summary>
        public SerializerFactory()
        {
            serializerCache = new Dictionary<Type, XmlSerializer>();
        }

        /// <summary>
        /// Serializes a specified object into a read-only local data stream.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>Read-only local stream in memory containing the serialized data.</returns>
        public MemoryStream SerializeToStream<T>(object obj)
        {
            XmlSerializer xmlSerializer = getSerializer<T>();

            // Create a MemoryStream so we can get the message length for the header information.
            MemoryStream ms = new MemoryStream();

            // Serialize the object to XML and write it to the stream.
            try
            {
                xmlSerializer.Serialize(ms, obj);
            }
            catch (Exception e)
            {
                throw new IOException($"Object of type {typeof(T).Name} could not be serialized", e);
            }

            return ms;
        }

        /// <summary>
        /// Deserializes and object of the specified type from a read-only copy of a segment of a data stream.
        /// </summary>
        /// <typeparam name="T">Expected type of incoming network object.</typeparam>
        /// <param name="ms">Read-only copy of data stream segment containing the serialized object data.</param>
        /// <returns>Deserialized object of type T.</returns>
        public T Deserialize<T>(MemoryStream ms)
        {
            // We have to use a MemoryStream because the NetworkStream can't tell us that
            // the object buffer is complete, so the XmlSerializer will hang if we just pass
            // the NetworkStream.

            XmlSerializer xmlSerializer = getSerializer<T>();

            object o;
            try
            {
                o = xmlSerializer.Deserialize(ms);
            }
            catch (Exception e)
            {
                throw new IOException($"Incoming network object could not be deserialized to a valid object.", e);
            }

            try
            {
                return (T)o;
            }
            catch (Exception e)
            {
                throw new IOException($"Incoming deserialized network object could not be converted to type {typeof(T).Name}.", e);
            }
        }

        /// <summary>
        /// Retrieves a copy of a serializer of specified type from cache, or creates one if it does not exist.
        /// </summary>
        /// <typeparam name="T">Type of serializer to search for.</typeparam>
        /// <returns>Serializer that is set up to work with objects of the specified type.</returns>
        private XmlSerializer getSerializer<T>()
        {
            if (serializerCache.ContainsKey(typeof(T))) return serializerCache[typeof(T)];

            return createSerializer<T>();
        }

        /// <summary>
        /// Creates a serializer that is initialized to work with objects of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of object that serializer should be initialized to support.</typeparam>
        /// <returns>Copy of serializer that has been added to the cache and initialized to support specified type.</returns>
        private XmlSerializer createSerializer<T>()
        {
            XmlSerializer xmlSerializer;
            try
            {
                xmlSerializer = new XmlSerializer(typeof(T));
            }
            catch (Exception e)
            {
                throw new ArgumentException($"An error occurred initializing a serializer for objects of type {typeof(T).Name}.", e);
            }

            serializerCache.Add(typeof(T), xmlSerializer);

            return xmlSerializer;
        }
    }
}
