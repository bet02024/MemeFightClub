using UnityEngine;
using System;
using System.Reflection;

namespace UFE3D
{
	public class ChallengeModeAfterBattleScreen : UFEScreen
	{
		#region protected enum definitions
		protected enum Option
		{
			RepeatChallenge = 0,
			NextChallenge = 1,
			MainMenu = 2,
		}
		#endregion

		#region public instance methods
		public virtual void GoToMainMenu()
		{
			this.SelectOption((int)Option.MainMenu, UFE.GetLocalPlayer());
		}

		public virtual void RepeatChallenge()
		{
			this.SelectOption((int)Option.RepeatChallenge, UFE.GetLocalPlayer());
		}

		public virtual void NextChallenge()
		{
			this.SelectOption((int)Option.NextChallenge, UFE.GetLocalPlayer());
		}
		#endregion

		#region public override methods
		public override void SelectOption(int option, int player)
		{
			Option selectedOption = (Option)option;
			if (selectedOption == Option.MainMenu)
			{
				UFE.StartMainMenuScreen();
			}
			else if (selectedOption == Option.NextChallenge)
			{
				UFE.NextChallenge();
				UFE.SetChallengeVariables();
				UFE.StartLoadingBattleScreen();
			}
			else if (selectedOption == Option.RepeatChallenge)
			{
				UFE.StartLoadingBattleScreen();
			}
		}
		#endregion
	}
}