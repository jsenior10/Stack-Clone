﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TheStack : MonoBehaviour {

	public Text scoreText;
	public Color32[] gameColors;
	private Color32 startColor;
	private Color32 endColor;
	public Material White;
	public GameObject EndPanel;
	private int lastColorIndex = 0;

	private const float BOUNDS_SIZE = 3.5f;
	private const float STACK_MOVING_SPEED = 5.0f;
	private const float ERROR_MARGIN = 0.2f;
	private const float BOUNDS_GAIN = 0.25f;
	private const int COMBO_GAIN_START = 3; 

	private float tileTransition = 0.0f;
	private float tileSpeed = 2.5f;
	private float colorTransition = 0;

	private GameObject[] theStack;
	private Vector2 stackBounds = new Vector2(BOUNDS_SIZE,BOUNDS_SIZE);
	
	private int stackIndex;
	private int scoreCount = 0;
	private int combo = 0;

	private float secondaryPosition;

	private bool isMovingOnX = true;
	private bool gameOver = false;

	private Vector3 desiredPosition;
	private Vector3 lastTilePosition;

	// Use this for initialization
	private void Start () {
		theStack = new GameObject[transform.childCount];
		startColor = gameColors[0];
		endColor = gameColors[1];
		for(int i = 0; i < transform.childCount; i++){
			theStack [i] = transform.GetChild (i).gameObject;
			ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
		}
			stackIndex = transform.childCount - 1;
	}
	
	// Update is called once per frame
	private void Update () 
	{
		if(gameOver)
			return;
		if(Input.GetMouseButtonDown(0) || Input.GetKeyDown("space")){
			if(PlaceTile()){
			SpawnTile();
			scoreCount++;
			scoreText.text = scoreCount.ToString();
		}
		else{
			EndGame();
		}
			
	}
		MoveTile();

		//move stack
		transform.position = Vector3.Lerp(transform.position,desiredPosition,STACK_MOVING_SPEED * Time.deltaTime);
	}

	private void CreateRubble(Vector3 pos, Vector3 scale){
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.transform.localPosition = pos;
		go.transform.localScale = scale;
		go.AddComponent<Rigidbody> ();

		go.GetComponent<MeshRenderer>().material = White;
		ColorMesh(go.GetComponent<MeshFilter> ().mesh);
	}

	private void MoveTile(){

		tileTransition += Time.deltaTime * tileSpeed;
		if(isMovingOnX)
			theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * (BOUNDS_SIZE + 0.5f),scoreCount,secondaryPosition);
		else 
			theStack[stackIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount, Mathf.Sin(tileTransition) * (BOUNDS_SIZE + 0.5f));

	}


	private void SpawnTile() {
		lastTilePosition = theStack[stackIndex].transform.localPosition;
		stackIndex--;
		if(stackIndex < 0)
			stackIndex = transform.childCount - 1;

		desiredPosition = (Vector3.down) * scoreCount;
		theStack [stackIndex].transform.localPosition = new Vector3 (0, scoreCount, 0);
		theStack [stackIndex].transform.localScale = (new Vector3(stackBounds.x,1,stackBounds.y));

		ColorMesh(theStack[stackIndex].GetComponent<MeshFilter> ().mesh);
	}


	private void ColorMesh(Mesh mesh){
	Vector3[] vertices = mesh.vertices;
		Color32[] colors = new Color32[vertices.Length];
		colorTransition += 0.05f;
		if (colorTransition > 1) 
		{
			colorTransition = 0.0f;
			startColor = endColor;
			int ci = lastColorIndex;
			while (ci == lastColorIndex)
				ci = Random.Range (0, gameColors.Length);
			endColor = gameColors [ci];
		}
		Color c = Color.Lerp(startColor,endColor,colorTransition);

		for (int i = 0; i < vertices.Length; i++)
			colors [i] = c;

		mesh.colors32 = colors;
	}
	private bool PlaceTile(){
		Transform t = theStack[stackIndex].transform;

		if(isMovingOnX){
			float deltaX = lastTilePosition.x - t.position.x;
			if(Mathf.Abs(deltaX) > ERROR_MARGIN){
				//cut tile
				combo = 0;
				stackBounds.x -=  Mathf.Abs (deltaX);
				if(stackBounds.x <= 0)
					return false;

				float middle = lastTilePosition.x + t.localPosition.x / 2;
				t.localScale = (new Vector3(stackBounds.x,1,stackBounds.y));
				CreateRubble
				(
					new Vector3((t.position.x > 0)
					? t.position.x + (t.localScale.x/2)
					: t.position.x - (t.localScale.x/2)
					,t.position.y
					,t.position.z),
					new Vector3(Mathf.Abs(deltaX),1,t.localScale.z)
				);
				t.localPosition = new Vector3(middle-(lastTilePosition.x/2), scoreCount, lastTilePosition.z);

			}
			else{
				if(combo > COMBO_GAIN_START){
					if(stackBounds.x > BOUNDS_SIZE)
						stackBounds.x = BOUNDS_SIZE;
					stackBounds.x += BOUNDS_GAIN;
					float middle = lastTilePosition.x + t.localPosition.x / 2;
					t.localScale = (new Vector3(stackBounds.x,1,stackBounds.y));
					t.localPosition = new Vector3(middle-(lastTilePosition.x/2), scoreCount, lastTilePosition.z);
				}
				combo++;
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, lastTilePosition.z);
			}
		}
		else{
			float deltaZ = lastTilePosition.z - t.position.z;
			if(Mathf.Abs(deltaZ) > ERROR_MARGIN){
				//cut tile
				combo = 0;
				stackBounds.y -=  Mathf.Abs (deltaZ);
				if(stackBounds.y <= 0)
					return false;

				
				float middle = lastTilePosition.z + t.localPosition.z / 2;
				t.localScale = new Vector3(stackBounds.x,1,stackBounds.y);
				CreateRubble
				(
					new Vector3 (t.position.x
						, t.position.y
						, (t.position.z > 0) 
						? t.position.z + (t.localScale.z / 2)
						: t.position.z - (t.localScale.z / 2)),
					new Vector3 (t.localScale.x, 1, Mathf.Abs (deltaZ))
				);
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount,middle - (lastTilePosition.z / 2));
		}
			else{
				if(combo > COMBO_GAIN_START){
					if(stackBounds.y > BOUNDS_SIZE)
						stackBounds.y = BOUNDS_SIZE;
					stackBounds.y += BOUNDS_GAIN;
					float middle = lastTilePosition.z + t.localPosition.z / 2;
					t.localScale = (new Vector3(stackBounds.x,1,stackBounds.y));
					t.localPosition = new Vector3(middle-(lastTilePosition.x/2), scoreCount, lastTilePosition.z);
				}
				combo++;
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, lastTilePosition.z);
			}
		}
				secondaryPosition = (isMovingOnX)
			? t.localPosition.x
			: t.localPosition.z;
		isMovingOnX = !isMovingOnX;
		return true;
		}


	private void EndGame(){
		if(PlayerPrefs.GetInt("score") < scoreCount)
			PlayerPrefs.SetInt("score", scoreCount);
		gameOver = true;
		EndPanel.SetActive(true);
		theStack[stackIndex].AddComponent<Rigidbody> ();
	}
	
	public void OnButtonClick(string sceneName){
		SceneManager.LoadScene(sceneName);
	}

	}

