
using System;

namespace csJson
{
    public struct jsonData
    {
        internal unsafe void* handle_;

        internal unsafe jsonData( void* data)
        {
            handle_ = data;
        }

        public string Serialize()
        {
            unsafe
            {
                if( handle_ == null)
                {
                    throw new NullReferenceException();
                }

                return ImportedFunctions.cjsonSerialize(handle_);
            }
        }

        public TypeId Type
        {
            get
            {
                unsafe
                {
                    if( handle_ == null)
                    {
                        throw new NullReferenceException();
                    }

                    return (TypeId) ImportedFunctions.cjsonGetDataType(handle_);
                }
            }
        }

        public Int64 Integer
        {
            get
            {
                var t = Type;
                if (t != TypeId.Integer)
                {
                    throw new TypeAccessException(
                        $"Trying to access JSON data of type {t} as integer"
                    );
                }

                unsafe
                {
                    return ImportedFunctions.cjsonGetDataAsInt(handle_);
                }
            }

            set
            {
                unsafe
                {
                    if( handle_ == null)
                    {
                        throw new NullReferenceException();
                    }

                    ImportedFunctions.cjsonSetDataAsInt(handle_,value);
                }
            }
        }

        public double Double
        {
            get
            {
                var t = Type;
                if (t != TypeId.Double)
                {
                    throw new TypeAccessException(
                        $"Trying to access JSON data of type {t} as double"
                    );
                }

                unsafe
                {
                    return ImportedFunctions.cjsonGetDataAsDouble(handle_);
                }
            }

            set
            {
                unsafe
                {
                    if (handle_ == null)
                    {
                        throw new NullReferenceException();
                    }

                    ImportedFunctions.cjsonSetDataAsDouble(handle_,value);
                }
            }
        }

        public bool Bool
        {
            get
            {
                var t = Type;
                if (t != TypeId.Boolean)
                {
                    throw new TypeAccessException(
                        $"Trying to access JSON data of type {t} as boolean"
                    );
                }

                unsafe
                {
                    return ImportedFunctions.cjsonGetDataAsBool(handle_);
                }
            }

            set
            {
                unsafe
                {
                    if (handle_ == null)
                    {
                        throw new NullReferenceException();
                    }

                    ImportedFunctions.cjsonSetDataAsBool(handle_,value);
                }
            }
        }

        public string String
        {
            get
            {
                var t = Type;
                if (t != TypeId.String)
                {
                    throw new TypeAccessException(
                        $"Trying to access JSON data of type {t} as String"
                    );
                }

                unsafe
                {
                    return ImportedFunctions.cjsonGetDataAsString(handle_);
                }
            }

            set
            {
                unsafe
                {
                    if (handle_ == null)
                    {
                        throw new NullReferenceException();
                    }

                    ImportedFunctions.cjsonSetDataAsString(handle_,value);
                }
            }
        }

        public jsonObject Object
        {
            get
            {
                var t = Type;
                if (t != TypeId.Object)
                {
                    throw new TypeAccessException(
                        $"Trying to access JSON data of type {t} as object"
                    );
                }

                unsafe
                {
                    var adr = ImportedFunctions.cjsonGetDataAsObject(handle_);
                    return new jsonObject(adr);
                }
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public jsonArray Array
        {
            get
            {
                var t = Type;
                if (t != TypeId.Array)
                {
                    throw new TypeAccessException(
                        $"Trying to access JSON data of type {t} as array"
                    );
                }

                unsafe
                {
                    var adr = ImportedFunctions.cjsonGetDataAsArray(handle_);
                    return new jsonArray(adr);
                }
            }

            set
            {
                throw new NotImplementedException();
            }
        }

    }
}
