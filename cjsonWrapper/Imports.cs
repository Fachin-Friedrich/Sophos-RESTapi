
using System;
using System.Runtime.InteropServices;

namespace csJson
{
    public enum TypeId
    {
        Invalid = 0,
        Object = 1,
        Array = 2,
        String = 3,
        Integer = 4,
        Double = 5,
        Boolean = 6,
        Null = 7
    }

    public enum ErrorCodes
    {
        NoError = 0,
        ObjectEncapsulationFailure = 1,
        StringEncapsulationFailure = 2,
        ArrayEncapsulationFailure = 3,
        DataInconsistency = 4,
        Unkown = 5
    }

    internal class ImportedFunctions
    {
        const string NativeBinary = @"cjson.dll";

        //TODO fix complex object overwrites in cjson.dll

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void cjsonSetDataAsInt(void* handle, Int64 value);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void cjsonSetDataAsDouble(void* handle, double value);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void cjsonSetDataAsBool(void* handle, bool value);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static unsafe extern void cjsonSetDataAsString(void* handle, string value);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void cjsonSetDataAsArray(void* handle, void* arr);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void cjsonSetDataAsObject(void* handle, void* obj);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void cjsonCleanUpString(IntPtr adr);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cjsonSerialize")]
        private static unsafe extern IntPtr cjsonSerialize__(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* cjsonGetArrayData(void* handle, UInt64 index);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern UInt64 cjsonGetArraySize(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* cjsonGetObjectFieldData(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cjsonGetObjectFieldName")]
        private static unsafe extern IntPtr cjsonGetObjectFieldName__(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* cjsonGetObjectFields(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* cjsonAccessObjectFields(void* handle, UInt64 index);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static unsafe extern void* cjsonGetObjectField(void* handle, string fieldname);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern UInt64 cjsonGetObjectElementCount(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern UInt32 cjsonGetDataType(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern Int64 cjsonGetDataAsInt(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern double cjsonGetDataAsDouble(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern bool cjsonGetDataAsBool(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cjsonGetDataAsString")]
        private static unsafe extern IntPtr cjsonGetDataAsString__(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* cjsonGetDataAsObject(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void* cjsonGetDataAsArray(void* handle);

        [DllImport(dllName:NativeBinary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static unsafe extern void* cjsonParse(string raw, UInt64 raw_sz, UInt32* error );

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl)]
        public static unsafe extern void cjsonFree(void* handle);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cjsonGetTypeName")]
        private static unsafe extern IntPtr cjsonGetTypeName__(UInt32 x);

        [DllImport(dllName: NativeBinary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cjsonGetErrorName")]
        private static unsafe extern IntPtr cjsonGetErrorName__(UInt32 x);

        //--------------------------------------------------------------------------

        public static string cjsonGetErrorName( UInt32 errorcode)
        {
            unsafe
            {
                var ptr = cjsonGetErrorName__(errorcode);
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        public static string cjsonGetTypeName(UInt32 typeid)
        {
            unsafe
            {
                var ptr = cjsonGetTypeName__(typeid);
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        public static unsafe string cjsonGetDataAsString( void* handle)
        {
            var ptr = cjsonGetDataAsString__(handle);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static unsafe string cjsonGetObjectFieldName(void* handle)
        {
            var ptr = cjsonGetObjectFieldName__(handle);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static unsafe string cjsonSerialize(void* handle)
        {
            var adr = cjsonSerialize__(handle);
            string res = Marshal.PtrToStringAnsi(adr);
            cjsonCleanUpString(adr);
            return res;
        }
    }
}