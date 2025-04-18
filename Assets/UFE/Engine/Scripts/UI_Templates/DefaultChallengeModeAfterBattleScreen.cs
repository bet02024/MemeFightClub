﻿using UnityEngine;
using System.Collections.Generic;
using UFE3D;
using UnityEngine.UI;

public class DefaultChallengeModeAfterBattleScreen : ChallengeModeAfterBattleScreen{
	#region public instance properties
	public AudioClip onLoadSound;
	public AudioClip music;
	public AudioClip selectSound;
	public AudioClip cancelSound;
	public AudioClip moveCursorSound;
	public bool stopPreviousSoundEffectsOnLoad = false;
	public float delayBeforePlayingMusic = 0.1f;
	public Button nextChallengeButton;
	#endregion

	#region public override methods
	public override void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs
	){
		base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

		this.DefaultNavigationSystem(
			player1PreviousInputs,
			player1CurrentInputs,
			player2PreviousInputs,
			player2CurrentInputs,
			this.moveCursorSound,
			this.selectSound,
			this.cancelSound,
			this.GoToMainMenu
		);
	}

	public override void OnShow (){
		base.OnShow ();

		if (nextChallengeButton != null && UFE.currentChallenge + 1 >= UFE.config.challengeModeOptions.Length)
        {
			nextChallengeButton.interactable = false;
		}
		
		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}
		
		if (this.stopPreviousSoundEffectsOnLoad){
			UFE.StopSounds();
		}
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforePlayingMusic);
		}
	}
	#endregion
}
