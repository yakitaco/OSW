using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//同盟関係
public enum AlcType
{
	None,
	Partner,    //同一チーム
    Ally,       //味方
    Neutrality, //中立
    Enemy       //敵
}
    
public class TeamState
{

	private int pnum;       //プレイヤー番号 (0からの通し番号/自動採番)
	private string pname;   //プレイヤー名 
	private int ptype;      //プレイヤー種別 (0:人間 1～ CPU(番号でCPU思考が異なる))
	private int pteam;      //プレイヤーのチーム
	private Color UnitCol;  //プレイヤーの色
	private List<AlcType> AlcList = new List<AlcType>(); //他チームとの同盟関係
	
	static int num = 0;
	
    public TeamState(string name, int type, int team, Color UnitCol, int maxTeam)
    {
        this.pnum  = num++;
        this.pname = name;
        this.ptype = type;
        this.pteam = team;
        for (int i=0 ; i<maxTeam ; i++){
        	if (i==this.pnum){
        		AlcList.Add(AlcType.Partner);
        	} else {
        	    AlcList.Add(AlcType.Enemy);
        	}
        }
    }
	
	
	
}
