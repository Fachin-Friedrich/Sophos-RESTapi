
using System;

namespace csJson
{
    public class jsonRoot
    {
        private jsonData root_;

        private jsonRoot()
        {
            //not available
        }

        private jsonRoot( string raw)
        {
            unsafe
            {
                UInt32 e = 0;
                var adr = ImportedFunctions.cjsonParse(raw, (UInt64) raw.Length, &e);
                if( e == 0) //no error
                {
                    root_ = new jsonData(adr);
                }
                else
                {
                    if( adr != null)
                    {
                        ImportedFunctions.cjsonFree(adr);
                    }

                    throw new Exception(
                        $"Parsing error: {ImportedFunctions.cjsonGetErrorName(e)}"
                    );
                }
            }
        }

        ~jsonRoot()
        {
            unsafe
            {
                ImportedFunctions.cjsonFree(root_.handle_);
            }
        }

        public static jsonRoot Parse(string raw) => new jsonRoot(raw);

        public string Serialize() => root_.Serialize();

        public TypeId Type => root_.Type;
        public Int64 Integer => root_.Integer;
        public double Double => root_.Double;
        public bool Bool => root_.Bool;
        public string String => root_.String;
        public jsonObject Object => root_.Object;
        public jsonArray Array => root_.Array;

        public jsonData this[string fieldname] => Object[fieldname];
        public jsonData this[UInt64 index] => Array[index];

        public UInt64 Elements
        {
            get
            {
                var t = Type;
                switch (t)
                {
                    case TypeId.Object : return Object.Elements;
                    case TypeId.Array : return Array.Elements;
                    default: throw new TypeAccessException(
                        $"Trying to access JSON data of type {t} as a container type"
                    );
                }
            }
        }
    }
}