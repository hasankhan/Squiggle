using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Reflection;
using System.IO;

namespace Squiggle.UI
{

    enum AudioAlertType
    {
        BuddyOnline,
        BuddyOffline,
        MessageReceived,
    }

    class AudioAlert
    {
        public static AudioAlert Instance = LoadAlerts();

        Dictionary<AudioAlertType, Action> sounds = new Dictionary<AudioAlertType, Action>();

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
            Action playAction;
            if (Enabled && sounds.TryGetValue(alertType, out playAction))
                playAction();
        }

        void LoadSound(AudioAlertType alertType)
        {
            var folder = Path.Combine(AppInfo.Location, "Sounds");

            string soundName = alertType.ToString() + ".wav";
            string soundLocation = Path.Combine(folder, soundName);

            if (File.Exists(soundLocation))
            {
                var player = new SoundPlayer(soundLocation);
                sounds[alertType] = () => player.Play();
            }
        }
    }
}
