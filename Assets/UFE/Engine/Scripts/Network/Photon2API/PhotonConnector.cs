using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UFE3D;
//using PlayFabIntegration.API;
//using PlayFab;
//using PlayFab.ClientModels;

public class PhotonConnector : MonoBehaviourPunCallbacks, IOnEventCallback
{
	#region public event definitions: Common Events
	public event MultiplayerAPI.OnInitializationErrorDelegate OnInitializeError;
	public event MultiplayerAPI.OnInitializationSuccessfulDelegate OnInitializeSuccessful;
	public event MultiplayerAPI.OnMessageReceivedDelegate OnMessageReceived;
	#endregion

	#region public class event definitions: Client Events
	public event MultiplayerAPI.OnDisconnectionDelegate OnDisconnection;
	public event MultiplayerAPI.OnJoinedDelegate OnJoined;
	public event MultiplayerAPI.OnJoinErrorDelegate OnJoinError;
	public event MultiplayerAPI.OnMatchesDiscoveredDelegate OnMatchesDiscovered;
	public event MultiplayerAPI.OnMatchDiscoveryErrorDelegate OnMatchDiscoveryError;
	#endregion

	#region public event definitions: Server Events
	public event MultiplayerAPI.OnMatchCreatedDelegate OnMatchCreated;
	public event MultiplayerAPI.OnMatchCreationErrorDelegate OnMatchCreationError;
	public event MultiplayerAPI.OnMatchDestroyedDelegate OnMatchDestroyed;
	public event MultiplayerAPI.OnPlayerConnectedToMatchDelegate OnPlayerConnectedToMatch;
	public event MultiplayerAPI.OnPlayerDisconnectedFromMatchDelegate OnPlayerDisconnectedFromMatch;
	#endregion

	#region public instance properties
	//public PlayFabConnector PlayFabConnector{get;set;}
    public bool debugInfo = true;
	#endregion

	#region protected instance fields
	protected bool _isMasterClient = false;
	protected string _uuid = null;
	protected List<RoomInfo> _rooms = new List<RoomInfo>();
	#endregion

	#region public override properties
	public virtual int Connections{
		get{
			if (PhotonNetwork.CurrentRoom != null){
				return PhotonNetwork.CurrentRoom.PlayerCount - 1;
			}

			return 0;
		}
	}

	public virtual MultiplayerAPI.PlayerInformation Player{
		get{
			return new MultiplayerAPI.PlayerInformation(PhotonNetwork.LocalPlayer);
		}
	}

	public virtual float SendRate{
		get{
			return PhotonNetwork.SendRate;
		}
		set{
			PhotonNetwork.SendRate = Mathf.CeilToInt(value);
			PhotonNetwork.SerializationRate = PhotonNetwork.SendRate;
		}
	}
	#endregion

	#region public override methods
	public virtual void CreateMatch (MultiplayerAPI.MatchCreationRequest request){
		this._isMasterClient = false;


		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsOpen = true;
		roomOptions.IsVisible = request.publicMatch;
		roomOptions.MaxPlayers = (byte)request.maxPlayers;

		PhotonNetwork.CreateRoom(request.matchName, roomOptions, null);
	}

	public virtual void DestroyMatch(){
		if (this._isMasterClient){
			PhotonNetwork.CurrentRoom.IsOpen = false;
			PhotonNetwork.CurrentRoom.IsVisible = false;
		}

		PhotonNetwork.LeaveRoom();
	}

	public virtual void DisconnectFromMatch(){
		PhotonNetwork.LeaveRoom();
	}

	public virtual bool IsInsideLobby(){
		return PhotonNetwork.InLobby;
	}

	public virtual NetworkState GetConnectionState(){
		//if (PhotonNetwork.connectionState == ConnectionState.Connected && PhotonNetwork.inRoom){
		if (PhotonNetwork.NetworkClientState == ClientState.Joined) {
			if (this._isMasterClient){
				return NetworkState.Server;
			}else{
				return NetworkState.Client;
			}
		}else{
			return NetworkState.Disconnected;
		}
	}

	public virtual int GetLastPing(){
		return PhotonNetwork.GetPing();
	}

