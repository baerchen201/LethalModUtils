using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LethalModUtils;

public class Image
{
    #region Loading Textures

    /// <summary>
    /// The <see cref="UnityWebRequest"/> did not succeed
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    public class RequestError(string errorMessage) : Exception($"WebRequest error: {errorMessage}");

    /// <summary>
    /// The <see cref="Texture2D"/> is null
    /// </summary>
    public class TextureError() : Exception("Texture is null");

#if false // Unity is fucking weird and I can't be fucked to fix it
    
    /// <summary>
    /// Synchronously loads an image file
    /// </summary>
    /// <param name="path">The image file to load</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>A display-ready Texture2D</returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="RequestError"/>
    /// <exception cref="TextureError"/>
    public static Texture2D Load(Uri path, TimeSpan? timeout = null)
    {
        LethalModUtils.Logger.LogDebug($">> Image.Load({path}, {timeout.str()})");

        var webRequest = UnityWebRequestTexture.GetTexture(path);
        webRequest.SendWebRequest();
        if (timeout != null)
        {
            var timeoutTask = Task.Delay(timeout.Value);
            while (!webRequest.isDone && !timeoutTask.IsCompleted) { }

            if (!webRequest.isDone)
            {
                var e = new TimeoutException();
                LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
                throw e;
            }
        }
        else
            while (!webRequest.isDone) { }

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            var e = new RequestError(webRequest.error);
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            throw e;
        }

        var texture2d = DownloadHandlerTexture.GetContent(webRequest);
        if (texture2d is null)
        {
            var e = new TextureError();
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            throw e;
        }

        LethalModUtils.Logger.LogInfo($"Loaded {path}");
        return texture2d;
    }

    /// <summary>
    /// Tries to synchronously load an image file
    /// </summary>
    /// <param name="path">The image file to load</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>
    /// On success: A display-ready Texture2D
    /// On failure: null
    /// </returns>
    public static Texture2D? TryLoad(Uri path, TimeSpan? timeout = null)
    {
        LethalModUtils.Logger.LogDebug($">> Image.TryLoad({path}, {timeout.str()})");
        try
        {
            return Load(path, timeout);
        }
        catch (Exception e)
        {
            LethalModUtils.Logger.LogDebug($"<< Image.TryLoad null ({e})");
        }

        return null;
    }

    /// <summary>
    /// Synchronously loads an image file
    /// </summary>
    /// <param name="path">The image file to load</param>
    /// <param name="webRequest">The <see cref="UnityWebRequest"/> used to obtain the file</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>A display-ready Texture2D</returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="RequestError"/>
    /// <exception cref="TextureError"/>
    public static Texture2D Load(Uri path, out UnityWebRequest webRequest, TimeSpan? timeout = null)
    {
        var ext = Path.GetExtension(path.AbsolutePath).ToLower();
        LethalModUtils.Logger.LogDebug($">> Image.Load({path}, {timeout.str()})");

        webRequest = UnityWebRequestTexture.GetTexture(path);
        webRequest.SendWebRequest();
        if (timeout != null)
        {
            var timeoutTask = Task.Delay(timeout.Value);
            while (!webRequest.isDone && !timeoutTask.IsCompleted) { }

            if (!webRequest.isDone)
            {
                var e = new TimeoutException();
                LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
                throw e;
            }
        }
        else
            while (!webRequest.isDone) { }

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            var e = new RequestError(webRequest.error);
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            throw e;
        }

        var texture2d = DownloadHandlerTexture.GetContent(webRequest);
        if (texture2d is null)
        {
            var e = new TextureError();
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            throw e;
        }

        LethalModUtils.Logger.LogInfo($"Loaded {path}");
        return texture2d;
    }

    /// <summary>
    /// Tries to synchronously load an image file
    /// </summary>
    /// <param name="path">The image file to load</param>
    /// <param name="webRequest">The <see cref="UnityWebRequest"/> used to obtain the file</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>
    /// On success: A display-ready Texture2D
    /// On failure: null
    /// </returns>
    public static Texture2D? TryLoad(
        Uri path,
        out UnityWebRequest? webRequest,
        TimeSpan? timeout = null
    )
    {
        webRequest = null;
        LethalModUtils.Logger.LogDebug($">> Image.TryLoad({path}, {timeout.str()})");
        try
        {
            return Load(path, out webRequest, timeout);
        }
        catch (Exception e)
        {
            LethalModUtils.Logger.LogDebug($"<< Image.TryLoad null ({e})");
        }

        return null;
    }
    
