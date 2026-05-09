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
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;

        foreach (SceneTrack ST in SceneMusic)
        {
            if (ST.SceneName == scene.name)
            {
                Shuffle(ST.songs);

                if (m_Playing != null) { m_Playing = null; }

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

            currentSongIndex++;

            if (currentSongIndex >= playlist.Length)
            {
                currentSongIndex = 0;
            }
        }
    }

    private void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            // Pick a random index from 0 to i
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            // Swap the elements
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }


    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