	public virtual void Initialize(string uuid){
		if (PhotonNetwork.IsConnected){
			this.RaiseOnInitializationSuccessful();

		}else{
			//PhotonNetwork.autoJoinLobby = true;
			this._isMasterClient = false;
			this._uuid = uuid;

			if (UFE.config.networkOptions.photonHostingService == PhotonHostingService.PhotonCloud){
				PhotonNetwork.ConnectToBestCloudServer();

			}else if (UFE.config.networkOptions.photonHostingService == PhotonHostingService.PhotonServer){
				PhotonNetwork.ConnectUsingSettings();

			/*}else if (UFE.config.networkOptions.photonHostingService == PhotonHostingService.PlayFab){
				if (this.PlayFabConnector.IsLogged()){
					this.PlayFabConnector.GetPhotonAuthenticationToken(
						delegate(GetPhotonAuthenticationTokenResult result) {
							if (PhotonNetwork.AuthValues == null){
								PhotonNetwork.AuthValues = new AuthenticationValues();
							}

							PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
							//PhotonNetwork.AuthValues.AddAuthParameter("username", result.PlayFabId);
							PhotonNetwork.AuthValues.AddAuthParameter("token", result.PhotonCustomAuthenticationToken);
							PhotonNetwork.ConnectUsingSettings(uuid);

						}
						,
						delegate(PlayFabError error) {
							this.RaiseOnInitializationError();
						}
					);
				}else{
					this.PlayFabConnector.Login(
						delegate(LoginResult result){
							this.PlayFabConnector.GetPhotonAuthenticationToken(
								delegate(GetPhotonAuthenticationTokenResult result2) {
									if (PhotonNetwork.AuthValues == null){
										PhotonNetwork.AuthValues = new AuthenticationValues();
									}

									PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
									PhotonNetwork.AuthValues.AddAuthParameter("username", result.PlayFabId);
									PhotonNetwork.AuthValues.AddAuthParameter("token", result2.PhotonCustomAuthenticationToken);
									PhotonNetwork.ConnectUsingSettings(uuid);

								}
								,
								delegate(PlayFabError error) {
									this.RaiseOnInitializationError();
								}
							);
						}
						,
						delegate(PlayFabError error){
							this.RaiseOnInitializationError();
						}
					);
				}*/
			}
		}
	}

	public virtual void JoinMatch(MultiplayerAPI.MatchInformation match, string password = null)
    {
        if (debugInfo) Debug.Log("PhotonConnector.JoinMatch");
        this._isMasterClient = false;
        PhotonNetwork.JoinRoom(match.matchName);
	}

	public virtual void JoinRandomMatch()
    {
        if (debugInfo) Debug.Log("PhotonConnector.JoinRandomMatch");
        this._isMasterClient = false;
		PhotonNetwork.JoinRandomRoom();
	}

	public virtual void StartSearchingMatches(int startPage = 0, int pageSize = 20, string filter = null)
    {
		if (debugInfo) Debug.Log("PhotonConnector.StartSearchingMatches -> " + PhotonNetwork.NetworkClientState);
		 
		if
		(
			this._rooms != null &&
			PhotonNetwork.NetworkClientState == ClientState.JoinedLobby
		)
        {
            if (debugInfo) Debug.Log("PhotonConnector.StartSearchingMatches");

            List<MultiplayerAPI.MatchInformation> matches = new List<MultiplayerAPI.MatchInformation>();

			for (int i = startPage * pageSize; i < this._rooms.Count && i < (startPage + 1) * pageSize; ++i){
				if (this._rooms[i] != null){
                    matches.Add(this.GetMatchInformation(this._rooms[i]));
				}
			}

			this.RaiseOnMatchesDiscovered(new ReadOnlyCollection<MultiplayerAPI.MatchInformation>(matches));
		}else{
			this.RaiseOnMatchDiscoveryError();
		}
	}

	public virtual void StopSearchingMatches(){}

	public virtual bool SendNetworkMessage(byte[] bytes){
		return PhotonNetwork.RaiseEvent(0, bytes, RaiseEventOptions.Default, SendOptions.SendUnreliable);
	}
	#endregion

	#region protected instance methods
	private void OnEvent(byte eventcode, object content, int senderid){
		if (eventcode == 0){
			// TODO: Check if the sender is allowed to send this message
			//Player sender = Player.Find(senderid);

			this.RaiseOnMessageReceived((byte[])content);
		}
	}
	#endregion

	#region IOnEventCallback methods
	public virtual void OnEvent(EventData photonEvent)
    {
		this.OnEvent(photonEvent.Code, photonEvent.CustomData, photonEvent.Sender);
    }
    #endregion

