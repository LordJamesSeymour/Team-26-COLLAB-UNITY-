using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private SceneTrack[] SceneMusic;
    private AudioSource musicSource;
    int currentSongIndex = 0;
    Coroutine m_Playing;

    [Serializable]
    private struct SceneTrack
    {
        public string SceneName;
        public AudioClip[] songs;
    };

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        musicSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        CheckScene(SceneManager.GetActiveScene());
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;

        CheckScene(scene);
    }

    void CheckScene(Scene scene)
    {
        foreach (SceneTrack ST in SceneMusic)
        {
            if (ST.SceneName == scene.name)
            {
                if (m_Playing != null) { m_Playing = null; }

                currentSongIndex = 0;
                m_Playing = StartCoroutine(PlayPlaylist(ST.songs));
                return;
            }
        }
    }

    IEnumerator PlayPlaylist(AudioClip[] playlist)
    {
        while (true) // Keep the playlist loop running forever
        {
            musicSource.clip = playlist[currentSongIndex];
            musicSource.Play();

            Debug.Log("Playing: " + playlist[currentSongIndex].name);

            // wait while isPlaying is true OR if the game is paused
            while (musicSource.isPlaying || Math.Abs(musicSource.time - musicSource.clip.length) < 0.1f)
            {
                yield return new WaitForSeconds(1.0f);
            }

            currentSongIndex = UnityEngine.Random.Range(0, playlist.Length);

            if (currentSongIndex >= playlist.Length)
            {
                currentSongIndex = 0;
            }
        }
    }


    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
