using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ARKitMeetup.Helpers;
using ARKitMeetup.Models;
using AVFoundation;
using CoreFoundation;
using CoreGraphics;
using CoreML;
using Foundation;
using MaterialComponents;
using Newtonsoft.Json;
using SoundAnalysis;
using UIKit;

namespace HelloiOS13.Demos.D4CoreML
{
    [DisplayInMenu(DisplayName = "Audio Classifier", DisplayDescription = "Use SoundAnalysis framework with a model trained on FreeSound audio in CreateML3")]
    public class AudioClassifierViewController : BaseViewController, ISNResultsObserving
    {
        NSUrl FreeSounds25ModelUrl => NSBundle.MainBundle.GetUrlForResource("FreeSoundsModel25", "mlmodelc");

        MLModel Model;
        AVAudioEngine AudioEngine;
        SNAudioStreamAnalyzer Analyser;
        DispatchQueue AnalysisQueue;

        UILabel TitleLabel;
        private UIView Bar;
        UILabel CurrentLabel;
        UIView AllFeaturesContainerView;
        UILabel AllFeaturesLabel;

        protected virtual MLModel GetModel()
            => MLModel.Create(FreeSounds25ModelUrl, out var modelLoadError);

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupViews();
            StartAnalysing();
        }

        private void StartAnalysing()
        {
            Model = GetModel();
            
            AudioEngine = new AVAudioEngine();
            AnalysisQueue = new DispatchQueue("com.r2.SoundAnalysis", false);

            var inputFormat = AudioEngine.InputNode.GetBusInputFormat(0);
            var request = new SNClassifySoundRequest(Model, out var soundRequestError);

            Analyser = new SNAudioStreamAnalyzer(inputFormat);
            Analyser.AddRequest(request, this, out var addRequestError);

            AudioEngine.InputNode.InstallTapOnBus(
                bus: 0,
                bufferSize: 8192,
                format: inputFormat,
                tapBlock: (buffer, when) =>
                    AnalysisQueue.DispatchAsync(() =>
                        Analyser.Analyze(buffer, when.SampleTime)));

            AudioEngine.Prepare();
            AudioEngine.StartAndReturnError(out var initEngineError);
        }

        public void DidProduceResult(ISNRequest request, ISNResult result)
        {
            // seems like SNClassificationResult is not properly bound yet
            // we can use valueForKey: to get results if we know what we're looking for
            // not sure whether i need to dispose after doing this
            using (var o = new NSObject(result.Handle))
            {
                var rawClassifications = (NSArray)o.ValueForKey(new NSString("classifications"));
                var results = rawClassifications
                           .AsEnumerable<SNClassification>()
                           .OrderByDescending(x => x.Confidence)
                           .ToDictionary(x => x.Identifier, x => Math.Round(x.Confidence,2));

                var best = results.First();

                Console.WriteLine(JsonConvert.SerializeObject(results, Formatting.Indented));

                DispatchQueue.MainQueue.DispatchSync(() =>
                { 
                    SetLabel(results);
                    OnClassification(best.Key, best.Value);
                });
            }
        }
        
        double threshold = .9;
        string current;
        private void OnClassification(string identifier, double confidence)
        {
            // changed classification, or current fell below threshold, remove
            var classificationNoLongerValid = (confidence < threshold || identifier != current) && current != null;
            if (classificationNoLongerValid && CurrentLabel != null)
                TransitionOutLabel(CurrentLabel);

            // new classification, add
            var newValidClassification = confidence >= threshold && identifier != current;
            if (newValidClassification)
                CurrentLabel = CreateAndAnimateLabel(identifier);

            current = confidence >= threshold ? identifier : null;
        }

        private void SetupViews()
        {
            View.BackgroundColor = UIColor.SystemBackgroundColor;

            AllFeaturesContainerView = new UIView { };
            View.FillWith(AllFeaturesContainerView);

            TitleLabel = new UILabel
            {
                Text = "Make a sound",
                Font = UIFont.BoldSystemFontOfSize(32),  
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            Bar = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.SystemBlueColor,
                Alpha = 0.1f,
            };

            AllFeaturesLabel = new UILabel { };

            View.AddSubviews(TitleLabel, AllFeaturesLabel, Bar);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(TitleLabel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.TopMargin, 1, 20),

                NSLayoutConstraint.Create(Bar, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(Bar, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterY, 1, -50),
                NSLayoutConstraint.Create(Bar, NSLayoutAttribute.Width, NSLayoutRelation.Equal, 1, 25),
                NSLayoutConstraint.Create(Bar, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 25),
            });

