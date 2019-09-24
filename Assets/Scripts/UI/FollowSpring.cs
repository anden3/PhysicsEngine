using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowSpring : MonoBehaviour
{
        public ParticleStandaloneSpring spring;

    void Update()
    {
        Vector3 tempPos = spring.transform.position;
        transform.position = Camera.main.WorldToScreenPoint(tempPos);

    }
}
