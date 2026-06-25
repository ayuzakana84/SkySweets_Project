using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoss
{
    event System.Action<int> OnBossDamageTaken;
    int MaxHP { get; }
}
