using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleAnimate : MonoBehaviour {

	private Animator anim;
	private string AnimatorName;
	public float CrossfadeVal = 0.25f;

	private GameObject ChangeAnimationButton;

	void Start () {
		ChangeAnimationButton = GameObject.Find("ChangeAnimationButton");

		anim = GetComponent<Animator> ();
		AnimatorName = anim.name;
		print ("name " + AnimatorName);
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
	public void ChangeAnimationButtonClicked()
	{
		anim.CrossFade (AnimatorName +  ChangeAnimationButton.GetComponentInChildren<Text> ().text, CrossfadeVal);
		print ("chaning Animation");
	}
}
