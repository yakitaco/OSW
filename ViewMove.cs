using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
 
public class ViewMove : MonoBehaviour {
    const float ROTATION_SPEED =  0.01f;
    const float ZOOM_SPEED = 200.0f;
    //public GameObject grid;
    
    public TileBase tile;
    //public MapCtl GridMap; 
    
     
    private Vector3 original;
    private float tx = 0.0f;
    private float ty = 0.0f;
    private float sx = 0.0f;
    private float sy = 0.0f;
    float fMinSize = 0.1f; 
    float fMaxSize = 30.0f; 
     
    private Vector3 pos;
    private bool isDragging = false;
    private bool isDragged = false;
    private bool isPinched = false;
    private bool istouch = false;
    private float interval = 0.0f;
 
    // Use this for initialization
    void Start () {
        Vector3 position = transform.position;
        tx = position.y;
        ty = position.x;
        original = transform.position;
        //Debug.Log("touchCount:" + Input.touchCount );
    }
     
/* エディタ上での動作 */
#if UNITY_EDITOR
    void Update () {
        if (Input.GetMouseButtonDown(0)) {
            Tap(Input.mousePosition);
        }
        if(Input.GetKey(KeyCode.RightArrow)){
            transform.position += new Vector3(0.1f, 0.0f, 0.0f);
        }
        if(Input.GetKey(KeyCode.LeftArrow)){
            transform.position -= new Vector3(0.1f, 0.0f, 0.0f);
        }
        if(Input.GetKey(KeyCode.UpArrow)){
            transform.position += new Vector3(0.0f, 0.1f, 0.0f);
        }
        if(Input.GetKey(KeyCode.DownArrow)){
            transform.position -= new Vector3(0.0f, 0.1f, 0.0f);
        }
        if(Input.GetKey(KeyCode.Z)){ //縮小
            GetComponent<Camera>().orthographicSize +=0.05f;
        }
        if(Input.GetKey(KeyCode.X)){ //拡大
            GetComponent<Camera>().orthographicSize -=0.05f;
        }
        
    }
/* スマホ上での動作 */
#else
    void Update () {
    
    
        //Debug.Log("touchCount:" + Input.touchCount );
        if (Input.touchCount == 1 && !isPinched) {
            //タッチ開始処理
            if (Event.current.type == EventType.MouseDrag) {
                isDragging = true;
                isDragged = true;
                
            } else {
                isDragging = false;
                if (Input.GetMouseButtonUp(0)) {
                    if (!isDragged){
                        //一瞬タップ -> タッチ処理
                        Tap(Input.mousePosition);
                    } else {
                        //ピンチイン・アウト終了
                        isDragged = false;
                        istouch = false;
                    }
                }
            }

        } else if (Input.touchCount == 0) {

            isPinched = false;
            istouch = false;
        }
        
        //if (Input.GetMouseButtonUp(0)&&!isDragging) Tap(Input.mousePosition);
        
        if (Input.touchCount == 2) {
            if (Input.touches[0].phase == TouchPhase.Began || Input.touches[1].phase == TouchPhase.Began) {
                interval = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            }
            float tmpInterval = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            GetComponent<Camera>().orthographicSize += ((interval - tmpInterval) / 2) * 0.004f; 
            GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize, fMinSize, fMaxSize);
            //original.z += (tmpInterval - interval) / ZOOM_SPEED;
            //if (original.z > 0) original.z = 0;
            interval = tmpInterval;
 
            //transform.position = transform.rotation * original;
            isPinched = true;
        } else if (isDragging && !isPinched) {
            tx = (Input.GetAxis("Mouse X") - sx) * GetComponent<Camera>().orthographicSize * ROTATION_SPEED;
            ty = (Input.GetAxis("Mouse Y") - sy) * GetComponent<Camera>().orthographicSize * ROTATION_SPEED;
            sx = 0f;
            sy = 0f;
            //transform.rotation = Quaternion.Euler(ty, angle_x, 0);
            transform.position -= new Vector3(tx, ty, 0f);
            //Tap(Input.mousePosition);
        }
    }
#endif

    private void Tap(Vector3 point) {
        // タップ時の処理を記述
        point.z = 0f;
        Vector3 screenToWorldPointPosition = Camera.main.ScreenToWorldPoint(point);
        //Grid grid = transform.parent.GetComponent<Grid>();
        //Vector3Int cellPosition = grid.GetComponent<Grid>().LocalToCell(screenToWorldPointPosition);
        //Debug.Log("["+ cellPosition.x +","+ cellPosition.y +"]");
        //GridMap.ClickMap(cellPosition);
    }
    
    
    //指定した位置へ視点移動
    public void MoveView(Vector3 point, float duration) {
    	//transform.DOMove(point - new Vector3 (0.0f, 0.0f, 10.0f), duration).SetEase(Ease.Linear);
    }
    
    //現在の視点位置応答
    public Vector3 GetViewPos() {
    	return transform.position;
    }
    
}





