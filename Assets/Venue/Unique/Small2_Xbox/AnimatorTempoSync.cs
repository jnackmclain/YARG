using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YARG.PlayMode;

namespace YARG
{
    public class AnimatorTempoSync : MonoBehaviour
    {
		Animator m_Animator;
		//float m_CurrentClipLength;
		public float speed = 1;

		void Start() {
			m_Animator = gameObject.GetComponent<Animator>();
			m_Animator.speed = Play.Instance.CurrentTempo / 60;
			//m_CurrentClipInfo = this.m_Animator.GetCurrentAnimatorClipInfo(0);
		}

		void Update() {
			print("BPM: " + Play.Instance.CurrentTempo + " / " + "Animation Speed: " + m_Animator.speed);
			m_Animator.speed = Play.Instance.CurrentTempo / 60 * speed;
		}
	}
}