    #region MonoBehaviourPunCallbacks methods
    /// <summary>
    /// Called when the initial connection got established but before you can use the server. OnJoinedLobby() or OnConnectedToMaster() are called when PUN is ready.
    /// </summary>
    /// <remarks>
    /// This callback is only useful to detect if the server can be reached at all (technically).
    /// Most often, it's enough to implement OnFailedToConnectToPhoton() and OnDisconnectedFromPhoton().
    ///
    /// <i>OnJoinedLobby() or OnConnectedToMaster() are called when PUN is ready.</i>
    ///
    /// When this is called, the low level connection is established and PUN will send your AppId, the user, etc in the background.
    /// This is not called for transitions from the masterserver to game servers.
    /// </remarks>
    public override void OnConnected()
    {
		if (debugInfo) Debug.Log("PhotonConnector.OnConnected");
		this.RaiseOnInitializationSuccessful();
	}

	/// <summary>
	/// Called when the local user/client left a room.
	/// </summary>
	/// <remarks>
	/// When leaving a room, PUN brings you back to the Master Server.
	/// Before you can use lobbies and join or create rooms, OnJoinedLobby() or OnConnectedToMaster() will get called again.
	/// </remarks>
	public override void OnLeftRoom()
    {
        if (debugInfo) Debug.Log("PhotonConnector.OnLeftRoom");

		if (this._isMasterClient){
			this._isMasterClient = false;
			this.RaiseOnMatchDestroyed();
		}else{
			this.RaiseOnDisconnection();
		}
	}

	/// <summary>
	/// Called after switching to a new MasterClient when the current one leaves.
	/// </summary>
	/// <remarks>
	/// This is not called when this client enters a room.
	/// The former MasterClient is still in the player list when this method get called.
	/// </remarks>
	public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
	{
		if (debugInfo) Debug.Log("PhotonConnector.OnMasterClientSwitched");
	}

	/// <summary>
	/// Called when a CreateRoom() call failed. The parameter provides ErrorCode and message (as array).
	/// </summary>
	/// <remarks>
	/// Most likely because the room name is already in use (some other client was faster than you).
	/// PUN logs some info if the PhotonNetwork.logLevel is >= PhotonLogLevel.Informational.
	/// </remarks>
	/// <param name="codeAndMsg">codeAndMsg[0] is a short ErrorCode and codeAndMsg[1] is a string debug msg.</param>
	public override void OnCreateRoomFailed(short errorCode, string errorMessage)
    {
		if (debugInfo) Debug.Log("PhotonConnector.OnPhotonCreateRoomFailed: " + errorCode + "\n" + errorMessage);

		this._isMasterClient = false;
		this.RaiseOnMatchCreationError();
	}

	/// <summary>
	/// Called when a JoinRoom() call failed. The parameter provides ErrorCode and message (as array).
	/// </summary>
	/// <remarks>
	/// Most likely error is that the room does not exist or the room is full (some other client was faster than you).
	/// PUN logs some info if the PhotonNetwork.logLevel is >= PhotonLogLevel.Informational.
	/// </remarks>
	/// <param name="codeAndMsg">codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.</param>
	public override void OnJoinRoomFailed(short errorCode, string errorMessage)
    {
		if (debugInfo) Debug.Log("PhotonConnector.OnPhotonJoinRoomFailed: " + errorCode + "\n" + errorMessage);

		this._isMasterClient = false;
		this.RaiseOnJoinError();
	}

	/// <summary>
	/// Called when this client created a room and entered it. OnJoinedRoom() will be called as well.
	/// </summary>
	/// <remarks>
	/// This callback is only called on the client which created a room (see PhotonNetwork.CreateRoom).
	///
	/// As any client might close (or drop connection) anytime, there is a chance that the
	/// creator of a room does not execute OnCreatedRoom.
	///
	/// If you need specific room properties or a "start signal", it is safer to implement
	/// OnMasterClientSwitched() and to make the new MasterClient check the room's state.
	/// </remarks>
	public override void OnCreatedRoom()
    {
		if (debugInfo) Debug.Log("PhotonConnector.OnCreatedRoom");
	}

