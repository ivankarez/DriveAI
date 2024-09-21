using System.Collections.Generic;
using UnityEngine;

namespace Ivankarez.DriveAI
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private StaticThirdPersonFollow followCamera;

        private readonly List<Agent> agents = new();
        private Agent followedAgent;

        public void RegisterAgent(Agent agent)
        {
            agents.Add(agent);
        }

        public void UnregisterAgent(Agent agent)
        {
            agents.Remove(agent);
        }

        private void Update()
        {
            if (agents.Count == 0)
            {
                followCamera.SetTarget(null);
                followedAgent = null;
                return;
            }

            for (int i = 1; i < agents.Count; i++)
            {
                var index = agents.Count - i;
                var prevIndex = index - 1;

                if (agents[index].Fitness > agents[prevIndex].Fitness)
                {
                    var temp = agents[index];
                    agents[index] = agents[prevIndex];
                    agents[prevIndex] = temp;
                }
            }

            if (followedAgent == null || followedAgent != agents[0])
            {
                followedAgent = agents[0];
                followCamera.SetTarget(followedAgent.AiVehicleController.transform);
            }
        }
    }
}
