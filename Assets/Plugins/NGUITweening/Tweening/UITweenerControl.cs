using UnityEngine;
using System.Collections;

public class UITweenerControl : MonoBehaviour {
	[SerializeField] bool playOnEnable = false;
	[SerializeField] bool playBackwards = true;
	[SerializeField] bool waitForFinish = true;

	UITweener[] tweeners;

	UITweener[] Tweeners{
		get{
			if(tweeners == null)
			{
				tweeners = GetComponents<UITweener>();
			}
			
			return tweeners;
		}
	}


	public bool PlayBackwards {
		get {
			return playBackwards;
		}
		set {
			playBackwards = value;
		}
	}

	public bool WaitForFinish {
		get {
			return waitForFinish;
		}
	}

	void OnEnable()
	{
		if(playOnEnable)
		{
			foreach(var tween in Tweeners)
			{
				tween.enabled = true;
				tween.PlayForward();
				tween.ResetToBeginning();
				tween.PlayForward();
			}
		}
	}
}
