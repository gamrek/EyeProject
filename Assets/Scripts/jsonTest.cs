using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MiniJSON;

public class jsonTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
		j.AddField("email", "dereklam0528@gmail.com");
		j.AddField("password", "1234567890");
		//Dictionary<string, object> dict = new Dictionary<string, object>();
//		dict["email"] = "dereklam0528@gmail.com";
//		dict["password"] = 1234567890;
//		dict["first_name"] = "Derek";
//		dict["last_name"] = "Lam";
//		dict["birth_date"] = System.DateTime.Today.ToString("d");
//		dict["email"] = "dereklam0528@gmail.com";
//		dict["password"] = "1234567890";
		string a = j.print();
		StartCoroutine(sendData("user", "login", a));
		Debug.Log(a);
	}

	IEnumerator sendData(string className, string functionName, string parameter){
		WWWForm wwwForm = new WWWForm();
		wwwForm.AddField("c", className);
		wwwForm.AddField("m", functionName);
		wwwForm.AddField("data", parameter);
		WWW newWWW = new WWW("http://dev.ullon.com/mrpatch/api/", wwwForm);
		yield return newWWW;
		if(newWWW.error != null){
			Debug.Log("Has error " + newWWW.error);
		} else {
			//Debug.Log("OK " + newWWW.text);
			getUserData(newWWW.text);
		}
	}

	void getUserData(string txt){
		txt = txt.Replace(@"\", string.Empty);
		txt = txt.Remove(0, 1);
		txt = txt.Remove(txt.Length - 1, 1);
		var dic = Json.Deserialize(txt) as Dictionary<string, object>;
		Debug.Log(dic["status"].ToString());
		var moreData = new List<object>();
		var newObj = dic["data"];
		moreData = (List<object>)newObj;
		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["id"]));
		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["email"]));
		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["password"]));
		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["first_name"]));
		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["last_name"]));
		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["birth_date"]));
		Debug.Log((string)(((Dictionary<string, object>)moreData[0])["status"]));
	}

	// Update is called once per frame
	void Update () {
	
	}
}
