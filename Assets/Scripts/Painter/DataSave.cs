using System.IO;
using UnityEngine;

namespace Painter
{
    public class DataSave
    {
        public void SaveTextureToFile(Texture2D texture, string fileName)
        {
            var bytes=texture.EncodeToPNG();
            var file = File.Open(Application.dataPath + "/"+fileName,FileMode.Create);
            var binary= new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();
        }
    }
}