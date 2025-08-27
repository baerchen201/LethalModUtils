using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace LethalModUtils;

public static class Audio
{
    #region Loading Audio Files

    /// <summary>
    /// The file extension of the audio file is unknown
    /// </summary>
    /// <param name="fileExtension">The file extension that was parsed</param>
    public class UnknownTypeError(string fileExtension)
        : Exception($"Unrecognized audio file type: {fileExtension}");

    /// <summary>
    /// The <see cref="UnityWebRequest"/> did not succeed
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    public class RequestError(string errorMessage) : Exception($"WebRequest error: {errorMessage}");

    /// <summary>
    /// The <see cref="AudioClip"/> is in an invalid state
    /// </summary>
    /// <param name="state">The state of the <see cref="AudioClip"/></param>
    public class ClipError(AudioDataLoadState? state)
        : Exception($"Clip load error: {state?.ToString() ?? "null"}");

    /// <summary>
    /// Assumes an <see cref="AudioType"/> value based on a file extension.
    /// </summary>
    /// <param name="ext">The file extension, in lowercase, with the dot (for example <c>.mp3</c>)</param>
    /// <returns>The <see cref="AudioType"/> for the file extension, or <see cref="AudioType.UNKNOWN"/></returns>
    private static AudioType GetAudioType(string ext) =>
        ext.ToLower() switch
        {
            ".ogg" => AudioType.OGGVORBIS,
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            ".m4a" => AudioType.ACC,
            ".aiff" => AudioType.AIFF,
            _ => AudioType.UNKNOWN,
        };

    /// <summary>
    /// Synchronously loads an audio file
    /// </summary>
    /// <param name="path">The audio file to load</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>A playback-ready AudioClip</returns>
    /// <exception cref="UnknownTypeError"/>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="RequestError"/>
    /// <exception cref="ClipError"/>
    public static AudioClip Load(Uri path, TimeSpan? timeout = null)
    {
        var ext = Path.GetExtension(path.AbsolutePath).ToLower();
        var audioType = GetAudioType(ext);
        LethalModUtils.Logger.LogDebug(
            $">> Audio.Load({path}, {timeout.str()}) audioType:{audioType}"
        );
        if (audioType == AudioType.UNKNOWN)
        {
            var e = new UnknownTypeError(ext);
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            throw e;
        }

        var webRequest = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
        ((DownloadHandlerAudioClip)webRequest.downloadHandler).streamAudio = !LethalModUtils
            .Instance
            .PreloadAudio;
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

        var audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
        if (audioClip is null or { loadState: not AudioDataLoadState.Loaded })
        {
            var e = new ClipError(audioClip?.loadState);
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            throw e;
        }

        LethalModUtils.Logger.LogInfo($"Loaded {path}");
        return audioClip;
    }

    /// <summary>
    /// Tries to synchronously load an audio file
    /// </summary>
    /// <param name="path">The audio file to load</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>
    /// On success: A playback-ready AudioClip
    /// On failure: null
    /// </returns>
    public static AudioClip? TryLoad(Uri path, TimeSpan? timeout = null)
    {
        LethalModUtils.Logger.LogDebug($">> Audio.TryLoad({path}, {timeout.str()})");
        try
        {
            return Load(path, timeout);
        }
        catch (Exception e)
        {
            LethalModUtils.Logger.LogDebug($"<< Audio.TryLoad null ({e})");
        }

        return null;
    }

