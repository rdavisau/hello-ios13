using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using HelloiOS13.Demos.D2PencilKit;
using Newtonsoft.Json;
using SceneKit;
using UIKit;

namespace ARKitMeetup.Helpers
{
    public static class Extensions
    {
        private static Random _r = new Random();

        public static UIViewController GetViewController(this UIView view)
        {
            var nr = view.NextResponder;
            while (nr != null)
                if (nr as UIViewController != null)
                    return (UIViewController)nr;
                else
                    nr = nr.NextResponder;

            return null;
        }

        public static void FillWith(this UIView parent, UIView child, int hInset = 0, int vInset = 0)
        {
            child.TranslatesAutoresizingMaskIntoConstraints = false;
            parent.AddSubview(child);
            parent.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(child, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, parent, NSLayoutAttribute.Leading, 1, hInset),
                NSLayoutConstraint.Create(child, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, parent, NSLayoutAttribute.Trailing, 1, -hInset),
                NSLayoutConstraint.Create(child, NSLayoutAttribute.Top, NSLayoutRelation.Equal, parent, NSLayoutAttribute.Top, 1, vInset),
                NSLayoutConstraint.Create(child, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, parent, NSLayoutAttribute.Bottom, 1, -vInset),
            });
        }
        
        public static UIImage Resize(this UIImage image, float toWidth) 
        {
            var canvasSize = new CGSize(toWidth, Math.Ceiling(toWidth/image.Size.Width * image.Size.Height)); 
            var rect = new CGRect(0, 0, canvasSize.Width, canvasSize.Height);
        
            UIGraphics.BeginImageContextWithOptions(canvasSize, false, image.CurrentScale);
            image.Draw(rect);
            var newImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
        
            return newImage; 
        }        

        public static void TileTexture(this SCNGeometry box, int num)
        {
            box.FirstMaterial.Diffuse.WrapS = SCNWrapMode.Mirror;
            box.FirstMaterial.Diffuse.WrapT = SCNWrapMode.Mirror;
            box.FirstMaterial.Diffuse.ContentsTransform = SCNMatrix4.Scale(num, num, num);
        }

        public static Task<List<TOut>> SelectToListAsync<TIn, TOut>(this IEnumerable<TIn> items,
            Func<TIn, Task<TOut>> selector)
            => items.Select(selector)
                .Results();
                
                
        public static async Task<List<T>> Results<T>(this IEnumerable<Task<T>> tasks)
        {
            var ts = await Task.WhenAll(tasks);
            return ts.ToList();
        }
                
        public static IEnumerable<T> DoEach<T>(this IEnumerable<T> objs, Action<T> action)
        {
            foreach (var obj in objs)
            {
                action(obj);
                yield return obj;
            }
        }
        
        public static TAttribute GetCustomAttribute<TAttribute>(this Type t)
            where TAttribute : Attribute
            => (TAttribute)t.GetCustomAttributes(typeof(TAttribute)).FirstOrDefault();

        public static T Random<T>(this IList<T> items)
        {
            return items[_r.Next(0, items.Count())];
        }

        public static T Clone<T>(this T obj)
            => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));

        public static T CloneInto<T>(this object obj)
            => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));

        public static T GetOrDefault<T, U>(this Dictionary<U, T> dict, U key)
            => dict.TryGetValue(key, out var val)
                ? val
                : default(T);

        public static HashSet<T> ToHashSet<T, U>(this IEnumerable<U> items, Func<U, T> selector)
            => new HashSet<T>(items.Select(selector));

        public static IEnumerable<T> AsEnumerable<T>(this NSArray arr)
            where T : NSObject
            => Enumerable
                    .Range(0, (int)((NSArray)arr).Count)
                    .Select(i => arr.GetItem<T>((nuint)i));
    }

    public static class SCNActionExtensions
    {
        public static SCNAction Ease(this SCNAction action, SCNActionTimingMode mode)
        {
            action.TimingMode = mode;

            return action;
        }

        public static void MoveBy(this SCNNode node, float x, float y, float z, double timeInSeconds, SCNActionTimingMode timingMode = SCNActionTimingMode.EaseOut)
            => node.RunAction(SCNAction.MoveBy(x, y, z, timeInSeconds).Ease(timingMode));

        public static void MoveTo(this SCNNode node, SCNNode target, double timeInSeconds, SCNActionTimingMode timingMode = SCNActionTimingMode.EaseOut)
            => node.RunAction(SCNAction.MoveTo(new SCNVector3(target.Position.X, target.Position.Y, target.Position.Z), timeInSeconds).Ease(timingMode));

        public static void RotateBy(this SCNNode node, float x, float y, float z, double timeInSeconds, SCNActionTimingMode timingMode = SCNActionTimingMode.EaseOut)
            => node.RunAction(SCNAction.RotateBy(x, y, z, timeInSeconds).Ease(timingMode));

        public static SCNNode RotateForever(this SCNNode node, double duration)
        {
            var action =
                SCNAction.RepeatActionForever(
                    SCNAction.RotateBy(0, .05f, 0, duration));

            node.RunAction(action);

            return node;
        }

        public static void DrawOnto(this PKEventingCanvasView canvasView, SCNNode node) => canvasView.OnImage = img =>
        {
            if (img is null)
                return;

            node.Geometry.FirstMaterial.Diffuse.Contents = img;
        };
    }
}