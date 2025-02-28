using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : SingletonMono<SoundManager>
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioSource backgroundAS;

    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private Transform audioSourceContainer;

    private readonly Queue<AudioSource> audioSources = new Queue<AudioSource>();
    private readonly LinkedList<AudioSource> inuse = new LinkedList<AudioSource>();
    private readonly Queue<LinkedListNode<AudioSource>> nodePool = new Queue<LinkedListNode<AudioSource>>();

    private GameData _gameD;
    private bool _isInit;

    private void Start()
    {
        if(!_isInit)
        {
            Init();
        }
    }

    public void Init()
    {
        _isInit = true;

        AudioSettings.OnAudioConfigurationChanged += (value) =>
        {
            
        };

        _gameD = Global.Instance.GameD;
        Global.Instance.GameE.PlaySFX = PlaySFX;
    }

    private int lastCheckFrame = -1;

    private void CheckInUse()
    {
        var node = inuse.First;
        while (node != null)
        {
            var current = node;
            node = node.Next;

            if (!current.Value.isPlaying)
            {
                audioSources.Enqueue(current.Value);
                inuse.Remove(current);
                nodePool.Enqueue(current);
            }
        }
    }

    public void PlayBackgroundMusic(bool isMainMenu)
    {
        if(!_gameD.IsMusicActive)
        {
            return;
        }

        backgroundAS.clip = isMainMenu ? GetClip(AudioEnum.MainMenu) : GetClip(AudioEnum.Gameplay);
        backgroundAS.Play();
    }

    public void PauseBackgroundMusic()
    {
        backgroundAS.Stop();
    }

    public void PlaySFX(AudioEnum audioType, float delay)
    {
        if(!_gameD.IsSoundActive)
        {
            return;
        }

        PlayAtPoint(Instance.GetClip(audioType), Vector3.zero, delay);
    }

    public void Play3DSFX(AudioEnum audioType, Vector3 position, float delay = 0, float pitch = 1f)
    {
        if (!_gameD.IsSoundActive)
        {
            return;
        }

        PlayAtPoint(Instance.GetClip(audioType), position, delay, pitch);
    }

    public void PlayAtPoint(AudioClip clip, Vector3 position, float Delay = 0, float pitch = 1f)
    {
       
        AudioSource source;

        if (lastCheckFrame != Time.frameCount)
        {
            lastCheckFrame = Time.frameCount;
            CheckInUse();
        }

        if (audioSources.Count == 0)
            source = GameObject.Instantiate(audioSourcePrefab, audioSourceContainer);
        else
            source = audioSources.Dequeue();

        if (nodePool.Count == 0)
            inuse.AddLast(source);
        else
        {
            var node = nodePool.Dequeue();
            node.Value = source;
            inuse.AddLast(node);
        }

        source.transform.position = position;
        source.clip = clip;
        source.pitch = pitch;
        source.volume = 1.0f;

        source.PlayDelayed(Delay);
    }

    public AudioClip GetClip(AudioEnum type)
    {
        if ((int)type >= clips.Length) 
        { 
            Debug.LogError("AudioEnum " + type + " is not found in the SoundManager clips array");
            return null;
        }

        return clips[(int)type];
    }
}

public enum AudioEnum
{
    MainMenu = 0,
    Gameplay = 1,
    ButtonClick = 2,
    Player_Explosion = 3,
    GetHit = 4,
    Powerup = 5,
    TeleportIn = 6
}
