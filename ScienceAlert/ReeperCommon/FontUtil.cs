/**
 * 
 *  IMPORTANT
 *  
 *  currently nonfunctional since I haven't found a way to specify character
 *  kerning or spacing for a custom Font instance
 * 
 * 
 * 
 */
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary; 
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace ReeperCommon
{
    // somewhat based off of http://wiki.unity3d.com/index.php?title=SaveFontTexture
    public class FontUtil : MonoBehaviour
    {
        private class PackedFont
        {
            public string name;
            public byte[] texture;              
            public CharacterInfo[] chars;
        }

        #region serialization surrogates


        // this little nugget required because the BinaryFormatter seems to
        // include the assembly name when serializing objects ... and of course
        // that's a problem because Assembly-CSharp-Editor isn't available
        // in the game runtime
        sealed class CurrentAssemblyDeserializationBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                return Type.GetType(String.Format("{0}, {1}", typeName, System.Reflection.Assembly.GetExecutingAssembly().FullName));
            }
        }

        sealed class PackedFontSurrogate : ISerializationSurrogate
        {
            public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
            {
                PackedFont f = (PackedFont)obj;

                info.AddValue("name", f.name);
                info.AddValue("chCount", f.chars.Length);

                for (int i = 0; i < f.chars.Length; ++i)
                    info.AddValue(i.ToString(), f.chars[i]);

                info.AddValue("texture", System.Convert.ToBase64String(f.texture));
            }

            // Deserialize the Employee object to set the object�s name and address fields. 
            public System.Object SetObjectData(System.Object obj,
                                        SerializationInfo info,
                                        StreamingContext context,
                                        ISurrogateSelector selector)
            {
                PackedFont f = (PackedFont)obj;

                f.name = info.GetString("name");

                int chCount = info.GetInt32("chCount");
                f.chars = new CharacterInfo[chCount];

                for (int i = 0; i < f.chars.Length; ++i)
                    f.chars[i] = (CharacterInfo)info.GetValue(i.ToString(), typeof(CharacterInfo));


                string b64 = info.GetString("texture");

                var bytes = System.Convert.FromBase64String(b64);
                f.texture = bytes;

                return null;
            }
        }



        sealed class CharInfoSurrogate : ISerializationSurrogate
        {
            //[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
            public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
            {
                CharacterInfo ci = (CharacterInfo)obj;
                info.AddValue("index", ci.index);
                info.AddValue("size", ci.size);
                info.AddValue("style", ci.style);
                info.AddValue("uv", ci.uv);
                info.AddValue("vert", ci.vert);
                info.AddValue("width", ci.width);
            }

            // Deserialize the Employee object to set the object�s name and address fields. 
            public System.Object SetObjectData(System.Object obj, 
                                        SerializationInfo info, 
                                        StreamingContext context,
                                        ISurrogateSelector selector)
            {
                CharacterInfo ci = (CharacterInfo)obj;
                ci.index = info.GetInt32("index");
                ci.size = info.GetInt32("size");
                ci.style = (FontStyle)info.GetInt32("style");
                ci.uv = (Rect)info.GetValue("uv", typeof(Rect));
                ci.vert = (Rect)info.GetValue("vert", typeof(Rect));
                ci.width = info.GetSingle("width");

                return ci;
            }
        }



        sealed class RectSurrogate : ISerializationSurrogate
        {
            public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
            {
                Rect r = (Rect)obj;
                info.AddValue("x", r.x);
                info.AddValue("y", r.y);
                info.AddValue("width", r.width);
                info.AddValue("height", r.height);
            }

            public System.Object SetObjectData(System.Object obj,
                                        SerializationInfo info,
                                        StreamingContext context,
                                        ISurrogateSelector selector)
            {
                Rect r = (Rect)obj;
                r.x = info.GetSingle("x");
                r.y = info.GetSingle("y");
                r.width = info.GetSingle("width");
                r.height = info.GetSingle("height");

                return r;
            }
        }



        private static BinaryFormatter CreateBinaryFormatter()
        {
            SurrogateSelector ss = new SurrogateSelector();
            ss.AddSurrogate(typeof(PackedFont), new StreamingContext(StreamingContextStates.All), new PackedFontSurrogate());
            ss.AddSurrogate(typeof(CharacterInfo), new StreamingContext(StreamingContextStates.All), new CharInfoSurrogate());
            ss.AddSurrogate(typeof(Rect), new StreamingContext(StreamingContextStates.All), new RectSurrogate());

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.SurrogateSelector = ss;

            formatter.Binder = new CurrentAssemblyDeserializationBinder();
            return formatter;
        }

        #endregion