#endif

    /// <summary>
    /// Asynchronously loads an image file
    /// </summary>
    /// <param name="path">The image file to load</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>A display-ready Texture2D</returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="RequestError"/>
    /// <exception cref="TextureError"/>
    public static Task<Texture2D> LoadAsync(Uri path, TimeSpan? timeout = null)
    {
        LethalModUtils.Logger.LogDebug($">> Image.LoadAsync({path}, {timeout.str()})");
        var task = new TaskCompletionSource<Texture2D>();

        LethalModUtils.Instance.StartCoroutine(LoadEnumerator(path, task));

        if (timeout != null)
        {
            var timeoutTask = Task.Delay(timeout.Value);
            if (Task.WhenAny(task.Task, timeoutTask) == timeoutTask)
            {
                var e = new TimeoutException();
                LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
                throw e;
            }
        }

        return task.Task;
    }

    /// <summary>
    /// Asynchronously loads an image file using Unity's Coroutines
    /// </summary>
    /// <param name="path">The image file to load</param>
    /// <param name="task">A <see cref="TaskCompletionSource{Texture2D}"/> which contains the result</param>
    /// <returns>A display-ready Texture2D</returns>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="RequestError"/>
    /// <exception cref="TextureError"/>
    private static IEnumerator LoadEnumerator(Uri path, TaskCompletionSource<Texture2D> task)
    {
        LethalModUtils.Logger.LogDebug($">> Image.LoadEnumerator({path}, {task})");

        var webRequest = UnityWebRequestTexture.GetTexture(path);
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            var e = new RequestError(webRequest.error);
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            task.SetException(e);
            yield break;
        }

        var texture2d = DownloadHandlerTexture.GetContent(webRequest);
        if (texture2d is null)
        {
            var e = new TextureError();
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            task.SetException(e);
            yield break;
        }

        LethalModUtils.Logger.LogInfo($"Loaded {path}");
        task.SetResult(texture2d);
    }

    /// <summary>
    /// Tries to asynchronously load an image file
    /// </summary>
    /// <param name="path">The image file to load</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>
    /// On success: A display-ready Texture2D
    /// On failure: null
    /// </returns>
    public async static Task<Texture2D?> TryLoadAsync(Uri path, TimeSpan? timeout = null)
    {
        LethalModUtils.Logger.LogDebug($">> Image.TryLoadAsync({path}, {timeout.str()})");
        try
        {
            return await LoadAsync(path, timeout);
        }
        catch (Exception e)
        {
            LethalModUtils.Logger.LogDebug($"<< Image.TryLoadAsync null ({e})");
        }

        return null;
    }

    #endregion

    #region Sprites

    /// <summary>
    /// Creates a <see cref="UnityEngine.Sprite"/> object from a <see cref="Texture2D"/>
    /// </summary>
    /// <param name="texture">The source texture</param>
    /// <param name="centered">Whether the texture should be centered in the sprite</param>
    /// <returns>The newly created <see cref="Sprite"/></returns>
    public static Sprite Texture2DToSprite(Texture2D texture, bool centered = true)
    {
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            centered ? new Vector2(0.5f, 0.5f) : Vector2.zero
        );
    }

    #endregion
}

public static class Texture2DExtensions
{
    /// <inheritdoc cref="Image.Texture2DToSprite"/>
    public static Sprite ToSprite(this Texture2D texture, bool centered = true) =>
        Image.Texture2DToSprite(texture, centered);
}
