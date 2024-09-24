using UnityEngine;

public class AudioManager : MonoBehaviour
{

	public static AudioManager instance;
	public Sound[] sounds;

	public void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		foreach (Sound sound in sounds)
		{
			sound.audioSource = gameObject.AddComponent<AudioSource>();
			sound.audioSource.clip = sound.clip;
			sound.audioSource.volume = sound.volume;
			sound.audioSource.pitch = sound.pitch;
			sound.audioSource.loop = sound.loop;
		}
		DontDestroyOnLoad(gameObject);
	}

	public void PlaySound(string soundName)
	{
		foreach (Sound sound in sounds)
		{
			if (sound.name == soundName)
			{
				sound.audioSource.Play();
			}
		}
	}
	

}