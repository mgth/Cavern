﻿using UnityEngine;

namespace Cavern {
    /// <summary>Creates an atmosphere of the given <see cref="Clips"/>.</summary>
    [AddComponentMenu("Audio/3D Atmosphere")]
    public class Atmosphere : MonoBehaviour {
        /// <summary>The possible clips that will be played at random positions.</summary>
        [Tooltip("The possible clips that will be played at random positions.")]
        public AudioClip[] Clips;
        /// <summary>The amount of audio sources in the atmosphere.</summary>
        [Tooltip("The amount of audio sources in the atmosphere.")]
        public int Sources;
        /// <summary>The shape of the atmosphere. Cubic if disabled.</summary>
        [Tooltip("The shape of the atmosphere. Cubic if disabled.")]
        public bool Spherical = true;
        /// <summary>Minimal distance to spawn sources from the object's position.</summary>
        [Tooltip("Minimal distance to spawn sources from the object's position.")]
        public float MinDistance = 5;
        /// <summary>Maximum distance to spawn sources from the object's position.</summary>
        [Tooltip("Maximum distance to spawn sources from the object's position.")]
        public float MaxDistance = 20;
        /// <summary>Atmosphere volume.</summary>
        [Tooltip("Atmosphere volume.")]
        [Range(0, 1)] public float Volume = .25f;

        struct AtmosphereObject {
            public GameObject Object;
            public AudioSource3D Source;
        }

        AtmosphereObject[] Objects;

        void Start() {
            Objects = new AtmosphereObject[Sources];
        }

        void OnDrawGizmosSelected() {
            if (!gameObject.activeInHierarchy)
                return;
            if (Spherical) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, MinDistance);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, MaxDistance);
            } else {
                float CorrectMinDist = MinDistance * 2, CorrectMaxDist = MaxDistance * 2;
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, new Vector3(CorrectMinDist, CorrectMinDist, CorrectMinDist));
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, new Vector3(CorrectMaxDist, CorrectMaxDist, CorrectMaxDist));
            }
        }

        void Update() {
            for (int Source = 0; Source < Sources; ++Source) {
                if (!Objects[Source].Object) {
                    Objects[Source].Object = new GameObject();
                    Vector3 Angles = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
                    Vector3 Direction = Spherical ? CavernUtilities.PlaceInSphere(Angles) : CavernUtilities.PlaceInCube(Angles);
                    float Distance = (MaxDistance - MinDistance) * Random.value + MinDistance;
                    if (Spherical)
                        Distance /= (Direction.magnitude + .0001f);
                    Objects[Source].Object.transform.position = transform.position + Direction * Distance;
                    Objects[Source].Source = Objects[Source].Object.AddComponent<AudioSource3D>();
                    Objects[Source].Source.Clip = Clips[Random.Range(0, Clips.Length)];
                    Objects[Source].Source.RandomPosition = true;
                    Objects[Source].Source.Volume = Volume / Sources;
                } else if (!Objects[Source].Source.IsPlaying)
                    Destroy(Objects[Source].Object);
            }
        }
    }
}