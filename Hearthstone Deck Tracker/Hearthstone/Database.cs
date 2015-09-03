﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Database
	{
		private static Dictionary<string, Card> _cards;
		private static readonly List<string> InValidCardSets = new List<string>
		{
			"Credits",
			"Missions",
			"Debug",
			"System"
		};

		static Database()
		{
			Load();
		}

		private static void Load()
		{
			var language = Helper.LanguageDict.ContainsValue(Config.Instance.SelectedLanguage) ? Config.Instance.SelectedLanguage : "enUS";
			try
			{
				var db = XmlManager<CardDb>.Load(string.Format("Files/cardDB.{0}.xml", "enUS"));
				_cards = db.Cards.Where(x => InValidCardSets.All(set => x.CardSet != set)).ToDictionary(x => x.CardId, x => x.ToCard());
				if(language != "enUS")
				{
					var localized = XmlManager<CardDb>.Load(string.Format("Files/cardDB.{0}.xml", language));
					foreach(var card in localized.Cards)
					{
						Card c;
						if(_cards.TryGetValue(card.CardId, out c))
						{
							c.LocalizedName = card.Name;
							c.EnglishText = c.Text;
							c.Text = card.Text;
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.WriteLine("Error loading db: \n" + e, "Game");
			}
		}

		public static Card GetCardFromId(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return null;
			Card card;
			if(_cards.TryGetValue(cardId, out card))
				return (Card)card.Clone();
			Logger.WriteLine("Could not find entry in db for cardId: " + cardId, "Game");
			return new Card(cardId, null, "UNKNOWN", "Minion", "UNKNOWN", 0, "UNKNOWN", 0, 1, "", "", 0, 0, "UNKNOWN", null, 0, "", "");
		}

		public static Card GetCardFromName(string name, bool localized = false)
		{
			var card =
				GetActualCards()
					.FirstOrDefault(c => string.Equals(localized ? c.LocalizedName : c.Name, name, StringComparison.InvariantCultureIgnoreCase));
			if(card != null)
				return (Card)card.Clone();

			//not sure with all the values here
			Logger.WriteLine("Could not get card from name: " + name, "Game");
			return new Card("UNKNOWN", null, "UNKNOWN", "Minion", name, 0, name, 0, 1, "", "", 0, 0, "UNKNOWN", null, 0, "", "");
		}

		public static List<Card> GetActualCards()
		{
			return (from card in _cards.Values
					where card.Type == "Minion" || card.Type == "Spell" || card.Type == "Weapon"
					where Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 1)) || card.Id == "AT_063t"
					where Helper.IsNumeric(card.Id.ElementAt(card.Id.Length - 2))
					where !CardIds.InvalidCardIds.Any(id => card.Id.Contains(id))
					select card).ToList();
		}

		public static string GetHeroNameFromId(string id, bool returnIdIfNotFound = true)
		{
			string name;
			var match = Regex.Match(id, @"(?<base>(.*_\d+)).*");
			if(match.Success)
				id = match.Groups["base"].Value;
			if(CardIds.HeroIdDict.TryGetValue(id, out name))
				return name;
			var card = GetCardFromId(id);
			if(card == null || string.IsNullOrEmpty(card.Name) || card.Name == "UNKNOWN" || card.Type != "Hero")
				return returnIdIfNotFound ? id : null;
			return card.Name;
		}
	}
}