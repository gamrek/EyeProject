using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

[System.Serializable]
public class Tester{
	public string firstName;
	public string lastName;
	public string email;
	public string birthday;
	public int ID;
	public int status;
}

[System.Serializable]
public class Data{
	public int gameSessionID;
	public string timeStamp;
	public bool fix;
	public float gazeX;
	public float gazeY;
	public float leftX;
	public float leftY;
	public float leftPupilSize;
	public float leftPupilX;
	public float leftPupilY;
	public float rightX;
	public float rightY;
	public float rightPupilSize;
	public float rightPupilX;
	public float rightPupilY;
	public float referencePointX;
	public float referencePointY;
	public int dataType;
	public float headDistance;
}

public class TestController : MonoBehaviour {

	public enum GameStatus{
		None,
		Ready,
		Start,
		End
	}

	public GameStatus myGameStatus;

	public static TestController instance;

	public Tester currentTester;

	public Data currentData;

	float diagonalScreenSize = 15.4f;

	public int screenDPI;
	public int screenWidth;
	public int screenHeight;

	public int currentGameSessionID;

	float testDuration = 40f;
	public float timeCounter;

	public bool useMouse;

	public GameObject redDot;

	public GameObject trackerDot;

	Vector3 trackerPos;

	int numberOfWWW;
	int numberOfWWWFinished;

	float nextDotTime;

	void Awake(){
		instance = this;
		myGameStatus = GameStatus.None;
	}

