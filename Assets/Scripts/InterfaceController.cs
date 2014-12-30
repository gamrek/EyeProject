using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using MiniJSON;

public class InterfaceController : MonoBehaviour {

	public static InterfaceController instance;

	public enum Stage{
		Main,
		Login,
		Register,
		CreateGame,
		StartGame,
		EndGame
	}

	public Stage myStage;
	public CanvasGroup welcomePanel;
	public CanvasGroup registerPanel;
	public GameObject summaryPanel;
	public GameObject loadingPanel;
	public GameObject warningPanel;
	public GameObject exitPanel;
	public GameObject inGamePanel;
	public GameObject gameEndPanel;

	public Text warningText;

	public InputField emailLogin;
	public InputField passwordLogin;

	public InputField registerEmail;
	public InputField registerPassword;
	public InputField registerConfirmPassword;
	public InputField registerFirstName;
	public InputField registerLastName;
	public InputField registerYear;
	public InputField registerMonth;
	public InputField registerDate;

	public Text firstName;
	public Text lastName;
	public Text birthday;

	public Text inGameTimer;

	int sleepTime = 7;
	int stressLevel = 0;

	void Awake(){
		instance = this;
		welcomePanel.gameObject.SetActive(true);
		registerPanel.gameObject.SetActive(false);
		summaryPanel.SetActive(false);
		loadingPanel.SetActive(false);
		warningPanel.SetActive(false);
		exitPanel.SetActive(false);
		inGamePanel.SetActive(false);
		gameEndPanel.SetActive(false);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)){
			if(!exitPanel.activeInHierarchy){
				exitPanel.SetActive(true);
			} else {
				exitPanel.SetActive(false);
			}
		}
		if(Input.GetKeyDown(KeyCode.Return)){
			if(exitPanel.activeInHierarchy){
				pressExit();
			}
		}
		if(inGamePanel.activeInHierarchy){
			inGameTimer.text = TestController.instance.timeCounter.ToString("f0");
		}
	}

	public void login(){
		myStage = Stage.Login;
		if(emailLogin.text == string.Empty || passwordLogin.text == string.Empty || !emailLogin.text.Contains("@")){
			outWarning("Invalid email address or password");
			return;
		}
		loadingPanel.SetActive(true);
		welcomePanel.gameObject.SetActive(false);
		JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
		j.AddField("email", emailLogin.text);
		j.AddField("password", passwordLogin.text);
		string data = j.print();
		StartCoroutine(submitToServer("user", "login", data));
	}

	public void register(){
		myStage = Stage.Register;
		if(registerEmail.text == string.Empty || !registerEmail.text.Contains("@") || registerPassword.text == string.Empty || registerConfirmPassword.text == string.Empty || registerPassword.text != registerConfirmPassword.text || registerFirstName.text == string.Empty || registerLastName.text == string.Empty || registerYear.text == string.Empty || registerMonth.text == string.Empty || registerDate.text == string.Empty || registerYear.text.Length != 4 || registerMonth.text.Length != 2 || registerDate.text.Length != 2){
			outWarning("Invalid information");
			return;
		}
		DateTime temp;
		string birthdayString = registerYear.text + "-" + registerMonth.text + "-" + registerDate.text;
		if(!DateTime.TryParseExact(birthdayString, "yyyy-MM-dd", new System.Globalization.CultureInfo("en-US"), System.Globalization.DateTimeStyles.None, out temp)){
			outWarning("Invalid information");
			Debug.Log("Wrong Date");
			return;
		}
		loadingPanel.SetActive(true);
		registerPanel.gameObject.SetActive(false);
		JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
		j.AddField("email", registerEmail.text);
		j.AddField("password", registerPassword.text);
		j.AddField("first_name", registerFirstName.text);
		j.AddField("last_name", registerLastName.text);
		j.AddField("birth_date", temp.ToString("yyyy-MM-dd"));
		string data = j.print();
		StartCoroutine(submitToServer("user", "register", data));
	}

	public void OKPress(){
		switch(myStage){
		case Stage.Login:
			emailLogin.text = string.Empty;
			passwordLogin.text = string.Empty;
			welcomePanel.gameObject.SetActive(true);
			break;

		case Stage.Register:
			registerPassword.text = string.Empty;
			registerConfirmPassword.text = string.Empty;
			registerFirstName.text = string.Empty;
			registerLastName.text = string.Empty;
			registerYear.text = string.Empty;
			registerMonth.text = string.Empty;
			registerDate.text = string.Empty;
			registerPanel.gameObject.SetActive(true);
			break;

		case Stage.CreateGame:
			summaryPanel.gameObject.SetActive(true);
			break;
		}
	}

	public void createGame(){
		myStage = Stage.CreateGame;
		JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
		j.AddField("user_id", TestController.instance.currentTester.ID);
		j.AddField("screen_dpi", TestController.instance.screenDPI);
		j.AddField("screen_width", TestController.instance.screenWidth);
		j.AddField("screen_height", TestController.instance.screenHeight);
		j.AddField("sleep_time", sleepTime);
		j.AddField("stress_level", stressLevel);
		string data = j.print();
		StartCoroutine(submitToServer("game", "create", data));
	}

	public void pressExit(){
		Application.Quit();
	}

	void outWarning(string warning){
		warningText.text = warning;
		warningPanel.SetActive(true);
	}

	IEnumerator submitToServer(string className, string method, string data){
		WWWForm wwwForm = new WWWForm();
		wwwForm.AddField("c", className);
		wwwForm.AddField("m", method);
		wwwForm.AddField("data", data);
		WWW newWWW = new WWW("http://dev.ullon.com/mrpatch/api/", wwwForm);
		yield return newWWW;
		loadingPanel.SetActive(false);
		if(newWWW.error != null){
			Debug.Log("Has error " + newWWW.error);
			handleError();
		} else {
			Debug.Log("OK " + newWWW.text);
			handleResult(newWWW.text);
		}
		newWWW.Dispose();
	}

	void handleError(){
		switch(myStage){
		case Stage.Login:
			outWarning("Login Failed");
			break;
		case Stage.Register:
			outWarning("Register Failed");
			break;
			
		case Stage.CreateGame:
			outWarning("Create Game Failed");
			break;
		}
	}

	void handleResult(string result){
		result = result.Replace(@"\", string.Empty);
		result = result.Remove(0, 1);
		result = result.Remove(result.Length - 1, 1);
		var dic = Json.Deserialize(result) as Dictionary<string, object>;
		if(dic.ContainsKey("status") && (string)dic["status"] == "error"){
			switch(myStage){
			case Stage.Login:
				if(dic.ContainsKey("message")){
					outWarning((string)dic["message"]);
				} else {
					outWarning("Login Failed");
				}
				break;
			case Stage.Register:
				if(dic.ContainsKey("message")){
					outWarning((string)dic["message"]);
				} else {
					outWarning("Register Failed");
				}
				break;

			case Stage.CreateGame:
				loadingPanel.SetActive(false);
				if(dic.ContainsKey("message")){
					outWarning((string)dic["message"]);
				} else {
					outWarning("Create Game Failed");
				}
				break;
			}
			return;
		}
		var moreData = new List<object>();
		var newObj = dic["data"];

		switch(myStage){
		case Stage.Login:
			moreData = (List<object>)newObj;
			userSummary(moreData);
			break;

		case Stage.Register:
			moreData = (List<object>)newObj;
			userSummary(moreData);
			break;

		case Stage.CreateGame:
			var newObj2 = (Dictionary<string, object>)newObj;
			TestController.instance.currentGameSessionID = int.Parse((string)newObj2["game_session_id"]);
			myStage = Stage.StartGame;
			loadingPanel.SetActive(false);
			TestController.instance.gameReady();
			inGamePanel.SetActive(true);
			break;
		}
	}

	public void gameEnd(){
		inGamePanel.SetActive(false);
		inGameTimer.text = "0";
		//loadingPanel.SetActive(true);
		myStage = Stage.EndGame;
		gameEndPanel.SetActive(true);
	}

	public void displayEndPanel(){
		loadingPanel.SetActive(false);
		gameEndPanel.SetActive(true);
	}

	public void returnToMenu(){
		emailLogin.text = string.Empty;
		passwordLogin.text = string.Empty;
		registerPassword.text = string.Empty;
		registerConfirmPassword.text = string.Empty;
		registerFirstName.text = string.Empty;
		registerLastName.text = string.Empty;
		registerYear.text = string.Empty;
		registerMonth.text = string.Empty;
		registerDate.text = string.Empty;
	}

	public void seeResultPress(){
		TestController.instance.seeResult();
	}

	void userSummary(List<object> moreData){
//		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["id"]));
//		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["email"]));
//		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["password"]));
//		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["first_name"]));
//		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["last_name"]));
//		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["birth_date"]));
//		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["status"]));
		firstName.text = (string)(((Dictionary<string, object>)moreData[0])["first_name"]);
		lastName.text = (string)(((Dictionary<string, object>)moreData[0])["last_name"]);
		birthday.text = (string)(((Dictionary<string, object>)moreData[0])["birth_date"]);
		summaryPanel.SetActive(true);
		TestController.instance.currentTester.firstName = (string)(((Dictionary<string, object>)moreData[0])["first_name"]);
		TestController.instance.currentTester.lastName = (string)(((Dictionary<string, object>)moreData[0])["last_name"]);
		TestController.instance.currentTester.birthday = (string)(((Dictionary<string, object>)moreData[0])["birth_date"]);
		TestController.instance.currentTester.ID = int.Parse((string)(((Dictionary<string, object>)moreData[0])["id"]));
		TestController.instance.currentTester.email = (string)(((Dictionary<string, object>)moreData[0])["email"]);
		TestController.instance.currentTester.status = int.Parse((string)(((Dictionary<string, object>)moreData[0])["status"]));
	}
}
