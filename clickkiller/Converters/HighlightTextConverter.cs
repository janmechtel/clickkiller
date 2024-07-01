using Avalonia.Controls.Documents;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace clickkiller.Converters
{
    public class HighlightTextConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count != 2 || !(values[0] is string text) || !(values[1] is string highlightText))
            {
                return new InlineCollection();
            }

            var searchTerms = Regex.Split(highlightText, @"[\s,.]+").Where(w => !string.IsNullOrWhiteSpace(w)).ToList();

            var inlines = new InlineCollection();
            var currentRun = new Run();

            var regex = new Regex(string.Join("|", searchTerms.Select(Regex.Escape)), RegexOptions.IgnoreCase);
            var matches = regex.Matches(text);

            int lastIndex = 0;
            foreach (Match match in matches)
            {
                // Add the text before the match                                                                                                                                                                                                             
                if (match.Index > lastIndex)
                {
                    currentRun.Text += text.Substring(lastIndex, match.Index - lastIndex);
                }

                // Add the matched text with highlighting                                                                                                                                                                                                    
                if (currentRun.Text?.Length > 0)
                {
                    inlines.Add(currentRun);
                    currentRun = new Run();
                }
                var faTheme = App.Current?.Styles.OfType<FluentTheme>().FirstOrDefault();
                faTheme.TryGetResource("SystemControlBackgroundAccentBrush", App.Current.RequestedThemeVariant, out var resource);
                

                inlines.Add(new Run
                {
                    Text = match.Value,
                    FontWeight = FontWeight.Bold,
                    // Background = new SolidColorBrush(Color.FromRgb(255, 0, 0)) 
                    // https://github.com/AvaloniaUI/Avalonia/discussions/13968
                    Background = (IBrush)resource,

                });                                     

                lastIndex = match.Index + match.Length;
            }

            // Add any remaining text after the last match                                                                                                                                                                                                   
            if (lastIndex < text.Length)
            {
                currentRun.Text += text.Substring(lastIndex);
            }

            if (currentRun.Text?.Length > 0)
            {
                inlines.Add(currentRun);
            }

            return inlines;
        }
    }
}
