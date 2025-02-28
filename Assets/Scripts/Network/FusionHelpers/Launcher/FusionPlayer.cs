using Fusion;
using System;
using UnityEngine;

namespace FusionHelpers
{
    /// <summary>
    /// Base class for an object that will exist in exactly one instance per player.
    /// This *could* be the players avatar (visual game object), but it need not be - it's perfectly valid to just treat this as a data placeholder for the player.
    /// Implement the abstract InitNetworkState to initialize networked data on the State Authority.
    /// </summary>

    public abstract class FusionPlayer : NetworkBehaviour
	{

        [SerializeField] TickAlignedEventRelay _eventRelayPrefab;

		// This is set by FusionSession when the player spawns and should not be used by the application. Use PlayerIndex instead.
		[Networked] public int NetworkedPlayerIndex { private get; set; }
		[Networked] public string NetworkedPlayerName { private get; set; }
        [Networked] public NetworkTeamData NetworkedTeamData { get; set; }
        [Networked] public NetworkCharacterData NetworkedCharacterData { get; set; }
        [Networked] private TickAlignedEventRelay eventRelay { get; set; }
		[Networked] public bool IsBot { get; set; }


		// These are local properties so they remain valid when the network state goes away (also, they don't change during the life of the NO).
		public PlayerRef PlayerId { get; private set; } = PlayerRef.None;
		public int PlayerIndex { get; private set; } = -1;
		public string PlayerName { get; private set; }

        private TickAlignedEventRelay _eventStub;

		public override void Spawned()
		{
			
			// Getting this here because it will revert to -1 if the player disconnects, but we still want to remember the Id we were assigned for clean-up purposes
			PlayerId = Object.InputAuthority;
			PlayerIndex = NetworkedPlayerIndex;
            PlayerName = NetworkedPlayerName;
          
            Debug.Log("Player Name--- : " + NetworkedPlayerName);

            // The EventRelay only makes sense in shared mode, and most games don't support both hosted and shared,
            // so this particular check is very specific to Tanknarok.
            if (Runner.Topology == Topologies.Shared)
			{
				if(HasStateAuthority)
				{
                    eventRelay = Runner.Spawn(_eventRelayPrefab);
					eventRelay.transform.parent = transform;
                }
					
				_eventStub = eventRelay;
			}
			else
			{
				// For hosted mode, we avoid spawning the event relay since it doesn't really do anything except calling the local peer.
				_eventStub = gameObject.AddComponent<TickAlignedEventRelay>();
			}

			Debug.Log($"Spawned Player with InputAuth {PlayerId}, Index {PlayerIndex}");

			GameManager.Instance.AddPlayerAvatar(this);
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			Debug.Log($"Despawned Player with InputAuth {PlayerId}, Index {PlayerIndex}");

            GameManager.Instance.RemovePlayerAvatar(this);
		}

		protected void RegisterEventListener<T>(Action<T> listener) where T : unmanaged, INetworkEvent
		{
			_eventStub.RegisterEventListener(listener);
		}

		public void RaiseEvent<T>(T evt) where T:unmanaged, INetworkEvent
		{
			FusionPlayer stateAuth = Runner.GetPlayerObject(Runner.LocalPlayer).GetComponent<FusionPlayer>();
			stateAuth._eventStub.RaiseEventFor( _eventStub, evt);
		}

		public abstract void InitNetworkState();
	}
}

public struct NetworkTeamData : INetworkStruct
{
    public int TeamId;
    public int TeamScore;
    public int OwnerScore;

    public NetworkTeamData(int teamId, int teamScore, int ownerScore)
    {
        TeamId = teamId;
        TeamScore = teamScore;
        OwnerScore = ownerScore;
    }
}

public struct NetworkCharacterData : INetworkStruct
{
    public int SkinId;
	public int WeaponId;

    public NetworkCharacterData(int skinId, int weaponId)
    {
		this.SkinId = skinId;
		this.WeaponId = weaponId;
    }
}
