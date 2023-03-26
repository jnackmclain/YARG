using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using YARG.Data;
using YARG.Serialization;

namespace YARG {
	public static class SongLibrary {
		private const int CACHE_VERSION = 1;
		private class SongCacheJson {
			public int version = CACHE_VERSION;
			public List<SongInfo> songs;
		}

		public static DirectoryInfo songFolder = new(GetSongFolder());

		public static float loadPercent = 0f;

		/// <value>
		/// The location of the local or remote cache (depending on whether we are connected to a server).
		/// </value>
		public static FileInfo CacheFile => new(Path.Combine(songFolder.ToString(), "yarg_cache.json"));

		/// <value>
		/// A list of all of the playable songs.<br/>
		/// You must call <see cref="CreateSongInfoFromFiles"/> first.
		/// </value>
		public static List<SongInfo> Songs {
			get;
			private set;
		} = null;

		private static string GetSongFolder() {
			if (!string.IsNullOrEmpty(PlayerPrefs.GetString("songFolder"))) {
				// Load song folder from player prefs (if available)
				return PlayerPrefs.GetString("songFolder");
			} else {
				// Otherwise look for Clone Hero...
				var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				var cloneHeroPath = Path.Combine(documentsPath, $"Clone Hero{Path.DirectorySeparatorChar}Songs");
				var yargPath = Path.Combine(documentsPath, $"YARG{Path.DirectorySeparatorChar}Songs");

				if (Directory.Exists(cloneHeroPath)) {
					return cloneHeroPath;
				} else if (!Directory.Exists(yargPath)) {
					Directory.CreateDirectory(yargPath);
				}

				// And if not, create our own
				return yargPath;
			}
		}

		/// <summary>
		/// Should be called before you access <see cref="Songs"/>.
		/// </summary>
		public static bool FetchSongs() {
			if (Songs != null) {
				return true;
			}

			if (CacheFile.Exists || GameManager.client != null) {
				var success = ReadCache();
				if (success) {
					return true;
				}
			}

			ThreadPool.QueueUserWorkItem(_ => {
				Songs = new();

				loadPercent = 0f;
				CreateSongInfoFromFiles(songFolder);
				loadPercent = 0.1f;
				ReadSongIni();
				loadPercent = 0.9f;
				CreateCache();
				loadPercent = 1f;
			});
			return false;
		}

		/// <summary>
		/// Populate <see cref="Songs"/> with <see cref="songFolder"/> contents.<br/>
		/// This is create a basic <see cref="SongInfo"/> object for each song.<br/>
		/// We need to look at the <c>song.ini</c> files for more details.
		/// </summary>
		private static void CreateSongInfoFromFiles(DirectoryInfo songDir) {
			if (!songDir.Exists) {
				Directory.CreateDirectory(songDir.FullName);
			}

			foreach (var folder in songDir.EnumerateDirectories()) {
				if (new FileInfo(Path.Combine(folder.FullName, "song.ini")).Exists) {
					// If the folder has a song.ini, it is a song folder
					Songs.Add(new SongInfo(folder));
				} else {
					// Otherwise, treat it as a sub-folder
					CreateSongInfoFromFiles(folder);
				}
			}
		}

		/// <summary>
		/// Reads the <c>song.ini</c> for each <see cref="SongInfo"/> in <see cref="Songs"/>.<br/>
		/// <see cref="Songs"/> is expected to be populated.
		/// </summary>
		private static void ReadSongIni() {
			foreach (var song in Songs) {
				SongIni.CompleteSongInfo(song);

				// song.ini loading accounts for 80% of loading
				loadPercent += 1f / Songs.Count * 0.8f;
			}
		}

		/// <summary>
		/// Creates a cache from <see cref="Songs"/> so we don't have to read all of the <c>song.ini</c> again.<br/>
		/// <see cref="Songs"/> is expected to be populated and filled with <see cref="ReadSongIni"/>.
		/// </summary>
		private static void CreateCache() {
			var jsonObj = new SongCacheJson {
				songs = Songs
			};

			var json = JsonConvert.SerializeObject(jsonObj);
			Directory.CreateDirectory(CacheFile.DirectoryName);
			File.WriteAllText(CacheFile.ToString(), json.ToString());
		}

		/// <summary>
		/// Reads the song cache so we don't need to read of a the <c>song.ini</c> files.<br/>
		/// <see cref="CacheFile"/> should exist. If not, call <see cref="CreateCache"/>.
		/// </summary>
		private static bool ReadCache() {
			string json = File.ReadAllText(CacheFile.ToString());

			try {
				var jsonObj = JsonConvert.DeserializeObject<SongCacheJson>(json);
				if (jsonObj.version != CACHE_VERSION) {
					return false;
				}
			} catch (JsonException) {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Force reset songs. This makes the game re-scan if needed.
		/// </summary>
		public static void Reset() {
			Songs = null;
		}
	}
}