	/// <summary>
	/// Called on entering a lobby on the Master Server. The actual room-list updates will call OnReceivedRoomListUpdate().
	/// </summary>
	/// <remarks>
	/// Note: When PhotonNetwork.autoJoinLobby is false, OnConnectedToMaster() will be called and the room list won't become available.
	///
	/// While in the lobby, the roomlist is automatically updated in fixed intervals (which you can't modify).
	/// The room list gets available when OnReceivedRoomListUpdate() gets called after OnJoinedLobby().
	/// </remarks>
	public override void OnJoinedLobby()
    {
		if (debugInfo) Debug.Log("PhotonConnector.OnJoinedLobby");
		this._isMasterClient = false;
	}

	/// <summary>
	/// Called after leaving a lobby.
	/// </summary>
	/// <remarks>
	/// When you leave a lobby, [CreateRoom](@ref PhotonNetwork.CreateRoom) and [JoinRandomRoom](@ref PhotonNetwork.JoinRandomRoom)
	/// automatically refer to the default lobby.
	/// </remarks>
	public override void OnLeftLobby()
    {
		if (debugInfo) Debug.Log("PhotonConnector.OnLeftLobby");
		PhotonNetwork.JoinLobby();
	}

	/// <summary>
	/// Called if a connect call to the Photon server failed before the connection was established, followed by a call to OnDisconnectedFromPhoton().
	/// </summary>
	/// <remarks>
	/// This is called when no connection could be established at all.
	/// It differs from OnConnectionFail, which is called when an existing connection fails.
	/// </remarks>
	public override void OnDisconnected(DisconnectCause cause)
    {
		if (debugInfo) Debug.Log("PhotonConnector.OnDisconnected : " + cause);
		this.RaiseOnInitializationError();
	}

	///// <summary>
	///// Called on all scripts on a GameObject (and children) that have been Instantiated using PhotonNetwork.Instantiate.
	///// </summary>
	///// <remarks>
	///// PhotonMessageInfo parameter provides info about who created the object and when (based off PhotonNetworking.time).
	///// </remarks>
	//public override void OnPhotonInstantiate(PhotonMessageInfo info)
	//{
	//	if (debugInfo) Debug.Log("PhotonConnector.OnPhotonInstantiate");
	//}

	/// <summary>
	/// Called for any update of the room-listing while in a lobby (PhotonNetwork.InLobby) on the Master Server.
	/// </summary>
	/// <remarks>
	/// PUN provides the list of rooms by PhotonNetwork.GetRoomList().<br/>
	/// Each item is a RoomInfo which might include custom properties (provided you defined those as lobby-listed when creating a room).
	///
	/// Not all types of lobbies provide a listing of rooms to the client. Some are silent and specialized for server-side matchmaking.
	/// </remarks>
	public override void OnRoomListUpdate(List<RoomInfo> rooms)
    {
		this._rooms = rooms;
		if (debugInfo) Debug.Log("PhotonConnector.OnReceivedRoomListUpdate -> " + rooms.Count);
	}

	/// <summary>
	/// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
	/// </summary>
	/// <remarks>
	/// This method is commonly used to instantiate player characters.
	/// If a match has to be started "actively", you can call an [PunRPC](@ref PhotonView.RPC) triggered by a user's button-press or a timer.
	///
	/// When this is called, you can usually already access the existing players in the room via PhotonNetwork.playerList.
	/// Also, all custom properties should be already available as Room.customProperties. Check Room.playerCount to find out if
	/// enough players are in the room to start playing.
	/// </remarks>
	public override void OnJoinedRoom()
    {
		this._isMasterClient = PhotonNetwork.LocalPlayer == PhotonNetwork.MasterClient;
		if (debugInfo) Debug.Log("PhotonConnector.OnJoinedRoom | Players = " + PhotonNetwork.CurrentRoom.PlayerCount + " | Master Client = " + this._isMasterClient);

		if (PhotonNetwork.CurrentRoom != null){
			if (this._isMasterClient){
				this.RaiseOnMatchCreated(new MultiplayerAPI.CreatedMatchInformation(PhotonNetwork.CurrentRoom.Name));
			}else{
				this.RaiseOnJoined(new MultiplayerAPI.JoinedMatchInformation(PhotonNetwork.CurrentRoom.Name));
			}
		}
	}

	/// <summary>
	/// Called when a remote player entered the room. This Player is already added to the playerlist at this time.
	/// </summary>
	/// <remarks>
	/// If your game starts with a certain number of players, this callback can be useful to check the
	/// Room.playerCount and find out if you can start.
	/// </remarks>
	public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
		if (debugInfo) Debug.Log("PhotonConnector.OnPlayerConnected");

