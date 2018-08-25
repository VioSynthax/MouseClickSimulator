﻿using System;
using System.Text;
using System.Threading.Tasks;
using TTMouseclickSimulator.Core.Actions;
using TTMouseclickSimulator.Core.Environment;

namespace TTMouseclickSimulator.Core.ToontownCorporateClash.Actions.Speedchat
{
    [Serializable]
    public class SpeedchatAction : AbstractAction
    {
        private static readonly int[] xWidths =
        {
            215,
            215 + 250,
            215 + 250 + 180
        };

        private readonly int[] menuItems;
        

        public SpeedchatAction(params int[] menuItems)
        {
            this.menuItems = menuItems;
            if (menuItems.Length > 3)
                throw new ArgumentException("Only 3 levels are supported.");
            if (menuItems.Length == 0)
                throw new ArgumentException("The menuItems array must not be empty.");
        }


        public override sealed async Task RunAsync(IInteractionProvider provider)
        {
            // Click on the Speedchat Icon.
            var c = new Coordinates(122, 40);
            await MouseHelpers.DoSimpleMouseClickAsync(provider, c, VerticalScaleAlignment.Left, 100);

            int currentYNumber = 0;
            for (int i = 0; i < this.menuItems.Length; i++)
            {
                await provider.WaitAsync(300);

                currentYNumber += this.menuItems[i];
                c = new Coordinates(xWidths[i], (40 + currentYNumber * 38));
                await MouseHelpers.DoSimpleMouseClickAsync(provider, c, VerticalScaleAlignment.Left, 100);
            }
        }


        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < this.menuItems.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(this.menuItems[i]);
            }
            return $"Speedchat – Items: [{sb.ToString()}]";
        }
    }
}