	// Use this for initialization
	void Start () {
		if(Screen.dpi == 0){
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			screenWidth = (int)(Screen.currentResolution.width * 2f);
			screenHeight = (int)(Screen.currentResolution.height * 2f);
#else
			screenWidth = (int)Screen.currentResolution.width;
			screenHeight = (int)Screen.currentResolution.height;
#endif
			float diagonalResolution = Mathf.Sqrt((Mathf.Pow((float)screenWidth, 2f) + Mathf.Pow((float)screenHeight, 2f)));
			screenDPI = (int)(diagonalResolution / diagonalScreenSize);
		} else {
			screenWidth = (int)Screen.currentResolution.width;
			screenHeight = (int)Screen.currentResolution.height;
			screenDPI = (int)Screen.dpi;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(myGameStatus == GameStatus.Ready){
			if(timeCounter > 0f){
				timeCounter -= Time.deltaTime;
				if(timeCounter <= 0f){
					startGame();
				}
			}
		} else if(myGameStatus == GameStatus.Start){
			if(!trackerDot.activeInHierarchy){
				trackerDot.SetActive(true);
			}

			if(timeCounter > 0f){
				timeCounter -= Time.deltaTime;
				if(timeCounter <= 0f){
					gameEnd();
				}
			}

			if(useMouse){
				trackerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				trackerDot.transform.position = new Vector3(trackerPos.x, trackerPos.y, trackerDot.transform.position.z);
				currentData.gameSessionID = currentGameSessionID;
				currentData.timeStamp = System.Math.Round(System.DateTime.Now.TimeOfDay.TotalMilliseconds).ToString();
				currentData.fix = true;
				currentData.gazeX = Camera.main.WorldToScreenPoint(trackerDot.transform.position).x;
				currentData.gazeY = Camera.main.WorldToScreenPoint(trackerDot.transform.position).y;
				currentData.leftX = Camera.main.WorldToScreenPoint(trackerDot.transform.position).x;
				currentData.leftY = Camera.main.WorldToScreenPoint(trackerDot.transform.position).y;
				currentData.leftPupilSize = 1;
				currentData.leftPupilX = Camera.main.WorldToScreenPoint(trackerDot.transform.position).x;
				currentData.leftPupilY = Camera.main.WorldToScreenPoint(trackerDot.transform.position).y;
				currentData.rightX = Camera.main.WorldToScreenPoint(trackerDot.transform.position).x;
				currentData.rightY = Camera.main.WorldToScreenPoint(trackerDot.transform.position).y;
				currentData.rightPupilSize = 1;
				currentData.rightPupilX = Camera.main.WorldToScreenPoint(trackerDot.transform.position).x;
				currentData.rightPupilY = Camera.main.WorldToScreenPoint(trackerDot.transform.position).y;
				currentData.referencePointX = Camera.main.WorldToScreenPoint(redDot.transform.position).x;
				currentData.referencePointY = Camera.main.WorldToScreenPoint(redDot.transform.position).y;
				currentData.dataType = 1;
				currentData.headDistance = 1f;
			} else {
				Screen.showCursor = false;
			}

			if(redDot.activeInHierarchy){
				//Debug.Log(Camera.main.WorldToViewportPoint(redDot.transform.position) + " " + trackerDot.transform.position);
				if(nextDotTime > 0f){
					nextDotTime -= Time.deltaTime;
					if(nextDotTime <= 0f){
						generateDot();
					}
				}
			}
			dataToJson();
		} else if(myGameStatus == GameStatus.End){
//			if(numberOfWWW == numberOfWWWFinished){
//
//			}
		}

		if(Input.GetKeyDown(KeyCode.Return)){
			//generateDot();
		}
	}

	public void gameReady(){
		myGameStatus = GameStatus.Ready;
		timeCounter = 3f;
		numberOfWWW = 0;
		numberOfWWWFinished = 0;
	}

	public void startGame(){
		myGameStatus = GameStatus.Start;
		timeCounter = testDuration;
		generateDot();
	}

	void generateDot(){
		redDot.transform.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0f, 1f), 0f, 0f)).x, redDot.transform.position.y, redDot.transform.position.z);
		redDot.SetActive(true);
		nextDotTime = Random.Range(2f, 5f);
	}

	public void gameEnd(){
		myGameStatus = GameStatus.End;
		InterfaceController.instance.gameEnd();
		//InterfaceController.instance.displayEndPanel();
		trackerDot.SetActive(false);
		redDot.SetActive(false);
	}

	public void seeResult(){
		Application.OpenURL("http://dev.ullon.com/mrpatch/analytics/london_saccadic.php?id=" + currentData.gameSessionID.ToString());
	}

	void dataToJson(){
		JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
		j.AddField("game_session_id", currentData.gameSessionID);
		j.AddField("timestamp", currentData.timeStamp);
		j.AddField("fix", currentData.fix);
		j.AddField("gaze_x", currentData.gazeX);
		j.AddField("gaze_y", currentData.gazeY);
		j.AddField("left_x", currentData.leftX);
		j.AddField("left_y", currentData.leftY);
		j.AddField("left_pupil_size", currentData.leftPupilSize);
		j.AddField("left_pupil_x", currentData.leftPupilX);
		j.AddField("left_pupil_y", currentData.leftPupilY);
		j.AddField("right_x", currentData.rightX);
		j.AddField("right_y", currentData.rightY);
		j.AddField("right_pupil_size", currentData.rightPupilSize);
		j.AddField("right_pupil_x", currentData.rightPupilX);
		j.AddField("right_pupil_y", currentData.rightPupilY);
		j.AddField("reference_point_x", currentData.referencePointX);
		j.AddField("reference_point_y", currentData.referencePointY);
		j.AddField("data_type", currentData.dataType);
		j.AddField("head_distance", currentData.headDistance);
		JSONObject newJ = new JSONObject(JSONObject.Type.OBJECT);
		newJ.AddField("game_data", j);
		string newData = newJ.print();
		//Debug.Log(newData);
		StartCoroutine(sendData(newData));
	}

	IEnumerator sendData(string data){
		numberOfWWW++;
		WWWForm wwwForm = new WWWForm();
		wwwForm.AddField("c", "game");
		wwwForm.AddField("m", "save");
		wwwForm.AddField("data", data);
		WWW newWWW = new WWW("http://dev.ullon.com/mrpatch/api/", wwwForm);
		yield return newWWW;
		if(newWWW.error != null){
			Debug.Log(data);
			Debug.Log("Send Data Error " + newWWW.error);
		} else {
			Debug.Log("Return " + newWWW.text);
		}
		newWWW.Dispose();
		numberOfWWWFinished++;
	}
}
