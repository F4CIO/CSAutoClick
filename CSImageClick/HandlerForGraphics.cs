using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSImageClick
{
    internal class HandlerForGraphics
    {
        public static void DrawRectangle(Graphics graphics, Rectangle rectangle, Color color)
        {
            using(Brush brush = new SolidBrush(color))
            {
                graphics.FillRectangle(brush, rectangle);
            }
        }

        public static Icon CreateIcon(Color color)
        {
            Icon icon = null;
            int width = 16; // Icon width
            int height = 16; // Icon height
            using(var bitmap = new Bitmap(width, height))
            {

                using(Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.Transparent); // Set background to transparent
                    using(Brush brush = new SolidBrush(color))
                    {
                        // Draw the letter "A"
                        Font font = new Font("Arial", 12, FontStyle.Bold);
                        g.DrawString("A", font, brush, new PointF(0, 0));
                    }

                    using(Pen pen = new Pen(color))
                    {
                        g.DrawEllipse(pen, new Rectangle(1, 1, width - 2, height - 2));
                    }
                }

                // Create the icon from the bitmap
                icon = Icon.FromHandle(bitmap.GetHicon());
            }

            // Return the icon, but ensure to release the handle when done
            return icon;
        }

        public static Bitmap CaptureAreaFromDesktop(Rectangle bounds)
        {
            Bitmap screenCapture = new Bitmap(bounds.Width, bounds.Height);
            using(Graphics g = Graphics.FromImage(screenCapture))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }
            return screenCapture; // Ensure to dispose of this Bitmap in the calling method
        }

        public static Bitmap CopyFromBitmap(Bitmap sourceBitmap, Rectangle sourceBounds)
        {
            // Ensure the bounds are within the source bitmap dimensions
            if(sourceBounds.X < 0 || sourceBounds.Y < 0 ||
                sourceBounds.Right > sourceBitmap.Width ||
                sourceBounds.Bottom > sourceBitmap.Height)
            {
                throw new ArgumentOutOfRangeException("The specified bounds are outside the source bitmap dimensions.");
            }

            Bitmap copiedBitmap = new Bitmap(sourceBounds.Width, sourceBounds.Height);
            using(Graphics g = Graphics.FromImage(copiedBitmap))
            {
                g.DrawImage(sourceBitmap, new Rectangle(0, 0, copiedBitmap.Width, copiedBitmap.Height),
                             sourceBounds, GraphicsUnit.Pixel);
            }

            return copiedBitmap; // Ensure to dispose of this Bitmap in the calling method
        }

        private static Random random = new Random();
        public static bool PixelByPixelCompare(Image template, Bitmap screenImage, int pointsCountToCompareAsPercent, int tiltX, int tiltY)
        {
            int totalPoints = template.Width * template.Height;
            int pointsToCheck = (int)(totalPoints * (pointsCountToCompareAsPercent / 100.0));
            HashSet<Point> checkedPoints = new HashSet<Point>();

            int checkedCount = 0; // Counter for how many points have been checked
            while(checkedCount < pointsToCheck)
            {
                // Generate random coordinates within the template dimensions (but awoid edges because we may tilt one image to side)
                int randomX = random.Next(Math.Abs(tiltX), template.Width - Math.Abs(tiltX) * 2);
                int randomY = random.Next(Math.Abs(tiltY), template.Height - Math.Abs(tiltY) * 2);
                Point point = new Point(randomX, randomY);

                // Check if this point has already been checked
                if(checkedPoints.Contains(point))
                {
                    continue; // Skip already checked points
                }

                // Mark this point as checked
                checkedPoints.Add(point);
                checkedCount++; // Increment the checked count

                // Get pixel colors from template and screen image
                Color templateColor = ((Bitmap)template).GetPixel(randomX, randomY);
                Color screenColor = screenImage.GetPixel(randomX + tiltX, randomY + tiltY);

                // Compare colors
                if(templateColor.ToArgb() != screenColor.ToArgb())
                {
                    return false; // Exit comparison on first mismatch
                }
            }

            checkedPoints.Clear();

            return true; // All compared points matched
        }
    }
}
