﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TTMouseclickSimulator.Core.Environment;

namespace TTMouseclickSimulator.Core.ToontownCorporateClash.Actions.Fishing
{
    public class AutomaticFishingAction : AbstractFishingRodAction
    {
        private FishingSpotData spotData;

        protected override int WaitingForFishResultDialogTime => 6000;

        public AutomaticFishingAction(int[] scan1, int[] scan2, byte[] bubbleColorRgb, byte[] toleranceRgb)
        {
            this.spotData = new FishingSpotData(
                new Coordinates(scan1[0], scan1[1]),
                new Coordinates(scan2[0], scan2[1]),
                new ScreenshotColor(bubbleColorRgb[0], bubbleColorRgb[1], bubbleColorRgb[2]),
                new Tolerance(toleranceRgb[0], toleranceRgb[1], toleranceRgb[2]));
        }
        

        protected override sealed async Task FinishCastFishingRodAsync(IInteractionProvider provider)
        {
            // Try to find a bubble.
            const string actionInformationScanning = "Scanning for fish bubbles…";
            OnActionInformationUpdated(actionInformationScanning);

            const int scanStep = 15;

            var sw = new Stopwatch();
            sw.Start();

            Coordinates? oldCoords = null;
            Coordinates? newCoords;
            int coordsMatchCounter = 0;
            while (true)
            {
                var screenshot = provider.GetCurrentWindowScreenshot();
                newCoords = null;

                // TODO: The fish bubble detection should be changed so that it does not scan
                // for a specific color, but instead checks that for a point if the color is
                // darker than the neighbor pixels (in some distance).
                for (int y = this.spotData.Scan1.Y; y <= this.spotData.Scan2.Y && !newCoords.HasValue; y += scanStep)
                {
                    for (int x = this.spotData.Scan1.X; x <= this.spotData.Scan2.X; x += scanStep)
                    {
                        var c = new Coordinates(x, y);
                        c = screenshot.WindowPosition.ScaleCoordinates(c,
                            MouseHelpers.ReferenceWindowSize);
                        if (CompareColor(this.spotData.BubbleColor, screenshot.GetPixel(c),
                            this.spotData.Tolerance))
                        {
                            newCoords = new Coordinates(x + 20, y + 20);
                            var scaledCoords = screenshot.WindowPosition.ScaleCoordinates(
                                newCoords.Value, MouseHelpers.ReferenceWindowSize);

                            OnActionInformationUpdated($"Found bubble at {scaledCoords.X}, {scaledCoords.Y}…");
                            break;
                        }
                    }
                }
                if (!newCoords.HasValue)
                    OnActionInformationUpdated(actionInformationScanning);


                if (newCoords.HasValue && oldCoords.HasValue
                    && Math.Abs(oldCoords.Value.X - newCoords.Value.X) <= scanStep
                    && Math.Abs(oldCoords.Value.Y - newCoords.Value.Y) <= scanStep)
                {
                    // The new coordinates are (nearly) the same as the previous ones.
                    coordsMatchCounter++;
                }
                else
                {
                    // Reset the counter and update the coordinates even if we currently didn't
                    // find them.
                    oldCoords = newCoords;
                    coordsMatchCounter = 0;
                }


                // Now position the mouse already so that we just need to release the button.
                if (!newCoords.HasValue)
                {
                    // If we couldn't find the bubble we use default destination x,y values.
                    newCoords = new Coordinates(800, 1009);
                }
                else
                {
                    // Calculate the destination coordinates.
                    newCoords = new Coordinates(
                        (int)Math.Round(800d + 120d / 429d * (800d - newCoords.Value.X) *
                        (0.75 + (820d - newCoords.Value.Y) / 820 * 0.38)),
                        (int)Math.Round(846d + 169d / 428d * (820d - newCoords.Value.Y))
                    );
                }

                // Note: Instead of using a center position for scaling the X coordinate,
                // TTCC seems to interpret it as being scaled from an 4/3 ratio. Therefore
                // we need to specify "NoAspectRatio" here.
                // However it could be that they will change this in the future, then 
                // we would need to use "Center".
                // Note: We assume the point to click on is exactly centered. Otherwise
                // we would need to adjust the X coordinate accordingly.
                var coords = screenshot.WindowPosition.ScaleCoordinates(newCoords.Value,
                    MouseHelpers.ReferenceWindowSize, VerticalScaleAlignment.NoAspectRatio);
                provider.MoveMouse(coords);


                if (coordsMatchCounter == 2)
                {
                    // If we found the same coordinates two times, we assume
                    // the bubble is not moving at the moment.
                    break;
                }
                
                await provider.WaitAsync(500);

                // Ensure we don't wait longer than 36 seconds.
                if (sw.ElapsedMilliseconds >= 36000)
                    break;
            }


            // There is no need to wait here because the mouse has already been positioned and we
            // waited at least 2x 500 ms at the new position, so now just release the mouse button.
            provider.ReleaseMouseButton();
        }


        public override string ToString() => $"Automatic Fishing – "
                + $"Color: [{this.spotData.BubbleColor.r}, {this.spotData.BubbleColor.g}, {this.spotData.BubbleColor.b}]";
    }
}
