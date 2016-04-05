using UnityEngine;
using System.Collections;

namespace Lockstep
{
    public class SelectionRingController : Ability
    {
        [SerializeField]
        private SelectionRing _ringTemplate;

        [SerializeField]
        private float _ringRadiusOffset = -2;

        SelectionRing RingTemplate { get { return _ringTemplate; } }

        SelectionRing RingObject;



        protected override void OnSetup()
        {
            Agent.onSelectedChange += HandleSelectedChange;
            Agent.onHighlightedChange += HandleHighlightedChange;
            RingObject = GameObject.Instantiate(_ringTemplate.gameObject).GetComponent<SelectionRing>();
            RingObject.Setup((Agent.SelectionRadius + _ringRadiusOffset) * 2);

            RingObject.transform.parent = this.transform;
            RingObject.transform.localPosition = Vector3.zero;
        }

        public void HandleSelectedChange()
        {
            if (!Agent.IsSelected)
            {
                if (Agent.IsHighlighted)
                {
                    RingObject.SetState(SelectionRingState.Highlighted);
                }
                else {
                    RingObject.SetState(SelectionRingState.None);
                }
            }
            else {
                RingObject.SetState(SelectionRingState.Selected);
            }
             
        }

        public void HandleHighlightedChange()
        {
            if (Agent.IsHighlighted) {
                if (Agent.IsSelected) {

                }
                else {
                    RingObject.SetState(SelectionRingState.Highlighted);
                }
            }
            else {
                if (!Agent.IsSelected) {
                    RingObject.SetState(SelectionRingState.None);
                }
            }
        }
    }
}