﻿using System.Threading.Tasks;
using TTMouseclickSimulator.Core.Actions;
using TTMouseclickSimulator.Core.Environment;

namespace TTMouseclickSimulator.Core.ToontownCorporateClash.Actions.DoodleInteraction
{
    /// <summary>
    /// An action that clicks on the doodle interaction panel (Feed, Scratch, Call)
    /// </summary>
    public class DoodlePanelAction : AbstractAction
    {

        private readonly DoodlePanelButton button;


        public DoodlePanelAction(DoodlePanelButton button)
        {
            this.button = button;
        }

        public override async Task RunAsync(IInteractionProvider provider)
        {
            var c = new Coordinates(1397, 206 + (int)this.button * 49);
            await MouseHelpers.DoSimpleMouseClickAsync(provider, c, VerticalScaleAlignment.Right);
        }


        public override string ToString() => $"Doodle Panel – Button: {this.button}";
        

        public enum DoodlePanelButton : int
        {
            Feed = 0,
            Scratch = 1,
            Call = 2
        }
    }
}
