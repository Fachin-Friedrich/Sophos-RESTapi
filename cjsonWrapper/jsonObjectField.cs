
using System;
using System.Collections;
using System.Collections.Generic;

namespace csJson
{
    public struct jsonObjectField
    {
        internal unsafe void* handle_;
        internal jsonData data_;

        internal unsafe jsonObjectField( void* adr)
        {
            handle_ = adr;

            if( adr == null)
            {
                throw new NullReferenceException();
            }

            data_ = new jsonData(ImportedFunctions.cjsonGetObjectFieldData(adr));
        }

        public jsonData Value
        {
            get => data_;
        }

        public TypeId Type => data_.Type;

        public string Name
        {
            get
            {
                unsafe
                {
                    if( handle_ == null)
                    {
                        throw new NullReferenceException();
                    }

                    return ImportedFunctions.cjsonGetObjectFieldName(handle_);
                }
            }
        }
    }

    public class jsonObjectEnumerator : IEnumerator<jsonObjectField>
    {
        internal unsafe void* handle_;
        private UInt64 position_;
        private UInt64 size_;
        private jsonObjectField field_;

        internal jsonObjectEnumerator( jsonObject obj)
        {
            unsafe
            {
                if( obj.handle_ == null)
                {
                    throw new NullReferenceException();
                }
                
                handle_ = ImportedFunctions.cjsonGetObjectFields(obj.handle_);
                position_ = 0;
                size_ = obj.Elements;
                field_ = new jsonObjectField(ImportedFunctions.cjsonAccessObjectFields(handle_, 0));
            }
        }

        public jsonObjectField Current => field_;

        object IEnumerator.Current => field_;

        public void Dispose()
        {
            //TODO export cleanup to cjson.dll
            //throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            if( position_ < size_)
            {
                unsafe
                {
                    field_.handle_ = ImportedFunctions.cjsonAccessObjectFields(handle_, position_++);
                    field_.data_.handle_ = ImportedFunctions.cjsonGetObjectFieldData(field_.handle_);
                }

                return true;
            }

            return false;
        }

        public void Reset()
        {
            position_ = 0;
            unsafe
            {
                field_.handle_ = ImportedFunctions.cjsonAccessObjectFields(handle_, 0);
                field_.data_.handle_ = ImportedFunctions.cjsonGetObjectFieldData(field_.handle_);
            }
        }
    }
}