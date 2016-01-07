using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScienceAlert.Game;
using strange.extensions.command.impl;

namespace ScienceAlert.VesselContext
{
    /// <summary>
    /// Since the ActiveVessel might not quite be ready for whatever reason (just spawned etc),
    /// splitting off the waiting logic seemed like a good idea
    /// </summary>
    //public class CommandBindActiveVessel : Command
    //{
    //    private readonly IGameFactory _gameFactory;

    //    public CommandBindActiveVessel(IGameFactory gameFactory)
    //    {
    //        if (gameFactory == null) throw new ArgumentNullException("gameFactory");
    //        _gameFactory = gameFactory;
    //    }

    //    public override void Execute()
    //    {
    //        Retain();
    //    }
    //}
}
