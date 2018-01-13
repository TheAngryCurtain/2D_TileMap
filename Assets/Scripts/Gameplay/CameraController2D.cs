using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    [SerializeField] private Transform mCachedTransform;
    [SerializeField] private float mLerpSpeed = 5f;

    private Transform mTargetTransform;
    private Vector3 mTargetPosition = Vector3.zero;

    private void OnEnable()
    {
        VSEventManager.Instance.AddListener<GameEvents.PlayerSpawnedEvent>(OnPlayerSpawned);
    }

    private void OnPlayerSpawned(GameEvents.PlayerSpawnedEvent e)
    {
        SetTarget(e.PlayerTransform);
    }

    public void SetTarget(Transform t)
    {
        mTargetTransform = t;
    }

    private void LateUpdate()
    {
        if (mTargetTransform != null)
        {
            mTargetPosition = Vector3.Lerp(mCachedTransform.position, mTargetTransform.position, Time.deltaTime * mLerpSpeed);
            mTargetPosition.z = -10;
            // TODO clamp at the edges of the map
            mCachedTransform.position = mTargetPosition;
        }
    }
}