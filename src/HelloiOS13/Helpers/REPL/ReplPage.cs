using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ARKitMeetup.Helpers;
using CoreGraphics;
using CSharpForMarkup;
using Foundation;
using Mono.CSharp;
using Newtonsoft.Json;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace ARKitMeetup
{
	public class ReplPage : ContentPage
	{
		public static string ClientId = Guid.NewGuid().ToString();

		Editor Editor;

		ObservableCollection<Evaluation> Evaluations = new ObservableCollection<Evaluation>();
		private EvaluatorHost evaluatorHost;
		CollectionView CollectionView;
		StackLayout Stack;
		ScrollView Scroll;
		Grid MainGrid;

		public Func<Task> Terminate { get; set; }

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			Terminate?.Invoke();
		}

		public ReplPage(object context = null)
		{
			evaluatorHost = new EvaluatorHost(context ?? "a");
			evaluatorHost.Evaluate("1");

			Content =

				new Grid
				{
					RowSpacing = 2,
					ColumnSpacing = 2,
					BackgroundColor = Color.Gray,
					RowDefinitions =
					{
						new RowDefinition() { Height = new GridLength(50, GridUnitType.Absolute) },
						new RowDefinition(),
						new RowDefinition { Height = new GridLength(100, GridUnitType.Absolute) },
						new RowDefinition { Height = new GridLength(30, GridUnitType.Absolute) }
					},
					ColumnDefinitions =
					{
						new ColumnDefinition(),
						new ColumnDefinition { Width = new GridLength(80, GridUnitType.Absolute) }
					},

					Children =
					{
						new Grid
						{
							BackgroundColor = Color.Black ,
							Children =
							{
								new Label
								{
									TextColor = Color.White,
									Text = "HAPPY HAPPY REPL",
									FontFamily = "Apple-Kid",
									FontSize = 42,
									HorizontalTextAlignment = TextAlignment.Center,
									VerticalTextAlignment = TextAlignment.Center,
								}
							}
						}.Row(0).ColSpan(2),

						new Grid
						{
							BackgroundColor = Color.Black
						}.Row(3).ColSpan(2),

						new CollectionView
						{
							BackgroundColor = Color.Black,
							ItemsSource = Evaluations,
							ItemTemplate = new DataTemplate(() =>
							{
								var cell = new EvaluationCell();
								cell.Bind(EvaluationCell.EvaluationProperty);

								return cell;
							}),
							IsVisible = false
						}
						.Assign(out CollectionView)
						.Row(1).ColSpan(2),

						GetReplView(),

						new Editor
						{
							IsSpellCheckEnabled = false,
							Keyboard = Keyboard.Create(KeyboardFlags.None),
							BackgroundColor = Color.Black,
							TextColor = Color.White,
						}.Row(2)
						.Invoke(x =>
						{
							x.TextChanged += async delegate
							{
								if (string.IsNullOrWhiteSpace(x.Text))
									return;

								var lastChar = x.Text.LastOrDefault();
								if (lastChar == '\n')
								{
									await Evaluate(Editor.Text);
									return;
								}
							};
						})
						.Assign(out Editor),

						new Button
						{
							Text = "Eval",
							TextColor = Color.White,
							FontFamily = "Apple-Kid",
							FontSize = 38,
							BackgroundColor = Color.Black
						}.Row(2).Col(1)
						.Invoke(x =>
						{
							x.Clicked += async delegate { await Evaluate(Editor.Text); };

						}),

					}
				}.Assign(out MainGrid);
		}

        public async Task SetContext(object context)
        {
            if (evaluatorHost.UpdateContext(context, true))
                await Evaluate("@this", unfocus: true, overrideInstructionDisplay: "@this is now:");
        }

        private ScrollView GetReplView()
		{
			return
			new ScrollView
			{
				BackgroundColor = Color.Black,
				Content =
					new StackLayout
					{
						VerticalOptions = LayoutOptions.End,
						BackgroundColor = Color.Black,
					}
					.Assign(out Stack)
			}
			.Assign(out Scroll)
			.Row(1).ColSpan(2);
		}

		double maxItems = 4;
		private async Task Evaluate(string cmd, string who = null, bool unfocus = false, string overrideInstructionDisplay = null)
		{
			cmd = cmd.Trim();

			if (String.IsNullOrWhiteSpace(cmd))
				return;

			if (!HandleReplCommand(cmd))
			{
				var result = await evaluatorHost.Evaluate(cmd);

				Evaluations.Add(result);

				var v = ViewForResult(result);
                if (!String.IsNullOrWhiteSpace(overrideInstructionDisplay))
                    v.Label.Text = overrideInstructionDisplay;

				if (who != null)
				{
					v.Label.Text = $"[Remote cmd from '{who}']\r\n{v.Label.Text}";
				}
				v.GestureRecognizers.Add(new TapGestureRecognizer(gr =>
				{
					OnResultTapped(result, v);
				}));

				v.Opacity = 0;
				v.TranslationY = 30;

				var i = 0;
				foreach (var item in Stack.Children.Reverse())
				{
					var targetAlpha = i > ((maxItems - 1) * 2) ? 0 : 1;
					Console.WriteLine(targetAlpha);
					item.FadeTo(targetAlpha, 500);
					i++;
				}
				foreach (var child in Stack.Children.Reverse().ToList().Skip((int)(maxItems * 2)))
				{
					Stack.Children.Remove(child);
				}

				var z = new BoxView { HeightRequest = 1 };
				Stack.Children.Add(v);
				await Task.Yield();
				Stack.Children.Add(z);

				v.TranslateTo(0, 0, 2, Easing.CubicOut);
				v.FadeTo(1);
			}

			Editor.Text = "";

            if (!unfocus)
    			Editor.Focus();
		}

		private async Task OnResultTapped(Evaluation result, View v)
		{
			Editor.Text = result.Command;

			string missingNamespace = "";
			if (IsProbablyMissingNameSpace(result, out missingNamespace))
			{
				await Evaluate($"using {missingNamespace}");
				await Task.Delay(TimeSpan.FromSeconds(.5));
				await Evaluate(result.Command);
			}

			Editor.Focus();
		}

		private bool IsProbablyMissingNameSpace(Evaluation result, out string missingNamespace)
		{
			missingNamespace = null;

			var missingNamespaceLog = result.Log.FirstOrDefault(x => x.Text.Contains("Are you missing "));
			if (missingNamespaceLog == null)
				return false;

			missingNamespace =
				missingNamespaceLog.Text
					.Split(new[] { "Are you missing `" }, StringSplitOptions.RemoveEmptyEntries)[1]
					.Split('\'')[0];

			return true;
		}

		static float FontSize = 28;
		public EvaluationStackCell ViewForResult(Evaluation eval)
		{
			var cell = new EvaluationStackCell { TranslationY = 0, VerticalOptions = LayoutOptions.End };
			cell.Label.Text = $"> {eval.Command}";

			var result = new Label { TextColor = Color.Gray, FontFamily = "Apple-Kid", FontSize = FontSize };

			var evalResult = eval.Result;
			var evalType = evalResult?.GetType();

			cell.Result.Content = new StackLayout { Children = { result } };

			var output =
				(eval.Log.Any(y => y.Kind == EvaluatorInputKind.Error)
				? JsonConvert.SerializeObject(eval.Log, Formatting.None) + Environment.NewLine : "")
				+ (eval.Result == null ? "" : JsonConvert.SerializeObject(eval.Result ?? null, Formatting.None));

			result.Text = output;

			return cell;
		}

		private bool HandleReplCommand(string cmd)
		{
			if (cmd == "!clear")
			{
				foreach (var eval in Evaluations.Reverse().ToList())
					Evaluations.Remove(eval);

				Stack.Children.Clear();
				MainGrid.Children.Remove(Scroll);
				MainGrid.Children.Add(GetReplView());

				return true;
			}

			if (cmd == "!reset")
			{
				evaluatorHost.Reset();
				return true;
			}

			return false;
		}

		public class EvaluationStackCell : ContentView
		{
			public Label Label;
			public ContentView Result;

			public EvaluationStackCell()
			{
				Content = new StackLayout
				{
					Padding = 10,
					VerticalOptions = LayoutOptions.Start,
					Children =
					{
						new Label { TextColor = Color.White, FontFamily = "Apple-Kid", FontSize = FontSize, LineBreakMode = LineBreakMode.WordWrap, }.Row(0).Assign(out Label),
						new ContentView() { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start }
							.Row(1).Assign(out Result),
					}
				};
			}
		}

		public class EvaluationCell : ContentView
		{
			static float FontSize = 24;

			public static readonly BindableProperty EvaluationProperty =
				BindableProperty.Create("Evaluation", typeof(object), typeof(EvaluationCell), null, propertyChanged: OnEvaluationChanged);

			public string Evaluation
			{
				get { return (string)GetValue(EvaluationProperty); }
				set { SetValue(EvaluationProperty, value); }
			}

			static void OnEvaluationChanged(BindableObject bindable, object oldValue, object newValue)
			{
				Debug.WriteLine(newValue);

				if (newValue == null)
					return;

				var @this = (EvaluationCell)bindable;
				var eval = (Evaluation)newValue;

				@this.Label.Text = "> " + eval.Command;

				var result = new Label { TextColor = Color.Gray, FontFamily = "Apple-Kid", FontSize = FontSize };

				Console.WriteLine(eval.Result);

				var evalResult = eval.Result;
				var evalType = evalResult?.GetType();
				
				@this.Result.Content = new StackLayout { Children = { result } };

				var output =
					(eval.Log.Any(y => y.Kind == EvaluatorInputKind.Error)
					? JsonConvert.SerializeObject(eval.Log, Formatting.None) + Environment.NewLine : "")
					+ JsonConvert.SerializeObject(eval.Result ?? "(null)", Formatting.None);

				result.Text = output;
		    }

			Label Label;
			ContentView Result;

			public EvaluationCell()
			{
				Content = new StackLayout
				{
					Padding = 10,
					VerticalOptions = LayoutOptions.Start,
					Children =
					{
						new Label { TextColor = Color.White, FontFamily = "Apple-Kid", FontSize = FontSize, LineBreakMode = LineBreakMode.WordWrap, }.Row(0).Assign(out Label),
						new ContentView() { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start }
							.Row(1).Assign(out Result),
					}
				};
			}

		}
	}

    public class EvaluatorHost
    {
        private bool _firstRun = true;
        protected List<EvaluatorOutput> Outputs = new List<EvaluatorOutput>();

        private string _sessionGuid = Guid.NewGuid().ToString();
        private object _context;

        public EvaluatorHost(object context)
        {
            UpdateContext(context);
        }

        public Task Reset()
        {
            Outputs?.Add(new EvaluatorOutput { Kind = EvaluatorInputKind.Error, Text = "The evaluator was reset." });

            return Task.CompletedTask;
        }
        
        public async Task<Evaluation> Evaluate(string s)
        {
            var isFirstInvocation = CheckAndClearFirstInvocationFlag();

            // get source to reload
            var source = s;

            // setup evaluator
            Outputs.Clear();
            Outputs.Add(
                new EvaluatorOutput
                {
                    Kind = EvaluatorInputKind.Error,
                    Text = "The evaluation capability is not included in this demo repo"
                });

            InitIfNeccessary(false, Outputs);

            return new Evaluation
            {
                Command = s,
                Log = Outputs.ToList(),
                Result = null,
            };
        }

        private void InitIfNeccessary(bool force = false, List<EvaluatorOutput> messages = null)
        {
            SetContext();
        }

        public bool UpdateContext(object context, bool set = false)
        {
            var changed = _context != context;

            _context = context;
            WH.Set(_sessionGuid, context);

            if (set)
                SetContext();

            return changed;
        }

        public void SetContext()
        {
            var tContextName = _context.GetType().FullName;
        }
        
        private bool CheckAndClearFirstInvocationFlag()
        {
            var isFirstRun = _firstRun;
            _firstRun = false;

            return isFirstRun;
        }
    }

    public class EvaluatorOutputHelper
    {
        public static EvaluatorOutput FromMessage(AbstractMessage msg)
        {
            var output = msg.CloneInto<EvaluatorOutput>();
            output.Kind = output.Kind == EvaluatorInputKind.Warning
                ? EvaluatorInputKind.Warning
                : EvaluatorInputKind.Information;

            return output;
        }

        public static EvaluatorOutput FromError(Exception ex)
            => new EvaluatorOutput
            {
                Kind = EvaluatorInputKind.Error,
                Text = ex.Message
            };
    }

    public class EvaluatorOutput
    {
        public EvaluatorInputKind Kind { get; set; }
        public string Text { get; set; }
    }

    public enum EvaluatorInputKind
    {
        Information,
        Warning,
        Error
    }

    public class Evaluation
    {
        public string Command { get; set; }
        public object Result { get; set; }

        public Evaluation Self => this;

        public List<EvaluatorOutput> Log { get; internal set; }
    }
}