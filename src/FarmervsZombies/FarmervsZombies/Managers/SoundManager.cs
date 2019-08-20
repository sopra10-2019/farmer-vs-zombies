using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace FarmervsZombies.Managers
{
    internal static class SoundManager
    {
        private static readonly Dictionary<string, SoundEffect> sSoundList = new Dictionary<string, SoundEffect>();
        private static readonly Dictionary<string, Song> sSongList = new Dictionary<string, Song>();

        private static SoundEffectInstance sCurrentSound;
        private static readonly List<SoundEffectInstance> sSoundQueue = new List<SoundEffectInstance>();
        private const int SoundQueueLimit = 4;

        private static float sTime;
        private static readonly Dictionary<string, float> sSoundLastTimePlayed = new Dictionary<string, float>();
        private static bool sSoundOn = true;
        private static bool sMusicOn = true;
        private static float sMasterVolume = 1.0f;
        private static float sSoundVolume = 1.0f;
        private static float sSongVolume = 1.0f;

        public static bool SoundOn
        {
            get => sSoundOn;
            set
            {
                sSoundOn = value;
                SoundEffect.MasterVolume = value ? 1.0f : 0.0f;
            }
        }

        public static bool MusicOn
        {
            get => sMusicOn;
            set
            {
                sMusicOn = value;
                MediaPlayer.Volume = value ? 1.0f : 0.0f;
            }
        }

        public static float MasterVolume
        {
            private get => sMasterVolume;
            set
            {
                sMasterVolume = value;
                SoundEffect.MasterVolume = sMasterVolume * sSoundVolume;
                MediaPlayer.Volume = sMasterVolume * sSongVolume;
            } 
        }

        public static float SoundVolume
        {
            private get => sSoundVolume;
            set
            {
                sSoundVolume = value;
                SoundEffect.MasterVolume = sMasterVolume * sSoundVolume;
            }
        }

        public static float SongVolume
        {
            private get => sSongVolume;
            set
            {
                sSongVolume = value;
                MediaPlayer.Volume = sMasterVolume * sSongVolume;
            }
        }


        /// <summary>Automatically loads all files in Content/Audio/Sounds and Content/Audio/Songs that are added to the project
        /// with the Pipeline Tool and stores them as the file name.</summary>
        public static void LoadContent(ContentManager content)
        {
            LoadSounds(content);
            LoadSongs(content);
        }

        /// <summary>Automatically loads all *.wav files in Content/Audio/Sounds that are added to the project
        /// with the Pipeline Tool and stores them as the file name.</summary>
        private static void LoadSounds(ContentManager content)
        {
            var soundFolder = new DirectoryInfo(Path.Combine(content.RootDirectory, @"Audio\\Sounds"));
            var soundFileList = soundFolder.GetFiles("*.xnb");

            foreach (var f in soundFileList)
            {
                var soundName = Path.GetFileNameWithoutExtension(f.Name);
                try
                {
                    sSoundList[soundName] = content.Load<SoundEffect>("Audio\\Sounds\\" + soundName);
                    sSoundList[soundName].Name = soundName;
                    Debug.WriteLine($"Successfully loaded sound \"{soundName}\"");
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Failed to load sound \"{soundName}\". ({e.GetType().Name})");
                }
            }
        }

        /// <summary>Automatically loads all *.mp3 files in Content/Audio/Songs that are added to the project
        /// with the Pipeline Tool and stores them as the file name.</summary>
        private static void LoadSongs(ContentManager content)
        {
            var songFolder = new DirectoryInfo(Path.Combine(content.RootDirectory, @"Audio\\Songs"));
            var songFileList = songFolder.GetFiles("*.xnb");

            foreach (var f in songFileList)
            {
                var songName = Path.GetFileNameWithoutExtension(f.Name);
                try
                {
                    sSongList[songName] = content.Load<Song>("Audio\\Songs\\" + songName);
                    Debug.WriteLine($"Successfully loaded song \"{songName}\"");
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Failed to load song \"{songName}\". ({e.GetType().Name})");
                    Debug.WriteLine(e);
                }
            }
        }

        /// <summary>Updates the SoundManager.</summary>
        public static void Update(GameTime gameTime)
        {
            sTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (sSoundQueue.Count == 0) return;
            if (sCurrentSound == null) sCurrentSound = sSoundQueue.First();
            // Play Sounds in queue
            if (sCurrentSound.State == SoundState.Playing) return;
            sCurrentSound = sSoundQueue.First();
            sSoundQueue.Remove(sSoundQueue.First());
            sCurrentSound.Play();
        }

        /// <summary>Gets a sound name and plays it if it exists.</summary>
        /// <returns>True if sound was successfully played, false if not.</returns>
        /// <param name="soundName">Name of the sound you want to play.</param>
        public static void PlaySound(string soundName)
        {
            if (!sSoundList.ContainsKey(soundName))
            {
                throw new ArgumentException($"Sound \"{soundName}\" does not exist.");
            }
            var soundInstance = sSoundList[soundName].CreateInstance();
            if (sSoundQueue.Count < SoundQueueLimit) sSoundQueue.Add(soundInstance);
        }

        /// <summary>Gets a sound name and plays it with the specified volume, pitch, and panning if it exists.</summary>
        /// <returns>True if sound was successfully played, false if not.</returns>
        /// <param name="soundName">Name of the sound you want to play.</param>
        /// <param name="volume">Volume, ranging from 0.0 (silence) to 1.0 (full volume)</param>
        /// <param name="pitch">Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave).</param>
        /// <param name="pan">Panning, ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker).</param>

        public static void PlaySound(string soundName, float volume, float pitch = 0f, float pan = 0f)
        {
            if (!sSoundList.ContainsKey(soundName))
            {
                throw new ArgumentException($"Sound \"{soundName}\" does not exist.");
            }

            var soundInstance = sSoundList[soundName].CreateInstance();
            soundInstance.Volume = volume;
            soundInstance.Pitch = pitch;
            soundInstance.Pan = pan;
            if (sSoundQueue.Count < SoundQueueLimit) sSoundQueue.Add(soundInstance);
        }

        /// <summary>Gets a sound name, plays it if it exists and isn't on cooldown and sets the specified cooldown for the given sound.</summary>
        /// <returns>True if sound was not on cooldown and successfully played, false if not.</returns>
        /// <param name="soundName">Name of the sound you want to play.</param>
        /// <param name="cooldown">Cooldown for sound in seconds.</param>
        /// <param name="volume">Volume, ranging from 0.0 (silence) to 1.0 (full volume)</param>
        /// <param name="pitch">Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave).</param>
        /// <param name="pan">Panning, ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker).</param>

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static bool PlaySoundWithCooldown(string soundName, float cooldown, float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
        {
            if (!sSoundList.ContainsKey(soundName))
            {
                throw new ArgumentException($"Sound \"{soundName}\" does not exist.");
            }

            if (sSoundLastTimePlayed.ContainsKey(soundName) && !(sTime - sSoundLastTimePlayed[soundName] > cooldown)) return false;
            sSoundLastTimePlayed[soundName] = sTime;
            var soundInstance = sSoundList[soundName].CreateInstance();
            soundInstance.Volume = volume;
            soundInstance.Pitch = pitch;
            soundInstance.Pan = pan;
            if (sSoundQueue.Count < SoundQueueLimit) sSoundQueue.Add(soundInstance);
            return true;
        }

        private static void StopCurrentSound()
        {
            sCurrentSound?.Stop();
        }

        // ReSharper disable once UnusedMember.Global
        public static void StopSounds()
        {
            sSoundQueue.Clear();
            StopCurrentSound();
        }
        

        /// <summary>Gets a song name and plays it (on repeat) if it exists.</summary>
        /// <param name="songName">Name of the song you want to play.</param>
        /// <param name="volume">Volume, ranging from 0.0 (silence) to 1.0 (full volume)</param>
        /// <param name="repeat">If true, the song is played on repeat</param>
        public static void PlaySong(string songName, float volume = 1.0f, bool repeat = true)
        {
            if (!sSongList.ContainsKey(songName))
            {
                throw new ArgumentException($"Song \"{songName}\" does not exist.");
            }

            if (!SoundOn) return;
            MediaPlayer.Stop();
            MediaPlayer.Play(sSongList[songName]);
            MediaPlayer.IsRepeating = repeat;
            MediaPlayer.Volume = MasterVolume * volume;
        }

        /// <summary>Stops the currently played Song.</summary>
        public static void StopSong()
        {
            MediaPlayer.Stop();
            MediaPlayer.Volume = 1.0f;
        }

    }
}