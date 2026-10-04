using Assets.Scripts;
using Assets.Scripts.Components;
using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PhotonView))]
public class Bullet3D : BaseBullet<BulletData>
{
}