		if (this._isMasterClient){
			PhotonNetwork.CurrentRoom.IsOpen = false;
			PhotonNetwork.CurrentRoom.IsVisible = false;
		}

		this.RaiseOnPlayerConnectedToMatch(new MultiplayerAPI.PlayerInformation(newPlayer));
	}

	/// <summary>
	/// Called when a remote player left the room. This Player is already removed from the playerlist at this time.
	/// </summary>
	/// <remarks>
	/// When your client calls PhotonNetwork.leaveRoom, PUN will call this method on the remaining clients.
	/// When a remote client drops connection or gets closed, this callback gets executed. after a timeout
	/// of several seconds.
	/// </remarks>
	public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
		if (debugInfo) Debug.Log("PhotonConnector.OnPlayerDisconnected");
		this.RaiseOnPlayerDisconnectedFromMatch(new MultiplayerAPI.PlayerInformation(otherPlayer));
	}

	/// <summary>
	/// Called when a JoinRandom() call failed. The parameter provides ErrorCode and message.
	/// </summary>
	/// <remarks>
	/// Most likely all rooms are full or no rooms are available. <br/>
	/// When using multiple lobbies (via JoinLobby or TypedLobby), another lobby might have more/fitting rooms.<br/>
	/// PUN logs some info if the PhotonNetwork.logLevel is >= PhotonLogLevel.Informational.
	/// </remarks>
	/// <param name="codeAndMsg">codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.</param>
	public override void OnJoinRandomFailed(short errorCode, string errorMessage)
    {
        if (debugInfo) Debug.Log("PhotonConnector.OnPhotonRandomJoinFailed: " + errorCode + "\n" + errorMessage);

		this._isMasterClient = false;
		this.RaiseOnJoinError();
	}

	/// <summary>
	/// Called after the connection to the master is established and authenticated but only when PhotonNetwork.autoJoinLobby is false.
	/// </summary>
	/// <remarks>
	/// If you set PhotonNetwork.autoJoinLobby to true, OnJoinedLobby() will be called instead of this.
	///
	/// You can join rooms and create them even without being in a lobby. The default lobby is used in that case.
	/// The list of available rooms won't become available unless you join a lobby via PhotonNetwork.joinLobby.
	/// </remarks>
	public override void OnConnectedToMaster(){
		if (debugInfo) Debug.Log("PhotonConnector.OnConnectedToMaster");
		PhotonNetwork.JoinLobby();
	}

	/// <summary>
	/// Called when a room's custom properties changed. The propertiesThatChanged contains all that was set via Room.SetCustomProperties.
	/// </summary>
	/// <remarks>
	/// Since v1.25 this method has one parameter: Hashtable propertiesThatChanged.<br/>
	/// Changing properties must be done by Room.SetCustomProperties, which causes this callback locally, too.
	/// </remarks>
	/// <param name="propertiesThatChanged"></param>
	public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged){
		StringBuilder sb = new StringBuilder();
		foreach (object key in propertiesThatChanged.Keys){
			if (sb.Length > 0){
				sb.Append(", ");
			}
			sb.Append(key).Append(" = ").Append(propertiesThatChanged[key]);
		}

		if (debugInfo) Debug.Log("PhotonConnector.OnPhotonCustomRoomPropertiesChanged: {" + sb.ToString() + "}");
		//Debug.Log("OnPhotonCustomRoomPropertiesChanged");
	}

	/// <summary>
	/// Called when custom player-properties are changed. Player and the changed properties are passed as object[].
	/// </summary>
	/// <remarks>
	/// Since v1.25 this method has one parameter: object[] playerAndUpdatedProps, which contains two entries.<br/>
	/// [0] is the affected Player.<br/>
	/// [1] is the Hashtable of properties that changed.<br/>
	///
	/// We are using a object[] due to limitations of Unity's GameObject.SendMessage (which has only one optional parameter).
	///
	/// Changing properties must be done by Player.SetCustomProperties, which causes this callback locally, too.
	///
	/// Example:<pre>
	/// void OnPlayerPropertiesChanged(object[] playerAndUpdatedProps) {
	///     Player player = playerAndUpdatedProps[0] as Player;
	///     Hashtable props = playerAndUpdatedProps[1] as Hashtable;
	///     //...
	/// }</pre>
	/// </remarks>
	/// <param name="playerAndUpdatedProps">Contains Player and the properties that changed See remarks.</param>
	public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player player, ExitGames.Client.Photon.Hashtable hashtable)
	{
		if (debugInfo) Debug.Log("PhotonConnector.OnPlayerPropertiesChanged");
	}

	/// <summary>
	/// Called when the server sent the response to a FindFriends request and updated PhotonNetwork.Friends.
	/// </summary>
	/// <remarks>
	/// The friends list is available as PhotonNetwork.Friends, listing name, online state and
	/// the room a user is in (if any).
	/// </remarks>
	public override void OnFriendListUpdate(List<FriendInfo> friends){
		if (debugInfo) Debug.Log("PhotonConnector.OnUpdatedFriendList");
	}

	/// <summary>
	/// Called when the custom authentication failed. Followed by disconnect!
	/// </summary>
	/// <remarks>
	/// Custom Authentication can fail due to user-input, bad tokens/secrets.
	/// If authentication is successful, this method is not called. Implement OnJoinedLobby() or OnConnectedToMaster() (as usual).
	///
	/// During development of a game, it might also fail due to wrong configuration on the server side.
	/// In those cases, logging the debugMessage is very important.
	///
	/// Unless you setup a custom authentication service for your app (in the [Dashboard](https://www.exitgames.com/dashboard)),
	/// this won't be called!
	/// </remarks>
	/// <param name="debugMessage">Contains a debug message why authentication failed. This has to be fixed during development time.</param>
	public override void OnCustomAuthenticationFailed(string debugMessage){
		if (debugInfo) Debug.Log("PhotonConnector.OnCustomAuthenticationFailed: " + debugMessage);
	}

	/// <summary>
	/// Called when your Custom Authentication service responds with additional data.
	/// </summary>
	/// <remarks>
	/// Custom Authentication services can include some custom data in their response.
	/// When present, that data is made available in this callback as Dictionary.
	/// While the keys of your data have to be strings, the values can be either string or a number (in Json).
	/// You need to make extra sure, that the value type is the one you expect. Numbers become (currently) int64.
	///
	/// Example: void OnCustomAuthenticationResponse(Dictionary&lt;string, object&gt; data) { ... }
	/// </remarks>
	/// <see cref="https://doc.photonengine.com/en/realtime/current/reference/custom-authentication"/>
	public override void OnCustomAuthenticationResponse(Dictionary<string, object> data){
		if (debugInfo) Debug.Log("PhotonConnector.OnCustomAuthenticationResponse");
	}

	/// <summary>
	/// Called by PUN when the response to a WebRPC is available. See PhotonNetwork.WebRPC.
	/// </summary>
	/// <remarks>
	/// Important: The response.ReturnCode is 0 if Photon was able to reach your web-service.
	/// The content of the response is what your web-service sent. You can create a WebResponse instance from it.
	/// Example: WebRpcResponse webResponse = new WebRpcResponse(operationResponse);
	///
	/// Please note: Class OperationResponse is in a namespace which needs to be "used":
	/// using ExitGames.Client.Photon;  // includes OperationResponse (and other classes)
	///
	/// The OperationResponse.ReturnCode by Photon is:<pre>
	///  0 for "OK"
	/// -3 for "Web-Service not configured" (see Dashboard / WebHooks)
	/// -5 for "Web-Service does now have RPC path/name" (at least for Azure)</pre>
	/// </remarks>
	public override void OnWebRpcResponse(OperationResponse response){
		if (debugInfo) Debug.Log("PhotonConnector.OnWebRpcResponse");
	}

	///// <summary>
	///// Called when another player requests ownership of a PhotonView from you (the current owner).
	///// </summary>
	///// <remarks>
	///// The parameter viewAndPlayer contains:
	/////
	///// PhotonView view = viewAndPlayer[0] as PhotonView;
	/////
	///// Player requestingPlayer = viewAndPlayer[1] as Player;
	///// </remarks>
	///// <param name="viewAndPlayer">The PhotonView is viewAndPlayer[0] and the requesting player is viewAndPlayer[1].</param>
	//public override void OnOwnershipRequest(PhotonView view, Photon.Realtime.Player player){
	//	if (debugInfo) Debug.Log("PhotonConnector.OnOwnershipRequest");
	//}

	/// <summary>
	/// Called when the Master Server sent an update for the Lobby Statistics, updating PhotonNetwork.LobbyStatistics.
	/// </summary>
	/// <remarks>
	/// This callback has two preconditions:
	/// EnableLobbyStatistics must be set to true, before this client connects.
	/// And the client has to be connected to the Master Server, which is providing the info about lobbies.
	/// </remarks>
	public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyInfos){
		if (debugInfo) Debug.Log("PhotonConnector.OnLobbyStatisticsUpdate");
	}
	#endregion

	#region protected instance methods
	protected virtual MultiplayerAPI.MatchInformation GetMatchInformation(RoomInfo room)
    {
        if (debugInfo) Debug.Log("PhotonConnector.GetMatchInformation");
        MultiplayerAPI.MatchInformation match = new MultiplayerAPI.MatchInformation();
		//this.averageEloScore =
		match.currentPlayers = room.PlayerCount;
		match.isPublic = room.IsOpen && room.IsVisible && !room.RemovedFromList;
		match.matchName = room.Name;
		match.maxPlayers = room.MaxPlayers;


		//match.directConnectInfos = new List<MatchDirectConnectInfo>();
		//match.matchAttributes = new Dictionary<string, long>();
		//room.customProperties // Hashtable

		return match;
	}
	#endregion

	#region protected instance methods: Common Events
	protected virtual void RaiseOnInitializationError()
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnInitializationError");
        if (this.OnInitializeError != null){
			this.OnInitializeError();
		}
	}

	protected virtual void RaiseOnInitializationSuccessful()
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnInitializationSuccessful");
        if (this.OnInitializeSuccessful != null){
			this.OnInitializeSuccessful();
		}
	}

	protected virtual void RaiseOnMessageReceived(byte[] bytes)
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnMessageReceived");
        if (this.OnMessageReceived != null){
			this.OnMessageReceived(bytes);
		}
	}
	#endregion

	#region protected instance methods: Client Events
	protected virtual void RaiseOnDisconnection()
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnDisconnection");
        if (this.OnDisconnection != null){
			this.OnDisconnection();
		}
	}

	protected virtual void RaiseOnJoined(MultiplayerAPI.JoinedMatchInformation match)
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnJoined");
        if (this.OnJoined != null){
			this.OnJoined(match);
		}
	}

	protected virtual void RaiseOnJoinError()
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnJoinError");
        if (this.OnJoinError != null){
			this.OnJoinError();
		}
	}

	protected virtual void RaiseOnMatchesDiscovered(ReadOnlyCollection<MultiplayerAPI.MatchInformation> matches)
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnMatchesDiscovered -> " + matches.Count);
        if (this.OnMatchesDiscovered != null){
			this.OnMatchesDiscovered(matches);
		}
	}

	protected virtual void RaiseOnMatchDiscoveryError()
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnMatchDiscoveryError");
        if (this.OnMatchDiscoveryError != null){
			this.OnMatchDiscoveryError();
		}
	}
	#endregion

	#region protected instance methods: Server Events
	protected virtual void RaiseOnMatchCreated(MultiplayerAPI.CreatedMatchInformation match)
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnMatchCreated");
        if (this.OnMatchCreated != null){
			this.OnMatchCreated(match);
		}
	}

	protected virtual void RaiseOnMatchCreationError()
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnMatchCreationError");
        if (this.OnMatchCreationError != null){
			this.OnMatchCreationError();
		}
	}

	protected virtual void RaiseOnMatchDestroyed()
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnMatchDestroyed");
        if (this.OnMatchDestroyed != null){
			this.OnMatchDestroyed();
		}
	}

	protected virtual void RaiseOnPlayerConnectedToMatch(MultiplayerAPI.PlayerInformation player)
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnPlayerConnectedToMatch");
        if (this.OnPlayerConnectedToMatch != null){
			this.OnPlayerConnectedToMatch(player);
		}
	}

	protected virtual void RaiseOnPlayerDisconnectedFromMatch(MultiplayerAPI.PlayerInformation player)
    {
        if (debugInfo) Debug.Log("PhotonConnector.RaiseOnPlayerDisconnectedFromMatch");
        if (this.OnPlayerDisconnectedFromMatch != null){
			this.OnPlayerDisconnectedFromMatch(player);
		}
	}
	#endregion
}
