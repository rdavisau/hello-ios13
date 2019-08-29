// FreeSoundsPlusModel25.cs
//
// This file was automatically generated and should not be edited.
//

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using CoreML;
using CoreVideo;
using Foundation;

namespace HelloiOS13 {
	/// <summary>
	/// Model Prediction Input Type
	/// </summary>
	public class FreeSoundsPlusModel25Input : NSObject, IMLFeatureProvider
	{
		static readonly NSSet<NSString> featureNames = new NSSet<NSString> (
			new NSString ("audioSamples")
		);

		MLMultiArray audioSamples;

		/// <summary>
		/// Input audio samples to be classified as 15600 1-dimensional array of floats
		/// </summary>
		/// <value>Input audio samples to be classified</value>
		public MLMultiArray AudioSamples {
			get { return audioSamples; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				audioSamples = value;
			}
		}

		public NSSet<NSString> FeatureNames {
			get { return featureNames; }
		}

		public MLFeatureValue GetFeatureValue (string featureName)
		{
			switch (featureName) {
			case "audioSamples":
				return MLFeatureValue.Create (AudioSamples);
			default:
				return null;
			}
		}

		public FreeSoundsPlusModel25Input (MLMultiArray audioSamples)
		{
			if (audioSamples == null)
				throw new ArgumentNullException (nameof (audioSamples));

			AudioSamples = audioSamples;
		}
	}

	/// <summary>
	/// Model Prediction Output Type
	/// </summary>
	public class FreeSoundsPlusModel25Output : NSObject, IMLFeatureProvider
	{
		static readonly NSSet<NSString> featureNames = new NSSet<NSString> (
			new NSString ("classLabelProbs"), new NSString ("classLabel")
		);

		NSDictionary<NSObject, NSNumber> classLabelProbs;
		string classLabel;

		/// <summary>
		/// Probability of each category as dictionary of strings to doubles
		/// </summary>
		/// <value>Probability of each category</value>
		public NSDictionary<NSObject, NSNumber> ClassLabelProbs {
			get { return classLabelProbs; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				classLabelProbs = value;
			}
		}

		/// <summary>
		/// Most likely sound category as string value
		/// </summary>
		/// <value>Most likely sound category</value>
		public string ClassLabel {
			get { return classLabel; }
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (value));

				classLabel = value;
			}
		}

		public NSSet<NSString> FeatureNames {
			get { return featureNames; }
		}

		public MLFeatureValue GetFeatureValue (string featureName)
		{
			MLFeatureValue value;
			NSError err;

			switch (featureName) {
			case "classLabelProbs":
				value = MLFeatureValue.Create (ClassLabelProbs, out err);
				if (err != null)
					err.Dispose ();
				return value;
			case "classLabel":
				return MLFeatureValue.Create (ClassLabel);
			default:
				return null;
			}
		}

		public FreeSoundsPlusModel25Output (NSDictionary<NSObject, NSNumber> classLabelProbs, string classLabel)
		{
			if (classLabelProbs == null)
				throw new ArgumentNullException (nameof (classLabelProbs));

			if (classLabel == null)
				throw new ArgumentNullException (nameof (classLabel));

			ClassLabelProbs = classLabelProbs;
			ClassLabel = classLabel;
		}
	}

	/// <summary>
	/// Class for model loading and prediction
	/// </summary>
	public class FreeSoundsPlusModel25 : NSObject
	{
		readonly MLModel model;

		static NSUrl GetModelUrl ()
		{
			return NSBundle.MainBundle.GetUrlForResource ("FreeSoundsPlusModel25", "mlmodelc");
		}

		public FreeSoundsPlusModel25 ()
		{
			NSError err;

			model = MLModel.Create (GetModelUrl (), out err);
		}

		FreeSoundsPlusModel25 (MLModel model)
		{
			this.model = model;
		}

		public static FreeSoundsPlusModel25 Create (NSUrl url, out NSError error)
		{
			if (url == null)
				throw new ArgumentNullException (nameof (url));

			var model = MLModel.Create (url, out error);

			if (model == null)
				return null;

			return new FreeSoundsPlusModel25 (model);
		}

		public static FreeSoundsPlusModel25 Create (MLModelConfiguration configuration, out NSError error)
		{
			if (configuration == null)
				throw new ArgumentNullException (nameof (configuration));

			var model = MLModel.Create (GetModelUrl (), configuration, out error);

			if (model == null)
				return null;

			return new FreeSoundsPlusModel25 (model);
		}

		public static FreeSoundsPlusModel25 Create (NSUrl url, MLModelConfiguration configuration, out NSError error)
		{
			if (url == null)
				throw new ArgumentNullException (nameof (url));

			if (configuration == null)
				throw new ArgumentNullException (nameof (configuration));

			var model = MLModel.Create (url, configuration, out error);

			if (model == null)
				return null;

			return new FreeSoundsPlusModel25 (model);
		}

		/// <summary>
		/// Make a prediction using the standard interface
		/// </summary>
		/// <param name="input">an instance of FreeSoundsPlusModel25Input to predict from</param>
		/// <param name="error">If an error occurs, upon return contains an NSError object that describes the problem.</param>
		public FreeSoundsPlusModel25Output GetPrediction (FreeSoundsPlusModel25Input input, out NSError error)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			var prediction = model.GetPrediction (input, out error);

			if (prediction == null)
				return null;

			var classLabelProbsValue = prediction.GetFeatureValue ("classLabelProbs").DictionaryValue;
			var classLabelValue = prediction.GetFeatureValue ("classLabel").StringValue;

			return new FreeSoundsPlusModel25Output (classLabelProbsValue, classLabelValue);
		}

		/// <summary>
		/// Make a prediction using the standard interface
		/// </summary>
		/// <param name="input">an instance of FreeSoundsPlusModel25Input to predict from</param>
		/// <param name="options">prediction options</param>
		/// <param name="error">If an error occurs, upon return contains an NSError object that describes the problem.</param>
		public FreeSoundsPlusModel25Output GetPrediction (FreeSoundsPlusModel25Input input, MLPredictionOptions options, out NSError error)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			if (options == null)
				throw new ArgumentNullException (nameof (options));

			var prediction = model.GetPrediction (input, options, out error);

			if (prediction == null)
				return null;

			var classLabelProbsValue = prediction.GetFeatureValue ("classLabelProbs").DictionaryValue;
			var classLabelValue = prediction.GetFeatureValue ("classLabel").StringValue;

			return new FreeSoundsPlusModel25Output (classLabelProbsValue, classLabelValue);
		}

		/// <summary>
		/// Make a prediction using the convenience interface
		/// </summary>
		/// <param name="audioSamples">Input audio samples to be classified as 15600 1-dimensional array of floats</param>
		/// <param name="error">If an error occurs, upon return contains an NSError object that describes the problem.</param>
		public FreeSoundsPlusModel25Output GetPrediction (MLMultiArray audioSamples, out NSError error)
		{
			var input = new FreeSoundsPlusModel25Input (audioSamples);

			return GetPrediction (input, out error);
		}

		/// <summary>
		/// Make a prediction using the convenience interface
		/// </summary>
		/// <param name="audioSamples">Input audio samples to be classified as 15600 1-dimensional array of floats</param>
		/// <param name="options">prediction options</param>
		/// <param name="error">If an error occurs, upon return contains an NSError object that describes the problem.</param>
		public FreeSoundsPlusModel25Output GetPrediction (MLMultiArray audioSamples, MLPredictionOptions options, out NSError error)
		{
			var input = new FreeSoundsPlusModel25Input (audioSamples);

			return GetPrediction (input, options, out error);
		}
	}
}
