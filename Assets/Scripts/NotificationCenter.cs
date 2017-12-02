using System.Collections.Generic;
using UnityEngine;

public class NotificationCenter
{
	// singleton pattern
	private static readonly NotificationCenter instance = new NotificationCenter();
	public static NotificationCenter Instance
	{
		get 
		{
			return instance; 
		}
	}

	public delegate void UpdateDelegator();

	Dictionary<string, UpdateDelegator> _delegateMap;

	private NotificationCenter()
	{
		_delegateMap = new Dictionary<string, UpdateDelegator> ();
	}
	public void Add(string subject, UpdateDelegator delegator)
	{
		if (_delegateMap.ContainsKey (subject) == false) 
		{
			_delegateMap[subject] = delegate() {};
		}

		_delegateMap [subject] += delegator;
	}

	public void Delete(string subject, UpdateDelegator delegator)
	{
		if (_delegateMap.ContainsKey (subject) == false) 
		{
			return;
		}

		_delegateMap [subject] -= delegator;
	}
	public void Notify(string subject)
	{
		if (_delegateMap.ContainsKey (subject) == false) 
		{
			return;
		}

		foreach(UpdateDelegator delegator in
			_delegateMap[subject].GetInvocationList())
		{
			try
			{
				delegator();
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}