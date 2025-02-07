using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    public class LoadingUI : MonoBehaviour
    {
        public TMP_Text loadingText;
        public static LoadingUI Instance;
        private static readonly int Fade = Animator.StringToHash("Fade");
        private static readonly int Show1 = Animator.StringToHash("Show");
        public Animator animator;
        public bool isEnabled;

        private void Awake()
        {
            if(Instance == null || animator is null)
                SetUp();
        }

        public void SetUp()
        {
            Instance = this;
            animator = GetComponent<Animator>();
        }

        public void SetText(string text) => loadingText.text = text;

        public void Show(bool fade = false)
        {
            isEnabled = true;
            gameObject.SetActive(true);
            animator.SetBool(Fade, fade);
            animator.SetBool(Show1, true);
        }


        public void HideLoader(bool hasToFade = false)
        {
            animator.SetBool(Fade, hasToFade);
            animator.SetBool(Show1, false);
            if(isActiveAndEnabled)
                StartCoroutine(Hide());
            isEnabled = false;
        }
        
        
        public IEnumerator Hide()
        {
            yield return new WaitForSeconds(1f);
            if(!isEnabled)
                gameObject.SetActive(false);
        }
    }
}