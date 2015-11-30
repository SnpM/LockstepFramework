using UnityEngine;
using System.Collections;
using System;
using Lockstep.Data;
namespace Lockstep
{
	public class LSEffect : MonoBehaviour
	{

		public float Duration {get; set;}



		#region For external use
		//TODO: Add needed variables for external mechanisms (I.e. Sinus).
		#endregion

		#region Default Values
		Vector3 defaultScale;
		[SerializeField]
		private float
			defaultDuration;
		#endregion

		public string MyEffectCode { get; private set; }
		public Transform CachedTransform {get; private set;}
		public GameObject CachedGameObject {get; private set;}

		public ParticleSystem CachedShuriken {get; private set;}
		float[] CachedShurikenVals = new float[2];

		public int ID { get; private set; }

		public event Action OnSetup;
		public event Action OnInitialize;
		public event Action OnVisualize;
		public event Action OnDeactivate;
		/// <summary>
		/// Called when this effect is first created.
		/// </summary>
		/// <param name="myEffectCode">My effect code.</param>
		public void Setup (string myEffectCode)
		{
			MyEffectCode = myEffectCode;
			CachedTransform = base.transform;
			CachedGameObject = base.gameObject;
			CachedShuriken = base.gameObject.GetComponent<ParticleSystem>();
			if(CachedShuriken){
				CachedShurikenVals[0] = CachedShuriken.startSpeed;
				CachedShurikenVals[1] = CachedShuriken.startSize;
			}
			if (OnSetup .IsNotNull ())
				OnSetup ();
			GameObject.DontDestroyOnLoad (CachedGameObject);

			defaultScale = CachedTransform.localScale;
		}

		/// <summary>
		/// For internal use.
		/// </summary>
		/// <param name="id">Identifier.</param>
		public bool Create (int id)
		{
			if (CachedGameObject == null)
			{
				EffectManager.DestroyEffect (this);
				return false;
			}
			ID = id;

			//Reset variables
			Duration = defaultDuration;
			CachedTransform.localScale = defaultScale;

			return true;
		}

		/// <summary>
		/// Call this when all parameters are supplied as desired.
		/// </summary>
		public void Initialize ()
		{

			CachedGameObject.SetActive (true);
			StartCoroutine (LifeTimer ());
			if (OnInitialize .IsNotNull ())
				OnInitialize ();
		}

		/// <summary>
		/// Called every Update frame.
		/// </summary>
		public void Visualize ()
		{
			if (OnVisualize .IsNotNull ()) 
				OnVisualize ();
		}
			
		/// <summary>
		/// Called when this effect is deactivated.
		/// </summary>
		public void Deactivate ()
		{
			CachedTransform.SetParent (null);
			CachedGameObject.SetActive (false);
			if (OnDeactivate .IsNotNull ())
				OnDeactivate ();
		}

		IEnumerator LifeTimer ()
		{
			//yield return Duration;
			yield return new WaitForSeconds(Duration);
			EffectManager.EndEffect (this);
		}
	}
}