            Bar.Layer.CornerRadius = 12.5f;
        }

        #region super ugly ui code
        private static void TransitionOutLabel(UILabel l)
            => UIView.Animate(2, () =>
            {
                l.Transform = CGAffineTransform.MakeTranslation(0, -200);
                l.Alpha = 0;
            });

        private UILabel CreateAndAnimateLabel(string identifier)
        {
            var l = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = identifier,
                Alpha = 0,
                Font = UIFont.BoldSystemFontOfSize(48)
            };
            
            View.AddSubview(l);
            View.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(l, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(l, NSLayoutAttribute.CenterY, NSLayoutRelation.Equal, View, NSLayoutAttribute.CenterY, 1, -50),
            });

            UIView.Animate(1, () => l.Alpha = 1);

            return l;
        }

        float minAlpha = .25f;
        public void SetLabel(Dictionary<string, double> classifications)
        {
            var existingLabel = AllFeaturesLabel;
            var newLabel = new UILabel
            {
                Font = UIFont.BoldSystemFontOfSize(24),
                AttributedText = new NSAttributedString(String.Join(" ", Categories)),
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Lines = 0,
                TextColor = UIColor.SecondaryLabelColor.ColorWithAlpha(minAlpha),
                Alpha = 0f
            };

            AllFeaturesLabel = newLabel;

            foreach (var classication in classifications)
                SetCategory(classication.Key, classication.Value);

            AllFeaturesContainerView.AddSubview(newLabel);
            AllFeaturesContainerView.AddConstraints(new[]
            {
                NSLayoutConstraint.Create(AllFeaturesLabel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, AllFeaturesContainerView, NSLayoutAttribute.CenterX, 1, 0),
                NSLayoutConstraint.Create(AllFeaturesLabel, NSLayoutAttribute.Width, NSLayoutRelation.Equal, AllFeaturesContainerView, NSLayoutAttribute.Width, 1, -20),
                NSLayoutConstraint.Create(AllFeaturesLabel, NSLayoutAttribute.BottomMargin, NSLayoutRelation.Equal, AllFeaturesContainerView, NSLayoutAttribute.BottomMargin, 1, -40),
            });

            if (existingLabel != null)
                UIView.Transition(AllFeaturesContainerView, .5, UIViewAnimationOptions.TransitionCrossDissolve | UIViewAnimationOptions.CurveLinear | UIViewAnimationOptions.AllowUserInteraction,
                () =>
                {
                    existingLabel.Alpha = 0f;
                    newLabel.Alpha = 1;
                },
                () => existingLabel.RemoveFromSuperview());

            var scale = (nfloat)classifications.First().Value * 15;
            var alpha = .05f + (float)(classifications.First().Value * .2f);

            UIView.AnimateNotify(
               .75f, 0, .9f, 0,
               UIViewAnimationOptions.CurveEaseIn | UIViewAnimationOptions.AllowUserInteraction,
               () =>
               {
                   Bar.Alpha = alpha;
                   Bar.Transform = CGAffineTransform.MakeScale(scale, scale);

               }, null);
        }

        private void SetCategory(string category, double pct)
        {
            var start = AllFeaturesLabel.Text.IndexOf(category, StringComparison.InvariantCultureIgnoreCase);
            var mut = new NSMutableAttributedString(AllFeaturesLabel.AttributedText);
            var alpha = (pct * (1 - minAlpha)) + minAlpha;

            mut.AddAttribute(new NSString("NSColor"), UIColor.LabelColor.ColorWithAlpha((float)alpha), new NSRange(start, category.Length));

            AllFeaturesLabel.AttributedText = mut;
        }

        #endregion

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            Analyser?.CompleteAnalysis();
        }

        protected virtual string[] Categories { get; } =
        {          "Hi-hat",          "Saxophone",          "Trumpet",          "Glockenspiel",          "Cello",          "Knock",          "Gunshot_or_gunfire",          "Clarinet",          "Computer_keyboard",          "Keys_jangling",          "Snare_drum",          "Writing",          "Laughter",          "Fart",          "Tearing",          "Oboe",          "Flute",          "Cough",          "Telephone",          "Bark",          "Chime",          "Bass_drum",          "Bus",          "Squeak",          "Scissors",          "Harmonica",          "Gong",          "Microwave_oven",          "Burping_or_eructation",          "Double_bass",          "Shatter",          "Fireworks",          "Tambourine",          "Cowbell",          "Electric_piano",          "Meow",          "Drawer_open_or_close",          "Applause",          "Acoustic_guitar",          "Violin_or_fiddle",          "Finger_snapping"        };
    }
}
