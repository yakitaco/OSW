using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PlayerCtl
{
	public int pnum;      //プレイヤー番号 (0からの通し番号/自動採番)
	public string pname;  //プレイヤー名 
	public int ptype;     //プレイヤー種別 (0:人間 1～ CPU(番号でCPU思考が異なる))
	public int pteam;     //プレイヤーのチーム
	public Color pColor;  //プレイヤーの色
	public List<UnitObj> utltList;  //ユニットテンプレートのリスト
	public ViewMap vMap;
	
	static int num = 0;
	
	//CPUコンポーネントの名前一覧
	static String[] cpulist = {null,"CpuCtl","CpuCtl"};
	
    public PlayerCtl(string name, int type, int team, Color _pColor)
    {
    
        this.pnum  = num++;
        this.pname = name;
        this.ptype = type;
        this.pteam = team;
        this.pColor  = _pColor;
        GameObject pObj = new GameObject("Player_" + this.pnum);
        utltList = UnitObj.initUnitTlt(this);
        if (type>0){
        	//指定した番号のCPUコンポーネントをロードする
	        pObj.AddComponent(Type.GetType(cpulist[ptype]));
        }
        //vMap = new ViewMap();
    }
	
    public List<Vector2Int> getTshipPos(){
    
    	return utltList.Where(cell => cell.units == Unit.TransportShip || cell.units == Unit.TransportShip).Select(cell => cell.pos).ToList();
    }
	
}
