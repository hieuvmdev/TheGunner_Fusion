using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace FusionHelpers
{
	/// <summary>
	/// Base class for you per-session state class.
	/// You can use this to track and access player avatars on all peers.
	/// Override OnPlayerAvatarAdded/Removed to be notified of players joining/leaving *after* their avatar is created or removed.
	/// Use GetPlayer/GetPlayerByIndex/AllPlayers to access or iterate over players on all peers.
	/// Use Runner.GetSingleton/Runner.WaitForSingleton to get your custom session instance on all peers.
	/// </summary>

	public abstract class FusionSession : NetworkBehaviour
	{
		public const int MAX_PLAYERS = 4;
		protected const int MAX_TEAM = 4;
		
		[SerializeField] private FusionPlayer[] _playerPrefab;
		[SerializeField] private FusionPlayer[] _botPrefab;

		[Networked, Capacity(MAX_PLAYERS)] public NetworkDictionary<int, PlayerRef> playerRefByIndex { get; }
		[Networked, Capacity(MAX_TEAM)] public NetworkArray<int> teamSize { get; } = MakeInitializer(new int[MAX_TEAM]);

        private Dictionary<PlayerRef, FusionPlayer> _players = new();

		private List<FusionPlayer> _botList = new();

		protected abstract void OnPlayerAvatarAdded(FusionPlayer fusionPlayer);
		protected abstract void OnPlayerAvatarRemoved(FusionPlayer fusionPlayer);

		public IEnumerable<FusionPlayer> AllPlayers => _players.Values;
		public int PlayerCount => _players.Count;
		public int SessionCount => playerRefByIndex.Count;

		
		public override void Spawned()
		{
				  Debug.Log($"Spawned Network Session for Runner: {Runner}");
				  Runner.RegisterSingleton(this);
				
        }

		public override void Render()
		{

			if(Runner && Runner.Topology==Topologies.Shared && _players.Count!=playerRefByIndex.Count)
				MaybeSpawnNextAvatar(false);

		
		}

		private void MaybeSpawnNextAvatar(bool isDebug = true)
		{
			if(isDebug)
				Debug.Log("Step 2");
            foreach (KeyValuePair<int,PlayerRef> refByIndex in playerRefByIndex)
			{
                if (isDebug)
                    Debug.Log("Step 1");

				if (Runner.IsServer || (Runner.Topology == Topologies.Shared && refByIndex.Value == Runner.LocalPlayer))
				{
                    if (isDebug)
                        Debug.Log("Step 4" + refByIndex.Value);
                    if (!_players.TryGetValue(refByIndex.Value, out _))
					{
						int activeSkin = Global.Instance.GameD.ActiveSkin;
						int activeWeapon = Global.Instance.GameD.ActiveWeapon;

						Debug.Log($"I am State Auth for Player Index {refByIndex.Key} - Spawning Avatar");
						Runner.Spawn(_playerPrefab[activeSkin], Vector3.zero, Quaternion.identity, refByIndex.Value, (runner, o) =>
						{
							Runner.SetPlayerObject(refByIndex.Value, o);
							FusionPlayer player = o.GetComponent<FusionPlayer>();
							if (player != null)
							{
                                int teamIndex = GetTeamFill();

								Debug.Log("Set Team Index: " + teamIndex);

                                player.NetworkedPlayerIndex = refByIndex.Key;
								player.NetworkedPlayerName = Runner.GetLevelManager().GetPlayerName();
								player.NetworkedTeamData = new NetworkTeamData(teamIndex, GetTeamScore(teamIndex), 0);
								player.NetworkedCharacterData = new NetworkCharacterData(activeSkin, activeWeapon);

                                player.InitNetworkState();
			
							}

                         
                        });

                        // TODO: Implement Bot Spawning Logic If No Players
                        //SpawnBotList(refByIndex.Key, activeSkin, activeWeapon);
                        if (isDebug)
                            Debug.Log("Step 5" + refByIndex.Value);
                    }



                }
			}
		}

		private void SpawnBotList(int key, int activeTank, int tankLevel)
		{
			if(_botList.Count > 0 || playerRefByIndex.Count >= 2)
			{
				return;
			}

			int botTankLevel = Random.Range(tankLevel, tankLevel + 2);
			botTankLevel = Mathf.Clamp(botTankLevel, tankLevel, 5);

			FusionPlayer newBot = Runner.Spawn(_botPrefab[Random.Range(0, _botPrefab.Length)], Vector3.zero, Quaternion.identity, null, (runner, o) =>
			{
                FusionPlayer player = o.GetComponent<FusionPlayer>();
				if (player != null)
				{
					int teamIndex = GetTeamFill();

					player.NetworkedPlayerIndex = key + MAX_PLAYERS;
					player.NetworkedPlayerName = "JakeHash45";
					player.NetworkedTeamData = new NetworkTeamData(teamIndex, GetTeamScore(teamIndex), 0);
					player.NetworkedCharacterData = new NetworkCharacterData(activeTank, botTankLevel);
					player.IsBot = true; 

                    player.InitNetworkState();
				}
			});

			_botList.Add(newBot);
		}

		public void AddPlayerAvatar(FusionPlayer fusionPlayer)
		{
			Debug.Log($"Adding PlayerRef {fusionPlayer.PlayerId}");
            _players[fusionPlayer.PlayerId] = fusionPlayer;
            OnPlayerAvatarAdded(fusionPlayer);

            teamSize.Set(fusionPlayer.NetworkedTeamData.TeamId, teamSize[fusionPlayer.NetworkedTeamData.TeamId] + 1);
		}

		public void RemovePlayerAvatar(FusionPlayer fusionPlayer)
		{
			Debug.Log($"Removing PlayerRef {fusionPlayer.PlayerId}");
			_players.Remove(fusionPlayer.PlayerId);

			if(Object != null && Object.IsValid)
			{
                bool isSuccess = playerRefByIndex.Remove(fusionPlayer.PlayerIndex);

                if (!isSuccess)
                {
                    Debug.LogError("Failed to remove playerRefByIndex");
                }
                Debug.Log($"PlayerCount: {PlayerCount} - SessionCount: {SessionCount}");

                teamSize.Set(fusionPlayer.NetworkedTeamData.TeamId, teamSize[fusionPlayer.NetworkedTeamData.TeamId] - 1);

				if (teamSize[fusionPlayer.NetworkedTeamData.TeamId] < 0)
				{
                    teamSize.Set(fusionPlayer.NetworkedTeamData.TeamId, 0);
                }

            }
            OnPlayerAvatarRemoved(fusionPlayer);
        }

		public T GetPlayer<T>(PlayerRef plyRef) where T: FusionPlayer
		{
			_players.TryGetValue(plyRef, out FusionPlayer ply);
			return (T)ply;
		}

		public T GetPlayerByIndex<T>(int idx) where T: FusionPlayer
		{
			if(idx > MAX_PLAYERS)
			{
				idx -= MAX_PLAYERS;
			}

			foreach (FusionPlayer player in _players.Values)
			{
				if (player.Object!=null && player.Object.IsValid && player.PlayerIndex == idx)
					return (T)player;
			}
			return default;
		}

        public T GetPlayerByArrayIndex<T>(int idx) where T : FusionPlayer
        {
			int arrayIndex = 0;
            foreach (FusionPlayer player in _players.Values)
            {
                if (player.Object != null && player.Object.IsValid && arrayIndex == idx)
                    return (T)player;

                arrayIndex++;
            }
            return default;
        }

        private int NextPlayerIndex()
		{
			Debug.Log("Runner.Config.Simulation.PlayerCount: " + Runner.Config.Simulation.PlayerCount);

            for (int idx=0;idx<Runner.Config.Simulation.PlayerCount;idx++)
			{
				if (!playerRefByIndex.TryGet(idx, out _) )
					return idx;
			}
			Debug.LogWarning("No free player index!");
			return -1;
		}

		public void PlayerLeft(PlayerRef playerRef)
		{
			Debug.Log($"Player {playerRef} Left");

			if (!Runner.IsShutdown)
			{
				FusionPlayer player = GetPlayer<FusionPlayer>(playerRef);
				if (player && player.Object.IsValid)
				{
					Debug.Log($"Despawning PlayerAvatar for PlayerRef {player.PlayerId}");
					Runner.Despawn(player.Object);

				}
			}
		}

		public void PlayerJoined(PlayerRef player)
		{
			int nextIndex = NextPlayerIndex();

			Debug.Log($"I am {Runner.LocalPlayer} and I am {(Runner.IsServer ? "Server":"Master")}. The Session StateAuth is: {Object.StateAuthority} - Assigning Index {nextIndex} to PlayerRef {player}");
			Debug.Log($"PlayerCount: {PlayerCount} - SessionCount: {SessionCount}");

            playerRefByIndex.Set(nextIndex, player);
            Debug.Log("Step 3");
            MaybeSpawnNextAvatar();
		}

		protected int[] GetPlayerScores()
		{
			int[] scores = new int[MAX_TEAM];
			foreach (FusionPlayer player in _players.Values)
			{
				if (player.Object != null && player.Object.IsValid)
				{
					Debug.Log(player.PlayerName + "----------" + player.NetworkedTeamData.OwnerScore);
					scores[player.NetworkedTeamData.TeamId] += player.NetworkedTeamData.OwnerScore;
				}
			}

			return scores;
        }

        private int GetTeamFill()
        {
            //init variables
            int teamNo = 0;

			int min = teamSize[0];
            //loop over teams to find the lowest fill
            for (int i = 0; i < teamSize.Length; i++)
            {
                //if fill is lower than the previous value
                //store new fill and team for next iteration
                if (teamSize[i] < min)
                {
                    min = teamSize[i];
                    teamNo = i;
                }
            }


            //return index of lowest team
            return teamNo;
        }

		// Update Later
		private int GetTeamScore(int teamId)
		{
			return 0;
		}
    }
}