using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UFE3D;
//using PlayFabIntegration.API;
//using PlayFab;
//using PlayFab.ClientModels;

public class PhotonMultiplayerAPI : MultiplayerAPI{
	#region protected instance fields
	protected PhotonConnector _photonConnector;
	//protected PlayFabConnector _playFabConnector;
	#endregion

	#region public override properties
	public override int Connections{
		get{
			return this._photonConnector.Connections;
		}
	}

	public override MultiplayerAPI.PlayerInformation Player{
		get{
			return this._photonConnector.Player;
		}
	}

	public override float SendRate{
		get{
			return this._photonConnector.SendRate;
		}
		set{
			this._photonConnector.SendRate = value;
		}
	}
	#endregion

	#region public override methods
	public override void CreateMatch(MultiplayerAPI.MatchCreationRequest request){
		this._photonConnector.CreateMatch(request);
	}

	public override void DestroyMatch(){
		this._photonConnector.DestroyMatch();
	}

	public override void DisconnectFromMatch(){
		this._photonConnector.DisconnectFromMatch();
	}

	public override NetworkState GetConnectionState(){
		return this._photonConnector.GetConnectionState();
	}

	public override int GetLastPing(){
		return this._photonConnector.GetLastPing();
    }

    public override void Disconnect()
    {
        if (UFE.config.debugOptions.connectionLog) Debug.Log("Disconnecting from Photon");
        PhotonNetwork.Disconnect();
    }

    public override void Initialize (string uuid){
        if (uuid != null)
        {
            this._uuid = uuid;

            /*if (this._playFabConnector == null){
				this._playFabConnector = new PlayFabConnector();
			}*/

            if (this._photonConnector == null){
				this._photonConnector = this.gameObject.GetComponent<PhotonConnector>();

				if (this._photonConnector == null){
					this._photonConnector = this.gameObject.AddComponent<PhotonConnector>();
                }
                this._photonConnector.debugInfo = UFE.config.debugOptions.connectionLog;

                // Begin Event Listener Security Guidelines
                this._photonConnector.OnInitializeError -= this.RaiseOnInitializationError;
                this._photonConnector.OnInitializeSuccessful -= this.RaiseOnInitializationSuccessful;
                this._photonConnector.OnMessageReceived -= this.RaiseOnMessageReceived;

                this._photonConnector.OnDisconnection -= this.RaiseOnDisconnection;
                this._photonConnector.OnJoined -= this.RaiseOnJoined;
                this._photonConnector.OnJoinError -= this.RaiseOnJoinError;
                this._photonConnector.OnMatchesDiscovered -= this.RaiseOnMatchesDiscovered;
                this._photonConnector.OnMatchDiscoveryError -= this.RaiseOnMatchDiscoveryError;

                this._photonConnector.OnMatchCreated -= this.RaiseOnMatchCreated;
                this._photonConnector.OnMatchCreationError -= this.RaiseOnMatchCreationError;
                this._photonConnector.OnMatchDestroyed -= this.RaiseOnMatchDestroyed;
                this._photonConnector.OnPlayerConnectedToMatch -= this.RaiseOnPlayerConnectedToMatch;
                this._photonConnector.OnPlayerDisconnectedFromMatch -= this.RaiseOnPlayerDisconnectedFromMatch;
                // End Event Listener Security Guidelines


                this._photonConnector.OnInitializeError += this.RaiseOnInitializationError;
				this._photonConnector.OnInitializeSuccessful += this.RaiseOnInitializationSuccessful;
				this._photonConnector.OnMessageReceived += this.RaiseOnMessageReceived;

				this._photonConnector.OnDisconnection += this.RaiseOnDisconnection;
				this._photonConnector.OnJoined += this.RaiseOnJoined;
				this._photonConnector.OnJoinError += this.RaiseOnJoinError;
				this._photonConnector.OnMatchesDiscovered += this.RaiseOnMatchesDiscovered;
				this._photonConnector.OnMatchDiscoveryError += this.RaiseOnMatchDiscoveryError;

				this._photonConnector.OnMatchCreated += this.RaiseOnMatchCreated;
				this._photonConnector.OnMatchCreationError += this.RaiseOnMatchCreationError;
				this._photonConnector.OnMatchDestroyed += this.RaiseOnMatchDestroyed;
				this._photonConnector.OnPlayerConnectedToMatch += this.RaiseOnPlayerConnectedToMatch;
				this._photonConnector.OnPlayerDisconnectedFromMatch += this.RaiseOnPlayerDisconnectedFromMatch;

				//this._photonConnector.PlayFabConnector = this._playFabConnector;

			}

			this._photonConnector.Initialize(uuid);
		}else{
			this.RaiseOnInitializationError();
		}

	}

	public override void JoinMatch(MultiplayerAPI.MatchInformation match, string password = null){
		this._photonConnector.JoinMatch(match, password);
	}

	public override void JoinRandomMatch(){
		this._photonConnector.JoinRandomMatch();
	}

	public override void StartSearchingMatches(int startPage = 0, int pageSize = 20, string filter = null){
		this._photonConnector.StartSearchingMatches(startPage, pageSize, filter);
	}

	public override void StopSearchingMatches(){
		this._photonConnector.StopSearchingMatches();
	}
	#endregion

	#region protected override methods
	protected override bool SendNetworkMessage(byte[] bytes){
		return PhotonNetwork.RaiseEvent(0, bytes, RaiseEventOptions.Default, SendOptions.SendUnreliable);
	}

	protected override void RaiseOnPlayerDisconnectedFromMatch(PlayerInformation player){
        base.RaiseOnPlayerDisconnectedFromMatch(player);
    }
	#endregion
}
