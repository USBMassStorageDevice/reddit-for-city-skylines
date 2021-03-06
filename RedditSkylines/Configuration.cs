﻿using ColossalFramework.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace RedditClient
{
    internal class Configuration
    {
        private const string TIMER_KEY = "updateFrequency";
        private const string FILTER_MESSAGES_KEY = "filterMessages";
        private const string LAST_ANNOUNCEMENT = "lastAnnouncementId";
        private const string ASSOCIATION_MODE = "associationMode";
        private const string HASHTAG_MODE = "hashtags";
        private const string CLICK_BEHAVIOUR = "clickBehaviour";

        public static List<string> Subreddits;
        public static int TimerInSeconds = 300;
        public static int AssociationMode = 0;
        public static int Hashtags = 1;
        public static int ClickBehaviour = 0;

        public static int FilterMessages = 0;
        public static int LastAnnouncement = 0;

        private static string ConfigPath
        {
            get
            {
                // base it on the path Cities: Skylines uses
                string path = string.Format("{0}/{1}/", DataLocation.localApplicationData, "ModConfig");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path += "reddit-for-chirpy.txt";

                return path;
            }
        }

        /// <summary>
        /// Attempts to load the configuration. If it fails, it'll load the defaults
        /// </summary>
        internal static void Load()
        {
            try
            {
                string[] configLines = File.ReadAllLines(ConfigPath);
                Regex r = new Regex("^[a-zA-Z0-9/]*$");

                Subreddits = new List<string>();
                bool requestSave = false;

                for (int i = 0; i < configLines.Length; ++i)
                {
                    // Remove unnecessary spaces
                    var line = configLines[i].Trim();

                    // Comment lines
                    if (line.StartsWith("#") || line.StartsWith(";"))
                        continue;

                    // Config options
                    if (line.StartsWith(TIMER_KEY + "="))
                    {
                        var time = line.Substring(TIMER_KEY.Length + 1);

                        int newTimer = -1;
                        if (Int32.TryParse(time, out newTimer) && newTimer >= 10)
                        {
                            TimerInSeconds = newTimer;
                        }
                    }
                    else if (line.StartsWith(ASSOCIATION_MODE + "="))
                    {
                        var sound = line.Substring(ASSOCIATION_MODE.Length + 1);

                        int mode = -1;
                        if (Int32.TryParse(sound, out mode) && (mode >= 0 || mode <= 2))
                        {
                            AssociationMode = mode;
                        }
                    }
                    else if (line.StartsWith(HASHTAG_MODE + "="))
                    {
                        var sound = line.Substring(HASHTAG_MODE.Length + 1);

                        int mode = -1;
                        if (Int32.TryParse(sound, out mode) && (mode >= 0 || mode <= 1))
                        {
                            Hashtags = mode;
                        }
                    }
                    else if (line.StartsWith(FILTER_MESSAGES_KEY + "="))
                    {
                        var filter = line.Substring(FILTER_MESSAGES_KEY.Length + 1);

                        int newVal = -1;
                        if (Int32.TryParse(filter, out newVal) && (newVal >= 0 || newVal <= 2))
                        {
                            FilterMessages = newVal;
                        }
                    }
                    else if (line.StartsWith(LAST_ANNOUNCEMENT + "="))
                    {
                        var announcement = line.Substring(LAST_ANNOUNCEMENT.Length + 1);

                        int newVal = 0;
                        if (Int32.TryParse(announcement, out newVal))
                        {
                            LastAnnouncement = newVal;
                        }
                    }
                    else if (line.StartsWith(CLICK_BEHAVIOUR + "="))
                    {
                        var clickb = line.Substring(CLICK_BEHAVIOUR.Length + 1);

                        int newVal = 0;
                        if (Int32.TryParse(clickb, out newVal) && (newVal >= 0 || newVal <= 3))
                        {
                            ClickBehaviour = newVal;
                        }
                    }

                    // Just reddit names, presumably
                    else if (line.Length > 1 && r.IsMatch(line))
                    {
                        if (line.IndexOf('/') == -1)
                        {
                            line = string.Format("/r/{0}/new", line);
                            configLines[i] = line;

                            requestSave = true;
                        }
                        Subreddits.Add(line);
                    }
                }

                if (requestSave)
                {
                    using (StreamWriter sw = new StreamWriter(ConfigPath))
                    {
                        foreach (string line in configLines)
                            sw.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("Reddit: Config regenerated {0}: {1}", e.GetType().ToString(), e.Message));

                TimerInSeconds = 300;
                AssociationMode = 0;
                FilterMessages = 0;
                Subreddits = DefaultSubreddits;
                LastAnnouncement = 0;
                Hashtags = 1;

                SaveConfig(true);
            }
        }

        internal static void SaveConfig(bool defaultConfig)
        {
            using (StreamWriter sw = new StreamWriter(ConfigPath))
            {
                sw.WriteLine("# Reddit for Chirpy");
                sw.WriteLine("# https://github.com/mabako/reddit-for-city-skylines/wiki/Configuration");
                sw.WriteLine();

                sw.WriteLine("# How often should new messages be displayed?");
                sw.WriteLine("# Default: 300 (which is 5 minutes)");
                sw.WriteLine("{0}={1}", TIMER_KEY, TimerInSeconds);
                sw.WriteLine();

                sw.WriteLine("# How should names be handled?");
                sw.WriteLine("# Default: 0 (disabled)");
                sw.WriteLine("# Set this to '1' to use CIM names instead of reddit usernames.");
                sw.WriteLine("# Set this to '2' to permanently rename CIMs to reddit users.");
                sw.WriteLine("{0}={1}", ASSOCIATION_MODE, AssociationMode);

                sw.WriteLine();
                sw.WriteLine("# One subreddit per line");

                foreach (string subreddit in Subreddits)
                    sw.WriteLine("{0}", subreddit);

                if (defaultConfig)
                {
                    sw.WriteLine();
                    sw.WriteLine("# Multireddit example (remove the '#' to use)");
                    sw.WriteLine("# /user/ccaatt/m/chirps/new");
                }

                sw.WriteLine();
                sw.WriteLine("# Filters some or all chirps made by your citizen if enabled");
                sw.WriteLine("# Default: 0 (disabled)");
                sw.WriteLine("# Set this to '1' to hide useless chirps");
                sw.WriteLine("# Set this to '2' to hide all chirps");
                sw.WriteLine("# This may break mod compatibility.");
                sw.WriteLine("{0}={1}", FILTER_MESSAGES_KEY, FilterMessages);

                sw.WriteLine();
                sw.WriteLine("# Enable automated hashtags?");
                sw.WriteLine("{0}={1}", HASHTAG_MODE, Hashtags);

                sw.WriteLine();
                sw.WriteLine("# What should happen when you click on reddit chirps?");
                sw.WriteLine("# Default: 0 (open Steam Overlay)");
                sw.WriteLine("# Set this to '1' to copy to clipboard");
                sw.WriteLine("# Set this to '2' to open your system browser");
                sw.WriteLine("# Set this to '3' for nothing to happen");
                sw.WriteLine("{0}={1}", CLICK_BEHAVIOUR, ClickBehaviour);

                sw.WriteLine();
                sw.WriteLine("# INTERNAL CONFIG");
                sw.WriteLine("# Make sure to show announcements only once.");
                sw.WriteLine("{0}={1}", LAST_ANNOUNCEMENT, LastAnnouncement);
            }
        }

        private static List<string> DefaultSubreddits
        {
            get
            {
                var s = new List<string>();
                s.Add("/r/ShowerThoughts/rising/");
                s.Add("/r/CrazyIdeas/new/");
                s.Add("/r/ChirpIt/new/");
                return s;
            }
        }
    }
}
