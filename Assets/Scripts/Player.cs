using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public Castle castle;
}