    /// <summary>
    /// Asynchronously loads an audio file
    /// </summary>
    /// <param name="path">The audio file to load</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>A playback-ready AudioClip</returns>
    /// <exception cref="UnknownTypeError"/>
    /// <exception cref="TimeoutException"/>
    /// <exception cref="RequestError"/>
    /// <exception cref="ClipError"/>
    public static Task<AudioClip> LoadAsync(Uri path, TimeSpan? timeout = null)
    {
        LethalModUtils.Logger.LogDebug($">> Audio.LoadAsync({path}, {timeout.str()})");
        var task = new TaskCompletionSource<AudioClip>();

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
    /// Asynchronously loads an audio file using Unity's Coroutines
    /// </summary>
    /// <param name="path">The audio file to load</param>
    /// <param name="task">A <see cref="TaskCompletionSource{AudioClip}"/> which contains the result</param>
    /// <returns>A playback-ready AudioClip</returns>
    /// <exception cref="UnknownTypeError"/>
    /// <exception cref="RequestError"/>
    /// <exception cref="ClipError"/>
    private static IEnumerator LoadEnumerator(Uri path, TaskCompletionSource<AudioClip> task)
    {
        var ext = Path.GetExtension(path.AbsolutePath).ToLower();
        var audioType = GetAudioType(ext);
        LethalModUtils.Logger.LogDebug(
            $">> Audio.LoadEnumerator({path}, {task}) audioType:{audioType}"
        );
        if (audioType == AudioType.UNKNOWN)
        {
            var e = new UnknownTypeError(ext);
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            task.SetException(e);
            yield break;
        }

        var webRequest = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
        ((DownloadHandlerAudioClip)webRequest.downloadHandler).streamAudio = !LethalModUtils
            .Instance
            .PreloadAudio;
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            var e = new RequestError(webRequest.error);
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            task.SetException(e);
            yield break;
        }

        var audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
        if (audioClip is null or { loadState: not AudioDataLoadState.Loaded })
        {
            var e = new ClipError(audioClip?.loadState);
            LethalModUtils.Logger.LogWarning($"Error loading {path}: {e.Message}");
            task.SetException(e);
            yield break;
        }

        LethalModUtils.Logger.LogInfo($"Loaded {path}");
        task.SetResult(audioClip);
    }

    /// <summary>
    /// Tries to asynchronously load an audio file
    /// </summary>
    /// <param name="path">The audio file to load</param>
    /// <param name="timeout">An optional timeout for the load operation</param>
    /// <returns>
    /// On success: A playback-ready AudioClip
    /// On failure: null
    /// </returns>
    public async static Task<AudioClip?> TryLoadAsync(Uri path, TimeSpan? timeout = null)
    {
        LethalModUtils.Logger.LogDebug($">> Audio.TryLoadAsync({path}, {timeout.str()})");
        try
        {
            return await LoadAsync(path, timeout);
        }
        catch (Exception e)
        {
            LethalModUtils.Logger.LogDebug($"<< Audio.TryLoadAsync null ({e})");
        }

        return null;
    }

    #endregion

    #region Playing AudioClips

    /// <summary>
    /// A manager class that allows simple playback of an AudioClip
    /// </summary>
    /// <seealso cref="Audio.Play"/>
    public class AudioPlayer
    {
        private AudioPlayer() => throw new InvalidOperationException();

        private readonly Action<AudioPlayer>? MainLoop;
        internal readonly GameObject GameObject;
        internal readonly AudioSource AudioSource;

        private class _MonoBehaviour : MonoBehaviour;

        internal AudioPlayer(AudioClip audioClip, Action<AudioPlayer>? mainLoop)
        {
            LethalModUtils.Logger.LogDebug($">> AudioPlayer({audioClip}, {mainLoop.str()})");
            MainLoop = mainLoop;

            GameObject = new GameObject();
            AudioSource = GameObject.AddComponent<AudioSource>();
            Object.DontDestroyOnLoad(GameObject);
            GameObject.hideFlags = HideFlags.HideAndDontSave;

            AudioSource.clip = audioClip;
            State = PlayerState.Paused;

            GameObject.AddComponent<_MonoBehaviour>().StartCoroutine(this.mainLoop());
        }

        public enum PlayerState
        {
            Playing,
            Paused,
            Finished,
        }

        /// <summary>
        /// The current state of the player
        /// </summary>
        public PlayerState State { get; private set; }

        /// <summary>
        /// <see cref="AudioSource.mute"/>
        /// </summary>
        public bool Muted
        {
            get => AudioSource.mute;
            set => AudioSource.mute = value;
        }

