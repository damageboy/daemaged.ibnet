using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace IBGrid
{
    public class PopupSelection : SourceGrid.Cells.Controllers.ControllerBase
    {
        public PopupSelection()
        {
        }

        public override void OnMouseUp(SourceGrid.CellContext sender, MouseEventArgs e)
        {
            base.OnMouseUp(sender, e);

            if (e.Button == MouseButtons.Right)
            {
                sender.Grid.Selection.ResetSelection(true);
                sender.Grid.Selection.SelectRow(sender.CellRange.Start.Row, true);
            }
        }
    }  

    class RotatedText : DevAge.Drawing.VisualElements.TextGDI
    {
        public RotatedText(float angle)
        {
            Angle = angle;
        }

        public float Angle = 0;

        protected override void OnDraw(DevAge.Drawing.GraphicsCache graphics, RectangleF area)
        {
            System.Drawing.Drawing2D.GraphicsState state = graphics.Graphics.Save();
            try
            {
                float width2 = area.Width / 2;
                float height2 = area.Height / 2;

                //For a better drawing use the clear type rendering
                graphics.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                //Move the origin to the center of the cell (for a more easy rotation)
                graphics.Graphics.TranslateTransform(area.X + width2, area.Y + height2);

                graphics.Graphics.RotateTransform(Angle);

                StringFormat.Alignment = StringAlignment.Center;
                StringFormat.LineAlignment = StringAlignment.Center;
                graphics.Graphics.DrawString(Value, Font, graphics.BrushsCache.GetBrush(ForeColor), 0, 0, StringFormat);
            }
            finally
            {
                graphics.Graphics.Restore(state);
            }
        }

        //Here I should also implement MeasureContent (think also for a solution to allow rotated text with any kind of allignment)
        protected override SizeF OnMeasureContent(DevAge.Drawing.MeasureHelper measure, SizeF maxSize)
        {
            SizeF size;
            SizeF boundSize = new SizeF();

            //For a better drawing use the clear type rendering
            measure.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            StringFormat.Alignment = StringAlignment.Center;
            StringFormat.LineAlignment = StringAlignment.Center;
            size = measure.Graphics.MeasureString(Value, Font);
            float boundAngle = Math.Abs(Angle);
            boundSize.Width = (float)Math.Cos((boundAngle / 180) * Math.PI) * size.Width;
            boundSize.Height = (float)Math.Sin((boundAngle / 180) * Math.PI) * size.Width;

            return boundSize;
        }
    }
}
