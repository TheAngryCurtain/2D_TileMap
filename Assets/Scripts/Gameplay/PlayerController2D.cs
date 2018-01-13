using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private Transform mCachedTransform;
    [SerializeField] private float mMoveSpeed;

    private GameEvents.PlayerPositionUpdateEvent mPosUpdated = new GameEvents.PlayerPositionUpdateEvent();

    private void Start()
    {
        VSEventManager.Instance.TriggerEvent(new GameEvents.PlayerSpawnedEvent(mCachedTransform));
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (h != 0f || v != 0f)
        {
            mCachedTransform.Translate((mCachedTransform.right * h + mCachedTransform.up * v) * Time.deltaTime * mMoveSpeed);
        }

        // broadcast world pos
        mPosUpdated.WorldPosition = mCachedTransform.position;
        VSEventManager.Instance.TriggerEvent(mPosUpdated);
    }
}
