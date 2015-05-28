using System;
using ScienceAlert.Annotations;
using UnityEngine;

namespace ScienceAlert.Commands
{
    public class SpawnInformationalPopupCommand : ICommand
    {
        private readonly string _title;
        private readonly string _message;
        private readonly string _buttonMessage;
        private readonly bool _persistAcrossScenes;
        private readonly GUISkin _skin;

        public SpawnInformationalPopupCommand(string title, string message, string buttonMessage, bool persistAcrossScenes,
            [NotNull] GUISkin skin)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentException("title");
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("message");
            if (string.IsNullOrEmpty(buttonMessage)) throw new ArgumentException("buttonMessage");
            if (skin == null) throw new ArgumentNullException("skin");

            _title = title;
            _message = message;
            _buttonMessage = buttonMessage;
            _persistAcrossScenes = persistAcrossScenes;
            _skin = skin;
        }


        public void Execute()
        {
            PopupDialog.SpawnPopupDialog(_title, _message, _buttonMessage, _persistAcrossScenes, _skin);
        }
    }
}