#if UNITY_EDITOR
        [MenuItem("Tools/Export KSP Font...")]
        static void Init()
	    {
            var selFonts = Selection.GetFiltered(typeof(Font), SelectionMode.Unfiltered);
            var selTextures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Unfiltered);



            if (selFonts.Length != 1 || selTextures.Length != 1)
            {
                Debug.LogError("Requires one Font and one Texture2D selected!");
                EditorUtility.DisplayDialog("Invalid selection", "You need to multi-select one Font and one Texture", "Cancel");
                return;
            }
            else
            {
                Font font = (Font)selFonts[0];
                PackedFont packed = new PackedFont();
                packed.name = font.name;

                Debug.Log("Selected font: " + font.name);
                

                // grab texture. Note: unfortunately we can't just grab the
                // font material's texture directly because it's not readable
                // by default and the trick I use to make them readable would
                // require unity pro. Having the user include the correct texture
                // was the best solution I could come up with
                Texture2D tex = (Texture2D)selTextures[0];
                packed.texture = tex.EncodeToPNG();

                // grab font metrics
                packed.chars = font.characterInfo;

                string filename = EditorUtility.SaveFilePanel("Export KSP Font", "", "kspfont", "kfnt");

                if (filename.Length > 0)
                {
                    using (Stream stream = File.Open(filename, FileMode.OpenOrCreate))
                    {
                        Debug.Log("Creating stream and surrogates...");
                        var formatter = CreateBinaryFormatter();

                        Debug.Log("Serializing...");

                        try
                        {
                            formatter.Serialize(stream, packed);
                            Debug.Log(string.Format("Exported KSP font to {0} successfully", filename));
                        } 
                        catch (SerializationException e)
                        {
                            Debug.LogException(e);
                            Debug.LogError("Failed to export KSP font!");
                        }

                        stream.Close();
                    }
                }
            }
	    }

#else


        public static Font LoadFont(string path, bool relativeToGameData = true)
        {
            if (relativeToGameData)
                path = KSPUtil.ApplicationRootPath + "GameData" + (path.StartsWith("/") ? "" : "/") + path;

            if (!File.Exists(path))
            {
                Log.Error("Failed to find '{0}'", path);
                return null;
            }

            using (Stream stream = File.Open(path, FileMode.Open))
            {
                if (stream == null)
                {
                    Log.Error("LoadFont: failed to open stream with '{0}'", path);
                    return null;
                }

                BinaryFormatter formatter = CreateBinaryFormatter();
                PackedFont packed = new PackedFont();
                
                try
                {
                    Log.Verbose("Deserializing font...");
                    packed = (PackedFont)formatter.Deserialize(stream);
                    Log.Verbose("Done");
                } catch (SerializationException e)
                {
                    Log.Error("Failed to deserialize '{0}': {1}", path, e);
                    return null;
                }

                // PackedFont->Font
                try 
                {
                    Log.Verbose("Unpacking font '{0}' from '{1}' ...", packed.name, path);

                    Font font = new Font(packed.name);
                    Texture2D fontTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

                    font.characterInfo = packed.chars;
                    fontTexture.LoadImage(packed.texture); fontTexture.Apply();

                    font.material = new Material(Shader.Find("GUI/Text Shader")) { mainTexture = fontTexture, color = Color.white };

                    Log.Verbose("Unpacked; {0} characters in font", font.characterInfo.Length);

                    return font;
                } catch (Exception e)
                {
                    Log.Error("Exception while unpacking font: {0}", e);
                    return null;
                }
            }
        }
#endif
    }
}