using System;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;

namespace ShirokuStudio.Core.Components
{
    public class SetParameter : StateMachineBehaviour
    {
        [Header("進入State時")]
        [SerializeField]
        [Tooltip("於進入State時觸發事件")]
        private UltEvent onEnter;

        [SerializeField]
        [Tooltip("於進入State時設定Parameter數值")]
        private AnimatorParameterMap parameterOnEnter;

        [SerializeField]
        [Tooltip("於進入State時呼叫EventProxy事件")]
        private List<string> eventProxyOnEnter = new List<string>();

        [Space]
        [Header("離開State時")]
        [SerializeField]
        [Tooltip("於離開State時觸發事件")]
        private UltEvent onExit;

        [SerializeField()]
        [Tooltip("於離開State時設定Parameter數值")]
        private AnimatorParameterMap parameterOnExit;

        [SerializeField]
        [Tooltip("於離開State時呼叫EventProxy事件")]
        private List<string> eventProxyOnExit = new List<string>();

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            foreach (var item in parameterOnEnter)
            {
                item.Value.SetParameter(animator, item.Key);
            }
            var eventProxy = animator.GetComponent<EventProxy>();
            if (eventProxy)
            {
                eventProxyOnEnter.ForEach(eventProxy.InvokeEvent);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            foreach (var item in parameterOnExit)
            {
                item.Value.SetParameter(animator, item.Key);
            }
            var eventProxy = animator.GetComponent<EventProxy>();
            if (eventProxy)
            {
                eventProxyOnExit.ForEach(eventProxy.InvokeEvent);
            }
        }

        [Serializable]
        public class AnimatorParameterMap : SerializableDictionary<string, AnimatorParameterValue>
        { }

        [Serializable]
        public class AnimatorParameterValue
        {
            [SerializeField]
            public AnimatorControllerParameterType Type = AnimatorControllerParameterType.Float;

            [SerializeField]
            public float FloatValue;

            [SerializeField]
            public int IntValue;

            [SerializeField]
            public bool BoolValue;

            public void SetParameter(Animator animator, string parameterName)
            {
                switch (Type)
                {
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(parameterName, FloatValue);
                        break;

                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(parameterName, IntValue);
                        break;

                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(parameterName, BoolValue);
                        break;

                    case AnimatorControllerParameterType.Trigger:
                        animator.SetTrigger(parameterName);
                        break;
                }
            }
        }
    }
}