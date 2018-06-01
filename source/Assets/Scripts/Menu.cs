﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

public Text scoreText;

private void Start(){
	scoreText.text = PlayerPrefs.GetInt("score").ToString();
}

public void ToGame(){
	SceneManager.LoadScene("Game");
}
}
