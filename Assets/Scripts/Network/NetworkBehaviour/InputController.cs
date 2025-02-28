using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class InputController : NetworkBehaviour, INetworkRunnerCallbacks
{
	[SerializeField] private Player player;
	[SerializeField] private Camera mainCamera;
	public static bool fetchInput = true;


	private NetworkInputData _inputData = new NetworkInputData();
	private Vector2 _moveInput;
	private Vector2 _viewInput;
	private bool _isJumping;

    private uint _buttonReset;
	private uint _buttonSample;

	/// <summary>
	/// Hook up to the Fusion callbacks so we can handle the input polling
	/// </summary>
	public override void Spawned()
	{
		// Technically, it does not really matter which InputController fills the input structure, since the actual data will only be sent to the one that does have authority,
		// but in the name of clarity, let's make sure we give input control to the gameobject that also has Input authority.
		if (Object.HasInputAuthority)
		{
			Runner.AddCallbacks(this);

            mainCamera = Camera.main;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
	}

	/// <summary>
	/// Get Unity input and store them in a struct for Fusion
	/// </summary>
	/// <param name="runner">The current NetworkRunner</param>
	/// <param name="input">The target input handler that we'll pass our data to</param>
	public void OnInput(NetworkRunner runner, NetworkInput input)
	{
		if (player != null && player.Object != null && player.state == PlayerState.Active)
		{
			_inputData.viewInput = _viewInput;
			_inputData.moveInput = _moveInput;
			_inputData.Buttons = _buttonSample;
            _inputData.aimForward = mainCamera.transform.forward;
			_inputData.isJumping = _isJumping;

            _buttonReset |= _buttonSample; // This effectively delays the reset of the read button flags until next Update() in case we're ticking faster than we're rendering
		}



		// Hand over the data to Fusion
		input.Set(_inputData);
		_inputData.Buttons = 0;
        _isJumping = false;

    }
		
	private void Update()
	{
		_buttonSample &= ~_buttonReset;

        if (Input.GetMouseButton(0))
            _buttonSample |= NetworkInputData.BUTTON_FIRE_PRIMARY;

        if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.R))
            _buttonSample |= NetworkInputData.BUTTON_FIRE_SECONDARY;

		if(Input.GetKeyDown(KeyCode.Space))
		{
			Debug.Log("Jumping");
            _isJumping = true;
        }

        _moveInput = Vector2.zero;

        _viewInput.x = Input.GetAxis("Mouse X");
        _viewInput.y = Input.GetAxis("Mouse Y") * -1;

		_moveInput.x = Input.GetAxis("Horizontal");
        _moveInput.y = Input.GetAxis("Vertical");
    }

	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
	public void OnConnectedToServer(NetworkRunner runner) { }
	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
		
	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
	public void OnSceneLoadDone(NetworkRunner runner) { }
	public void OnSceneLoadStart(NetworkRunner runner) { }
}

public struct NetworkInputData : INetworkInput
{
	public const uint BUTTON_FIRE_PRIMARY = 1 << 0;
	public const uint BUTTON_FIRE_SECONDARY = 1 << 1;

    public uint Buttons;
	public Vector2 viewInput;
	public Vector2 moveInput;
	public Vector3 aimForward;
    public bool isJumping;

    public bool IsUp(uint button)
	{
		return IsDown(button) == false;
	}

	public bool IsDown(uint button)
	{
		return (Buttons & button) == button;
	}

	public bool WasPressed(uint button, NetworkInputData oldInput)
	{
		return (oldInput.Buttons & button) == 0 && (Buttons&button)==button;
	}
}