        /// <summary>
        /// <see cref="AudioSource.bypassEffects"/>
        /// </summary>
        public bool BypassEffects
        {
            get => AudioSource.bypassEffects;
            set => AudioSource.bypassEffects = value;
        }

        /// <summary>
        /// <see cref="AudioSource.bypassListenerEffects"/>
        /// </summary>
        public bool BypassListenerEffects
        {
            get => AudioSource.bypassListenerEffects;
            set => AudioSource.bypassListenerEffects = value;
        }

        /// <summary>
        /// <see cref="AudioSource.bypassReverbZones"/>
        /// </summary>
        public bool BypassReverbZones
        {
            get => AudioSource.bypassReverbZones;
            set => AudioSource.bypassReverbZones = value;
        }

        /// <summary>
        /// <see cref="AudioSource.loop"/>
        /// </summary>
        public bool Loop
        {
            get => AudioSource.loop;
            set => AudioSource.loop = value;
        }

        /// <summary>
        /// <see cref="AudioSource.priority"/>
        /// </summary>
        public int Priority
        {
            get => AudioSource.priority;
            set => AudioSource.priority = value;
        }

        /// <summary>
        /// <see cref="AudioSource.volume"/>
        /// </summary>
        public float Volume
        {
            get => AudioSource.volume;
            set => AudioSource.volume = value;
        }

        /// <summary>
        /// <see cref="AudioSource.pitch"/>
        /// </summary>
        public float Pitch
        {
            get => AudioSource.pitch;
            set => AudioSource.pitch = value;
        }

        /// <summary>
        /// <see cref="AudioSource.dopplerLevel"/>
        /// </summary>
        public float DopplerLevel
        {
            get => AudioSource.dopplerLevel;
            set => AudioSource.dopplerLevel = value;
        }

        /// <summary>
        /// <see cref="AudioSource.maxDistance"/>
        /// </summary>
        /// <seealso cref="SetRange(float?,float?)"/>
        public float MaxDistance => AudioSource.maxDistance;

        /// <summary>
        /// <see cref="AudioSource.minDistance"/>
        /// </summary>
        /// <seealso cref="SetRange(float?,float?)"/>
        public float MinDistance => AudioSource.minDistance;

        /// <summary>
        /// Sets the maxDistance and minDistance options and enabled 3D playback
        /// </summary>
        /// <param name="maxDistance">The maximum distance the sound can be heard from (null: no change)</param>
        /// <param name="minDistance">The distance at which the sound will begin to fade (null: no change)</param>
        public void SetRange(float? maxDistance, float? minDistance = null)
        {
            AudioSource.spatialBlend = 1f;
            if (maxDistance != null)
                AudioSource.maxDistance = maxDistance.Value;
            if (minDistance != null)
                AudioSource.minDistance = minDistance.Value;
        }

        /// <summary>
        /// Disables 3D playback
        /// </summary>
        public void SetRange() => AudioSource.spatialBlend = 0f;

        /// <summary>
        /// <see cref="AudioSource.rolloffMode"/>
        /// </summary>
        /// <seealso cref="SetRolloff(bool)"/>
        /// <seealso cref="SetRolloff(AnimationCurve)"/>
        public AudioRolloffMode RolloffMode => AudioSource.rolloffMode;

        /// <summary>
        /// <see cref="AudioSource.GetCustomCurve"/>
        /// </summary>
        /// <seealso cref="SetRolloff(AnimationCurve)"/>
        public AnimationCurve RolloffCurve =>
            AudioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);

        /// <summary>
        /// Sets the 3D rolloff mode to <see cref="AudioRolloffMode.Linear"/> or <see cref="AudioRolloffMode.Logarithmic"/>
        /// </summary>
        /// <param name="linear">Which mode to use</param>
        public void SetRolloff(bool linear = true) =>
            AudioSource.rolloffMode = linear
                ? AudioRolloffMode.Linear
                : AudioRolloffMode.Logarithmic;

