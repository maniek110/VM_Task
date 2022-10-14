using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    public BoardCompononets boardCompononets;
    public Transform TargetParent;
    public override void InstallBindings()
    {
        Container.Bind<BoardManager>().AsSingle().WithArguments(boardCompononets,TargetParent).NonLazy();
    }
}
