using System;
using UnityEngine;
using System.Collections.Generic;

namespace Lockstep
{
    public class LSInfluencer
    {
        #region Static Helpers

        static LSAgent tempAgent;
        static GridNode tempNode;

        #endregion

        #region Collection Helper

        [NonSerialized]
        public int bucketIndex = -1;

        #endregion

        #region ScanNode Helper

        public int NodeTicket;

        #endregion

        public GridNode LocatedNode { get; private set; }

        public LSBody Body { get; private set; }

        public LSAgent Agent { get; private set; }

        public void Setup(LSAgent agent)
        {
            Agent = agent;
            Body = agent.Body;
        }

        public void Initialize()
        {
            LocatedNode = GridManager.GetNode(Body._position.x, Body._position.y);
            LocatedNode.Add(this);
        }

        public void Simulate()
        {

            if (Body.PositionChangedBuffer)
            {
                tempNode = GridManager.GetNode(Body._position.x, Body._position.y);

                if (tempNode.IsNull())
                    return;
				
                if (System.Object.ReferenceEquals(tempNode, LocatedNode) == false)
                {
                    if (LocatedNode != null)
                        LocatedNode.Remove(this);
                    tempNode.Add(this);
                    LocatedNode = tempNode;
                }
            }
        }

        public void Deactivate()
        {
            if (LocatedNode != null)
            {
                LocatedNode.Remove(this);
                LocatedNode = null;
            }
        }


        const AllegianceType AllAllegiance = (AllegianceType)~0;
    }


}