        /// <summary>
        /// Sets a custom 3D rolloff curve
        /// </summary>
        /// <param name="curve">The custom curve</param>
        public void SetRolloff(AnimationCurve curve)
        {
            AudioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
            AudioSource.rolloffMode = AudioRolloffMode.Custom;
        }

        /// <summary>
        /// Stop playback and destroy playback components
        /// </summary>
        public void Cancel()
        {
            LethalModUtils.Logger.LogDebug(
                $">> AudioPlayer.Cancel() State:{State} AudioSource:{AudioSource.str()} GameObject:{GameObject.str()}"
            );
            if (AudioSource)
                Object.Destroy(AudioSource);
            if (GameObject)
                Object.Destroy(GameObject);
            State = PlayerState.Finished;
        }

        /// <summary>
        /// Pause playback
        /// </summary>
        public void Pause()
        {
            LethalModUtils.Logger.LogDebug($">> AudioPlayer.Pause() State:{State}");
            if (State != PlayerState.Playing)
                return;
            AudioSource.Pause();
            State = PlayerState.Paused;
        }

        /// <summary>
        /// Reset position to the start and pause playback
        /// </summary>
        public void Stop()
        {
            LethalModUtils.Logger.LogDebug($">> AudioPlayer.Stop() State:{State}");
            if (State == PlayerState.Finished)
                return;
            AudioSource.Stop();
            AudioSource.Pause();
            State = PlayerState.Paused;
        }

        /// <summary>
        /// Resume playback after pausing
        /// </summary>
        public void Resume()
        {
            LethalModUtils.Logger.LogDebug($">> AudioPlayer.Resume() State:{State}");
            if (State != PlayerState.Paused)
                return;
            AudioSource.Play();
            State = PlayerState.Playing;
        }

        private IEnumerator mainLoop()
        {
            try
            {
                while (State != PlayerState.Finished && AudioSource && GameObject)
                {
                    MainLoop?.Invoke(this);
                    yield return null;
                }
            }
            finally
            {
                Cancel();
            }
        }

        /// <summary>
        /// Audio Source used to play sound, for more control
        /// </summary>
        public AudioSource _audioSource => AudioSource;

