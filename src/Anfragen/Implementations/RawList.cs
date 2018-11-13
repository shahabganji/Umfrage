﻿using Anfragen.Extensions;
using Anfragen.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Anfragen.Implementations {
	public class RawList : Question {



		private IList<IOption> _options;
		public IEnumerable<IOption> Options => this._options;

		public int VisibleOptions { get; protected set; }
		public RawList AddOption(IOption option) {
			this._options.Add(option);
			return this;
		}

		public RawList(string question, IEnumerable<IOption> options = null, int visibleOptions = 3, IQuestionnaire questionnaire = null) : base(question, questionnaire) {
			this._options = new List<IOption>();
			this.VisibleOptions = visibleOptions;
		}

		protected override Question TakeAnswer() {
			IUserTerminal terminal = this.Terminal;

			int column = Console.CursorLeft;
			int line = Console.CursorTop;

			int activeOption = -1;
			if (State == QuestionStates.Initilaized) {
				this.DrawOptions(terminal);
			}

			Console.SetCursorPosition(column, line);
			activeOption = HandleInput(terminal, column, line, activeOption);

			this.State = this.Validate() ? QuestionStates.Valid : QuestionStates.Invalid;

			if (this.State == QuestionStates.Invalid) {
				Console.SetCursorPosition(0, line + this.VisibleOptions + 1);
				this.PrintValidationErrors();
				Console.SetCursorPosition(column, line);
				this.TakeAnswer();

			} else {
				this.ClearLines(line + 1, line + this.VisibleOptions + 1);
			}

			// resets the cursor to the next line, 
			// because of the options the cursor might be in wrong position for the next question
			Console.SetCursorPosition(0, line + 1);

			terminal.ResetColor();
			return this;

		}

		protected virtual int HandleInput(IUserTerminal terminal, int column, int line, int activeOption) {
			bool answered = false;

			while (!answered) {

				ConsoleKeyInfo keyInfo = Console.ReadKey();
				Console.SetCursorPosition(column, line);

				switch (keyInfo.Key) {
					case ConsoleKey.Enter:
						this.Answer = activeOption > -1 ? this._options[ activeOption ].Text : null;
						answered = true;
						break;
					case ConsoleKey.UpArrow:

						this.ClearLines(line + 1, line + this._options.Count);
						Console.SetCursorPosition(column, line);

						--activeOption;
						if (activeOption == -1) {
							activeOption = this._options.Count - 1;
						}


						this.DrawOptions(terminal, activeOption);
						Console.SetCursorPosition(column, line);

						this.ClearAnswer(line);
						terminal.ForegroundColor = this.Questionnaire.Settings.AnswerColor;
						terminal.Printer.Write(this._options[ activeOption ].Text);

						break;
					case ConsoleKey.DownArrow:

						this.ClearLines(line + 1, line + this._options.Count);
						Console.SetCursorPosition(column, line);

						++activeOption;
						if (activeOption == this._options.Count) {
							activeOption = 0;
						}

						this.DrawOptions(terminal, activeOption);
						Console.SetCursorPosition(column, line);

						this.ClearAnswer(line);
						terminal.ForegroundColor = this.Questionnaire.Settings.AnswerColor;
						terminal.Printer.Write(this._options[ activeOption ].Text);
						break;
					default:
						this.ClearAnswer(line);
						break;
				}
			}

			return activeOption;
		}

		protected virtual void DrawOptions(IUserTerminal terminal, int active = -1) {
			// set terminal to next line
			terminal.ForegroundColor = this.Questionnaire.Settings.QuestionColor;
			terminal.Printer.WriteLine();

			int page = active / this.VisibleOptions;
			var visible_items = this._options.Skip( page * this.VisibleOptions ).Take(this.VisibleOptions).ToList();

			for (int index = 0; index < visible_items.Count; index++) {

				// which option is selected
				if (index == ( active >= this.VisibleOptions ? active - this.VisibleOptions : active)) {
					terminal.ForegroundColor = this.Questionnaire.Settings.AnswerColor;
				}
				//	**********************************************************

				terminal.Printer.Write("  > ");

				terminal.ForegroundColor = this.Questionnaire.Settings.QuestionColor;
				terminal.Printer.WriteLine($"{visible_items[ index ].Text}");

			}
		}
	}

	public class ListOption : IOption {
		public string Text { get; }

		public ListOption(string option) {
			this.Text = option;
		}
	}

	public interface IOption {
		string Text { get; }
	}
}
