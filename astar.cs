using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class astar : MonoBehaviour
{

	/// A-starノード.
	class ANode {
		enum eStatus {
			None,
			Open,
			Closed,
		}
		/// ステータス
		eStatus _status = eStatus.None;
		/// 実コスト
		int _cost = 0;
		/// ヒューリスティック・コスト
		int _heuristic = 0;
		/// 親ノード
		ANode _parent = null;
		/// 座標
		int _x = 0;
		int _y = 0;
		///方位 (上=0,右上=1,右=2,右下=3, ... ,左上=7)
		int _dir = 0;
		
		public int X {
			get { return _x; }
		}
		public int Y {
			get { return _y; }
		}
		public int Cost {
			get { return _cost; }
		}

		/// コンストラクタ.
		public ANode(int x, int y) {
			_x = x;
			_y = y;
		}
		/// スコアを計算する.
		public int GetScore() {
			return _cost + _heuristic;
		}
		/// ヒューリスティック・コストの計算.
		public int CalcHeuristic(bool allowdiag, Vector2Int goalPos) {

			if(allowdiag) {
				// 斜め移動あり
				var dx = (int)Mathf.Abs (goalPos.x - X);
				var dy = (int)Mathf.Abs (goalPos.y - Y);
				// 大きい方をコストにする
				_heuristic =  dx > dy ? dx : dy;
			}
			else {
				// 縦横移動のみ
				var dx = Mathf.Abs (goalPos.x - X);
				var dy = Mathf.Abs (goalPos.y - Y);
				//_heuristic = (int)(dx + dy);
				//if (dx > dy) {
				//	_heuristic = (dx + dy);
				//} else {
				//	_heuristic = dy;
				//}
				_heuristic =  Mathf.Max(dy, dx + (dy/2) + (( ((goalPos.y%2==0) && (Y%2==1) && (goalPos.x < X)) || ((Y%2==0) && (goalPos.y%2==1) && (X < goalPos.x)) ) ? 1 : 0 ));
				

			}
			Dump();
			return _heuristic;
		}
		/// ステータスがNoneかどうか.
		public bool IsNone() {
			return _status == eStatus.None;
		}
		/// ステータスをOpenにする.
		public void Open(ANode parent, int cost) {
			//Debug.Log (string.Format("Open: ({0},{1})", X, Y));
			_status = eStatus.Open;
			_cost   = cost;
			_parent = parent;
		}
		/// ステータスをClosedにする.
		public void Close() {
			//Debug.Log (string.Format ("Closed: ({0},{1})", X, Y));
			_status = eStatus.Closed;
		}
		/// パスを取得する
		public void GetPath(List<Vector2Int> pList) {
			pList.Add(new Vector2Int(X, Y));
			if(_parent != null) {
				_parent.GetPath(pList);
			}
		}
		public void Dump() {
			//Debug.Log (string.Format("({0},{1})[{2}] cost={3} heuris={4} score={5}", X, Y, _status, _cost, _heuristic, GetScore()));
		}
		public void DumpRecursive() {
			Dump ();
			if(_parent != null) {
				// 再帰的にダンプする.
				_parent.DumpRecursive();
			}
		}
	}

	/// A-starノード管理.
	class ANodeMgr {
		/// 地形レイヤー.
		TileObj[,] _map;
		/// 斜め移動を許可するかどうか.
		bool _allowdiag = true;
		/// オープンリスト.
		List<ANode> _openList = null;
		/// クローズリスト.
		List<ANode> _closeList = null;
		/// ノードインスタンス管理.
		Dictionary<int,ANode> _pool = null;
		/// 移動禁止リスト.
		List<Vector2Int> _Olist;
		/// 移動許可リスト.
		List<Vector2Int> _Alist;
		/// ゴール座標.
		Vector2Int _goalPos;
		/// ユニット種別
		int _unitType;
		
		public ANodeMgr(Vector2Int goalPos, TileObj[,] tileMap, List<Vector2Int> Olist, List<Vector2Int> Alist, int unitType) {
			_map = tileMap;
			_allowdiag = false;
			_openList = new List<ANode>();
			_closeList = new List<ANode>();
			_pool = new Dictionary<int, ANode>();
			_goalPos = goalPos;
			_Olist = Olist;
			_Alist = Alist;
			_unitType = unitType;
		}
		/// ノード生成する.
		public ANode GetNode(Vector2Int pos) {
			var idx = pos.x + StageCtl.TileLenX * pos.y; // 一元ID作成
			if(_pool.ContainsKey(idx)) {
				// 既に存在しているのでプーリングから取得.
				return _pool[idx];
			}

			// ないので新規作成.
			var node = new ANode(pos.x, pos.y);
			_pool[idx] = node;
			// ヒューリスティック・コストを計算する.
			node.CalcHeuristic(_allowdiag, _goalPos);
			return node;
		}
		/// ノードをオープンリストに追加する.
		public void AddOpenList(ANode node) {
			_openList.Add(node);
		}
		/// ノードをオープンリストから削除する.(クローズリストに追加)
		public void RemoveOpenList(ANode node) {
			_openList.Remove(node);
			_closeList.Add(node);
		}
		
		///指定ノードが通行可かチェック
		public bool CheckNodeAllow(Vector2Int nodePos) {
			TileGroup tg = _map[nodePos.x, nodePos.y].getGroup();
			switch(_unitType){
				case 0: /* 船 */
					if (tg != TileGroup.Sea) return false;
					break;
				case 1: /* 車両 */
					if (tg != TileGroup.Ground) return false;
					break;
				case 2: /* 人 */
					if (tg == TileGroup.Sea) return false;
					break;
			}
			return true;
		}
		
		/// 指定の座標にあるノードをオープンする.
		public ANode OpenNode(Vector2Int pos, int cost, ANode parent) {
		
			// 領域外チェック
			if((pos.x<0)||(pos.x>=StageCtl.TileLenX)||(pos.y<0)||(pos.y>=StageCtl.TileLenY)) {
				return null;
			}
			
			// 通行禁止リスト一致
			if ((_Olist?.Count > 0)&&(_Olist.Exists(s => s.Equals(pos)))){
				Debug.Log (string.Format("dell:{0}", pos));
				return null;
			}
			
			
			//タイルのタイプチェック
			if((!CheckNodeAllow(pos))&&(parent!=null)) {
				// 通過できない.
				
				// 通行許可リスト不一致
				if ((_Olist?.Count > 0)&&(_Olist.Exists(s => s.Equals(pos)))){
					/* 移動許可リスト一致ならOK */
				} else {
					return null;
				}
				
			}
			
			// ノードを取得する.
			var node = GetNode(pos);
			if(node.IsNone() == false) {
				// 既にOpenしているので何もしない
				return null;
			}

			// Openする.
			node.Open(parent, cost);
			AddOpenList(node);

			return node;
		}

		/// 周りをOpenする.
		public void OpenAround(ANode parent) {
			var xbase = parent.X; // 基準座標(X).
			var ybase = parent.Y; // 基準座標(Y).
			var cost = parent.Cost; // コスト.
			cost += 1; // 一歩進むので+1する.
			if(_allowdiag) {
				// 8方向を開く.
				for(int j = 0; j < 3; j++) {
					for(int i = 0; i < 3; i++) {
						var x = xbase + i - 1; // -1～1
						var y = ybase + j - 1; // -1～1
						OpenNode(new Vector2Int(x, y), cost, parent);
					}
				}
			}
			else {
				// 4方向を開く.
				var x = xbase;
				var y = ybase;
				
				//6方向開く
                if (ybase%2==0){
					if(x>0)                   OpenNode (new Vector2Int(x-1, y  ), cost + (int)_map[x-1, y  ].getType(), parent); // 右.
					if(x+1<StageCtl.TileLenX) OpenNode (new Vector2Int(x+1, y  ), cost + (int)_map[x+1, y  ].getType(), parent); // 左.
					if(y>0)                   OpenNode (new Vector2Int(x  , y-1), cost + (int)_map[x  , y-1].getType(), parent); // 右.
					if(y+1<StageCtl.TileLenY) OpenNode (new Vector2Int(x  , y+1), cost + (int)_map[x  , y+1].getType(), parent); // 右.
					if((x>0)&&(y>0))          OpenNode (new Vector2Int(x-1, y-1), cost + (int)_map[x-1, y-1].getType(), parent); // 右.
					if((x>0)&&(y+1<StageCtl.TileLenY)) OpenNode (new Vector2Int(x-1, y+1), cost + (int)_map[x-1, y+1].getType(), parent); // 右.
					if ((x>0)&&(y+1<StageCtl.TileLenY)&&(CheckNodeAllow(new Vector2Int(x-1, y+1)))&&(CheckNodeAllow(new Vector2Int(x, y+1)))){
						if(y+2<StageCtl.TileLenY) OpenNode (new Vector2Int(x, y+2), cost+2 + (int)_map[x, y+2].getType(), parent); // 右.
					}
					if ((x>0)&&(y>0)&&(CheckNodeAllow(new Vector2Int(x-1, y-1)))&&(CheckNodeAllow(new Vector2Int(x, y-1)))){
						if(y>1)                   OpenNode (new Vector2Int(x, y-2), cost+2 + (int)_map[x, y-2].getType(), parent); // 右.
					}
                } else {
					if(x>0)                   OpenNode (new Vector2Int(x-1, y  ), cost + (int)_map[x-1, y  ].getType(), parent); // 右.
					if(x+1<StageCtl.TileLenX) OpenNode (new Vector2Int(x+1, y  ), cost + (int)_map[x+1, y  ].getType(), parent); // 左.
					if(y>0)                   OpenNode (new Vector2Int(x  , y-1), cost + (int)_map[x  , y-1].getType(), parent); // 右.
					if(y+1<StageCtl.TileLenY) OpenNode (new Vector2Int(x  , y+1), cost + (int)_map[x  , y+1].getType(), parent); // 右.
					if((x+1<StageCtl.TileLenX)&&(y>0)) OpenNode (new Vector2Int(x+1, y-1), cost + (int)_map[x+1, y-1].getType(), parent); // 右.
					if((x+1<StageCtl.TileLenX)&&(y+1<StageCtl.TileLenY)) OpenNode (new Vector2Int(x+1, y+1), cost + (int)_map[x+1, y+1].getType(), parent); // 右.
					if ((x+1<StageCtl.TileLenX)&&(y+1<StageCtl.TileLenY)&&(CheckNodeAllow(new Vector2Int(x, y+1)))&&(CheckNodeAllow(new Vector2Int(x+1, y+1)))){
						if(y+2<StageCtl.TileLenY) OpenNode (new Vector2Int(x, y+2), cost+2 + (int)_map[x, y+2].getType(), parent); // 右.
					}
					if ((x+1<StageCtl.TileLenX)&&(y>0)&&(CheckNodeAllow(new Vector2Int(x, y-1)))&&(CheckNodeAllow(new Vector2Int(x+1, y-1)))){
						if(y>1)                   OpenNode (new Vector2Int(x, y-2), cost+2 + (int)_map[x, y-2].getType(), parent); // 右.
					}
                }
				
			}
		}

		/// 最小スコアのノードを取得する.
		public ANode SearchMinScoreNodeFromOpenList() {
			// 最小スコア
			int min = 9999;
			// 最小実コスト
			int minCost = 9999;
			ANode minNode = null;
			foreach(ANode node in _openList) {
				int score = node.GetScore();
				if(score > min) {
					// スコアが大きい
					continue;
				}
				if(score == min && node.Cost >= minCost) {
					// スコアが同じときは実コストも比較する
					continue;
				}

				// 最小値更新.
				min = score;
				minCost = node.Cost;
				minNode = node;
			}
			return minNode;
		}
		
		/// 最小スコアのノードを取得する.
		public ANode SearchMinScoreNodeFromCloseList(Vector2Int goalPos) {
			// 最小スコア
			int min = 9999;
			// 最小実コスト
			int minCost = 9999;
			ANode minNode = null;
			foreach(ANode node in _closeList) {
			//Debug.Log (string.Format("({0},{1}) cost={2} score={3}", node.X, node.Y, node.Cost, node.GetScore()));
				int score = node.CalcHeuristic(false, goalPos);
				if(score > min) {
					// スコアが大きい
					continue;
				}
				if(score == min && node.Cost >= minCost) {
					// スコアが同じときは実コストも比較する
					continue;
				}

				// 最小値更新.
				min = score;
				minCost = node.Cost;
				minNode = node;
			}
			return minNode;
		}
	}

    //A-Sterによる経路取得(ラッパー) unitType:0=船/1=車/2=人
    public void getRoute(Vector2Int startPos, Vector2Int goalPos, TileObj[,] tileMap, int unitType, Action<List<Vector2Int>, int> callback = null,  List<Vector2Int> Olist = null, List<Vector2Int> Alist = null){

		//コルーチンをコール
    	StartCoroutine(getRouteCo(startPos, goalPos, tileMap, unitType, callback, Olist, Alist));
    }
    
    //A-Sterによる経路取得(コルーチン)
    IEnumerator getRouteCo (Vector2Int startPos, Vector2Int goalPos, TileObj[,] tileMap, int unitType, Action<List<Vector2Int>, int> callback, List<Vector2Int> Olist, List<Vector2Int> Alist){
    	
    	List<Vector2Int> moveList = new List<Vector2Int>();
    	
    	//新規ノード管理
    	var mgr = new ANodeMgr(goalPos, tileMap, Olist, Alist, unitType);
    	
    	//スタートorゴールが通行不可
    	//if (!mgr.CheckNodeAllow(startPos)) {
    	//	Debug.Log ("no allow. from("+ startPos +") to("+ goalPos);
    	//	if (callback != null) callback(moveList, -1);
    	//	yield break;
    	//}
    	
		// スタート地点のノード取得
		// スタート地点なのでコストは「0」
		ANode node = mgr.OpenNode(startPos, 0, null);
    	mgr.AddOpenList(node);
    	
		// 試行回数。100回超えたら強制中断
		int cnt = 0;
		while(cnt < 100) {
			mgr.RemoveOpenList(node);
			// 周囲を開く
			mgr.OpenAround(node);
			// 最小スコアのノードを探す.
			node = mgr.SearchMinScoreNodeFromOpenList();
			if(node == null) {
				// 袋小路なのでおしまい.
				node = mgr.SearchMinScoreNodeFromCloseList(goalPos);
				Debug.LogWarning (string.Format ("[Not found path.] Min: ({0},{1})", node.X, node.Y));
				//node.DumpRecursive();
				// パスを取得する
				node.GetPath(moveList);
				// 反転する
				moveList.Reverse();
				break;
			}
			if(node.X == goalPos.x && node.Y == goalPos.y) {
				// ゴールにたどり着いた.
				//Debug.Log ("Success.");
				mgr.RemoveOpenList(node);
				//node.DumpRecursive();
				// パスを取得する
				node.GetPath(moveList);
				// 反転する
				moveList.Reverse();
				break;
			}
			cnt++;
			yield return null;//new WaitForSeconds(0.01f);
		}
		if(cnt == 100) {
			// 袋小路なのでおしまい.
			node = mgr.SearchMinScoreNodeFromCloseList(goalPos);
			Debug.LogWarning (string.Format ("[Over] Min: ({0},{1})", node.X, node.Y));
			//node.DumpRecursive();
			// パスを取得する
			node.GetPath(moveList);
			// 反転する
			moveList.Reverse();
		}
		
    	
    	if (callback != null) callback(moveList, node.CalcHeuristic(false, goalPos));
    	yield return new WaitForSeconds(0.01f);
    }
    
}
