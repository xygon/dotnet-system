using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Stewsoft.Runtime.Caching
{
    public sealed class InMemeoryCloneCache : InMemoryCache
    {
        public InMemeoryCloneCache() :
            base((value, ttlSeconds) => new CacheItem(value, ttlSeconds))
        {
            
        }

        class CacheItem : ICacheItem
        {
            public CacheItem(object value, int ttlSeconds)
            {
                _valueType = value?.GetType();

                if (value == null || _valueType.IsPrimitive || value is Guid || value is DateTime || value is DateTimeOffset || _valueType.IsEnum)
                {
                    _cloneType = "None";
                    _value = value;
                }
                else
                {
                    var typeConverter = TypeDescriptor.GetConverter(_valueType);
                    if (typeConverter.CanConvertTo(typeof (string)) && typeConverter.CanConvertFrom(typeof (string)))
                    {
                        _cloneType = "TypeConverter";
                        _value = typeConverter.ConvertTo(value, typeof (string));
                    }
                    else
                    {
                        _cloneType = "Binary";
                        _value = GetBytes(value);
                    }

                }
                _expiryTime = DateTime.UtcNow + TimeSpan.FromSeconds(ttlSeconds);
            }

            private readonly object _value;
            private readonly DateTime _expiryTime;
            private readonly string _cloneType;
            private readonly Type _valueType;

            public TValue GetValue<TValue>()
            {
                var clone = _value;
                switch (_cloneType)
                {
                    case "None":
                        break;
                    case "TypeConverter":
                        clone = TypeDescriptor.GetConverter(_valueType).ConvertFrom(_value);
                        break;
                    case "Binary":
                        clone = FromBytes();
                        break;
                }
                return (TValue)clone;
            }

            public bool HasExpired()
            {
                return DateTime.UtcNow > _expiryTime;
            }

            private static byte[] GetBytes(object value)
            {
                MemoryStream ms = null;
                try
                {
                    var formatter = new BinaryFormatter();
                    ms = new MemoryStream();
                    formatter.Serialize(ms, value);
                    return ms.ToArray();

                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Value could not be binary serialized - value type={value.GetType()}", ex);
                }
                finally
                {
                    ms?.Dispose();
                }
            }

            private object FromBytes()
            {
                var source = _value as byte[];
                if (source == null)
                {
                    throw new InvalidOperationException("Underlying value was not a byte[]");
                }
                MemoryStream ms = null;
                try
                {
                    var formatter = new BinaryFormatter();
                    ms = new MemoryStream(source);
                    var value = formatter.Deserialize(ms);
                    return value;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Value could not be binary de-serialized", ex);
                }
                finally
                {
                    ms?.Dispose();
                }
            }

        }

    }
}