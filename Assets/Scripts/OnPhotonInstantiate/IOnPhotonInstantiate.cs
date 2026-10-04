using System.Collections;
using System.Collections.Generic;
using Photon;
using UnityEngine;

public interface IOnPhotonInstantiate
{
   void OnPhotonInstantiateExecute(object[] data, GameObject gameObject);
}
