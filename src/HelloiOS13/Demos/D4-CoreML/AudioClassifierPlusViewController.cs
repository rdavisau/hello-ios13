using System.Linq;
using ARKitMeetup.Models;
using CoreML;
using Foundation;
using SoundAnalysis;

namespace HelloiOS13.Demos.D4CoreML
{
    [DisplayInMenu(DisplayName = "Audio Classifier Plus", DisplayDescription = "Use SoundAnalysis framework with a model trained on FreeSound audio in CreateML3, including a category trained on Ryan's voice")]
    public class AudioClassifierPlusViewController : AudioClassifierViewController, ISNResultsObserving
    {
        NSUrl FreeSoundsPlus25ModelUrl => NSBundle.MainBundle.GetUrlForResource("FreeSoundsPlusModel25", "mlmodelc");

        protected override MLModel GetModel()
            => MLModel.Create(FreeSoundsPlus25ModelUrl, out var modelLoadError);

        private string[] _categories;
        protected override string[] Categories
        {
            get
            {
                if (_categories != null)
                    return _categories;

                _categories = Enumerable.Concat(base.Categories, new[] { "Nothing", "Ryan" }).ToArray();

                return _categories;
            }
        }
    }
}