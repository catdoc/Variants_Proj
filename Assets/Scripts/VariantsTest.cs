using UnityEngine;
using System.IO;
using UnityEngine.Profiling;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;

public class VariantsTest : MonoBehaviour
{
    private AssetBundleManifest manifest;

    private string prefix
    {
        get { return Path.Combine(Application.streamingAssetsPath, "assets"); }
    }

    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();
    public static byte[] ReadBytesFromStreamingAssets(string dllName)
    {
        UnityEngine.Debug.Log($"ReadBytesFromStreamingAssets:{dllName}");
        return s_assetDatas[dllName];
    }

    private string GetWebRequestPath(string asset)
    {
        var path = $"{Application.streamingAssetsPath}/assets/{asset}";
        if (!path.Contains("://"))
        {
            path = "file://" + path;
        }
        return path;
    }

    bool bGameStarted = false;

    void Start()
    {
        StartCoroutine(DownLoadAssets(this.StartGame));
    }

    IEnumerator DownLoadAssets(Action onDownloadComplete)
    {
        var assets = new List<string>
        {
            "shader",
            "cubemulticompile",
            "cubefeature",
            "cubelit",
            "cubesurface",
            "cubenewsurface",
            "cubeunlit",
        };

        foreach (var asset in assets)
        {
            string dllPath = GetWebRequestPath(asset);
            UnityEngine.Debug.Log($"start download asset:{dllPath}");
            UnityWebRequest www = UnityWebRequest.Get(dllPath);
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
            {
                UnityEngine.Debug.Log(www.error);
            }
#else
            if (www.isHttpError || www.isNetworkError)
            {
                UnityEngine.Debug.Log(www.error);
            }
#endif
            else
            {
                // Or retrieve results as binary data
                byte[] assetData = www.downloadHandler.data;
                UnityEngine.Debug.Log($"dll:{asset}  size:{assetData.Length}");
                s_assetDatas[asset] = assetData;
            }
        }

        onDownloadComplete();
    }

    void StartGame()
    {
        bGameStarted = true;
        UnityEngine.Debug.Log("StartGame is invoking");
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(20, 20, 200, 100), "LoadVariants - Join"))
        {
            if (bGameStarted)
            {
                Profiler.BeginSample("LoadVariants");
                LoadVariants();
                LoadCube("cubemulticompile", new Vector3(2, 0, 0));
                LoadCube("cubeunlit", new Vector3(2, 2, 0));
                LoadCube("cubenewsurface", new Vector3(2, -2, 0));
                LoadCube("cubefeature", new Vector3(-2, 0, 0));
                LoadCube("cubelit", new Vector3(-2, 2, 0));
                LoadCube("cubesurface", new Vector3(-2, -2, 0));
                Profiler.EndSample();
            }
            else 
            {
                UnityEngine.Debug.Log("bGameStarted is false");
            }
        }
// #if 0
//         if (GUI.Button(new Rect(20, 140, 200, 100), "LoadVariants - Mat"))
//         {
//             Profiler.BeginSample("LoadVariants");
//             LoadVariants();
//             LoadMat("mat_shaderfeature");
//             LoadMat("mat_multicompile");
//             LoadCube("cubemulticompile", new Vector3(2, 0, 0));
//             LoadCube("cubefeature", new Vector3(-2, 0, 0));
//             LoadCube("cubelit", new Vector3(-3, 0, 0));
//             Profiler.EndSample();
//         }
// #endif
    }

    private void LoadVariants()
    {
// #if !UNITY_EDITOR
         AssetBundle b = AssetBundle.LoadFromMemory(ReadBytesFromStreamingAssets("shader"));
// #else         
//         var pat = Path.Combine(prefix, "shader");
//         var b = AssetBundle.LoadFromFile(pat);
// #endif 
        var svc = b.LoadAsset<ShaderVariantCollection>("MultiShaderVariants");
        
        Stopwatch sw = new Stopwatch();
		sw.Start();
        svc.WarmUp();
        sw.Stop();
		UnityEngine.Debug.Log(string.Format("WarmUp total: {0} ms, shader count: {1}, variant count: {2}",sw.ElapsedMilliseconds, svc.shaderCount, svc.variantCount));

        // svc的WarmUp就会触发相关Shader的预编译，触发预编译之后再加载Shader Asset即可
        var shaders = b.LoadAllAssets<Shader>();
        foreach (var shader in shaders)
        {
            UnityEngine.Debug.Log("shader: " + shader.name);
        }
        var mats = b.LoadAllAssets<Material>();
        foreach (var mat in mats)
        {
            UnityEngine.Debug.Log("mat: " + mat.name);
        }
        // b.Unload(false);
    }

    private void LoadMat(string mat)
    {
// #if !UNITY_EDITOR
         AssetBundle b = AssetBundle.LoadFromMemory(ReadBytesFromStreamingAssets(mat));
// #else    
//         var pat = Path.Combine(prefix, mat);
//         var b = AssetBundle.LoadFromFile(pat);
// #endif
        b.LoadAllAssets<Material>();
    }


    private void LoadCube(string name, Vector3 pos)
    {
// #if !UNITY_EDITOR
         AssetBundle b = AssetBundle.LoadFromMemory(ReadBytesFromStreamingAssets(name));
// #else 
//         var pat = Path.Combine(prefix, name);
//         var b = AssetBundle.LoadFromFile(pat);
// #endif      
        var obj = b.LoadAsset<GameObject>(name);
        var go = GameObject.Instantiate(obj);
        go.name = name + "...";
        go.transform.position = pos;
        b.Unload(false);
    }
}