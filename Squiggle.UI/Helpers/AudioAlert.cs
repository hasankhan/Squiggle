using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using Squiggle.Utilities;
using Squiggle.UI.Components;

namespace Squiggle.UI.Helpers
{

    enum AudioAlertType
    {
        Buzz,
        BuddyOnline,
        BuddyOffline,
        MessageReceived,
        VoiceChatDisconnected,
        VoiceChatRingingIn,
        VoiceChatRingingOut
    }

    class AudioAlert
    {
        public static AudioAlert Instance = LoadAlerts();

        Dictionary<AudioAlertType, SoundPlayer> sounds = new Dictionary<AudioAlertType, SoundPlayer>();

        public bool Enabled { get; set; }

        static AudioAlert LoadAlerts()
        {
            var audioAlert = new AudioAlert();

            foreach (AudioAlertType alertType in Enum.GetValues(typeof(AudioAlertType)))
                audioAlert.LoadSound(alertType);

            return audioAlert;
        }

        public void Play(AudioAlertType alertType)
        {
            SoundPlayer player;
            if (Enabled && sounds.TryGetValue(alertType, out player))
                player.Play();
        }

        public void Stop(AudioAlertType alertType)
        {
            SoundPlayer player;
            if (sounds.TryGetValue(alertType, out player))
                player.Stop();
        }

        void LoadSound(AudioAlertType alertType)
        {
            var folder = Path.Combine(AppInfo.Location, "Sounds");

            string soundName = alertType.ToString() + ".wav";
            string soundLocation = Path.Combine(folder, soundName);

            if (File.Exists(soundLocation))
            {
                var player = new SoundPlayer(soundLocation);
                sounds[alertType] = player;
            }
        }
    }
}
