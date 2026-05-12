using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;

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
                if (m_Playing != null) { StopCoroutine(m_Playing); m_Playing = null; }
                musicSource.Pause();
                currentSongIndex = 0;
                m_Playing = StartCoroutine(PlayPlaylist(ST.songs));
                return;
            }
        }
    }

    IEnumerator PlayPlaylist(AudioClip[] playlist)
    {
        while (true)
        {
            musicSource.clip = playlist[currentSongIndex];
            musicSource.Play();

            Debug.Log("Playing: " + playlist[currentSongIndex].name);

            // wait while isPlaying is true OR if the game is paused
            yield return new WaitForSeconds(musicSource.clip.length / Mathf.Abs(musicSource.pitch));

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
