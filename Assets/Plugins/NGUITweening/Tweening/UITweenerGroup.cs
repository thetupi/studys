using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UITweenerGroup : MonoBehaviour {

    [SerializeField] bool _playOnEnable = true;
	[SerializeField] bool _disableOnFinishForward;
	[SerializeField] bool _disableOnFinishBackward = true;
	[SerializeField] bool _waitOtherTweens = true;


	List<UITweener> _tweeners = null;
	bool _waiting;
	bool backward;
	static List<UITweenerGroup> _runningTweeners = new List<UITweenerGroup>();

	List<UITweener> Tweeners{
        get{
            if(_tweeners == null)
            {
				_tweeners = new List<UITweener>();
				_tweeners.AddRange(GetComponents<UITweener>());
				_tweeners.AddRange(GetComponentsInChildren<UITweener>());
            }

            return _tweeners;
        }
    }

	protected virtual void Awake()
	{
		foreach (var tweener in Tweeners)
		{
			if (tweener.enabled)
				tweener.ResetToBeginning();

			tweener.enabled = false;
			var control = tweener.GetComponent<UITweenerControl>();
			if(control != null && !control.WaitForFinish)
				continue;
			EventDelegate.Add(tweener.onFinished, TweenFinished);
		}
	}

	void TweenFinished()
	{

		FinishPlay();
		FinishBackward();
	}
	
	void FinishPlay()
	{
		if (!backward && !ActiveTweeners)
		{
			_runningTweeners.Remove(this);
			if(_disableOnFinishForward && !_waiting)
				gameObject.SetActive(false);
		}
	}
	
	void FinishBackward()
	{
		if (backward && !ActiveTweeners)
		{
			_runningTweeners.Remove(this);
			backward = false;
			if(_disableOnFinishBackward && !_waiting)
				gameObject.SetActive(false);
		}
	}

    void OnEnable()
    {
        if(_playOnEnable)
        {
			if(_waitOtherTweens && _runningTweeners.Count > 0)
			{
				if(!_waiting)
					StartCoroutine(WaitEnable());
			}
			else 
				Play();
        }
    }

	IEnumerator WaitEnable()
	{
		if(_runningTweeners.Contains(this) == false)
		{
			foreach(var tween in Tweeners)
			{
				tween.ResetToBeginning();
				tween.Sample(0,false);
				tween.enabled	= false;
			}
		}
		_waiting = true;
		while(_runningTweeners.Count > 0)
			yield return null;
		_waiting = false;

		Play();
	}

	public void Play()
	{
		if(gameObject.activeSelf == false && _playOnEnable)
		{
			gameObject.SetActive(true);
			return;
		}
		gameObject.SetActive(true);
		if(_runningTweeners.Contains(this))
		{
			if(!_waiting)
				StartCoroutine(WaitPlay());
		}
		else 
		{
			_runningTweeners.Add( this);
			foreach(var tween in Tweeners)
			{
				tween.gameObject.SetActive(true);
				tween.enabled = true;
				tween.PlayForward();
				tween.ResetToBeginning();
				tween.PlayForward();
			}
		}
	}

	IEnumerator WaitPlay()
	{
		_waiting = true;
		while(_runningTweeners.Contains(this))
			yield return null;

		_waiting = false;
		Play();
	}

	public void PlayBackward()
	{
		gameObject.SetActive(true);
		if(_runningTweeners.Contains(this))
		{
			if(!_waiting)
				StartCoroutine(WaitPlayBackward());
		}
		else 
		{
			_runningTweeners.Add( this);
			backward = true;
			foreach(var tween in Tweeners)
			{
				var control = tween.GetComponent<UITweenerControl>();
				if(control != null && !control.PlayBackwards)
				{
					if(_disableOnFinishBackward)
						tween.gameObject.SetActive(false);
					continue;
				}
				tween.enabled = true;
				tween.PlayReverse();
			}
		}
	}

	IEnumerator WaitPlayBackward()
	{
		_waiting = true;
		while(_runningTweeners.Contains(this))
			yield return null;
		_waiting = false;
		PlayBackward();
	}
	
	bool ActiveTweeners { 
		get { 

			foreach( var tween in Tweeners)
			{
				var control = tween.GetComponent<UITweenerControl>();
				bool ignore = control != null && control.WaitForFinish == false;
				if(tween.enabled && !ignore)
					return true;
			}
			return false;
		} 
	}
}
