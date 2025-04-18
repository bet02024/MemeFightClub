﻿using UnityEngine;

namespace UFE3D
{
    [System.Serializable]
    public class StanceInfo : ScriptableObject
    {
        public CombatStances combatStance = CombatStances.Stance1;
        public MoveInfo cinematicIntro;
        public MoveInfo cinematicOutro;
        public MoveInfo roundOutro;
        public MoveInfo timeOutOutro;

        public BasicMoves basicMoves = new BasicMoves();
        public MoveInfo[] attackMoves = new MoveInfo[0];

        public MoveSetData ConvertData()
        {
            MoveSetData moveSet = new MoveSetData();
            moveSet.combatStance = this.combatStance;
            moveSet.cinematicIntro = this.cinematicIntro;
            moveSet.cinematicOutro = this.cinematicOutro;
            moveSet.roundOutro = this.roundOutro;
            moveSet.timeOutOutro = this.timeOutOutro;
            moveSet.basicMoves = this.basicMoves;
            moveSet.attackMoves = this.attackMoves;

            return moveSet;
        }
    }
}