        public override string ToString()
        {
            return $"{GetType().Name} {{ State:{State}, Volume:{Volume}, Muted:{Muted}, MainLoop:{MainLoop.str()} }}";
        }
    }

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip and plays it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer Play(this AudioClip audioClip, Action<AudioPlayer>? mainLoop = null)
    {
        var audioPlayer = new AudioPlayer(audioClip, mainLoop);
        audioPlayer.Resume();
        return audioPlayer;
    }

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip. Use <see cref="AudioPlayer.Resume"/> to play it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer CreatePlayer(
        this AudioClip audioClip,
        Action<AudioPlayer>? mainLoop = null
    ) => new(audioClip, mainLoop);

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip at the specified position and plays it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="position">The position where the <see cref="AudioSource"/> is placed</param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer PlayAt(
        this AudioClip audioClip,
        Vector3 position,
        Action<AudioPlayer>? mainLoop = null
    )
    {
        var audioPlayer = new AudioPlayer(audioClip, mainLoop);
        audioPlayer.GameObject.transform.position = position;
        audioPlayer.Resume();
        return audioPlayer;
    }

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip at the specified position and plays it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="parent">The <see cref="GameObject"/> to which the <see cref="AudioSource"/> will be parented</param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer PlayAt(
        this AudioClip audioClip,
        Transform parent,
        Action<AudioPlayer>? mainLoop = null
    )
    {
        var audioPlayer = new AudioPlayer(audioClip, mainLoop);
        audioPlayer.GameObject.transform.parent = parent;
        audioPlayer.GameObject.transform.localPosition = Vector3.zero;
        audioPlayer.Resume();
        return audioPlayer;
    }

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip at the specified position and plays it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="parent">The <see cref="GameObject"/> to which the <see cref="AudioSource"/> will be parented</param>
    /// <param name="offset">The relative position where the <see cref="AudioSource"/> is placed</param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer PlayAt(
        this AudioClip audioClip,
        Transform parent,
        Vector3 offset,
        Action<AudioPlayer>? mainLoop = null
    )
    {
        var audioPlayer = new AudioPlayer(audioClip, mainLoop);
        audioPlayer.GameObject.transform.parent = parent;
        audioPlayer.GameObject.transform.localPosition = offset;
        audioPlayer.Resume();
        return audioPlayer;
    }

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip at the specified position and plays it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="position">The absolute position where the <see cref="AudioSource"/> is placed</param>
    /// <param name="parent">The <see cref="GameObject"/> to which the <see cref="AudioSource"/> will be parented</param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer PlayAt(
        this AudioClip audioClip,
        Vector3 position,
        Transform parent,
        Action<AudioPlayer>? mainLoop = null
    )
    {
        var audioPlayer = new AudioPlayer(audioClip, mainLoop);
        audioPlayer.GameObject.transform.position = position;
        audioPlayer.GameObject.transform.parent = parent;
        audioPlayer.Resume();
        return audioPlayer;
    }

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip at the specified position. Use <see cref="AudioPlayer.Resume"/> to play it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="position">The position where the <see cref="AudioSource"/> is placed</param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer CreatePlayerAt(
        this AudioClip audioClip,
        Vector3 position,
        Action<AudioPlayer>? mainLoop = null
    )
    {
        var audioPlayer = new AudioPlayer(audioClip, mainLoop);
        audioPlayer.GameObject.transform.position = position;
        return audioPlayer;
    }

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip at the specified position. Use <see cref="AudioPlayer.Resume"/> to play it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="parent">The <see cref="GameObject"/> to which the <see cref="AudioSource"/> will be parented</param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer CreatePlayerAt(
        this AudioClip audioClip,
        Transform parent,
        Action<AudioPlayer>? mainLoop = null
    )
    {
        var audioPlayer = new AudioPlayer(audioClip, mainLoop);
        audioPlayer.GameObject.transform.parent = parent;
        audioPlayer.GameObject.transform.localPosition = Vector3.zero;
        return audioPlayer;
    }

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip at the specified position. Use <see cref="AudioPlayer.Resume"/> to play it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="parent">The <see cref="GameObject"/> to which the <see cref="AudioSource"/> will be parented</param>
    /// <param name="offset">The relative position where the <see cref="AudioSource"/> is placed</param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer CreatePlayerAt(
        this AudioClip audioClip,
        Transform parent,
        Vector3 offset,
        Action<AudioPlayer>? mainLoop = null
    )
    {
        var audioPlayer = new AudioPlayer(audioClip, mainLoop);
        audioPlayer.GameObject.transform.parent = parent;
        audioPlayer.GameObject.transform.localPosition = offset;
        return audioPlayer;
    }

    /// <summary>
    /// Creates an <see cref="AudioPlayer"/> for this audio clip at the specified position. Use <see cref="AudioPlayer.Resume"/> to play it
    /// </summary>
    /// <param name="audioClip">The source <see cref="AudioClip"/></param>
    /// <param name="position">The absolute position where the <see cref="AudioSource"/> is placed</param>
    /// <param name="parent">The <see cref="GameObject"/> to which the <see cref="AudioSource"/> will be parented</param>
    /// <param name="mainLoop">An optional function that runs each frame</param>
    /// <returns>The newly created <see cref="AudioPlayer"/></returns>
    public static AudioPlayer CreatePlayerAt(
        this AudioClip audioClip,
        Vector3 position,
        Transform parent,
        Action<AudioPlayer>? mainLoop = null
    )
    {
        var audioPlayer = new AudioPlayer(audioClip, mainLoop);
        audioPlayer.GameObject.transform.position = position;
        audioPlayer.GameObject.transform.parent = parent;
        return audioPlayer;
    }

    #endregion
}
