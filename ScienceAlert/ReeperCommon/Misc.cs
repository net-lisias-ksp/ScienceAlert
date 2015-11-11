using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace ReeperCommon
{
    public delegate void FieldTraverseDelegate(object obj, FieldInfo field);

    internal static class StringDumper
    {
        public static string GetKSPString(int idx)
        {
            string text = "QXNzZW1ibHktQ1NoYXJwQXNzZW1ibHktQ1NoYXJw";
            byte[] array = Convert.FromBase64String(text);
            text = Encoding.UTF8.GetString(array, 0, array.Length);

            var manifestResourceStream = AssemblyLoader.loadedAssemblies.Single(la => la.dllName == "KSP").assembly.GetManifestResourceStream(text);

            return GetString(idx, DecodeStream(manifestResourceStream));
        }




        private static string GetString(int idx, byte[] SOH)
        {
            int num = 0;

            if ((SOH[idx] & 128) == 0)
            {
                num = SOH[idx];
                idx++;
            }
            else
            {
                if ((SOH[idx] & 64) == 0)
                {
                    num = (SOH[idx] & -129) << 8;
                    num |= SOH[idx + 1];
                    idx += 2;
                }
                else
                {
                    num = (SOH[idx] & -193) << 24;
                    num |= SOH[idx + 1] << 16;
                    num |= SOH[idx + 2] << 8;
                    num |= SOH[idx + 3];
                    idx += 4;
                }
            }

            if (num < 1)
                return string.Empty;

            string @string = Encoding.Unicode.GetString(SOH, idx, num);
            return string.Intern(@string);
        }



        private static byte[] DecodeStream(Stream stream)
        {
            byte b = (byte)stream.ReadByte();
            b = (byte)~b;

            for (int i = 1; i < 2; i++)
                stream.ReadByte();

            byte[] array = new byte[stream.Length - stream.Position];
            stream.Read(array, 0, array.Length);

            if ((b & 32) != 0)
                for (int j = 0; j < array.Length; j++)
                    array[j] = (byte)~array[j];

            return array;
        }
    }

    internal static class FieldDumper
    {
        public static object GetFieldValue(object obj, string fieldName, BindingFlags flags)
        {
            var field = obj.GetType().GetField(fieldName, flags);

            if (field == null)
            {
                Log.Warning("Could not get field from {0}", obj.ToString());
                return null;
            }
            else
            {
                return field.GetValue(obj);
            }
        }

        public static T GetFieldValue<T>(object obj, string fieldName, BindingFlags flags)
        {
            foreach (var field in obj.GetType().GetFields(flags))
                if (field.Name == fieldName && field.FieldType == typeof(T))
                    return (T)field.GetValue(obj);

            Log.Warning("Could not find field {0}", fieldName);
            return default(T);
        }

        public static void TraverseFieldsOfType<T>(object obj, BindingFlags flags, FieldTraverseDelegate ft)
        {
            foreach (var field in obj.GetType().GetFields(flags))
                if (field.FieldType == typeof(T))
                    ft(obj, field);
        }
    }
}
