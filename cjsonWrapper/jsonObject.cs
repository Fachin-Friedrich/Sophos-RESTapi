
using System;
using System.Collections;
using System.Collections.Generic;

namespace csJson
{

    public struct jsonObject : IEnumerable<jsonObjectField>
    {
        internal unsafe void* handle_;

        internal unsafe jsonObject( void* obj)
        {
            handle_ = obj;
        }

        public UInt64 Elements
        {
            get
            {
                unsafe
                {
                    if( handle_ == null)
                    {
                        throw new NullReferenceException();
                    }

                    return ImportedFunctions.cjsonGetObjectElementCount(handle_);
                }
            }
        }

        public jsonData this[string fieldname]
        {
            get
            {
                unsafe
                {
                    if(handle_ == null)
                    {
                        throw new NullReferenceException();
                    }

                    var adr = ImportedFunctions.cjsonGetObjectField(handle_, fieldname);
                    if( adr == null)
                    {
                        throw new KeyNotFoundException(
                            $"JSON object had no field called \"{fieldname}\""
                        ); ;
                    }

                    return new jsonData(adr);
                }
            }
        }

        public IEnumerator<jsonObjectField> GetEnumerator()
        {
            return new jsonObjectEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
