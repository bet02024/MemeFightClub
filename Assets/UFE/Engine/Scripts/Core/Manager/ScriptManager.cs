using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFE3D
{
    public class ScriptManager : MonoBehaviour
    {


        public string roomMatch = "default";
        public string signature = "";

        public string address = "";

        IEnumerator waiter()
        {
            yield return new WaitForSeconds(1);
        }

        public static string truncate(string str, int max = 11, string sep = "...")
        {
            max = max == 0 ? 10 : max;
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            int len = str.Length;
            if (len > max)
            {
                sep = string.IsNullOrEmpty(sep) ? "..." : sep;
                int seplen = sep.Length;
                if (seplen > max)
                {
                    return str.Substring(len - max);
                }
                double n = -0.5 * (max - len - seplen);
                double center = len / 2.0;
                string front = str.Substring(0, (int)(center - n));
                string back = str.Substring(len - (int)(center - n));
                return front + sep + back;
            }
            return str;
        }
        public void GoToRandomMatchScreen(string param)
        {
            /*string[] params = param.Split('|');
            this.roomMatch = params[0];
            this.signature = params[1];
            this.address = params[2];*/
            //UFE.StartSearchMatchScreen();
            string address1 = "0x0";
            if (param != null && param.Length > 0){
                address1 = param;
            }

            //UFE.GetCharacterSelectionScreen().TrySelectCharacter(5,2);

            //UFE.StartPlayerVersusCpu();

            UFE.SetCPU(1, false);
		    UFE.SetCPU(2, true);
            UFE3D.CharacterInfo[] selectableCharacters = new UFE3D.CharacterInfo[0];
            selectableCharacters = UFE.GetVersusModeSelectableCharacters();

            System.Random rnd = new System.Random();
            int randomIndex  = rnd.Next(0, selectableCharacters.Length -1);
            UFE.randomPlayer2 = randomIndex; 

            UFE.HideScreen(UFE.currentScreen);
            CharacterSelectionScreen charSelScreen = (UFE.config.selectedMatchType != MatchType.Singles) ? UFE.config.gameGUI.teamSelectionScreen : UFE.config.gameGUI.characterSelectionScreen;
            UFE.ShowScreen(charSelScreen);
            CameraFade.StartAlphaFade(UFE.config.gameGUI.screenFadeColor, true, 0);

            int randomIndex2  = rnd.Next(0, 20);

            string[] addreses = {"0xD8092298A19a40EE00C9122a3c62A96F889F2Beb", "0x9439018aAA031f6D5Af416e7Ce829d7051c07dD8", 
            "0x3B3370EcCAc43C96eC8a1e28363dC725a731B822", "0x8Ef03F90aDCC38F77d21a5986b2c51C4Ef886b18", "0xFf46f5761Ed3bED4582B6A2178D78e5E6309aa3f",
            "0x6C21a1c0Bd64FBF5947fdbc4B8044a6298Cbd7B5", "0x3ca5e4Fa8d00871c3a276468D38dF48FdFB8B5C1", "0xB6971BC55d37f3a0B31e79b093DDbB2402f87a42",
            "0xb39E1789c53dF3BBdF7a9146eFE75D99514a1A89", "0xa092379F6e928eDB1AaA61A71F17Bb03Ff8557fA", "0x534530072BdA204322aD6a1078F5E92aD49a64f1",
            "0x62c7797ecF3f0005FCe7ef12277B040029cc469b", "0x1e61a2f54d068643e379B46B9923810Eb8746fA5", "0x5D2595924D34148B842d7a4A9b3582cF73a38CDa",
            "0xE0014Ea1c0040D4acbc087617A457975Ff44381E", "0xFe2eE766eB3AE899B42cc5Aa80e6F04f5b109615", "0xFe2eE766eB3AE899B42cc5Aa80e6F04f5b109615",
            "0xdF74c84fdB8b595E61002B34597614C31683094b", "0x0697d06BEb7fBF4B7FB56b6eF8C993023C26Ae58", "0x15aa200432052763F1530cb0485D1fBd7EfEB57c",
            "0x81c31C475e961257a8BC5Da63D16fDf1D09F4375"};


            UFE.address1 =  truncate(address1, 11);
            UFE.address2 = truncate(addreses[randomIndex2], 11);

            UFE.addressFull1 =  address1;
            UFE.addressFull2 =  addreses[randomIndex2];


        }


         public void GoToStoryMode()
        {

            UFE.StartStoryMode();


        }

    }

}
