using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ReeperCommon
{

    /// <summary>
    /// 
    /// </summary>
    public class ShaderProperty
    {
        public enum PropertyType
        {
            String,
            Float,
            Color,
            Texture,
            Matrix,
            Unknown
        }

        private string rawString = string.Empty;


        private static bool IdentifyType(string raw, out PropertyType type)
        {
            type = PropertyType.Unknown;

            // the right format is:
            // TypeId, ParamID, {string representation of type}
            if (string.IsNullOrEmpty(raw))
                return false;

            if (raw.Count(c => c == ',') < 2)
                return false;
            
            var index = raw.IndexOf(',');
            List<string> goodIds = Enum.GetNames(typeof(PropertyType)).Except(new List<string>{"Unknown"}).ToList();

            string sType = raw.Substring(0, index).Trim();

            if (goodIds.Contains(sType))
            {
                try
                {
                    type = (PropertyType)Enum.Parse(typeof(PropertyType), sType);
                    return true;
                }
                catch
                {
                    // intentionally left empty
                }
            }

            Log.Error("'{0}' from {1} is not a valid shader property type", sType, raw);
            return false;
        }

        private static bool GetPropertyName(string raw, out string propertyName)
        {
            propertyName = string.Empty;

            if (string.IsNullOrEmpty(raw))
                return false;

            if (raw.Count(c => c == ',') < 2)
                return false;

            int firstComma = raw.IndexOf(',');
            int secondComma = raw.IndexOf(',', firstComma + 1);

            propertyName = raw.Substring(firstComma + 1, secondComma - firstComma - 2).Trim();

            Log.Debug("GetPropertyName from {0} yields {1}", raw, propertyName);

            return !string.IsNullOrEmpty(propertyName);
        }

        private static bool GetPropertyValue(string raw, out string value)
        {
            value = string.Empty;

            if (string.IsNullOrEmpty(raw))
                return false;

            if (raw.Count(c => c == ',') < 2)
                return false;

            int start = raw.IndexOf(',', raw.IndexOf(',') + 1);

            value = raw.Substring(start + 1).Trim();

            return !string.IsNullOrEmpty(value);
        }

        public ShaderProperty(string raw)
        {
            rawString = raw;

            PropertyType param;

            if (!IdentifyType(raw, out param))
                throw new Exception(string.Format("Could not identify shader parameter type from '{0}'", raw));

            Type = param;
        }

        public bool ApplyTo(Material material)
        {
            string propertyName;
            string propertyValue;

            if (Type == PropertyType.Unknown)
            {
                Log.Error("Cannot apply an unknown shader parameter type.");
                return false;
            }

            if (!GetPropertyName(rawString, out propertyName) || !(GetPropertyValue(rawString, out propertyValue)))
            {
                Log.Error("Failed to identify parameter name or value from {0}", rawString);
                return false;
            }

            switch (Type)
            {
                case PropertyType.Color:
                    var v4 = KSPUtil.ParseVector4(propertyValue);
                    material.SetColor(propertyName, new Color(v4.x, v4.y, v4.z, v4.w));
                    return true;

                case PropertyType.Float:
                    float f = 0f;
                    if (float.TryParse(propertyValue, out f))
                    {
                        material.SetFloat(propertyName, f);
                        return true;
                    }
                    else
                    {
                        Log.Error("Could not parse float from {0}", propertyValue);
                        return false;
                    }

                default:
                    Log.Error("Shader property type '{0}' is not supported at this time.", Type);
                    break;
            }
            return false;
        }

        public PropertyType Type
        {
            get;
            private set;
        }
    }



    public class ShaderProperties
    {
        public List<ShaderProperty> properties = new List<ShaderProperty>();

        public ShaderProperties(ConfigNode node)
        {
            foreach (var entry in node.values.DistinctNames())
            {
                try
                {
                    var newProperty = new ShaderProperty(node.GetValue(entry));
                    properties.Add(newProperty);
                } catch (Exception)
                {
                    // left blank intentionally
                }
            }

        }

        public bool ApplyTo(Material material)
        {
            return properties.TrueForAll(p => p.ApplyTo(material));
        }
    }


}
