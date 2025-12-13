using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    PlayerBasePoint _playerBasePoint;
    public void Start()
    {
        _playerBasePoint = GameObject.FindAnyObjectByType<PlayerBasePoint>();
        transform.position = _playerBasePoint.transform.position;
    }
}
