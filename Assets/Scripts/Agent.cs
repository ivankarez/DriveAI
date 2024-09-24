using System;
using System.Linq;
using UnityEngine;

namespace Ivankarez.DriveAI
{
    public class Agent : MonoBehaviour
    {
        private Entity entity;
        private Action<Agent> episodeEndCallback;
        private float runtime = 0f;
        [SerializeField] private AiVehicleController fullAiVehicleController;

        public Entity Entity
        {
            get
            {
                if (!IsInitialized)
                {
                    throw new InvalidOperationException("Agent is not initialized");
                }

                return entity;
            }
        }
        public bool IsInitialized { get; private set; } = false;
        public float Fitness { get; private set; }
        public AiVehicleController AiVehicleController => fullAiVehicleController;

        public void Initialize(Entity entity, Action<Agent> episodeEndCallback)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (episodeEndCallback == null)
            {
                throw new ArgumentNullException(nameof(episodeEndCallback));
            }

            this.entity = entity;
            this.episodeEndCallback = episodeEndCallback;
            IsInitialized = true;
            var raceTrack = FindObjectOfType<Racetrack>();
            raceTrack.MoveToStart(transform);
            fullAiVehicleController.Initialize(DnaUtils.CreateNeuralNetwork(entity.Dna.ToArray()), EndEpisode);
            runtime = 0f;

            FindObjectOfType<CameraController>().RegisterAgent(this);
        }

        private void Update()
        {
            runtime += Time.deltaTime;
            Fitness = CalculateFitness();

            if (runtime > 10 + (fullAiVehicleController.CheckpointsReached * 0.5f))
            {
                EndEpisode();
            }
            if (fullAiVehicleController.CheckpointsReached > 2000)
            {
                EndEpisode();
            }
            if (runtime > 2f)
            {
                CheckVehicleTooSlow(fullAiVehicleController);
            }
        }

        private void CheckVehicleTooSlow(AiVehicleController controller)
        {
            if (controller.enabled && controller.Speed < 0.1f)
            {
                EndEpisode();
            }
        }

        private void EndEpisode()
        {
            entity.Fitness = Fitness;
            FindObjectOfType<CameraController>().UnregisterAgent(this);
            episodeEndCallback(this);
        }

        private float CalculateFitness()
        {
            var checkpoints = fullAiVehicleController.CheckpointsReached;
            return checkpoints;
        }
    }
}
