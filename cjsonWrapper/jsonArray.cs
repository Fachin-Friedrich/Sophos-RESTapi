
using System;

namespace csJson
{
    public struct jsonArray
    {
        internal unsafe void* handle_;

        internal unsafe jsonArray( void* arr)
        {
            handle_ = arr;
        }

        public jsonData this[UInt64 index]
        {
            get
            {                
                unsafe
                {
                    var sz = Elements;
                    if( index >= sz )
                    {
                        throw new ArgumentOutOfRangeException(
                            $"Trying to access JSON array with length {sz} at index {index}"
                        );
                    }

                    var adr = ImportedFunctions.cjsonGetArrayData(handle_, index);
                    return new jsonData(adr);
                }
            }
        }

        public UInt64 Elements
        {
            get
            {
                unsafe
                {
                    if( handle_ == null)
                    {
                        throw new NotImplementedException();
                    }

                    return ImportedFunctions.cjsonGetArraySize(handle_);
                }
            }
        }
    }
}