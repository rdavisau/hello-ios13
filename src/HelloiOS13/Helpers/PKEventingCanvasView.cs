using System;
using CoreGraphics;
using PencilKit;
using UIKit;

namespace HelloiOS13.Demos.D2PencilKit
{
    public class PKEventingCanvasView : PKCanvasView
    {
        public Action<UIImage> OnImage { get; set; }
        public bool SuppressChanges { get; internal set; }

        public PKEventingCanvasView() => Delegate = new PKDelegate();

        public class PKDelegate : PKCanvasViewDelegate
        {
            public override void DrawingDidChange(PKCanvasView canvasView)
            {
                if (!(canvasView is PKEventingCanvasView pk))
                    return;

                var rect = new CGRect(new CGPoint(0, 0), canvasView.Frame.Size);
                var img = canvasView.Drawing.GetImage(rect, 1);

                if (img != null && !pk.SuppressChanges)
                    pk.OnImage?.Invoke(img);
            }
        }
    }
}
