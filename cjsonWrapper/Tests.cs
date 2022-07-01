
using System;

namespace csJson
{
    public class Tests
    {
        public static void testObjectEnumeration( jsonObject obj)
        {
            unsafe
            {
                var en = ImportedFunctions.cjsonGetObjectFields(obj.handle_);
                for( UInt64 i = 0; i < obj.Elements; ++i)
                {
                    var field = ImportedFunctions.cjsonAccessObjectFields(en, i);
                    var name = ImportedFunctions.cjsonGetObjectFieldName(field);
                    var dt = new jsonData(ImportedFunctions.cjsonGetObjectFieldData(field));

                    Console.WriteLine($"Fieldname={name}  Type={dt.Type}");
                }
            }
        }
        
        public static void testObjectEnumerator( jsonObject obj)
        {
            foreach( var field in obj)
            {
                Console.WriteLine(
                    $"Field={field.Name}  Type={field.Type}"
                );
            }
        }

        public static void testEnums()
        {
            for( UInt32 i = 0; i < 16; ++i)
            {
                Console.WriteLine($"Index={i}");
                var tp = ImportedFunctions.cjsonGetTypeName(i);
                Console.WriteLine($"Type={tp}");
                var er = ImportedFunctions.cjsonGetErrorName(i);
                Console.WriteLine($"Error={er}");

                Console.WriteLine();
            }

        }
    }
}