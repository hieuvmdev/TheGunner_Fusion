using UnityEngine;
using System.Collections;
//using Spine.Unity;
// Cartoon FX  - (c) 2013,2014 Jean Moreno

// Automatically destructs an object when it has stopped emitting particles and when they have all disappeared from the screen.
// Check is performed every 0.5 seconds to not query the particle system's state every frame.
// (only deactivates the object if the OnlyDeactivate flag is set, automatically used with CFX Spawn System)
public class SelfReturnToPool : MonoBehaviour
{
	// If true, deactivate the object instead of destroying it
	public bool OnlyDeactivate;
	public ObjectType type;
	public float lifeTime;
	public ParticleSystem particle;

	void OnEnable()
	{
		if (type.Equals(ObjectType.Particle))
		{
			StartCoroutine(CheckIfAlive());
		}
		else if (type.Equals(ObjectType.Spine))
		{
			StartCoroutine(ReturnOnComplete());
		}
		else
		{
			StartCoroutine(ReturnAfterTime());
		}
	}

	IEnumerator CheckIfAlive()
	{
		ParticleSystem ps = this.GetComponent<ParticleSystem>();

		while (true && particle != null)
		{
			yield return new WaitForSeconds(0.5f);
			if (!particle.IsAlive(true))
			{
				if (OnlyDeactivate)
				{
					gameObject.SetActive(false);
				}
				else
					PoolManager.Instance.ReturnObjectToPool(gameObject);
				break;
			}
		}
	}

	private IEnumerator ReturnOnComplete()
	{
		yield return new WaitForSeconds(lifeTime);
		//gameObject.GetComponent<SkeletonAnimation>().state.ClearTracks();
		PoolManager.Instance.ReturnObjectToPool(gameObject);
	}

	private IEnumerator ReturnAfterTime()
	{
		yield return new WaitForSeconds(lifeTime);
		PoolManager.Instance.ReturnObjectToPool(gameObject);
	}
}

public enum ObjectType
{
	Particle,
	Spine,
	None,
}