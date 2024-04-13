using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;
using System;
using TableConverter.Interfaces;
using Avalonia.Threading;
using System.IO;

namespace TableConverter.Services
{
    internal class ConverterHandlerHtmlOutputService : ConverterHandlerOutputAbstract
    {
        private bool MinifyHtml { get; set; } = false;

        private bool IncludeTheadTbody { get; set; } = false;

        public override void InitializeControls()
        {

            var minify_html_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var minify_html_check_box = new CheckBox()
            {
                IsChecked = MinifyHtml
            };

            minify_html_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    MinifyHtml = check_box.IsChecked ?? false;
                }
            };

            var minify_html_label = new Label()
            {
                Content = "Minify HTML",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Center
            };

            minify_html_stack_panel.Children.Add(minify_html_check_box);
            minify_html_stack_panel.Children.Add(minify_html_label);

            Controls?.Add(minify_html_stack_panel);

            var include_thead_tbody_stack_panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10
            };

            var include_thead_tbody_check_box = new CheckBox()
            {
                IsChecked = IncludeTheadTbody
            };

            include_thead_tbody_check_box.IsCheckedChanged += (sender, e) =>
            {
                if (sender is CheckBox check_box)
                {
                    IncludeTheadTbody = check_box.IsChecked ?? false;
                }
            };

            var include_thead_tbody_label = new Label()
            {
                Content = "Include thead/tbody",
                FontFamily = App.Current?.Resources["JetBrainsMono"] as FontFamily ?? throw new NullReferenceException(),
                VerticalAlignment = VerticalAlignment.Center
            };

            include_thead_tbody_stack_panel.Children.Add(include_thead_tbody_check_box);
            include_thead_tbody_stack_panel.Children.Add(include_thead_tbody_label);

            Controls?.Add(include_thead_tbody_stack_panel);
        }

        public override Task<string> ConvertAsync(string[] headers, string[][] rows, object? progress_bar)
        {
            return Task.Run(() =>
            {
                StringWriter string_writer = new StringWriter();

                int tab_count = 0;

                string_writer.Write($"<table>{(MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tab_count))}");

                if (IncludeTheadTbody)
                {
                    string_writer.Write($"<thead>{(MinifyHtml ? "" : Environment.NewLine + new string('\t', ++tab_count))}");
                }

                string_writer.Write($"<tr>{(MinifyHtml ? "" : Environment.NewLine)}");

                tab_count++;

                for (long i = 0; i < headers.LongLength; i++)
                {
                    string_writer.Write($"{(MinifyHtml ? "" : new string('\t', tab_count))}<th>");
                    string_writer.Write(headers[i]);
                    string_writer.Write($"</th>{(MinifyHtml ? "" : Environment.NewLine)}");
                }

                string_writer.Write($"{(MinifyHtml ? "" : new string('\t', --tab_count))}</tr>");

                if (IncludeTheadTbody)
                {
                    string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', --tab_count))}</thead>");
                }

                for (long i = 0; i < rows.LongLength; i++)
                {
                    if (IncludeTheadTbody)
                    {
                        string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', tab_count++))}<tbody>");
                    }

                    string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', tab_count++))}<tr>");

                    for (long j = 0; j < headers.LongLength; j++)
                    {
                        string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', tab_count))}<td>");
                        string_writer.Write(rows[i][j]);
                        string_writer.Write("</td>");
                    }

                    string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', --tab_count))}</tr>");

                    if (IncludeTheadTbody)
                    {
                        string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine + new string('\t', --tab_count))}</tbody>");
                    }

                    SetProgressBarValue(progress_bar, i, 0, rows.LongLength - 1);
                }

                string_writer.Write($"{(MinifyHtml ? "" : Environment.NewLine)}</table>");

                return string_writer.ToString();
            });
        }
    }
}
