using Bib3;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using BotSharpBotsNavigation = BotSharp.UI.Wpf.BotsNavigation;

namespace Sanderling.UI
{
	public class BotsNavigation
	{
		static (string, string) SupportEmailAddress => ("mailto:", "support@botengine.org");

		static (string, string) ForumAddress => ("http://", "forum.botengine.de");

		static string FindTheRightBotLink => "http://forum.botengine.de/t/how-to-automate-anything-in-eve-online/774";

		static public BotSharpBotsNavigation.NavigationContent NavigationRoot(
			IEnumerable<(string, byte[])> botsOfferedAtRoot,
			BotSharpBotsNavigation.Bot defaultBotForDevelopmentEnvironment)
		{
			var optionsFromBotsOfferedAtRoot =
				botsOfferedAtRoot.EmptyIfNull()
				.Select(botOfferedAtRoot =>
					(botOfferedAtRoot.Item1,
						new BotSharpBotsNavigation.EventHandling
						{
							NavigateInto = new BotSharpBotsNavigation.NavigationContent
							{
								PreviewBot = new BotSharpBotsNavigation.Bot { SerializedBot = botOfferedAtRoot.Item2 },
							},
						}))
				.ToList();

			return
				new BotSharpBotsNavigation.NavigationContent
				{
					Caption = "How may I assist you?",

					SingleChoiceOptions =
						optionsFromBotsOfferedAtRoot
						.Concat(
						new[]
						{
							//	TODO: Add shortcut to last used bot.

							("Automate Something Else",
								new BotSharpBotsNavigation.EventHandling
								{
									NavigateInto = new BotSharpBotsNavigation.NavigationContent
									{
										Caption = "Automate Anything",
										Children = new[]
										{
											new BotSharpBotsNavigation.NavigationContent
											{
												FlowDocument = hyperlinkClickHandler => AutomateSomethingElseGuide(defaultBotForDevelopmentEnvironment, hyperlinkClickHandler),
											},
											new BotSharpBotsNavigation.NavigationContent
											{
												SingleChoiceOptions = new[]
												{
													("📂 Load Bot From File", EventHandlingLoadBotFromFile),
													("Open Development Environment",
													new BotSharpBotsNavigation.EventHandling
													{
														NavigateInto = new BotSharpBotsNavigation.NavigationContent
														{
															BotToInitDevelopmentEnvironment = defaultBotForDevelopmentEnvironment,
														},
													}),
												},
											},
										},
									},
								}),

							("Something Else",
								new BotSharpBotsNavigation.EventHandling
								{
									NavigateInto = new BotSharpBotsNavigation.NavigationContent
									{
										Caption = "General Support",
										FlowDocument = SomethingElseDocument
									},
								}),
						})
						.ToArray(),
				};
		}

		static public FlowDocument AutomateSomethingElseGuide(
			BotSharpBotsNavigation.Bot defaultBotForDevelopmentEnvironment,
			RoutedEventHandler hyperlinkClickHandler)
		{
			var findBotParagraph = new Paragraph();

			Inline linkToDevelopmentEnvironment(string displayText) =>
				BotSharpBotsNavigation.LinkFromDisplayTextAndEventHandling(
					displayText,
					new BotSharpBotsNavigation.EventHandling
					{
						NavigateInto = new BotSharpBotsNavigation.NavigationContent
						{
							BotToInitDevelopmentEnvironment = defaultBotForDevelopmentEnvironment,
						},
					},
					hyperlinkClickHandler);

			findBotParagraph.Inlines.AddRange(new Inline[]
			{
					new Run("To find the right bot for your use case, see the guide at "),
					BotSharpBotsNavigation.LinkToUrlInline(("", FindTheRightBotLink), hyperlinkClickHandler),
					new LineBreak(),
					new Run("When you have chosen a bot, use the "),
					BotSharpBotsNavigation.LinkFromDisplayTextAndEventHandling(
						"📂 Load Bot From File", EventHandlingLoadBotFromFile, hyperlinkClickHandler),
					new Run(" button to run it."),
					new LineBreak(),
					new Run("You can also use the "),
					linkToDevelopmentEnvironment("development environment"),
					new Run(" to create a new bot from scratch."),
			});

			return new FlowDocument(findBotParagraph);
		}

		static public BotSharpBotsNavigation.EventHandling EventHandlingLoadBotFromFile =>
			new BotSharpBotsNavigation.EventHandling
			{
				NavigateInto = new BotSharpBotsNavigation.NavigationContent
				{
					LoadBotFromDialogToPreview = true,
				},

				DropHandling = new BotSharpBotsNavigation.EventHandling
				{
					NavigateInto = new BotSharpBotsNavigation.NavigationContent
					{
						LoadBotFromDropToPreview = true,
					},
				},
			};

		static public FlowDocument SomethingElseDocument(RoutedEventHandler hyperlinkClickHandler)
		{
			var paragraph = new Paragraph();

			paragraph.Inlines.AddRange(new Inline[]
			{
					new Run("For general help, post on the forum at "),
					BotSharpBotsNavigation.LinkToUrlInline(ForumAddress, hyperlinkClickHandler),
					new Run(" or contact us via "),
					BotSharpBotsNavigation.LinkToUrlInline(SupportEmailAddress, hyperlinkClickHandler),
			});

			return new FlowDocument(paragraph);
		}
	}
}
