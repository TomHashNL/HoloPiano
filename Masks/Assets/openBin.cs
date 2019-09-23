using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class openBin : MonoBehaviour
{
    // Start is called before the first frame update

    public readonly static int w = 448;
    public readonly static int h = 450;
    public readonly static int d = 2;
    float[] uv = new float[w * h * d];
    string[] linesDepth = new string[0];
    string[] linesReflectivity = new string[0];
    int index = 1;
    string[] headersD;
    string[] headersR;
    string folder = @"C:\Users\t.aalbers\source\repos\datasets\22\";

    Texture2D texMask;
    Texture2D texCheck;
    Texture2D texReflectivity;

    public GameObject raytraceCamera;
    void Start()
    {
        ////unzip files!
        //var folderPGMDepth = $"{folder}short_throw_depth/";
        //if (!Directory.Exists(folderPGMDepth))
        //    System.IO.Compression.ZipFile.ExtractToDirectory($"{folder}short_throw_depth.zip", folderPGMDepth);
        //var folderPGMReflectivity = $"{folder}short_throw_reflectivity/";
        //if (!Directory.Exists(folderPGMReflectivity))
        //    System.IO.Compression.ZipFile.ExtractToDirectory($"{folder}short_throw_reflectivity.zip", folderPGMReflectivity);

        ////PGM to PNG
        //var folderDepth = $"{folder}depth/";
        //if (!Directory.Exists(folderDepth))
        //{
        //    var headerLength = 17;
        //    foreach (var file in Directory.EnumerateFiles(folderPGMDepth))
        //    {
        //        var bytes = File.ReadAllBytes(file);

        //        var texx = new Texture2D(w, h, UnityEngine.TextureFormat.R16, false);
        //        for (int y = 0; y < h; y++)
        //            for (int x = 0; x < w; x++)
        //            {
        //                var shift = (ushort)(bytes[(y * w + x) * sizeof(ushort) + headerLength] & ((ushort)(bytes[(y * w + x) * sizeof(ushort) + 1 + headerLength]) << 8));
        //                texx.SetPixel(x, y, new Color(shift / ushort.MaxValue, 0, 0));
        //            }

        //        var filePNG = $"{file.Substring(0, file.Length - 4)}.png";
        //        File.WriteAllBytes(filePNG, texx.EncodeToPNG());
        //    }
        //}
        //var folderReflectivity = $"{folder}rgb/";
        //if (!Directory.Exists(folderReflectivity))
        //{
        //    var headerLength = 15;
        //    foreach (var file in Directory.EnumerateFiles(folderPGMReflectivity))
        //    {
        //        var bytes = File.ReadAllBytes(file);

        //        var texx = new Texture2D(w, h, UnityEngine.TextureFormat.R8, false);
        //        for (int y = 0; y < h; y++)
        //            for (int x = 0; x < w; x++)
        //            {
        //                //var shift = (ushort)(bytesByte[y * w + x] << 8 & bytesByte[y * w + x]);
        //                var shift = bytes[y * w + x + headerLength];
        //                texx.SetPixel(x, y, new Color(shift / byte.MaxValue, 0, 0));
        //            }

        //        var filePNG = $"{file.Substring(0, file.Length - 4)}.png";
        //        File.WriteAllBytes(filePNG, texx.EncodeToPNG());
        //    }
        //}


        linesDepth = File.ReadAllLines($"{folder}short_throw_depth.csv");
        linesReflectivity = File.ReadAllLines($"{folder}short_throw_reflectivity.csv");
        headersD = linesDepth[0].Split(',');
        headersR = linesReflectivity[0].Split(',');

        {
            var bytes = File.ReadAllBytes(@"Assets/short_throw_depth_camera_space_projection.bytes");
            Buffer.BlockCopy(bytes, 0, uv, 0, bytes.Length);
        }

        texMask = new Texture2D(w, h);
        texReflectivity = new Texture2D(w, h);
        texCheck = new Texture2D(w, h);

        Directory.CreateDirectory($"{folder}mask/");
        Directory.CreateDirectory($"{folder}check/");
    }

    void Update()
    {
        index++;
        if (index < linesReflectivity.Length)
        {
            if (linesReflectivity[index].Length > 0)
            {
                var line = linesReflectivity[index];
                var cols = line.Split(',');
                var frameToOrigin = new Matrix4x4(
                    new Vector4(float.Parse(cols[02]), float.Parse(cols[03]), float.Parse(cols[04]), float.Parse(cols[05])),
                    new Vector4(float.Parse(cols[06]), float.Parse(cols[07]), float.Parse(cols[08]), float.Parse(cols[09])),
                    new Vector4(float.Parse(cols[10]), float.Parse(cols[11]), float.Parse(cols[12]), float.Parse(cols[13])),
                    new Vector4(float.Parse(cols[14]), float.Parse(cols[15]), float.Parse(cols[16]), float.Parse(cols[17])));

                var cameraView = new Matrix4x4(
                    new Vector4(float.Parse(cols[18]), float.Parse(cols[19]), float.Parse(cols[20]), float.Parse(cols[21])),
                    new Vector4(float.Parse(cols[22]), float.Parse(cols[23]), float.Parse(cols[24]), float.Parse(cols[25])),
                    new Vector4(float.Parse(cols[26]), float.Parse(cols[27]), float.Parse(cols[28]), float.Parse(cols[29])),
                    new Vector4(float.Parse(cols[30]), float.Parse(cols[31]), float.Parse(cols[32]), float.Parse(cols[33])));

                var TRS = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 180, 180), new Vector3(1, 1, 1) * 0.67f);//rotate globally 180* in Y and Z, and scale
                var TRS2 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 180, 180), new Vector3(1, 1, 1) * 1.0f);//rotate 180* locally arround Y and Z

                Camera.main.worldToCameraMatrix = TRS2 * cameraView * frameToOrigin * TRS;

                //var inverse = Camera.main.worldToCameraMatrix;//.inverse;
                var inverse = Camera.main.worldToCameraMatrix.inverse;

                if (raytraceCamera != null)
                {
                    raytraceCamera.transform.localPosition = inverse.GetColumn(3);
                    raytraceCamera.transform.localRotation = inverse.rotation;
                    //raytraceCamera.transform.localScale = inverse.lossyScale;
                }

                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        //(y, x)
                        var lal = new Vector3(-uv[(y + x * h) * d], -uv[(y + x * h) * d + 1], 1).normalized;

                        if (float.IsNaN(lal.x))
                        {
                            texMask.SetPixel(x, y, Color.black);
                            continue;
                        }

                        var lal2 = new Vector3(0, 0, -1);//forward real!

                        var lal3 = (inverse.rotation * Quaternion.FromToRotation(lal2, lal)) * Vector3.forward;
                        ////fix the orientation?
                        ////lal3.x *= -1;
                        ////lal3.z *= -1;
                        var v4 = inverse.GetColumn(3);
                        //var v4 = Camera.main.transform.localPosition;
                        ////v4.z *= -1;
                        var hit = Physics.Raycast(v4, lal3);
                        var col = hit ? Color.white : Color.black;
                        //var col = new Color(lal3.x, lal3.y, lal3.z);
                        texMask.SetPixel(x, y, col);
                    }


                //GetComponent<MeshRenderer>().material.mainTexture = texMask;
                File.WriteAllBytes($"{folder}mask/" + $"00{cols[0]}.png", texMask.EncodeToPNG());

                var done = texReflectivity.LoadImage(File.ReadAllBytes($"{folder}rgb/" + $"00{cols[0]}.png"));

                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        var col = texReflectivity.GetPixel(x, y);
                        texCheck.SetPixel(x, y, new Color(col.r, texMask.GetPixel(x, y).r * .4f, 0));
                    }

                File.WriteAllBytes($"{folder}check/" + $"00{cols[0]}.png", texCheck.EncodeToPNG());

                Debug.Log($"{cols[0]} processed!");
            }
        }
        else if (index == linesReflectivity.Length)
            Debug.Log(@"Totally done!");
    }
}
